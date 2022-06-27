// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text;
using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Common;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Common
{
    public enum ParameterContext
    {
        Undefined,
        TaskExecutions,
        Executions,
        DicomSeries
    }

    public class ConditionalParameterParser : IConditionalParameterParser
    {
        private const string ExecutionsTask = "context.executions.task";
        private const string ContextDicomSeries = "context.dicom.series";

        private readonly ILogger<ConditionalParameterParser> _logger;
        private readonly DicomStore _dicom;


        private readonly Regex _squigglyBracketsRegex = new Regex(@"\{{(.*?)\}}");

        public WorkflowInstance? WorkflowInstance { get; private set; } = null;

        public ConditionalParameterParser(ILogger<ConditionalParameterParser> logger, IStorageService storageService)
        {
            _logger = logger;
            //TODO: Fix DI
            //_storageService = new Lazy<IStorageService>(() => storageService ?? throw new ArgumentNullException(nameof(storageService)));
            _dicom = new DicomStore(
                new Lazy<IStorageService>(() => storageService ?? throw new ArgumentNullException(nameof(storageService)))
            );
        }


        public bool TryParse(string conditions, WorkflowInstance workflowInstance)
        {
            Guard.Against.NullOrEmpty(conditions);
            Guard.Against.Null(workflowInstance);
            try
            {
                conditions = ResolveParameters(conditions, workflowInstance);
                var conditionalGroup = ConditionalGroup.Create(conditions);
                return conditionalGroup.Evaluate();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failure attemping to parse condition", conditions, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Resolves parameters in query string.
        /// </summary>
        /// <param name="conditions">The query string Example: {{ context.executions.task['other task'].'Fred' }}</param>
        /// <param name="workflowInstance">workflow instance to resolve metadata parameter</param>
        /// <returns></returns>
        public string ResolveParameters(string conditions, WorkflowInstance workflowInstance)
        {
            Guard.Against.NullOrEmpty(conditions);
            Guard.Against.Null(workflowInstance);

            WorkflowInstance = workflowInstance;

            try
            {
                var matches = _squigglyBracketsRegex.Matches(conditions);
                if (!matches.Any())
                {
                    WorkflowInstance = null;
                    return conditions;
                }

                var parameters = ParseMatches(matches).Reverse();
                foreach (var parameter in parameters)
                {
                    conditions = conditions
                        .Remove(parameter.Key.Index, parameter.Key.Length)
                        .Insert(parameter.Key.Index, $"'{parameter.Value.Result ?? "null"}'");
                }

                WorkflowInstance = null;
                return conditions;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                WorkflowInstance = null;
                throw e;
            }
        }

        /// <summary>
        /// Parses regex match collection for brackets
        /// </summary>
        /// <param name="matches">regex collection of matches</param>
        /// <returns>
        /// Returns dictionary:
        /// Key: the match will be used to replace resolved match in string via index
        /// Value: is a tuple of resolution.
        /// </returns>
        private Dictionary<Match, (string? Result, ParameterContext Context)> ParseMatches(MatchCollection matches)
        {
            var valuePairs = new Dictionary<Match, (string? Value, ParameterContext Context)>();
            foreach (Match match in matches)
            {
                valuePairs.Add(match, ResolveMatch(match.Value));
            }
            return valuePairs;
        }

        /// <summary>
        /// Resolves a query between two brackets {{ query }}
        /// </summary>
        /// <param name="value">The query Example: {{ context.executions.task['other task'].'Fred' }}</param>
        /// <returns>
        /// Tuple:
        /// Result of the resolution
        /// Context of type of resolution required to resolve query
        /// </returns>
        private (string? Result, ParameterContext Context) ResolveMatch(string value)
        {
            Guard.Against.NullOrWhiteSpace(value);

            value = value.Substring(2, value.Length - 4).Trim();
            var context = ParameterContext.Undefined;
            if (value.StartsWith(ExecutionsTask))
            {
                return ResolveExecutionTasks(value);
            }
            if (value.StartsWith(ContextDicomSeries))
            {
                return ResolveDicom(value);
            }
            return (Result: null, Context: context);
        }

        private (string? Result, ParameterContext Context) ResolveDicom(string value)
        {
            Guard.Against.NullOrWhiteSpace(value);
            Guard.Against.Null(WorkflowInstance);

            var subValue = value.Trim().Substring(ContextDicomSeries.Length, value.Length - ContextDicomSeries.Length);
            var valueArr = subValue.Split('\'');
            var keyId = $"{valueArr[1]}{valueArr[3]}";

            if (subValue.StartsWith(".any"))
            {
                var task = Task.Run(async () => await _dicom.GetAnyValueAsync(keyId, WorkflowInstance.PayloadId, WorkflowInstance.BucketId));
                task.Wait();
                var dicomValue = task.Result;
                return (Result: dicomValue, Context: ParameterContext.DicomSeries);
            }
            if (subValue.StartsWith(".all"))
            {
                var task = Task.Run(async () => await _dicom.GetAllValueAsync(keyId, WorkflowInstance.PayloadId, WorkflowInstance.BucketId));
                task.Wait();
                var dicomValue = task.Result;
                return (Result: dicomValue, Context: ParameterContext.DicomSeries);
            }
            return (Result: null, Context: ParameterContext.DicomSeries);
        }

        private (string? Result, ParameterContext Context) ResolveExecutionTasks(string value)
        {
            var subValue = value.Trim().Substring(ExecutionsTask.Length, value.Length - ExecutionsTask.Length);
            var subValues = subValue.Split('[', ']');
            var id = subValues[1].Trim('\'');
            var task = WorkflowInstance?.Tasks.First(t => t.TaskId == id);

            if (task is null || (task is not null && !task.Metadata.Any()))
            {
                return (Result: null, Context: ParameterContext.TaskExecutions);
            }

            var metadataKey = subValues[2].Split('\'')[1];

            if (task is not null && task.Metadata.ContainsKey(metadataKey))
            {
                var result = task.Metadata[metadataKey];

                if (result is string resultStr)
                {
                    return (Result: resultStr, Context: ParameterContext.TaskExecutions);
                }
            }

            return (Result: null, Context: ParameterContext.TaskExecutions);
        }
    }

    public class DicomStore
    {
        private readonly Lazy<IStorageService> _storageService;

        public DicomStore(Lazy<IStorageService> storageService)
        {
            _storageService = storageService;
        }

        public IStorageService StorageService => _storageService.Value;

        /// <summary>
        /// If any keyid exists return first occurance
        /// if no matchs return 'null'
        /// </summary>
        /// <param name="keyId">example of keyId 00100040</param>
        /// <param name="workflowInstance"></param>
        /// <returns></returns>
        public async Task<string> GetAnyValueAsync(string keyId, string payloadId, string bucketId)
        {
            Guard.Against.NullOrWhiteSpace(keyId);
            Guard.Against.NullOrWhiteSpace(payloadId);
            Guard.Against.NullOrWhiteSpace(bucketId);

            var path = $"{payloadId}\\dcm";
            var listOfFiles = await StorageService.ListObjectsAsync(bucketId, path, true);
            var listOfJsonFiles = listOfFiles.Where(file => file.Filename.EndsWith(".json")).ToList();
            var fileCount = listOfJsonFiles.Count;
            for (int i = 0; i < fileCount; i++)
            {
                var matchValue = await GetDcmJsonFileValueAtIndexAsync(i, path, bucketId, keyId, listOfJsonFiles);

                if (matchValue != null)
                {
                    return matchValue;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// If series contains given value
        /// if all values exist for given key example 0010 0040 then and
        /// they are all same value return that value otherwise return
        /// 'null'
        /// </summary>
        /// <param name="keyId"></param>
        /// <param name="matchValue"></param>
        /// <param name="workflowInstance"></param>
        /// <returns></returns>
        public async Task<string> GetAllValueAsync(string keyId, string payloadId, string bucketId)
        {
            Guard.Against.NullOrWhiteSpace(keyId);
            Guard.Against.NullOrWhiteSpace(payloadId);
            Guard.Against.NullOrWhiteSpace(bucketId);

            // look at dcm folder

            // look at series folders

            // get first json

            // jump to series 2 does it match with series 1

            // check agianst rest of series



            var path = $"{payloadId}/dcm";
            var listOfFiles = await StorageService.ListObjectsAsync(bucketId, path, true);
            var listOfJsonFiles = listOfFiles.Where(file => file.Filename.EndsWith(".json")).ToList();
            var matchValue = await GetDcmJsonFileValueAtIndexAsync(0, path, bucketId, keyId, listOfJsonFiles);
            var fileCount = listOfJsonFiles.Count;
            for (int i = 0; i < fileCount; i++)
            {
                if (listOfJsonFiles[i].Filename.EndsWith(".dcm"))
                {
                    var currentValue = await GetDcmJsonFileValueAtIndexAsync(i, path, bucketId, keyId, listOfJsonFiles);
                    if (currentValue != matchValue)
                    {
                        return string.Empty;
                    }
                }
            }
            return matchValue;
        }

        /// <summary>
        /// Gets file at position
        /// </summary>
        /// <param name="index"></param>
        /// <param name="path"></param>
        /// <param name="bucketId"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public async Task<string> GetDcmJsonFileValueAtIndexAsync(int index,
                                                              string path,
                                                              string bucketId,
                                                              string keyId,
                                                              List<VirtualFileInfo> items)
        {
            Guard.Against.NullOrWhiteSpace(bucketId);
            Guard.Against.NullOrWhiteSpace(path);
            Guard.Against.NullOrWhiteSpace(keyId);
            Guard.Against.Null(items);

            if (index > items.Count)
            {
                return string.Empty;
            }

            var stream = await StorageService.GetObjectAsync(bucketId, items[index].FilePath);
            var jsonStr = Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());

            var dict = JsonConvert.DeserializeObject<Dictionary<string, DicomValue>>(jsonStr);
            dict.TryGetValue(keyId, out var value);
            if (value is not null && value.Value is not null)
            {
                var str = value?.Value.Cast<string>();
                if (str is not null)
                {
                    return string.Concat(str);
                }
            }
            return string.Empty;
        }

        public async Task<string> GetFirstValueAsync(IList<VirtualFileInfo> items, string payloadId, string bucketId, string keyId)
        {
            Guard.Against.NullOrWhiteSpace(bucketId);
            Guard.Against.NullOrWhiteSpace(payloadId);
            Guard.Against.NullOrWhiteSpace(keyId);

            try
            {
                var jsonStr = string.Empty;
                var value = new DicomValue();
                if (items is null || items.Count == 0)
                {
                    return string.Empty;
                }

                foreach (var item in items)
                {
                    if (!item.FilePath.EndsWith(".dcm.json"))
                    {
                        continue;
                    }

                    var stream = await StorageService.GetObjectAsync(bucketId, $"{payloadId}/dcm/{item.Filename}");
                    jsonStr = Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());

                    var dict = JsonConvert.DeserializeObject<Dictionary<string, DicomValue>>(jsonStr);
                    dict.TryGetValue(keyId, out value);
                    if (value is null || value.Value is null)
                    {
                        continue;
                    }

                    var firstValue = value.Value.FirstOrDefault()?.ToString();

                    if (!string.IsNullOrWhiteSpace(firstValue))
                    {
                        return firstValue;
                    }
                }
            }
            catch (Exception e)
            {
                //_logger.FailedToGetDicomTag(payloadId, keyId, bucketId, e);
            }

            return string.Empty;
        }

    }
    public class DicomValue
    {
        [JsonProperty(PropertyName = "vr")]
        public string Vr { get; set; }
        [JsonProperty(PropertyName = "Value")]
        public object[] Value { get; set; }
    }

}

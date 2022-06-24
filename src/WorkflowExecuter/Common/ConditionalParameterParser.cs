// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text;
using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Storage;
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
            finally
            {
                WorkflowInstance = null;
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

            //_dicom.readfiles
            if (subValue.StartsWith(".any"))
            {
                var dicomValue = _dicom.GetAnyValue(keyId, WorkflowInstance.PayloadId, WorkflowInstance.BucketId);
            }
            if (subValue.StartsWith(".all"))
            {
                var dicomValue = _dicom.GetAllValue(keyId, WorkflowInstance.PayloadId, WorkflowInstance.BucketId);
            }
            // loop through files
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

    public class DicomStore : IDicomStore
    {
        private readonly Lazy<IStorageService> _storageService;

        public DicomStore(Lazy<IStorageService> storageService)
        {
            _storageService = storageService;
        }

        public IStorageService StorageService => _storageService.Value;

        private int GetFileCount(string path, string bucketId) => StorageService.ListObjects(bucketId, path, true).Count;

        /// <summary>
        /// If any keyid exists return first occurance
        /// if no matchs return 'null'
        /// </summary>
        /// <param name="keyId">example of keyId 00100040</param>
        /// <param name="workflowInstance"></param>
        /// <returns></returns>
        public string GetAnyValue(string keyId, string payloadId, string bucketId)
        {
            Guard.Against.NullOrWhiteSpace(keyId);
            Guard.Against.NullOrWhiteSpace(payloadId);
            Guard.Against.NullOrWhiteSpace(bucketId);

            var path = $"{payloadId}/dcm";
            var fileCount = GetFileCount(path, bucketId);
            for (int i = 0; i < fileCount; i++)
            {
                var matchValue = GetDcmFileValueAtIndex(i, path, bucketId, keyId);

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
        public string GetAllValue(string keyId, string payloadId, string bucketId)
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
            var fileCount = GetFileCount(path, bucketId);
            var matchValue = GetDcmFileValueAtIndex(0, path, bucketId, keyId);
            for (int i = 0; i < fileCount; i++)
            {
                var currentValue = GetDcmFileValueAtIndex(i, path, bucketId, keyId);
                if (currentValue != matchValue)
                {
                    return string.Empty;
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
        public string GetDcmFileValueAtIndex(int index, string path, string bucketId, string keyId)
        {
            Guard.Against.NullOrWhiteSpace(bucketId);
            Guard.Against.NullOrWhiteSpace(path);
            Guard.Against.NullOrWhiteSpace(keyId);

            var count = GetFileCount(path, bucketId); //TODO get rid off
            if (index > count)
            {
                return string.Empty;
            }

            var items = StorageService.ListObjects(bucketId, path, true);
            var jsonStr = string.Empty;

            if (items is null)
            {
                return string.Empty;
            }

            var stream = new MemoryStream();
            StorageService.GetObject(bucketId, items[index].Filename, s => s.CopyTo(stream));
            jsonStr = Encoding.UTF8.GetString(stream.ToArray());

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
            dict.TryGetValue(keyId, out var value);

            return value ?? string.Empty;
        }

        public string GetFirstValue(string payloadId, string bucketId, string keyId)
        {
            Guard.Against.NullOrWhiteSpace(bucketId);
            Guard.Against.NullOrWhiteSpace(payloadId);
            Guard.Against.NullOrWhiteSpace(keyId);

            var items = StorageService.ListObjects(bucketId, $"{payloadId}/dcm", true);
            var jsonStr = string.Empty;
            var value = string.Empty;
            if (items is null || items.Count == 0)
            {
                return string.Empty;
            }

            foreach (var item in items)
            {
                var stream = new MemoryStream();
                StorageService.GetObject(bucketId, item.Filename, s => s.CopyTo(stream));
                jsonStr = Encoding.UTF8.GetString(stream.ToArray());

                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
                dict.TryGetValue(keyId, out value);
                if (value is not null && value.Trim() != string.Empty)
                {
                    return value;
                }
            }

            return value ?? string.Empty;
        }
    }
}

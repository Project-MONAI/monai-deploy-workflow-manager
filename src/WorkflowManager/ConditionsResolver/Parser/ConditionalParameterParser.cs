/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Globalization;
using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Constants;
using Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Extensions;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Logging;
using Monai.Deploy.WorkflowManager.Common.Storage.Services;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Resovler;

namespace Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Parser
{
    public enum ParameterContext
    {
        Undefined,
        TaskExecutions,
        Executions,
        DicomSeries,
        PatientDetails,
        Workflow
    }

    public class ConditionalParameterParser : IConditionalParameterParser
    {
        private const string ExecutionsTask = "context.executions";
        private const string ContextDicomSeries = "context.dicom.series";
        private const string PatientDetails = "context.input.patient_details";
        private const string ContextWorkflow = "context.workflow";

        private readonly IWorkflowInstanceService _workflowInstanceService;
        private readonly ILogger<ConditionalParameterParser> _logger;
        private readonly IDicomService _dicom;
        private readonly IPayloadService _payloadService;
        private readonly IWorkflowService _workflowService;

        private readonly Regex _squigglyBracketsRegex = new(@"\{{(.*?)\}}", RegexOptions.None, matchTimeout: TimeSpan.FromSeconds(2));

        private WorkflowInstance? _workflowInstance = null;
        private string? _workflowInstanceId = null;

        public ConditionalParameterParser(ILogger<ConditionalParameterParser> logger,
                                          IDicomService dicomService,
                                          IWorkflowInstanceService workflowInstanceService,
                                          IPayloadService payloadService,
                                          IWorkflowService workflowService)
        {
            _workflowInstanceService = workflowInstanceService;
            _logger = logger;
            _dicom = dicomService;
            _payloadService = payloadService;
            _workflowService = workflowService;
        }

        public WorkflowInstance? WorkflowInstance
        {
            get
            {
                if (_workflowInstance is null && _workflowInstanceId is not null)
                {
                    var task = Task.Run(async () => await _workflowInstanceService.GetByIdAsync(_workflowInstanceId));
                    task.Wait();
                    var wf = task.Result;
                    _workflowInstance = wf;
                    _workflowInstanceId = wf.Id;
                }
                return _workflowInstance;
            }
            private set
            {
                if (value == null)
                {
                    _workflowInstance = null;
                    _workflowInstanceId = null;
                }
                _workflowInstance = value;
                _workflowInstanceId = value?.Id;
            }
        }

        public bool TryParse(string[] conditions, WorkflowInstance workflowInstance, out string resolvedConditional)
        {
            Guard.Against.NullOrEmpty(conditions, nameof(conditions));
            ArgumentNullException.ThrowIfNull(workflowInstance, nameof(workflowInstance));

            var joinedConditions = conditions.CombineConditionString();
            return TryParse(joinedConditions, workflowInstance, out resolvedConditional);
        }

        public bool TryParse(string conditions, WorkflowInstance workflowInstance, out string resolvedConditional)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(conditions, nameof(conditions));
            ArgumentNullException.ThrowIfNull(workflowInstance, nameof(workflowInstance));
            resolvedConditional = string.Empty;

            try
            {
                resolvedConditional = ResolveParameters(conditions, workflowInstance);
                var conditionalGroup = ConditionalGroup.Create(resolvedConditional);
                var result = conditionalGroup.Evaluate();
                _logger.ConditionalParserResult(conditions, resolvedConditional, result.ToString());
                return result;
            }
            catch (Exception ex)
            {
                _logger.FailedToParseCondition(resolvedConditional, conditions, ex);
                return false;
            }
        }

        public string ResolveParameters(string conditions, WorkflowInstance workflowInstance)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(conditions, nameof(conditions));
            ArgumentNullException.ThrowIfNull(workflowInstance, nameof(workflowInstance));

            WorkflowInstance = workflowInstance;
            return ResolveParameters(conditions, workflowInstance.Id);
        }

        public string ResolveParameters(string conditions, string workflowInstanceId)
        {
            _workflowInstanceId = workflowInstanceId;
            try
            {
                var matches = _squigglyBracketsRegex.Matches(conditions);
                if (!matches.Any())
                {
                    ClearWorkflowParser();
                    return conditions;
                }

                var parameters = ParseMatches(matches).Reverse();
                foreach (var parameter in parameters)
                {
                    var result = parameter.Value.Result ?? "";
                    if (ResultNullCheck(result.Trim()))
                    {
                        result = "NULL";
                    }
                    if (result.StartsWith('[') && result.EndsWith(']'))
                    {
                        conditions = conditions
                            .Remove(parameter.Key.Index, parameter.Key.Length)
                            .Insert(parameter.Key.Index, $"{result}");
                    }
                    else
                    {
                        conditions = conditions
                            .Remove(parameter.Key.Index, parameter.Key.Length)
                            .Insert(parameter.Key.Index, $"'{result}'");
                    }
                }

                ClearWorkflowParser();
                return conditions;
            }
            catch
            {
                ClearWorkflowParser();
                throw;
            }
        }

        private static bool ResultNullCheck(string? result)
        {
            var isNull =
                string.IsNullOrWhiteSpace(result)
                || result.ToUpper() == "NULL"
                || result.ToUpper() == "UNDEFINED";
            return isNull;
        }

        private void ClearWorkflowParser()
        {
            WorkflowInstance = null;
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
        /// <param name="value">The query Example: {{ context.executions.other_task.Result.'Fred' }}</param>
        /// <returns>
        /// Tuple:
        /// Result of the resolution
        /// Context of type of resolution required to resolve query
        /// </returns>
        private (string? Result, ParameterContext Context) ResolveMatch(string value)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(value, nameof(value));

            value = value.Substring(2, value.Length - 4).Trim();

            var context = ParameterContext.Undefined;
            (string? Result, ParameterContext Context) result = (Result: null, Context: context);
            if (value.StartsWith(ExecutionsTask))
            {
                result = ResolveExecutionTasks(value);
            }
            if (value.StartsWith(ContextDicomSeries))
            {
                result = ResolveDicom(value);
            }
            if (value.StartsWith(PatientDetails))
            {
                result = ResolvePatientDetails(value);
            }
            if (value.StartsWith(ContextWorkflow))
            {
                result = ResolveContextWorkflow(value);
            }

            _logger.ResolveValue(value, result.Result);

            return result;
        }

        private (string? Result, ParameterContext Context) ResolveDicom(string value)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(value, nameof(value));
            ArgumentNullException.ThrowIfNull(WorkflowInstance, nameof(WorkflowInstance));

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
            var subValues = subValue.Split('.');
            var id = subValues[1].Trim('\'');

            var task = WorkflowInstance?.Tasks.FirstOrDefault(t => t.TaskId == id);

            if (task is null)
            {
                return (Result: null, Context: ParameterContext.TaskExecutions);
            }

            var subValueKey = subValues[2];
            string? keyValue = null;

            if (subValues.Length > 3)
            {
                keyValue = subValues[3];
            }

            _logger.ResolveExecutionTask(subValueKey);
            var resultStr = null as string;
            switch (subValueKey.ToLower())
            {
                case ParameterConstants.TaskId:
                    resultStr = task.TaskId;
                    break;

                case ParameterConstants.Status:
                    resultStr = task.Status.ToString();
                    break;

                case ParameterConstants.ExecutionId:
                    resultStr = task.ExecutionId;
                    break;

                case ParameterConstants.OutputDirectory:
                    resultStr = task.OutputDirectory;
                    break;

                case ParameterConstants.TaskType:
                    resultStr = task.TaskType;
                    break;

                case ParameterConstants.PreviousTaskId:
                    resultStr = task.PreviousTaskId;
                    break;

                case ParameterConstants.ErrorMessage:
                    resultStr = task.Reason.ToString();
                    break;

                case ParameterConstants.Result:
                    resultStr = GetValueFromDictionary(task.ResultMetadata, keyValue);
                    break;

                case ParameterConstants.ExecutionStats:
                    resultStr = GetValueFromDictionary(task.ExecutionStats, keyValue);
                    break;

                case ParameterConstants.StartTime:
                    resultStr = task.TaskStartTime.ToString("s", CultureInfo.InvariantCulture);
                    break;

                case ParameterConstants.EndTime:
                    resultStr = task.TaskEndTime?.ToString("s", CultureInfo.InvariantCulture);
                    break;

                default:
                    break;
            }

            return (Result: resultStr, Context: ParameterContext.TaskExecutions);
        }

        private static string? GetValueFromDictionary(Dictionary<string, object> dictionary, string? key)
        {
            if (key is null)
            {
                return null;
            }

            if (dictionary.TryGetValue(key, out var value))
            {
                if (value is string valueStr)
                {
                    return valueStr;
                }

                if (value is DateTime valueDate)
                {
                    return valueDate.ToString("s", CultureInfo.InvariantCulture);
                }

                return value.ToString();
            }

            return null;
        }

        private static string? GetValueFromDictionary(Dictionary<string, string> dictionary, string? key)
        {
            if (key is not null && dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }

        private (string? Result, ParameterContext Context) ResolveContextWorkflow(string value)
        {
            var subValue = value.Trim().Substring(ContextWorkflow.Length, value.Length - ContextWorkflow.Length);
            var keyValue = subValue.Replace(".", "");
            var workflowId = WorkflowInstance?.WorkflowId;

            if (workflowId is null || keyValue is null)
            {
                return (Result: null, Context: ParameterContext.Workflow);
            }

            var task = Task.Run(async () => await _workflowService.GetAsync(workflowId));
            task.Wait();
            var workflowSpecValue = task.Result?.Workflow;

            if (workflowSpecValue is not null)
            {
                _logger.ResolveWorkflow(keyValue);
                var resultStr = null as string;
                switch (keyValue)
                {
                    case ParameterConstants.Name:
                        resultStr = workflowSpecValue.Name;
                        break;

                    case ParameterConstants.Description:
                        resultStr = workflowSpecValue.Description;
                        break;

                    default:
                        break;
                }

                return (Result: resultStr, Context: ParameterContext.Workflow);
            }

            return (Result: null, Context: ParameterContext.Workflow);
        }

        private (string? Result, ParameterContext Context) ResolvePatientDetails(string value)
        {
            var subValue = value.Trim().Substring(PatientDetails.Length, value.Length - PatientDetails.Length);
            var keyValue = subValue.Replace(".", "");
            var payloadId = WorkflowInstance?.PayloadId;

            if (payloadId is null || keyValue is null)
            {
                return (Result: null, Context: ParameterContext.PatientDetails);
            }

            var task = Task.Run(async () => await _payloadService.GetByIdAsync(payloadId));
            task.Wait();
            var patientValue = task.Result?.PatientDetails;

            if (patientValue is not null)
            {
                var resultStr = null as string;
                switch (keyValue)
                {
                    case ParameterConstants.PatientId:
                        resultStr = patientValue.PatientId;
                        break;

                    case ParameterConstants.PatientName:
                        resultStr = patientValue.PatientName;
                        break;

                    case ParameterConstants.PatientSex:
                        resultStr = patientValue.PatientSex;
                        break;

                    case ParameterConstants.PatientDob:
                        resultStr = patientValue.PatientDob?.ToString("dd/MM/yyyy");
                        break;

                    case ParameterConstants.PatientAge:
                        resultStr = patientValue.PatientAge;
                        break;

                    case ParameterConstants.PatientHospitalId:
                        resultStr = patientValue.PatientHospitalId;
                        break;

                    default:
                        break;
                }

                return (Result: resultStr, Context: ParameterContext.PatientDetails);
            }

            return (Result: null, Context: ParameterContext.PatientDetails);
        }
    }
}

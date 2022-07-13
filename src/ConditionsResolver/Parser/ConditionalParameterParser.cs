// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Storage.Services;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Parser
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
        private const string ExecutionsTask = "context.executions.task";
        private const string ContextDicomSeries = "context.dicom.series";
        private const string PatientDetails = "context.input.patient_details";
        private const string ContextWorkflow = "context.workflow";

        private readonly IWorkflowInstanceService _workflowInstanceService;
        private readonly ILogger<ConditionalParameterParser> _logger;
        private readonly IDicomService _dicom;
        private readonly IPayloadService _payloadService;
        private readonly IWorkflowService _workflowService;

        private readonly Regex _squigglyBracketsRegex = new Regex(@"\{{(.*?)\}}");

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
                    conditions = conditions
                        .Remove(parameter.Key.Index, parameter.Key.Length)
                        .Insert(parameter.Key.Index, $"'{parameter.Value.Result ?? "null"}'");
                }

                ClearWorkflowParser();
                return conditions;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                ClearWorkflowParser();
                throw e;
            }
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
            if (value.StartsWith(PatientDetails))
            {
                return ResolvePatientDetails(value);
            }
            if (value.StartsWith(ContextWorkflow))
            {
                return ResolveContextWorkflow(value);
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

            if (task is null || task is not null && !task.Metadata.Any())
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
                var resultStr = null as string;
                switch (keyValue)
                {
                    case "name":
                        resultStr = workflowSpecValue.Name;
                        break;
                    case "description":
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
                    case "id":
                        resultStr = patientValue.PatientId;
                        break;
                    case "name":
                        resultStr = patientValue.PatientName;
                        break;
                    case "sex":
                        resultStr = patientValue.PatientSex;
                        break;
                    case "dob":
                        resultStr = patientValue.PatientDob?.ToString("dd/MM/yyyy");
                        break;
                    case "age":
                        resultStr = patientValue.PatientAge;
                        break;
                    case "hospital_id":
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

// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Common;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Common
{
    public enum ParameterContext
    {
        Undefined,
        TaskExecutions,
        Executions,
        Dicom
    }

    public class ConditionalParameterParser : IConditionalParameterParser
    {
        private const string ExecutionsTask = "context.executions.task";
        private const string ContextExecutions = "context.executions";
        private const string ContextDicomTags = "context.dicom.tags";

        private readonly ILogger<ConditionalParameterParser> _logger;

        private readonly Regex _regex = new Regex(@"\{{(.*?)\}}");

        public WorkflowInstance? WorkflowInstance { get; private set; } = null;

        public ConditionalParameterParser(ILogger<ConditionalParameterParser> logger)
        {
            _logger = logger;
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

        public string ResolveParameters(string conditions, WorkflowInstance workflowInstance)
        {
            Guard.Against.NullOrEmpty(conditions);
            Guard.Against.Null(workflowInstance);

            WorkflowInstance = workflowInstance;

            try
            {
                var matches = _regex.Matches(conditions);
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

        private Dictionary<Match, (string? Result, ParameterContext Context)> ParseMatches(MatchCollection matches)
        {
            var valuePairs = new Dictionary<Match, (string? Value, ParameterContext Context)>();
            foreach (Match match in matches)
            {
                valuePairs.Add(match, ResolveMatch(match.Value));
            }
            return valuePairs;
        }

        private (string? Result, ParameterContext Context) ResolveMatch(string originalValue)
        {
            Guard.Against.NullOrWhiteSpace(originalValue);

            originalValue = originalValue.Substring(2, originalValue.Length - 4).Trim();
            //"{{ context.executions.task['2dbd1af7-b699-4467-8e99-05a0c22422b4'].'Fred' }}"
            var value = originalValue;
            var context = ParameterContext.Undefined;
            if (originalValue.StartsWith(ExecutionsTask))
            {
                context = ParameterContext.TaskExecutions;
                var subValue = value.Trim().Substring(ExecutionsTask.Length, value.Length - ExecutionsTask.Length);
                // "['2dbd1af7-b699-4467-8e99-05a0c22422b4'].'Fred'"
                var subValues = subValue.Split('[', ']');
                var id = subValues[1].Trim('\'');
                var task = WorkflowInstance?.Tasks.First(t => t.TaskId == id);
                if (task is null || (task is not null && !task.Metadata.Any()))
                {
                    return (Result: null, Context: context);
                }
                var metadataKey = subValues[2].Split('\'')[1];
                if (task.Metadata.Keys.Contains(metadataKey))
                {
                    var result = task.Metadata[metadataKey];
                    if (result is not null && result is string resultStr)
                    {
                        return (Result: resultStr, Context: context);
                    }
                }
            }
            else if (originalValue.StartsWith(ContextExecutions))
            {
                context = ParameterContext.Executions;
                //TODO: handle context executions parameter resoultion
            }
            if (originalValue.StartsWith(ContextDicomTags))
            {
                context = ParameterContext.Dicom;
                //TODO: handle dicom context parameter resoultion
            }
            //TODO: part2
            return (Result: null, Context: context);
        }
    }
}

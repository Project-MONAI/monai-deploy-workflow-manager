// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Services;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Common
{
    public enum ParameterContext
    {
        Undefined,
        Executions,
        Dicom
    }

    public class ConditionalParameterParser
    {
        private readonly ILogger<WorkflowExecuterService> _logger;

        private readonly Regex _regex = new Regex(@"\{{(.*?)\}}");

        public ConditionalParameterParser(ILogger<WorkflowExecuterService> logger)
        {
            _logger = logger;
        }

        public bool TryParse(string conditions, WorkflowInstance? workflowInstance = null)
        {
            Guard.Against.NullOrEmpty(conditions);

            try
            {
                conditions = ResolveParameters(conditions, null);
                var conditionalGroup = ConditionalGroup.Create(conditions);
                return conditionalGroup.Evaluate();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failure attemping to parse condition", conditions, ex.Message);
                return false;
            }
        }

        public string ResolveParameters(string conditions, WorkflowInstance? workflowInstance)
        {
             Guard.Against.NullOrEmpty(conditions);
            //Guard.Against.Null(workflowInstance); // TODO Uncomment

            var matches = _regex.Matches(conditions);
            if (!matches.Any())
            {
                return conditions;
            }

            var parameters = ParseMatches(matches).Reverse();
            var index = parameters.Count() + 1;
            foreach (var parameter in parameters)
            {
                index--;
                conditions = conditions
                    .Remove(parameter.Key.Index, parameter.Key.Length)
                    .Insert(parameter.Key.Index, $"'{parameter.Value.Result} {index}'");
            }

            return conditions;
        }

        private Dictionary<Match, (string Result, ParameterContext Context)> ParseMatches(MatchCollection matches)
        {
            var valuePairs = new Dictionary<Match, (string Value, ParameterContext Context)>();
            var index = 0;
            foreach (Match match in matches)
            {
                index++;
                valuePairs.Add(match, ResolveMatch(match.Value));
            }
            return valuePairs;
        }

        private (string Result, ParameterContext Context) ResolveMatch(string value)
        {
            var context = ParameterContext.Undefined;
            var result = "";
            if (value.StartsWith("{{context.executions"))
            {
                context = ParameterContext.Executions;
                //TODO: handle excution context parameter resoultion
                result = "match";
            }
            if (value.StartsWith("{{context.dicom.tags"))
            {
                context = ParameterContext.Dicom;
                //TODO: handle dicom context parameter resoultion
            }
            //TODO: part2
            return (Result: result, Context: context);
        }
    }
}

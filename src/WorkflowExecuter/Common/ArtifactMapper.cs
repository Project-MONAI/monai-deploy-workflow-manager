﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Common
{
    public class ArtifactMapper : IArtifactMapper
    {
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;

        public ArtifactMapper(IWorkflowInstanceRepository workflowInstanceRepository)
        {
            _workflowInstanceRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
        }

        public async Task<Dictionary<string, string>> ConvertArtifactVariablesToPath(Artifact[] artifacts, string payloadId, string workflowInstanceId)
        {
            Guard.Against.Null(artifacts);
            Guard.Against.NullOrWhiteSpace(payloadId);
            Guard.Against.NullOrWhiteSpace(workflowInstanceId);

            var artifactPathDictionary = new Dictionary<string, string>();

            foreach (var artifact in artifacts)
            {
                Guard.Against.NullOrWhiteSpace(artifact.Value);
                Guard.Against.NullOrWhiteSpace(artifact.Name);

                if (!TrimArtifactVariable(artifact.Value, out var variableString))
                {
                    continue;
                }

                var mappedArtifact = await ConvertVariableStringToPath(artifact, variableString, workflowInstanceId, payloadId);

                if (mappedArtifact.Equals(default(KeyValuePair<string, string>)))
                {
                    continue;
                }

                artifactPathDictionary.Add(mappedArtifact.Key, mappedArtifact.Value);
            }

            return artifactPathDictionary;
        }

        private static bool TrimArtifactVariable(string valueString, out string variableString)
        {
            var variableStrings = valueString.Split(" ");

            if (variableStrings.Length < 2)
            {
                variableString = null;

                return false;
            }

            variableString = variableStrings[1];

            return true;
        }

        private async Task<KeyValuePair<string, string>> ConvertVariableStringToPath(Artifact artifact, string variableString, string workflowInstanceId, string payloadId)
        {
            if (variableString.StartsWith("context.input"))
            {
                return new KeyValuePair<string, string>(artifact.Name, $"{payloadId}/dcm");
            }

            if (variableString.StartsWith("context.executions"))
            {
                var variableWords = variableString.Split(".");

                var variableTaskId = variableWords[2];
                var variableLocation = variableWords[3];

                var task = await _workflowInstanceRepository.GetTaskByIdAsync(workflowInstanceId, variableTaskId);

                if (variableLocation == "output_dir")
                {
                    return new KeyValuePair<string, string>(artifact.Name, task.OutputDirectory);
                }

                if (variableLocation == "artifacts")
                {
                    var artifactName = variableWords[4];
                    var outputArtifact = task.OutputArtifacts.FirstOrDefault(a => a.Key == artifactName);

                    if (!outputArtifact.Equals(default(KeyValuePair<string, string>)))
                    {
                        return new KeyValuePair<string, string>(outputArtifact.Key, outputArtifact.Value);
                    }
                }
            }

            return default;
        }
    }
}

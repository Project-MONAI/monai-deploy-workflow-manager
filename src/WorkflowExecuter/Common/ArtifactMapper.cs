// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
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

                var variableString = artifact.Value.Split(" ")?[1];

                if (variableString.StartsWith("context.input"))
                {
                    artifactPathDictionary.Add(artifact.Name, $"{payloadId}/dcm");
                    continue;
                }

                if (variableString.StartsWith("context.executions"))
                {
                    var variableWords = variableString.Split(".");

                    var variableTaskId = variableWords[2];
                    var variableLocation = variableWords[3];

                    var task = await _workflowInstanceRepository.GetTaskByIdAsync(workflowInstanceId, variableTaskId);

                    if (variableLocation == "output_dir")
                    {
                        artifactPathDictionary.Add(artifact.Name, task.OutputDirectory);
                        continue;
                    }

                    if (variableLocation == "artifacts")
                    {
                        var artifactName = variableWords[4];
                        var outputArtifact = task.OutputArtifacts.FirstOrDefault(a => a.Key == artifactName);

                        if (!outputArtifact.Equals(default(KeyValuePair<string, string>)))
                        {
                            artifactPathDictionary.Add(outputArtifact.Key, outputArtifact.Value);
                        }

                        continue;
                    }

                }

            }

            return artifactPathDictionary;
        }
    }
}

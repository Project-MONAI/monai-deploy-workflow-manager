/*
 * Copyright 2021-2022 MONAI Consortium
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

using Ardalis.GuardClauses;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;

namespace Monai.Deploy.WorkflowManager.WorkfowExecuter.Common
{
    public class ArtifactMapper : IArtifactMapper
    {
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        private readonly IStorageService _storageService;

        public ArtifactMapper(
            IWorkflowInstanceRepository workflowInstanceRepository,
            IStorageService storageService
            )
        {
            _workflowInstanceRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        public async Task<Dictionary<string, string>> ConvertArtifactVariablesToPath(Artifact[] artifacts, string payloadId, string workflowInstanceId, string bucketId, bool shouldExistYet = true)
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
                    if (artifact.Mandatory is false)
                    {
                        continue;
                    }

                    throw new FileNotFoundException($"Mandatory artifact failed to be parsed: {artifact.Name}, {artifact.Value}");
                }

                var mappedArtifact = await ConvertVariableStringToPath(artifact, variableString, workflowInstanceId, payloadId, bucketId, shouldExistYet);

                if (mappedArtifact.Equals(default(KeyValuePair<string, string>)) is false)
                {
                    artifactPathDictionary.Add(mappedArtifact.Key, mappedArtifact.Value);

                    continue;
                }

                if (artifact.Mandatory)
                {
                    throw new FileNotFoundException($"Mandatory artifact was not found: {artifact.Name}, {artifact.Value}");
                }
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

        private async Task<KeyValuePair<string, string>> ConvertVariableStringToPath(Artifact artifact, string variableString, string workflowInstanceId, string payloadId, string bucketId, bool shouldExistYet)
        {
            if (variableString.StartsWith("context.input.dicom", StringComparison.InvariantCultureIgnoreCase))
            {
                return await VerifyExists(new KeyValuePair<string, string>(artifact.Name, $"{payloadId}/dcm/"), bucketId, shouldExistYet);
            }

            if (variableString.StartsWith("context.executions", StringComparison.InvariantCultureIgnoreCase))
            {
                var variableWords = variableString.Split(".");

                var variableTaskId = variableWords[2];
                var variableLocation = variableWords[3];

                var task = await _workflowInstanceRepository.GetTaskByIdAsync(workflowInstanceId, variableTaskId);

                if (task is null)
                {
                    return default;
                }

                if (string.Equals(variableLocation, "output_dir", StringComparison.InvariantCultureIgnoreCase))
                {
                    return await VerifyExists(new KeyValuePair<string, string>(artifact.Name, task.OutputDirectory), bucketId, shouldExistYet);
                }

                if (string.Equals(variableLocation, "artifacts", StringComparison.InvariantCultureIgnoreCase))
                {
                    var artifactName = variableWords[4];
                    var outputArtifact = task.OutputArtifacts?.FirstOrDefault(a => a.Key == artifactName);

                    if (outputArtifact is null)
                    {
                        return default;
                    }

                    return await VerifyExists((KeyValuePair<string, string>)outputArtifact, bucketId, shouldExistYet);
                }
            }

            return default;
        }

        private async Task<KeyValuePair<string, string>> VerifyExists(KeyValuePair<string, string> artifact, string bucketId, bool shouldExistYet)
        {
            if (artifact.Equals(default(KeyValuePair<string, string>)))
            {
                return default;
            }

            if (shouldExistYet)
            {
                artifact = await _storageService.VerifyObjectExistsAsync(bucketId, artifact);
            }

            return artifact;
        }
    }
}

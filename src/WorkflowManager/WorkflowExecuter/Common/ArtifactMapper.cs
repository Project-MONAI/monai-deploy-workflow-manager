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

using Microsoft.Extensions.Logging;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Logging;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Common
{
    public class ArtifactMapper : IArtifactMapper
    {
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        private readonly IStorageService _storageService;
        private readonly ILogger<ArtifactMapper> _logger;

        public ArtifactMapper(
            IWorkflowInstanceRepository workflowInstanceRepository,
            IStorageService storageService,
            ILogger<ArtifactMapper> logger)
        {
            _workflowInstanceRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool TryConvertArtifactVariablesToPath(Artifact[] artifacts, string payloadId, string workflowInstanceId, string bucketId, bool shouldExistYet, out Dictionary<string, string> artifactPaths)
        {
            using var loggingScope = _logger.BeginScope(new LoggingDataDictionary<string, object>
            {
                ["payloadId"] = payloadId,
                ["workflowInstanceId"] = workflowInstanceId,
                ["bucketId"] = bucketId,
            });

            try
            {
                var task = ConvertArtifactVariablesToPath(artifacts, payloadId, workflowInstanceId, bucketId, shouldExistYet);
                task.Wait();
                artifactPaths = task.Result;
                return true;
            }
            catch (FileNotFoundException ex)
            {
                _logger.ConvertArtifactVariablesToPathError(ex);
                artifactPaths = new Dictionary<string, string>();
                return false;
            }
            catch (AggregateException ex)
            {
                _logger.ConvertArtifactVariablesToPathError(ex);
                if (ex.InnerException is FileNotFoundException)
                {
                    artifactPaths = new Dictionary<string, string>();
                    return false;
                }
                throw;
            }
        }

        public async Task<Dictionary<string, string>> ConvertArtifactVariablesToPath(Artifact[] artifacts, string payloadId, string workflowInstanceId, string bucketId, bool shouldExistYet = true)
        {
            ArgumentNullException.ThrowIfNull(artifacts, nameof(artifacts));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(payloadId, nameof(payloadId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            var artifactPathDictionary = new Dictionary<string, string>();

            foreach (var artifact in artifacts)
            {
                ArgumentNullException.ThrowIfNullOrWhiteSpace(artifact.Value, nameof(artifact.Value));
                ArgumentNullException.ThrowIfNullOrWhiteSpace(artifact.Name, nameof(artifact.Name));

                if (!TrimArtifactVariable(artifact.Value, out var variableString))
                {
                    if (artifact.Mandatory is false)
                    {
                        continue;
                    }

                    throw new FileNotFoundException($"Mandatory artifact failed to be parsed: {artifact.Name}, {artifact.Value}");
                }

                var suffix = GetArtifactSuffix(artifact.Value);

                var mappedArtifact = await ConvertVariableStringToPath(artifact, variableString ?? string.Empty, workflowInstanceId, payloadId, bucketId, shouldExistYet, suffix);

                if (mappedArtifact.Equals(default(KeyValuePair<string, string>)) is false)
                {
                    artifactPathDictionary.Add(mappedArtifact.Key, mappedArtifact.Value);

                    _logger.LogArtifactPassing(artifact, mappedArtifact.Value, shouldExistYet ? "Input" : "Pre-Task Output Path Mapping", true);

                    continue;
                }

                _logger.LogArtifactPassing(artifact, mappedArtifact.Value, shouldExistYet ? "Input" : "Pre-Task Output Path Mapping", false);

                if (artifact.Mandatory)
                {
                    throw new FileNotFoundException($"Mandatory artifact was not found: {artifact.Name}, {artifact.Value}");
                }
            }

            return artifactPathDictionary;
        }

        private static string? GetArtifactSuffix(string valueString)
        {
            var variableStrings = valueString.Split("}");

            if (variableStrings.Length < 2)
            {
                return null;
            }

            return variableStrings[1];
        }

        private static bool TrimArtifactVariable(string valueString, out string? variableString)
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

        private async Task<KeyValuePair<string, string>> ConvertVariableStringToPath(Artifact artifact, string variableString, string workflowInstanceId, string payloadId, string bucketId, bool shouldExistYet, string? suffix = "")
        {
            _logger.ConvertingVariableStringToPath(variableString);
            if (variableString.StartsWith("context.input.dicom", StringComparison.InvariantCultureIgnoreCase))
            {
                return await VerifyExists(new KeyValuePair<string, string>(artifact.Name, $"{payloadId}/dcm{suffix}"), bucketId, shouldExistYet);
            }

            if (variableString.StartsWith("context.executions", StringComparison.InvariantCultureIgnoreCase))
            {
                var variableWords = variableString.Replace("{", "").Replace("}", "").Split(".");

                var variableTaskId = variableWords[2];
                var variableLocation = variableWords[3];

                var task = await _workflowInstanceRepository.GetTaskByIdAsync(workflowInstanceId, variableTaskId);

                if (task is null)
                {
                    return default;
                }

                if (string.Equals(variableLocation, "output_dir", StringComparison.InvariantCultureIgnoreCase))
                {
                    return await VerifyExists(new KeyValuePair<string, string>(artifact.Name, $"{task.OutputDirectory}{suffix}"), bucketId, shouldExistYet);
                }

                if (string.Equals(variableLocation, "artifacts", StringComparison.InvariantCultureIgnoreCase))
                {
                    var artifactName = variableWords[4];
                    var outputArtifact = task.OutputArtifacts?.FirstOrDefault(a => a.Key == artifactName);

                    if (!outputArtifact.HasValue || string.IsNullOrEmpty(outputArtifact.Value.Value))
                    {
                        return default;
                    }

                    return await VerifyExists(outputArtifact.Value, bucketId, shouldExistYet);
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
                _logger.VerifyArtifactExistence(bucketId, artifact.Key, artifact.Value);
                artifact = await _storageService.VerifyObjectExistsAsync(bucketId, artifact.Value) ? artifact : default;
            }

            return artifact;
        }
    }
}

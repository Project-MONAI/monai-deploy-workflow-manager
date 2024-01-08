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

using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Extensions
{
    public static class ValidationExtensions
    {
        public static bool IsValid(this WorkflowRequestEvent workflowRequestMessage, out IList<string> validationErrors)
        {
            ArgumentNullException.ThrowIfNull(workflowRequestMessage, nameof(workflowRequestMessage));

            validationErrors = new List<string>();

            var valid = true;

            valid &= IsAeTitleValid(workflowRequestMessage.GetType().Name, workflowRequestMessage.DataTrigger.Source, validationErrors);
            valid &= IsAeTitleValid(workflowRequestMessage.GetType().Name, workflowRequestMessage.DataTrigger.Destination, validationErrors);
            valid &= IsBucketValid(workflowRequestMessage.GetType().Name, workflowRequestMessage.Bucket, validationErrors);
            valid &= IsCorrelationIdValid(workflowRequestMessage.GetType().Name, workflowRequestMessage.CorrelationId, validationErrors);
            valid &= IsPayloadIdValid(workflowRequestMessage.GetType().Name, workflowRequestMessage.PayloadId.ToString(), validationErrors);

            return valid;
        }

        public static bool IsValid(this ArtifactsReceivedEvent artifactReceivedMessage, out IList<string> validationErrors)
        {
            ArgumentNullException.ThrowIfNull(artifactReceivedMessage, nameof(artifactReceivedMessage));

            validationErrors = new List<string>();

            var valid = true;

            valid &= IsAeTitleValid(artifactReceivedMessage.GetType().Name, artifactReceivedMessage.DataTrigger.Source, validationErrors);
            valid &= IsAeTitleValid(artifactReceivedMessage.GetType().Name, artifactReceivedMessage.DataTrigger.Destination, validationErrors);
            valid &= IsBucketValid(artifactReceivedMessage.GetType().Name, artifactReceivedMessage.Bucket, validationErrors);
            valid &= IsCorrelationIdValid(artifactReceivedMessage.GetType().Name, artifactReceivedMessage.CorrelationId, validationErrors);
            valid &= IsPayloadIdValid(artifactReceivedMessage.GetType().Name, artifactReceivedMessage.PayloadId.ToString(), validationErrors);
            valid &= string.IsNullOrEmpty(artifactReceivedMessage.WorkflowInstanceId) is false && string.IsNullOrEmpty(artifactReceivedMessage.TaskId) is false;
            valid &= AllArtifactsAreValid(artifactReceivedMessage, validationErrors);
            return valid;
        }

        private static bool AllArtifactsAreValid(this ArtifactsReceivedEvent artifactReceivedMessage, IList<string> validationErrors)
        {
            ArgumentNullException.ThrowIfNull(artifactReceivedMessage, nameof(artifactReceivedMessage));

            var valid = artifactReceivedMessage.Artifacts.All(a => a.Type != ArtifactType.Unset);

            if (valid is false)
            {
                var unsetArtifacts = string.Join(',', artifactReceivedMessage.Artifacts.Where(a => a.Type == ArtifactType.Unset).Select(a => a.Path));
                validationErrors.Add($"The following artifacts are have unset artifact types: {unsetArtifacts}");
            }

            return valid;
        }

        public static bool IsInformaticsGatewayNotNull(string source, InformaticsGateway informaticsGateway, IList<string> validationErrors)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(source, nameof(source));

            if (informaticsGateway is not null) return true;

            validationErrors?.Add($"'{nameof(informaticsGateway)}' cannot be null (source: {source}).");
            return false;
        }

        public static bool IsAeTitleValid(string source, string aeTitle, IList<string> validationErrors)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(aeTitle) && aeTitle.Length <= 15) return true;

            validationErrors?.Add($"AeTitle is required in the InformaticsGateaway section.");
            return false;
        }

        public static bool IsBucketValid(string source, string bucket, IList<string> validationErrors = null)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(bucket) && bucket.Length >= 3 && bucket.Length <= 63) return true;

            validationErrors?.Add($"'{bucket}' is not a valid Bucket Name: must be 3-63 characters (source: {source}).");

            return false;
        }

        public static bool IsCorrelationIdValid(string source, string correlationId, IList<string> validationErrors = null)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(correlationId) && Guid.TryParse(correlationId, out var _)) return true;

            validationErrors?.Add($"'{correlationId}' is not a valid {nameof(correlationId)}: must be a valid guid (source: {correlationId}).");

            return false;
        }

        public static bool IsPayloadIdValid(string source, string payloadId, IList<string> validationErrors = null)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(source, nameof(source));

            var parsed = Guid.TryParse(payloadId, out var parsedGuid);

            if (!string.IsNullOrWhiteSpace(payloadId) && parsed && parsedGuid != Guid.Empty) return true;

            validationErrors?.Add($"'{payloadId}' is not a valid {nameof(payloadId)}: must be a valid guid (source: {payloadId}).");

            return false;
        }

        private static bool ValueIsNullOrWhiteSpace(this string value, string source, IList<string> validationErrors)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                validationErrors.Add($"'{value}' is not a valid value (source: {source}).");
                return true;
            }

            return false;
        }
    }
}

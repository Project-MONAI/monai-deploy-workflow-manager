// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Extensions
{
    public static class ValidationExtensions
    {
        public static bool IsValid(this WorkflowRequestEvent workflowRequestMessage, out IList<string> validationErrors)
        {
            Guard.Against.Null(workflowRequestMessage, nameof(workflowRequestMessage));

            validationErrors = new List<string>();

            var valid = true;

            valid &= IsAeTitleValid(workflowRequestMessage.GetType().Name, workflowRequestMessage.CallingAeTitle, validationErrors);
            valid &= IsAeTitleValid(workflowRequestMessage.GetType().Name, workflowRequestMessage.CalledAeTitle, validationErrors);
            valid &= IsBucketValid(workflowRequestMessage.GetType().Name, workflowRequestMessage.Bucket, validationErrors);
            valid &= IsCorrelationIdValid(workflowRequestMessage.GetType().Name, workflowRequestMessage.CorrelationId, validationErrors);
            valid &= IsPayloadIdValid(workflowRequestMessage.GetType().Name, workflowRequestMessage.PayloadId.ToString(), validationErrors);

            return valid;
        }

        public static bool IsAeTitleValid(string source, string aeTitle, IList<string> validationErrors)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(aeTitle) && aeTitle.Length <= 15) return true;

            validationErrors?.Add($"'{aeTitle}' is not a valid AE Title (source: {source}).");
            return false;
        }

        public static bool IsBucketValid(string source, string bucket, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(bucket) && bucket.Length >= 3 && bucket.Length <= 63) return true;

            validationErrors?.Add($"'{bucket}' is not a valid Bucket Name: must be 3-63 characters (source: {source}).");

            return false;
        }

        public static bool IsCorrelationIdValid(string source, string correlationId, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(correlationId) && Guid.TryParse(correlationId, out var _)) return true;

            validationErrors?.Add($"'{correlationId}' is not a valid {nameof(correlationId)}: must be a valid guid (source: {correlationId}).");

            return false;
        }

        public static bool IsPayloadIdValid(string source, string payloadId, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(payloadId) && Guid.TryParse(payloadId, out var _)) return true;

            validationErrors?.Add($"'{payloadId}' is not a valid {nameof(payloadId)}: must be a valid guid (source: {payloadId}).");

            return false;
        }

        public static bool IsValid(this TaskUpdateEvent taskUpdateMessage, out IList<string> validationErrors)
        {
            Guard.Against.Null(taskUpdateMessage, nameof(taskUpdateMessage));

            validationErrors = new List<string>();

            var valid = true;

            valid &= !taskUpdateMessage.WorkflowId.ValueIsNullOrWhiteSpace(taskUpdateMessage.GetType().Name, validationErrors);
            valid &= !taskUpdateMessage.TaskId.ValueIsNullOrWhiteSpace(taskUpdateMessage.GetType().Name, validationErrors);

            return valid;
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

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

            return valid;
        }

        public static bool IsAeTitleValid(string source, string aeTitle, IList<string> validationErrors)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(aeTitle) && aeTitle.Length <= 15) return true;

            validationErrors?.Add($"'{aeTitle}' is not a valid AE Title (source: {source}).");
            return false;
        }
    }
}

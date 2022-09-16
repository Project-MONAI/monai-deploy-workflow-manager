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
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Extensions
{
    public static class WorkflowExtensions
    {
        private const int WorkflowNameLimit = 15;
        private const int WorkflowDescriptionLimit = 200;
        private const int TaskIdLengthLimit = 50;
        private const int TaskDescriptionLengthLimit = 2000;
        private const int TaskTypeLengthLimit = 2000;
        #region Workflow

        public static bool IsValid(this Workflow workflow, out IList<string> validationErrors)
        {
            Guard.Against.Null(workflow, nameof(workflow));

            validationErrors = new List<string>();

            var valid = true;

            var workflowName = string.IsNullOrWhiteSpace(workflow.Name) ? "Unnamed workflow" : workflow.Name;

            valid &= IsNameValid(workflowName, workflow.Name, validationErrors);
            valid &= IsDescriptionValid(workflowName, workflow.Description, validationErrors);
            valid &= IsInformaticsGatewayValid(workflowName, workflow.InformaticsGateway, validationErrors);

            foreach (var task in workflow?.Tasks)
            {
                valid &= IsTaskObjectValid(workflowName, task, validationErrors);
            }

            return valid;
        }

        public static bool IsNameValid(string source, string name, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (name is not null && name.Length <= WorkflowNameLimit) return true;

            validationErrors?.Add($"'{name}' is not a valid Workflow Name (source: {source}).");

            return false;
        }

        public static bool IsDescriptionValid(string source, string description, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(description) && description.Length <= WorkflowDescriptionLimit) return true;

            validationErrors?.Add($"'{description}' is not a valid Workflow Description (source: {source}).");

            return false;
        }

        #endregion

        #region InformaticsGateway

        public static bool IsInformaticsGatewayValid(string source, InformaticsGateway informaticsGateway, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));
            Guard.Against.Null(informaticsGateway, nameof(informaticsGateway));

            var valid = true;

            valid &= ValidationExtensions.IsAeTitleValid(informaticsGateway.GetType().Name, informaticsGateway.AeTitle, validationErrors);
            valid &= IsExportDestinationsValid(informaticsGateway.GetType().Name, informaticsGateway.ExportDestinations, validationErrors);

            return valid;
        }

        public static bool IsExportDestinationsValid(string source, string[] exportDestinations, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (exportDestinations?.Length > 0) return true;

            validationErrors?.Add($"'{exportDestinations}' is not a valid Informatics Gateway - {nameof(exportDestinations)} (source: {source}).");

            return false;
        }

        #endregion

        #region TaskObject

        public static bool IsTaskObjectValid(string source, TaskObject taskObject, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            var valid = true;

            valid &= IsTaskIdValid(taskObject.Id, taskObject.Id, validationErrors);
            valid &= IsTaskDescriptionValid(taskObject.Id, taskObject.Description, validationErrors);
            valid &= IsTaskTypeValid(taskObject.Id, taskObject.Type, validationErrors);

            return valid;
        }

        public static bool IsTaskIdValid(string source, string taskId, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(taskId) && taskId.Length <= TaskIdLengthLimit) return true;

            validationErrors?.Add($"'{taskId}' is not a valid {nameof(taskId)} (source: {source}).");

            return false;
        }

        public static bool IsTaskDescriptionValid(string source, string taskDescription, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(taskDescription) && taskDescription.Length <= TaskDescriptionLengthLimit) return true;

            validationErrors?.Add($"'{taskDescription}' is not a valid {nameof(taskDescription)} (source: {source}).");

            return false;
        }

        public static bool IsTaskTypeValid(string source, string taskType, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(taskType) && taskType.Length <= TaskTypeLengthLimit) return true;

            validationErrors?.Add($"'{taskType}' is not a valid {nameof(taskType)} (source: {source}).");

            return false;
        }

        #endregion
    }
}

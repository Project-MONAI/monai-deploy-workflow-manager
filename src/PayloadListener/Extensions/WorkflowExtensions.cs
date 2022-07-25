// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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

            valid &= IsNameValid(workflow.GetType().Name, workflow.Name, validationErrors);
            valid &= IsDescriptionValid(workflow.Name, workflow.Description, validationErrors);
            valid &= IsInformaticsGatewayValid(workflow.Name, workflow.InformaticsGateway, validationErrors);

            Guard.Against.Null(workflow.Tasks, nameof(workflow.Tasks));
            valid &= workflow.Tasks.Length > 0;

            foreach (var task in workflow.Tasks)
            {
                valid &= IsTaskObjectValid(workflow.Name, task, validationErrors);
            }

            return valid;
        }

        public static bool IsNameValid(string source, string name, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(name) && name.Length <= WorkflowNameLimit) return true;

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
            valid &= IsDataOriginsValid(informaticsGateway.GetType().Name, informaticsGateway.DataOrigins, validationErrors);
            valid &= IsExportDestinationsValid(informaticsGateway.GetType().Name, informaticsGateway.ExportDestinations, validationErrors);

            return valid;
        }

        public static bool IsDataOriginsValid(string source, string[] dataOrigins, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (dataOrigins?.Length > 0) return true;

            validationErrors?.Add($"'{dataOrigins}' is not a valid Informatics Gateway - {nameof(dataOrigins)} (source: {source}).");

            return false;
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
            valid &= IsArgsValid(taskObject.Id, taskObject.Args, validationErrors);

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

        public static bool IsArgsValid(string source, Dictionary<string, string> args, IList<string> validationErrors = null)
        {
            Guard.Against.NullOrWhiteSpace(source, nameof(source));

            if (args != null) return true;

            validationErrors?.Add($"'{args}' is not a valid {nameof(args)} (source: {source}).");

            return false;
        }

        #endregion
    }
}

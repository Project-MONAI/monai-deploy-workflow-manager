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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Extensions;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Logging;
using Monai.Deploy.WorkflowManager.Shared;
using static Monai.Deploy.WorkflowManager.Shared.ValidationConstants;

namespace Monai.Deploy.WorkflowManager.Validators
{
    /// <summary>
    /// Workflow Validator used for validating workflows.
    /// </summary>
    public class WorkflowValidator
    {
        private readonly ILogger<WorkflowValidator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowValidator"/> class.
        /// </summary>
        /// <param name="workflowService">The workflow service.</param>
        public WorkflowValidator(IWorkflowService workflowService, ILogger<WorkflowValidator> logger)
        {
            WorkflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets or sets errors from workflow validation.
        /// </summary>
        private List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets successful task paths. (not accounting for conditons).
        /// </summary>
        private List<string> SuccessfulPaths { get; set; } = new List<string>();

        private IWorkflowService WorkflowService { get; }

        /// <summary>
        /// used for checking for duplicates, if OrignalName is empty it will be determined as a create
        /// workflow attempt and check for duplicates or if this is not equal to workflow template it will
        /// check for duplicates.
        /// if workflow name is same as original name then we response user is updating workflow some other way
        /// will have duplicate.
        /// </summary>
        public string OrignalName { get; internal set; } = string.Empty;

        /// <summary>
        /// Resets the validator.
        /// </summary>
        public void Reset()
        {
            Errors.Clear();
            SuccessfulPaths.Clear();
            OrignalName = string.Empty;
        }

        /// <summary>
        /// Validates workflow against...
        /// - Make sure that Json Format is correct.
        /// - Check schema against spec
        /// - Make sure all task destinations reference existing tasks
        /// - Make sure all export destinations reference existing destination
        /// - Check against circular references
        /// - Ensure branches don't converge
        /// - Unreferenced tasks other than root task.
        /// </summary>
        /// <param name="workflow">Workflow to validate.</param>
        /// <param name="checkForDuplicates">Check for duplicates.</param>
        /// <param name="isUpdate">Used to check for duplicate name if it is a new workflow.</param>
        /// <returns>if any validation errors are produced while validating workflow.</returns>
        public async Task<(List<string> Errors, List<string> SuccessfulPaths)> ValidateWorkflow(Workflow workflow)
        {
            var tasks = workflow.Tasks;
            var firstTask = tasks.FirstOrDefault();

            await ValidateWorkflowSpec(workflow);
            DetectUnreferencedTasks(tasks, firstTask);
            ValidateTask(tasks, firstTask, 0);
            ValidateExportDestinations(workflow);

            if (Errors.Any())
            {
                _logger.WorkflowValidationErrors(string.Join(Environment.NewLine, Errors));
            }

            var results = (Errors.ToList(), SuccessfulPaths.ToList());
            Reset();
            return results;
        }

        private void ValidateExportDestinations(Workflow workflow)
        {
            if (workflow.Tasks.Any() is false)
            {
                return;
            }

            foreach (var task in workflow.Tasks.Where(task => task.ExportDestinations.IsNullOrEmpty() is false))
            {
                var taskExportDestinationNames = task.ExportDestinations.Select(td => td.Name);
                if (taskExportDestinationNames.Any() && (workflow.InformaticsGateway?.ExportDestinations?.IsNullOrEmpty() ?? true))
                {
                    Errors.Add("InformaticsGateway ExportDestinations destinations can not be null with an Export Task.");
                    return;
                }

                var diff = taskExportDestinationNames.Except(workflow.InformaticsGateway?.ExportDestinations).ToList();
                if (!diff.IsNullOrEmpty())
                {
                    foreach (var missingDestination in diff)
                    {
                        Errors.Add($"Task: '{task.Id}' export_destination: '{missingDestination}' must be registered in the informatics_gateway object.");
                    }
                }
            }
        }

        private void DetectUnreferencedTasks(TaskObject[] tasks, TaskObject firstTask)
        {
            if (tasks.Any() is false || firstTask is null)
            {
                return;
            }

            var otherTasks = tasks.Where(t => t.Id != firstTask.Id);
            var ids = otherTasks.Select(t => t.Id);
            var destinations = tasks.Where(t => t.TaskDestinations is not null)
                                    .SelectMany(t => t.TaskDestinations)
                                    .Select(td => td.Name);
            var diff = ids.Except(destinations);
            if (diff.Any())
            {
                Errors.Add($"Found Task(s) without any task destinations to it: {string.Join(",", diff)}");
            }
        }

        private async Task ValidateWorkflowSpec(Workflow workflow)
        {
            if (string.IsNullOrWhiteSpace(workflow.Name) is true)
            {
                Errors.Add("Missing Workflow Name.");
            }

            if (string.IsNullOrWhiteSpace(OrignalName) || OrignalName != workflow.Name)
            {
                var checkForDuplicates = await WorkflowService.GetByNameAsync(workflow.Name);
                if (checkForDuplicates != null)
                {
                    Errors.Add($"Workflow with name '{workflow.Name}' already exists, please review.");
                }
            }

            if (string.IsNullOrWhiteSpace(workflow.Version))
            {
                Errors.Add("Missing Workflow Version.");
            }

            ValidateInformaticsGateaway(workflow.InformaticsGateway);

            if (workflow.Tasks is null || workflow.Tasks.Any() is false)
            {
                Errors.Add("Workflow does not contain Tasks, please review Workflow.");
            }

            var taskIds = workflow.Tasks.Select(t => t.Id);
            var pattern = new Regex(@"^[a-zA-Z0-9-_]+$");
            foreach (var taskId in taskIds)
            {
                if (pattern.IsMatch(taskId) is false)
                {
                    Errors.Add($"TaskId: '{taskId}' Contains Invalid Characters.");
                }
            }
        }

        private void ValidateInformaticsGateaway(InformaticsGateway informaticsGateway)
        {
            if (informaticsGateway is null)
            {
                Errors.Add("Missing InformaticsGateway section.");
                return;
            }

            if (string.IsNullOrWhiteSpace(informaticsGateway.AeTitle) || informaticsGateway.AeTitle.Length > 15)
            {
                Errors.Add("AeTitle is required in the InformaticsGateaway section and must be under 16 charachters.");
                return;
            }
        }

        private void ValidateTask(TaskObject[] tasks, TaskObject currentTask, int iterationCount, List<string> paths = null)
        {
            if (tasks.Any() is false || currentTask is null)
            {
                return;
            }

            if (iterationCount > 100)
            {
                Errors.Add($"Detected task convergence on path: {string.Join(" => ", paths.Take(5))} => ∞");
                return;
            }

            paths ??= new List<string>();

            ValidateTaskArtifacts(currentTask);

            TaskTypeSpecificValidation(tasks, currentTask);

            ValidateTaskDestinations(tasks, currentTask, ref iterationCount, ref paths);
        }

        private void ValidateTaskArtifacts(TaskObject currentTask)
        {
            if (currentTask.Artifacts != null && currentTask.Artifacts.Output.IsNullOrEmpty() is false)
            {
                var uniqueOutputNames = new HashSet<string>();
                var allOutputsUnique = currentTask.Artifacts.Output.All(x => uniqueOutputNames.Add(x.Name));

                if (allOutputsUnique is false)
                {
                    Errors.Add($"Task: '{currentTask.Id}' has multiple output names with the same value.");
                }
            }
        }

        private void ValidateTaskDestinations(TaskObject[] tasks, TaskObject currentTask, ref int iterationCount, ref List<string> paths)
        {
            if (currentTask.TaskDestinations.IsNullOrEmpty())
            {
                paths.Add(currentTask.Id);
                SuccessfulPaths.Add(string.Join(" => ", paths));
                return;
            }

            foreach (var tasksDestinationName in currentTask.TaskDestinations.Select(td => td.Name))
            {
                if (paths.Contains(currentTask.Id.ToString()))
                {
                    Errors.Add($"Detected task convergence on path: {string.Join(" => ", paths)} => ∞");
                    paths = new List<string>();
                    continue;
                }

                paths.Add(currentTask.Id.ToString());
                var nextTask = tasks.FirstOrDefault(t => t.Id == tasksDestinationName);
                if (nextTask == null)
                {
                    Errors.Add($"Task: '{currentTask.Id}' has task destination: '{tasksDestinationName}' that could not be found.");
                    paths = new List<string>();
                    continue;
                }

                ValidateTask(tasks, nextTask, iterationCount += 1, paths);

                paths = new List<string>();
            }
        }

        private void TaskTypeSpecificValidation(TaskObject[] tasks, TaskObject currentTask)
        {
            if (ValidTaskTypes.Contains(currentTask.Type.ToLower()) is false)
            {
                Errors.Add($"Task: '{currentTask.Id}' has an invalid type, please specify: {string.Join(", ", ValidationConstants.ValidTaskTypes)}");
                return;
            }

            ValidateInputs(tasks, currentTask);

            if (currentTask.Type.Equals(ArgoTaskType, StringComparison.OrdinalIgnoreCase) is true)
            {
                ValidateArgoTask(currentTask);
            }

            if (currentTask.Type.Equals(ClinicalReviewTaskType, StringComparison.OrdinalIgnoreCase) is true)
            {
                ValidateClinicalReviewTask(tasks, currentTask);
            }
        }

        private void ValidateInputs(TaskObject[] tasks, TaskObject currentTask)
        {
            if (currentTask.Type.ToLower() == RouterTaskType)
            {
                return;
            }

            var inputs = currentTask?.Artifacts?.Input;

            if (inputs.IsNullOrEmpty())
            {
                Errors.Add($"Task: '{currentTask.Id}' must have Input Artifacts specified.");
                return;
            }

            if (inputs.Any((input) => string.IsNullOrWhiteSpace(input.Name)))
            {
                Errors.Add($"Task: '{currentTask.Id}' Input Artifacts must have a Name.");
            }

            if (inputs.Any((input) => string.IsNullOrWhiteSpace(input.Value)))
            {
                Errors.Add($"Task: '{currentTask.Id}' Input Artifacts must have a Value.");
            }
        }

        private void ValidateArgoTask(TaskObject currentTask)
        {
            if (!currentTask.Args.ContainsKey(WorkflowTemplateName))
            {
                Errors.Add($"Task: '{currentTask.Id}' workflow_template_name must be specified, this corresponds to an Argo template name.");
            }
        }

        private void ValidateClinicalReviewTask(TaskObject[] tasks, TaskObject currentTask)
        {
            ValidateClinicalReviewRequiredFields(tasks, currentTask);

            var inputs = currentTask?.Artifacts?.Input;

            foreach (var inputArtifact in inputs)
            {
                if (inputArtifact.Value.Contains("context.input.dicom"))
                {
                    continue;
                }

                var valueStringSplit = inputArtifact.Value.Split('.');

                if (valueStringSplit.Length < 3)
                {
                    Errors.Add($"Invalid Value property on input artifact '{inputArtifact.Name}' in task: '{currentTask.Id}'. Incorrect format.");
                    continue;
                }

                var referencedId = valueStringSplit[2];

                if (referencedId == currentTask.Id)
                {
                    Errors.Add($"Invalid Value property on input artifact '{inputArtifact.Name}' in task: '{currentTask.Id}'. Self referencing task ID.");
                    continue;
                }

                if (tasks.FirstOrDefault(t => t.Id == referencedId) == null)
                {
                    Errors.Add($"Invalid input artifact '{inputArtifact.Name}' in task '{currentTask.Id}': No matching task for ID '{referencedId}'");
                }
            }
        }

        private void ValidateClinicalReviewRequiredFields(TaskObject[] tasks, TaskObject currentTask)
        {
            if (!currentTask.Args.ContainsKey(ApplicationName))
            {
                Errors.Add($"Task: '{currentTask.Id}' application_name must be specified.");
            }

            if (!currentTask.Args.ContainsKey(ApplicationVersion))
            {
                Errors.Add($"Task: '{currentTask.Id}' application_version must be specified.");
            }

            if (!currentTask.Args.ContainsKey(Mode) || !Enum.TryParse(typeof(ModeValues), currentTask.Args[Mode], true, out var _))
            {
                Errors.Add($"Task: '{currentTask.Id}' mode is incorrectly specified, please specify 'QA', 'Research' or 'Clinical'");
            }

            if (!currentTask.Args.ContainsKey(ReviewedTaskId))
            {
                Errors.Add($"Task: '{currentTask.Id}' reviewed_task_id must be specified.");
            }
            else if (tasks.Any(t => t.Id.ToLower() == currentTask.Args[ReviewedTaskId].ToLower()) is false)
            {
                Errors.Add($"Task: '{currentTask.Id}' reviewed_task_id: '{currentTask.Args[ReviewedTaskId]}' could not be found in the workflow.");
            }
        }
    }
}

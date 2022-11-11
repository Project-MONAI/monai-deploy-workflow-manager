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
using Monai.Deploy.WorkflowManager.PayloadListener.Extensions;
using Monai.Deploy.WorkflowManager.Shared;

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
        /// Resets the validator.
        /// </summary>
        public void Reset()
        {
            Errors.Clear();
            SuccessfulPaths.Clear();
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
        /// <returns>if any validation errors are produced while validating workflow.</returns>
        public async Task<(List<string> Errors, List<string> SuccessfulPaths)> ValidateWorkflow(Workflow workflow, bool checkForDuplicates = true)
        {
            workflow.IsValid(out var validationErrors);
            Errors.AddRange(validationErrors);
            var tasks = workflow.Tasks;
            var firstTask = tasks.FirstOrDefault();

            await ValidateWorkflowSpec(workflow, checkForDuplicates);
            DetectUnreferencedTasks(tasks, firstTask);
            ValidateTask(tasks, firstTask, 0);
            ValidateTaskDestinations(workflow);
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
                if (taskExportDestinationNames.Any() && workflow.InformaticsGateway.ExportDestinations.IsNullOrEmpty())
                {
                    Errors.Add("InformaticsGateway ExportDestinations destinations can not be null");
                    return;
                }

                var diff = taskExportDestinationNames.Except(workflow.InformaticsGateway.ExportDestinations).ToList();
                if (!diff.IsNullOrEmpty())
                {
                    foreach (var missingDestination in diff)
                    {
                        Errors.Add($"Missing destination {missingDestination} in task {task.Id}");
                    }
                }
            }
        }

        /// <summary>
        /// Make sure all task destinations reference existing tasks.
        /// </summary>
        /// <param name="workflow">workflow.</param>
        private void ValidateTaskDestinations(Workflow workflow)
        {
            if (workflow.Tasks.Any() is false)
            {
                return;
            }

            var tasksDestinations = workflow.Tasks.Where(task => task.TaskDestinations is not null)
                .SelectMany(task => task.TaskDestinations.Select(td => td.Name));
            foreach (var taskDestination in tasksDestinations)
            {
                var destinationCount = workflow.Tasks.Count(t => t.Id == taskDestination);
                if (destinationCount == 0)
                {
                    // Make sure all task destinations reference existing tasks
                    Errors.Add($"ERROR: Task destination {taskDestination} not found.\n");
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

        private async Task ValidateWorkflowSpec(Workflow workflow, bool checkForDuplicates)
        {
            if (string.IsNullOrWhiteSpace(workflow.Name) is true)
            {
                Errors.Add("Missing Workflow Name.");
            }

            if (checkForDuplicates
                && string.IsNullOrWhiteSpace(workflow.Name) is false
                && await WorkflowService.GetByNameAsync(workflow.Name) != null)
            {
                Errors.Add($"A Workflow with the name: {workflow.Name} already exists.");
            }

            if (string.IsNullOrWhiteSpace(workflow.Version))
            {
                Errors.Add("Missing Workflow Version.");
            }

            if (workflow.Tasks is null || workflow.Tasks.Any() is false)
            {
                Errors.Add("Missing Workflow Tasks.");
            }

            var taskIds = workflow.Tasks.Select(t => t.Id);
            var pattern = new Regex(@"^[a-zA-Z0-9-_]+$");
            foreach (var taskId in taskIds)
            {
                if (pattern.IsMatch(taskId) is false)
                {
                    Errors.Add($"TaskId: {taskId} Contains Invalid Characters.");
                }
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
                Errors.Add($"Detected infinite task loop on path: {string.Join(" => ", paths.Take(5))} => ∞");
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
                    Errors.Add($"Task: \"{currentTask.Id}\" has multiple output names with the same value.\n");
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
                    Errors.Add($"Task: \"{currentTask.Id}\" has task destination: \"{tasksDestinationName}\" that could not be found.\n");
                    paths = new List<string>();
                    continue;
                }

                ValidateTask(tasks, nextTask, iterationCount += 1, paths);

                paths = new List<string>();
            }
        }

        private void TaskTypeSpecificValidation(TaskObject[] tasks, TaskObject currentTask)
        {
            if (currentTask.Type.Equals("argo", StringComparison.OrdinalIgnoreCase) is true)
            {
                ValidateArgoTask(currentTask);
            }

            if (currentTask.Type.Equals("clinical-review", StringComparison.OrdinalIgnoreCase) is true)
            {
                ValidateClinicalReviewTask(tasks, currentTask);
            }
        }

        private void ValidateArgoTask(TaskObject currentTask)
        {
            var missingKeys = new List<string>();

            foreach (var key in ValidationConstants.ArgoRequiredParameters)
            {
                if (!currentTask.Args.ContainsKey(key))
                {
                    missingKeys.Add(key);
                }
            }

            if (missingKeys.Count > 0)
            {
                Errors.Add($"Required parameter to execute Argo workflow is missing: {string.Join(", ", missingKeys)}");
            }
        }

        private void ValidateClinicalReviewTask(TaskObject[] tasks, TaskObject currentTask)
        {
            var inputs = currentTask?.Artifacts?.Input;

            if (inputs.IsNullOrEmpty())
            {
                Errors.Add($"Missing inputs for clinical review task: {currentTask.Id}");
                return;
            }

            var missingKeys = new List<string>();

            foreach (var key in ValidationConstants.ClinicalReviewRequiredParameters)
            {
                if (!currentTask.Args.ContainsKey(key))
                {
                    missingKeys.Add(key);
                }
            }

            if (missingKeys.Count > 0)
            {
                Errors.Add($"Required parameter for clinical review args are missing: {string.Join(", ", missingKeys)}");
            }

            foreach (var inputArtifact in inputs)
            {
                if (inputArtifact.Value.Contains("context.input.dicom"))
                {
                    continue;
                }

                var valueStringSplit = inputArtifact.Value.Split('.');

                if (valueStringSplit.Length < 3)
                {
                    Errors.Add($"Invalid Value property on input artifact {inputArtifact.Name} in task: {currentTask.Id}. Incorrect format.");
                    continue;
                }

                var referencedId = valueStringSplit[2];

                if (referencedId == currentTask.Id)
                {
                    Errors.Add($"Invalid Value property on input artifact {inputArtifact.Name} in task: {currentTask.Id}. Self referencing task ID.");
                    continue;
                }

                if (tasks.FirstOrDefault(t => t.Id == referencedId) == null)
                {
                    Errors.Add($"Invalid input artifact '{inputArtifact.Name}' in task '{currentTask.Id}': No matching task for ID '{referencedId}'");
                }
            }
        }
    }
}

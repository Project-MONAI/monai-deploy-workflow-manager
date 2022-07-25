// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using System.Linq;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Extentions;
using Monai.Deploy.WorkflowManager.PayloadListener.Extensions;

namespace Monai.Deploy.WorkflowManager.Validators
{
    /// <summary>
    /// Workflow Validator used for validating workflows.
    /// </summary>
    public class WorkflowValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowValidator"/> class.
        /// </summary>
        public WorkflowValidator()
        {
        }

        /// <summary>
        /// Gets a value indicating whether result of ValidateWorkflow which is if workflow is valid.
        /// </summary>
        public bool IsWorkflowValid { get => Errors.Any(); }

        /// <summary>
        /// Gets errors from workflow validation.
        /// </summary>
        public List<string> Errors { get; private set; } = new List<string>();

        /// <summary>
        /// Gets successful task paths. (not accounting for conditons).
        /// </summary>
        public List<string> SuccessfulPaths { get; private set; } = new List<string>();

        /// <summary>
        /// Validates workflow against...
        /// - Make sure that Json Format is correct.
        /// - Check schema against spec
        /// - Make sure all task destinations reference existings tasks
        /// - Make sure all export destinations reference existing destination
        /// - Check against circular references
        /// - Ensure branches don't converge
        /// - Unreferenced tasks other than root task.
        /// </summary>
        /// <param name="workflow">Workflow to validate.</param>
        /// <returns>if any validation errors are produced while validating workflow.</returns>
        public bool ValidateWorkflow(Workflow workflow)
        {
            workflow.IsValid(out var validationErrors);
            Errors.AddRange(validationErrors);
            var tasks = workflow.Tasks;
            var firstTask = tasks.First();

            ValidateWorkflowSpec(workflow);
            DetectUnreferencedTasks(tasks, firstTask);
            ValidateTask(tasks, firstTask, 0);
            ValidateTaskDestinations(workflow);
            ValidateExportDestinations(workflow);

            return Errors.Any();
        }

        private void ValidateExportDestinations(Workflow workflow)
        {
            foreach (var task in workflow.Tasks.Where(task => task.ExportDestinations is not null))
            {
                var taskExportDestinationNames = task.ExportDestinations.Select(td => td.Name);
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
        /// Make sure all task destinations reference existings tasks.
        /// </summary>
        /// <param name="workflow">workflow.</param>
        private void ValidateTaskDestinations(Workflow workflow)
        {
            var tasksDestinations = workflow.Tasks.Where(task => task.TaskDestinations is not null)
                .SelectMany(task => task.TaskDestinations.Select(td => td.Name));
            foreach (var taskDestination in tasksDestinations)
            {
                var destinationCount = workflow.Tasks.Count(t => t.Id == taskDestination);
                if (destinationCount == 0)
                {
                    // Make sure all task destinations reference existings tasks
                    Errors.Add($"ERROR: Task destination {taskDestination} not found.\n");
                }
            }
        }

        private void DetectUnreferencedTasks(TaskObject[] tasks, TaskObject firstTask)
        {
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

        private void ValidateWorkflowSpec(Workflow workflow)
        {
            if (string.IsNullOrWhiteSpace(workflow.Name))
            {
                Errors.Add("Missing Workflow Name.");
            }

            if (string.IsNullOrWhiteSpace(workflow.Version))
            {
                Errors.Add("Missing Workflow Version.");
            }

            if (workflow.Tasks is null || workflow.Tasks.Length == 0)
            {
                Errors.Add("Missing Workflow Tasks.");
            }
        }

        private void ValidateTask(TaskObject[] tasks, TaskObject currentTask, int iterationCount, List<string>? paths = null)
        {
            if (iterationCount > 100)
            {
                Errors.Add($"Detected task convergence on path: {string.Join(" => ", paths)} => ∞");
            }

            if (paths == null)
            {
                paths = new List<string>();
            }

            if (currentTask.TaskDestinations is null)
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

                ValidateTask(tasks, nextTask, iterationCount++, paths);

                paths = new List<string>();
            }
        }
    }
}

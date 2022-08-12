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

using System.Collections.Generic;
using System.Linq;
using Monai.Deploy.WorkflowManager.Common.Extensions;
using System.Text.RegularExpressions;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.PayloadListener.Extensions;

namespace Monai.Deploy.WorkflowManager.Validators
{
    /// <summary>
    /// Workflow Validator used for validating workflows.
    /// </summary>
    public static class WorkflowValidator
    {
        /// <summary>
        /// Gets or sets errors from workflow validation.
        /// </summary>
        private static List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets successful task paths. (not accounting for conditons).
        /// </summary>
        private static List<string> SuccessfulPaths { get; set; } = new List<string>();

        /// <summary>
        /// Resets the validator.
        /// </summary>
        public static void Reset()
        {
            Errors.Clear();
            SuccessfulPaths.Clear();
        }

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
        /// <param name="results">Workflow validation results.</param>
        /// <returns>if any validation errors are produced while validating workflow.</returns>
        public static bool ValidateWorkflow(Workflow workflow, out (List<string> Errors, List<string> SuccessfulPaths) results)
        {
            workflow.IsValid(out var validationErrors);
            Errors.AddRange(validationErrors);
            var tasks = workflow.Tasks;
            var firstTask = tasks.FirstOrDefault();

            ValidateWorkflowSpec(workflow);
            DetectUnreferencedTasks(tasks, firstTask);
            ValidateTask(tasks, firstTask, 0);
            ValidateTaskDestinations(workflow);
            ValidateExportDestinations(workflow);

            results = (Errors.ToList(), SuccessfulPaths.ToList());
            Reset();
            return results.Errors.Any();
        }

        private static void ValidateExportDestinations(Workflow workflow)
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
        /// Make sure all task destinations reference existings tasks.
        /// </summary>
        /// <param name="workflow">workflow.</param>
        private static void ValidateTaskDestinations(Workflow workflow)
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
                    // Make sure all task destinations reference existings tasks
                    Errors.Add($"ERROR: Task destination {taskDestination} not found.\n");
                }
            }
        }

        private static void DetectUnreferencedTasks(TaskObject[] tasks, TaskObject firstTask)
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

        private static void ValidateWorkflowSpec(Workflow workflow)
        {
            if (string.IsNullOrWhiteSpace(workflow.Name))
            {
                Errors.Add("Missing Workflow Name.");
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

        private static void ValidateTask(TaskObject[] tasks, TaskObject currentTask, int iterationCount, List<string> paths = null)
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

            if (paths == null)
            {
                paths = new List<string>();
            }

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

                ValidateTask(tasks, nextTask, iterationCount++, paths);

                paths = new List<string>();
            }
        }
    }
}

// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.WorkfowExecuter.Services
{
    internal class WorkflowValidator
    {
        private readonly ILogger<WorkflowExecuterService> _logger;

        public bool IsWorkflowValid { get; private set; }

        public List<string> Errors { get; set; }

        public WorkflowValidator(ILogger<WorkflowExecuterService> logger)
        {
            _logger = logger;
        }

        public bool ValidateWorkflow(WorkflowRevision workflow, out List<string> errors)
        {
            IsWorkflowValid = true;
            errors = new List<string>();
            var tasks = workflow.Workflow.Tasks;
            var tasksDestinations = tasks.Where(task => task.TaskDestinations is not null)
                                                           .SelectMany(task => task.TaskDestinations.Select(td => td.Name));
            foreach (var taskDestination in tasksDestinations)
            {
                var destinationCount = workflow.Workflow.Tasks.Count(t => t.Id == taskDestination);
                if (destinationCount == 0)
                {
                    //Make sure all task destinations reference existings tasks
                    _logger.LogError($"ERROR: Task destination {taskDestination} not found.\n");
                    IsWorkflowValid = false;
                }
            }

            var firstTask = tasks.First();
            var validationReuslt = ValidateTask(tasks, firstTask, 0);

            errors.AddRange(validationReuslt);

            if (errors.Any())
            {
                throw new ArgumentException($"{string.Join(", ", errors)}");
            }

            return errors.Any();
        }

        private bool ValidateTask(TaskObject[] tasks, TaskObject currentTask, int iterationCount, List<string>? paths = null)
        {
            if (currentTask.TaskDestinations is null)
            {
                return true;
            }

            if (paths == null)
            {
                paths = new List<string>();
            }

            foreach (var tasksDestinationName in currentTask.TaskDestinations.Select(td => td.Name))
            {
                if (paths.Contains(currentTask.Id.ToString()))
                {
                    _logger.LogError($"Detected task convergence on path: {string.Join(" => ", paths)}");
                    IsWorkflowValid = false;
                }

                paths.Add(currentTask.Id.ToString());
                var nextTask = tasks.FirstOrDefault(t => t.Id == tasksDestinationName);
                if (nextTask == null)
                {
                    _logger.LogError($"Task: \"{currentTask.Id}\" has task destination: \"{tasksDestinationName}\" that could not be found.\n");
                    IsWorkflowValid = false;
                    continue;
                }

                return ValidateTask(tasks, nextTask, iterationCount++, paths);
            }

            return true;
        }
    }
}

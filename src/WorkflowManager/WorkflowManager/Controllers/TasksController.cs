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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Logging;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Filter;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;
using Monai.Deploy.WorkflowManager.Common.Models;

namespace Monai.Deploy.WorkflowManager.Common.ControllersShared
{
    /// <summary>
    /// Tasks Api endpoint controller.
    /// </summary>
    [ApiController]
    [Route("tasks")]
    public class TasksController : AuthenticatedApiControllerBase
    {
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly ITasksService _tasksService;

        private readonly ILogger<TasksController> _logger;
        private readonly IUriService _uriService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TasksController"/> class.
        /// </summary>
        /// <param name="tasksService">Task service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="uriService">Uri Service.</param>
        /// <param name="options">Options.</param>
        /// <exception cref="ArgumentNullException">Undeclared Services.</exception>
        public TasksController(
            ITasksService tasksService,
            ILogger<TasksController> logger,
            IUriService uriService,
            IOptions<WorkflowManagerOptions> options)
        : base(options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _tasksService = tasksService ?? throw new ArgumentNullException(nameof(tasksService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uriService = uriService ?? throw new ArgumentNullException(nameof(uriService));
        }

        /// <summary>
        /// Gets a list of all running tasks.
        /// </summary>
        /// <param name="filter">Pagination Filters.</param>
        /// <returns>All running tasks.</returns>
        [HttpGet("running")]
        [ProducesResponseType(typeof(PagedResponse<List<TaskExecution>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListAsync([FromQuery] PaginationFilter filter)
        {
            try
            {
                var route = Request?.Path.Value ?? string.Empty;
                var pageSize = filter.PageSize ?? _options.Value.EndpointSettings.DefaultPageSize;
                var validFilter = new PaginationFilter(filter.PageNumber, pageSize, _options.Value.EndpointSettings.MaxPageSize);

                var pagedData = await _tasksService.GetAllAsync(
                    (validFilter.PageNumber - 1) * validFilter.PageSize,
                    validFilter.PageSize);

                var pagedReponse = CreatePagedResponse(pagedData.Tasks.ToList(), validFilter, pagedData.Count, _uriService, route);

                return Ok(pagedReponse);
            }
            catch (Exception e)
            {
                _logger.TasksGetRunningListAsyncError(e);
                return Problem($"Unexpected error occurred: {e.Message}", $"/tasks/running", InternalServerError);
            }
        }

        /// <summary>
        /// Gets individual task.
        /// </summary>
        /// <param name="request">TasksRequest model.</param>
        /// <returns>Task Information.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(TaskExecution), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromBody] TasksRequest request)
        {
            var workflowInstanceId = request.WorkflowInstanceId;
            var taskId = request.TaskId;
            var executionId = request.ExecutionId;

            var wfIdValid = string.IsNullOrWhiteSpace(workflowInstanceId) || !Guid.TryParse(workflowInstanceId, out _);
            var taskIdValid = string.IsNullOrWhiteSpace(taskId) || !Guid.TryParse(taskId, out _);
            var execIdValid = string.IsNullOrWhiteSpace(executionId) || !Guid.TryParse(executionId, out _);

            if (wfIdValid || taskIdValid || execIdValid)
            {
                _logger.LogDebug($"{nameof(GetAsync)} - Failed to validate ids");

                return Problem($"Failed to validate ids, not a valid guid", $"/tasks/", BadRequest);
            }

            try
            {
                var task = await _tasksService.GetTaskAsync(workflowInstanceId, taskId, executionId);
                if (task is null)
                {
                    return Problem($"Failed to validate ids, workflow or task not found", $"/tasks/", NotFound);
                }

                return Ok(task);
            }
            catch (Exception e)
            {
                _logger.TasksGetAsyncError(e);
                return Problem($"Unexpected error occurred: {e.Message}", $"/tasks/", InternalServerError);
            }
        }
    }
}

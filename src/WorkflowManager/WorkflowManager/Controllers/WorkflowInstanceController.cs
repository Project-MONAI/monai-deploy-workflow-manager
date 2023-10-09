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
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Exceptions;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Filter;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;

namespace Monai.Deploy.WorkflowManager.Common.ControllersShared
{
    /// <summary>
    /// Workflow Instances Controller.
    /// </summary>
    [ApiController]
    [Route("workflowinstances")]
    public class WorkflowInstanceController : AuthenticatedApiControllerBase
    {
        // ReSharper disable once InconsistentNaming
        private const string ENDPOINT = "/workflowinstances/";
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IWorkflowInstanceService _workflowInstanceService;

        private readonly ILogger<WorkflowInstanceController> _logger;
        private readonly IUriService _uriService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowInstanceController"/> class.
        /// </summary>
        /// <param name="workflowInstanceService">Workflow Instance Service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="uriService">Uri Service.</param>
        /// <param name="options">Workflow Manager options.</param>
        public WorkflowInstanceController(
            IWorkflowInstanceService workflowInstanceService,
            ILogger<WorkflowInstanceController> logger,
            IUriService uriService,
            IOptions<WorkflowManagerOptions> options)
            : base(options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _workflowInstanceService = workflowInstanceService ?? throw new ArgumentNullException(nameof(workflowInstanceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uriService = uriService ?? throw new ArgumentNullException(nameof(uriService));
        }

        /// <summary>
        /// Get a list of workflowInstances.
        /// </summary>
        /// <param name="filter">Filters.</param>
        /// <param name="status">Workflow instance status filter.</param>
        /// <param name="payloadId">PayloadId.</param>
        /// <param name="disablePagination">Disabled pagination.</param>
        /// <returns>A list of workflow instances.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IList<WorkflowInstance>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListAsync([FromQuery] PaginationFilter filter, [FromQuery] string status = null, [FromQuery] string payloadId = null, [FromQuery] bool disablePagination = false)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(payloadId) && !Guid.TryParse(payloadId, out _))
                {
                    _logger.LogDebug($"{nameof(GetListAsync)} - Failed to validate {nameof(payloadId)}");

                    return Problem($"Failed to validate {nameof(payloadId)}, not a valid GUID", $"{ENDPOINT}{payloadId}", BadRequest);
                }

                Status? parsedStatus = status == null ? null : Enum.Parse<Status>(status, true);

                if (disablePagination is true)
                {
                    var unpagedData = await _workflowInstanceService.GetAllAsync(null, null, parsedStatus, payloadId);

                    return Ok(unpagedData);
                }

                var route = Request?.Path.Value ?? string.Empty;
                var pageSize = filter.PageSize ?? _options.Value.EndpointSettings.DefaultPageSize;
                var validFilter = new PaginationFilter(filter.PageNumber, pageSize, _options.Value.EndpointSettings.MaxPageSize);

                var pagedData = await _workflowInstanceService.GetAllAsync(
                    (validFilter.PageNumber - 1) * validFilter.PageSize,
                    validFilter.PageSize,
                    parsedStatus,
                    payloadId);

                var dataTotal = await _workflowInstanceService.FilteredCountAsync(parsedStatus, payloadId);

                var pagedReponse = CreatePagedResponse(pagedData.ToList(), validFilter, dataTotal, _uriService, route);

                return Ok(pagedReponse);
            }
            catch (Exception e)
            {
                _logger.WorkflowinstancesGetListAsyncError(e);
                return Problem($"Unexpected error occurred: {e.Message}", $"{ENDPOINT}", InternalServerError);
            }
        }

        /// <summary>
        /// Get a list of workflowInstances.
        /// </summary>
        /// <param name="id">The Workflow Instance Id.</param>
        /// <returns>A list of workflow instances.</returns>
        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(WorkflowInstance), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                _logger.LogDebug($"{nameof(GetByIdAsync)} - Failed to validate {nameof(id)}");

                return Problem($"Failed to validate {nameof(id)}, not a valid GUID", $"{ENDPOINT}{id}", BadRequest);
            }

            using var loggingScope = _logger.BeginScope(new LoggingDataDictionary<string, object>
            {
                ["workflowId"] = id,
            });

            try
            {
                var workflowInstance = await _workflowInstanceService.GetByIdAsync(id);

                if (workflowInstance is null)
                {
                    _logger.LogDebug($"{nameof(GetByIdAsync)} - Failed to find workflow instance with Id: {id}");

                    return Problem($"Failed to find workflow instance with Id: {id}", $"{ENDPOINT}{id}", NotFound);
                }

                return Ok(workflowInstance);
            }
            catch (Exception e)
            {
                _logger.WorkflowinstancesGetByIdAsyncError(id, e);
                return Problem($"Unexpected error occurred: {e.Message}", $"{ENDPOINT}{nameof(id)}", InternalServerError);
            }
        }

        /// <summary>
        /// Returns failed workflow instances, that have an empty value of.
        /// </summary>
        /// <param name="acknowledged">Failed workflow's since this date as ISO 8601 standard ("YYYY-MM-DDT00:00").</param>
        /// <returns>This should be a new endpoint to return failed workflow's instances, that have an empty value of.</returns>
        [Route("failed")]
        [HttpGet]
        [ProducesResponseType(typeof(IList<WorkflowInstance>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFailedAsync()
        {
            try
            {
                var workflowInstances = await _workflowInstanceService.GetAllFailedAsync();

                return Ok(workflowInstances);
            }
            catch (Exception e)
            {
                _logger.WorkflowinstancesGetFailedAsyncError(e);
                return Problem($"Unexpected error occurred.", $"{ENDPOINT}failed", InternalServerError);
            }
        }

        /// <summary>
        /// Acknowledges a task error and acknowledges a workflow if all tasks are acknowledged.
        /// </summary>
        /// <param name="id">The Workflow Instance Id.</param>
        /// <param name="executionId">The Task Execution Id.</param>
        /// <returns>An updated workflow.</returns>
        [Route("{id}/executions/{executionId}/acknowledge")]
        [HttpPut]
        [ProducesResponseType(typeof(WorkflowInstance), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AcknowledgeTaskError([FromRoute] string id, [FromRoute] string executionId)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                _logger.LogDebug($"{nameof(AcknowledgeTaskError)} - Failed to validate {nameof(id)}");

                return Problem($"Failed to validate {nameof(id)}, not a valid GUID", $"/workflows/{id}/executions/{executionId}/acknowledge", BadRequest);
            }

            if (string.IsNullOrWhiteSpace(executionId) || !Guid.TryParse(executionId, out _))
            {
                _logger.LogDebug($"{nameof(AcknowledgeTaskError)} - Failed to validate {nameof(executionId)}");

                return Problem($"Failed to validate {nameof(executionId)}, not a valid GUID", $"/workflows/{id}/executions/{executionId}/acknowledge", BadRequest);
            }

            using var loggingScope = _logger.BeginScope(new LoggingDataDictionary<string, object>
            {
                ["workflowId"] = id,
                ["executionId"] = executionId,
            });

            try
            {
                var workflowInstance = await _workflowInstanceService.AcknowledgeTaskError(id, executionId);

                return Ok(workflowInstance);
            }
            catch (MonaiBadRequestException e)
            {
                _logger.WorkflowinstancesAcknowledgeTaskError(id, executionId, e);
                return Problem(e.Message, $"/workflows/{id}/executions/{executionId}/acknowledge", BadRequest);
            }
            catch (MonaiNotFoundException e)
            {
                _logger.WorkflowinstancesAcknowledgeTaskError(id, executionId, e);
                return Problem(e.Message, $"/workflows/{id}/executions/{executionId}/acknowledge", NotFound);
            }
            catch (Exception e)
            {
                _logger.WorkflowinstancesAcknowledgeTaskError(id, executionId, e);
                return Problem($"Unexpected error occurred: {e.Message}", $"/workflows/{id}/executions/{executionId}/acknowledge", InternalServerError);
            }
        }
    }
}

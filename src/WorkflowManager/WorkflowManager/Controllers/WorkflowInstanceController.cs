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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Extensions;
using Monai.Deploy.WorkflowManager.Common.Exceptions;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Filter;
using Monai.Deploy.WorkflowManager.Services;

namespace Monai.Deploy.WorkflowManager.Controllers
{
    /// <summary>
    /// Workflow Instances Controller.
    /// </summary>
    [ApiController]
    [Route("workflowinstances")]
    public class WorkflowInstanceController : AuthenticatedApiControllerBase
    {
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
        public async Task<IActionResult> GetListAsync([FromQuery] PaginationFilter filter, [FromQuery] string status = null, [FromQuery] string? payloadId = null, [FromQuery] bool disablePagination = false)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(payloadId) && !Guid.TryParse(payloadId, out _))
                {
                    _logger.LogDebug($"{nameof(GetListAsync)} - Failed to validate {nameof(payloadId)}");

                    return Problem($"Failed to validate {nameof(payloadId)}, not a valid guid", $"{ENDPOINT}{payloadId}", BadRequest);
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

                var pagedReponse = CreatePagedReponse(pagedData.ToList(), validFilter, dataTotal, _uriService, route);

                return Ok(pagedReponse);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GetListAsync)} - Failed to get workflowInstances", e);

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
        public async Task<IActionResult> GetByIdAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                _logger.LogDebug($"{nameof(GetByIdAsync)} - Failed to validate {nameof(id)}");

                return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"{ENDPOINT}{id}", BadRequest);
            }

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
        public async Task<IActionResult> GetFailedAsync([FromQuery] DateTime? acknowledged)
        {
            if (acknowledged is null)
            {
                return Problem($"Failed to validate, no {nameof(acknowledged)} parameter provided", $"{ENDPOINT}failed", BadRequest);
            }

            var acknowledgedDateTime = acknowledged.Value;

            if (acknowledgedDateTime > DateTime.UtcNow)
            {
                return Problem($"Failed to validate {nameof(acknowledged)} value: {acknowledgedDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}, provided time is in the future.", $"{ENDPOINT}failed", BadRequest);
            }

            try
            {
                var workflowInstances = await _workflowInstanceService.GetAllFailedAsync(acknowledgedDateTime);
                if (workflowInstances.IsNullOrEmpty())
                {
                    return Problem($"Request failed, no workflow instances found since {acknowledgedDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}", $"{ENDPOINT}failed", NotFound);
                }

                return Ok(workflowInstances);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GetFailedAsync)} - Failed to get failed workflowInstances", e);

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
        public async Task<IActionResult> AcknowledgeTaskError([FromRoute] string id, [FromRoute] string executionId)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                _logger.LogDebug($"{nameof(AcknowledgeTaskError)} - Failed to validate {nameof(id)}");

                return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"/workflows/{id}/executions/{executionId}/acknowledge", BadRequest);
            }

            if (string.IsNullOrWhiteSpace(executionId) || !Guid.TryParse(executionId, out _))
            {
                _logger.LogDebug($"{nameof(AcknowledgeTaskError)} - Failed to validate {nameof(executionId)}");

                return Problem($"Failed to validate {nameof(executionId)}, not a valid guid", $"/workflows/{id}/executions/{executionId}/acknowledge", BadRequest);
            }

            try
            {
                var workflowInstance = await _workflowInstanceService.AcknowledgeTaskError(id, executionId);

                return Ok(workflowInstance);
            }
            catch (MonaiBadRequestException e)
            {
                _logger.LogDebug($"{nameof(AcknowledgeTaskError)} - {e.Message}");

                return Problem(e.Message, $"/workflows/{id}/executions/{executionId}/acknowledge", BadRequest);
            }
            catch (MonaiNotFoundException e)
            {
                _logger.LogDebug($"{nameof(AcknowledgeTaskError)} - {e.Message}");

                return Problem(e.Message, $"/workflows/{id}/executions/{executionId}/acknowledge", NotFound);
            }
            catch (Exception e)
            {
                _logger.LogDebug($"{nameof(AcknowledgeTaskError)} - Unexpected error occured: {e.Message}");

                return Problem($"Unexpected error occured: {e.Message}", $"/workflows/{id}/executions/{executionId}/acknowledge", InternalServerError);
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
        public async Task<IActionResult> AcknowledgeTaskError([FromRoute] string id, [FromRoute] string executionId)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                _logger.LogDebug($"{nameof(GetByIdAsync)} - Failed to validate {nameof(id)}");

                return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"/workflows/{id}/executions/{executionId}/acknowledge", BadRequest);
            }

            if (string.IsNullOrWhiteSpace(executionId) || !Guid.TryParse(executionId, out _))
            {
                _logger.LogDebug($"{nameof(GetByIdAsync)} - Failed to validate {nameof(executionId)}");

                return Problem($"Failed to validate {nameof(executionId)}, not a valid guid", $"/workflows/{id}/executions/{executionId}/acknowledge", BadRequest);
            }

            try
            {
                var workflowInstance = await _workflowInstanceService.AcknowledgeTaskError(id, executionId);

                return Ok(workflowInstance);
            }
            catch (Exception e)
            {
                return Problem($"Unexpected error occured: {e.Message}", $"/workflows/{id}/executions/{executionId}/acknowledge", InternalServerError);
            }
        }
    }
}

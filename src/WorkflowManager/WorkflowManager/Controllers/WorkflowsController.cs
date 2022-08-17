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

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Contracts.Responses;
using Monai.Deploy.WorkflowManager.Filter;
using Monai.Deploy.WorkflowManager.Services;
using Monai.Deploy.WorkflowManager.Validators;

namespace Monai.Deploy.WorkflowManager.Controllers
{
    /// <summary>
    /// Workflows Controller.
    /// </summary>
    [ApiController]
    [Route("workflows")]
    public class WorkflowsController : AuthenticatedApiControllerBase
    {
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<WorkflowsController> _logger;
        private readonly IUriService _uriService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowsController"/> class.
        /// </summary>
        /// <param name="workflowService">IWorkflowService.</param>
        /// <param name="logger">ILogger.WorkflowsController.</param>
        /// <param name="uriService">Uri Service.</param>
        /// <param name="options">Workflow Manager options.</param>
        /// <exception cref="ArgumentNullException">ArgumentNullException.</exception>
        public WorkflowsController(
            IWorkflowService workflowService,
            ILogger<WorkflowsController> logger,
            IUriService uriService,
            IOptions<WorkflowManagerOptions> options)
            : base(options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uriService = uriService ?? throw new ArgumentNullException(nameof(uriService));
        }

        /// <summary>
        /// Gets a list of all workflows.
        /// </summary>
        /// <param name="filter">Pagination filter.</param>
        /// <returns>The ID of the created Workflow.</returns>
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] PaginationFilter filter)
        {
            try
            {
                var route = Request?.Path.Value ?? string.Empty;
                var pageSize = filter.PageSize ?? _options.Value.EndpointSettings.DefaultPageSize;
                var validFilter = new PaginationFilter(filter.PageNumber, pageSize, _options.Value.EndpointSettings.MaxPageSize);

                var pagedData = await _workflowService.GetAllAsync(
                    (validFilter.PageNumber - 1) * validFilter.PageSize,
                    validFilter.PageSize);

                var dataTotal = await _workflowService.CountAsync();
                var pagedReponse = CreatePagedReponse(pagedData.ToList(), validFilter, dataTotal, _uriService, route);

                return Ok(pagedReponse);
            }
            catch (Exception e)
            {
                return Problem($"Unexpected error occured: {e.Message}", $"/workflows", InternalServerError);
            }
        }

        /// <summary>
        /// Get a workflow by the ID.
        /// </summary>
        /// <param name="id">The Workflow Id.</param>
        /// <returns>The specified workflow for a given Id.</returns>
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                _logger.LogDebug($"{nameof(GetAsync)} - Failed to validate {nameof(id)}");

                return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"/workflows/{id}", BadRequest);
            }

            try
            {
                var workflow = await _workflowService.GetAsync(id);
                if (workflow is null)
                {
                    return Problem($"Failed to validate {nameof(id)}, workflow not found", $"/workflows/{id}", NotFound);
                }

                return Ok(workflow);
            }
            catch (Exception e)
            {
                return Problem($"Unexpected error occured: {e.Message}", $"/workflows/{nameof(id)}", InternalServerError);
            }
        }

        /// <summary>
        /// Create a workflow.
        /// </summary>
        /// <param name="workflow">The Workflow.</param>
        /// <returns>The ID of the created Workflow.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] Workflow workflow)
        {
            if (WorkflowValidator.ValidateWorkflow(workflow, out var results))
            {
                var validationErrors = string.Join(", ", results.Errors);
                _logger.LogDebug($"{nameof(CreateAsync)} - Failed to validate {nameof(workflow)}: {validationErrors}");

                return Problem($"Failed to validate {nameof(workflow)}: {string.Join(", ", validationErrors)}", $"/workflows", BadRequest);
            }

            try
            {
                var workflowId = await _workflowService.CreateAsync(workflow);

                return StatusCode(StatusCodes.Status201Created, new CreateWorkflowResponse(workflowId));
            }
            catch (Exception e)
            {
                return Problem($"Unexpected error occured: {e.Message}", $"/workflows", InternalServerError);
            }
        }

        /// <summary>
        /// Updates a workflow and creates a new revision.
        /// </summary>
        /// <param name="workflow">The Workflow.</param>
        /// <param name="id">The id of the workflow.</param>
        /// <returns>The ID of the created Workflow.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateWorkflowResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromBody] Workflow workflow, [FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                _logger.LogDebug($"{nameof(UpdateAsync)} - Failed to validate {nameof(id)}");

                return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"/workflows/{id}", BadRequest);
            }

            if (WorkflowValidator.ValidateWorkflow(workflow, out var results))
            {
                var validationErrors = string.Join(", ", results.Errors);
                _logger.LogDebug($"{nameof(UpdateAsync)} - Failed to validate {nameof(workflow)}: {validationErrors}");

                return Problem($"Failed to validate {nameof(workflow)}: {string.Join(", ", validationErrors)}", $"/workflows/{id}", BadRequest);
            }

            try
            {
                var workflowId = await _workflowService.UpdateAsync(workflow, id);

                if (workflowId == null)
                {
                    _logger.LogDebug($"{nameof(UpdateAsync)} - Failed to find workflow with Id: {id}");

                    return NotFound($"Failed to find workflow with Id: {id}");
                }

                return StatusCode(StatusCodes.Status201Created, new CreateWorkflowResponse(workflowId));
            }
            catch (Exception e)
            {
                return Problem($"Unexpected error occured: {e.Message}", $"/workflows", InternalServerError);
            }
        }

        /// <summary>
        /// Soft deletes a workflow by the ID.
        /// </summary>
        /// <param name="id">The Workflow Id.</param>
        /// <returns>The specified workflow for a given Id.</returns>
        [Route("{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                _logger.LogDebug($"{nameof(GetAsync)} - Failed to validate {nameof(id)}");

                return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"/workflows/{id}", BadRequest);
            }

            try
            {
                var workflow = await _workflowService.GetAsync(id);
                if (workflow == null || workflow.IsDeleted)
                {
                    return Problem($"Failed to validate {nameof(id)}, workflow not found", $"/workflows/{id}", NotFound);
                }

                workflow.Deleted = DateTime.UtcNow;

                var workflowId = await _workflowService.DeleteWorkflowAsync(workflow);

                return Ok(workflow);
            }
            catch (Exception e)
            {
                return Problem(
                    $"Unexpected error occured: {e.Message}",
                    $"/workflows/{nameof(id)}",
                    InternalServerError);
            }
        }
    }
}

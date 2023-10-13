/*
 * Copyright 2023 MONAI Consortium
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
using Monai.Deploy.WorkflowManager.Common.Contracts.Responses;
using Monai.Deploy.WorkflowManager.Common.Logging;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Exceptions;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Filter;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;
using Monai.Deploy.WorkflowManager.Common.Validators;

namespace Monai.Deploy.WorkflowManager.Common.ControllersShared
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
        private readonly WorkflowValidator _workflowValidator;
        private readonly ILogger<WorkflowsController> _logger;
        private readonly IUriService _uriService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowsController"/> class.
        /// </summary>
        /// <param name="workflowService">IWorkflowService.</param>
        /// <param name="workflowValidator">WorkflowValidator.</param>
        /// <param name="logger">ILogger.WorkflowsController.</param>
        /// <param name="uriService">Uri Service.</param>
        /// <param name="options">Workflow Manager options.</param>
        /// <exception cref="ArgumentNullException">ArgumentNullException.</exception>
        public WorkflowsController(
            IWorkflowService workflowService,
            WorkflowValidator workflowValidator,
            ILogger<WorkflowsController> logger,
            IUriService uriService,
            IOptions<WorkflowManagerOptions> options)
            : base(options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _workflowValidator = workflowValidator ?? throw new ArgumentNullException(nameof(workflowValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uriService = uriService ?? throw new ArgumentNullException(nameof(uriService));
        }

        /// <summary>
        /// Gets a list of all workflows.
        /// </summary>
        /// <param name="filter">Pagination filter.</param>
        /// <returns>The ID of the created Workflow.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<List<WorkflowRevision>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
                var pagedReponse = CreatePagedResponse(pagedData.ToList(), validFilter, dataTotal, _uriService, route);

                return Ok(pagedReponse);
            }
            catch (Exception e)
            {
                _logger.WorkflowGetListError(e);
                return Problem($"Unexpected error occurred: {e.Message}", $"/workflows", InternalServerError);
            }
        }

        /// <summary>
        /// Get a workflow by the ID.
        /// </summary>
        /// <param name="id">The Workflow Id.</param>
        /// <returns>The specified workflow for a given Id.</returns>
        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(WorkflowRevision), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
                _logger.WorkflowGetAsyncError(id, e);
                return Problem($"Unexpected error occurred: {e.Message}", $"/workflows/{nameof(id)}", InternalServerError);
            }
        }

        /// <summary>
        /// Validates a workflow.
        /// </summary>
        /// <param name="request">The validate request object.</param>
        /// <returns>A 204 when the workflow is valid.</returns>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(CreateWorkflowResponse), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateAsync([FromBody] WorkflowUpdateRequest request)
        {
            var workflow = request.Workflow;
            _workflowValidator.OrignalName = request.OriginalWorkflowName;

            try
            {
                var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

                if (errors.Count > 0)
                {
                    var validationErrors = WorkflowValidator.ErrorsToString(errors);
                    _logger.LogDebug($"{nameof(ValidateAsync)} - Failed to validate {nameof(workflow)}: {validationErrors}");

                    return Problem($"Failed to validate {nameof(workflow)}: {string.Join(", ", validationErrors)}", $"/workflows/validate", BadRequest);
                }
            }
            catch (MonaiInternalServerException ex)
            {
                _logger.LogDebug($"{nameof(ValidateAsync)} - Internal server error while validating {nameof(workflow)}: {ex.InnerException}");
                return Problem($"Internal server error while validating {nameof(workflow)}", $"/workflows/validate", InternalServerError);
            }

            return StatusCode(StatusCodes.Status204NoContent);
        }

        /// <summary>
        /// Create a workflow.
        /// </summary>
        /// <param name="workflow">The Workflow.</param>
        /// <returns>The ID of the created Workflow.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CreateWorkflowResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync([FromBody] Workflow workflow)
        {
            try
            {
                var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

                if (errors.Count > 0)
                {
                    var validationErrors = WorkflowValidator.ErrorsToString(errors);
                    _logger.LogDebug($"{nameof(CreateAsync)} - Failed to validate {nameof(workflow)}: {validationErrors}");

                    return Problem($"Failed to validate {nameof(workflow)}: {string.Join(", ", validationErrors)}", $"/workflows", BadRequest);
                }
            }
            catch (MonaiInternalServerException ex)
            {
                _logger.LogDebug($"{nameof(CreateAsync)} - Internal server error while validating {nameof(workflow)}: {ex.InnerException}");
                return Problem($"Internal server error while validating {nameof(workflow)}", $"/workflows", InternalServerError);
            }

            try
            {
                var workflowId = await _workflowService.CreateAsync(workflow);

                return StatusCode(StatusCodes.Status201Created, new CreateWorkflowResponse(workflowId));
            }
            catch (Exception e)
            {
                _logger.WorkflowCreateAsyncError(e);
                return Problem($"Unexpected error occurred: {e.Message}", $"/workflows", InternalServerError);
            }
        }

        /// <summary>
        /// Updates a workflow and creates a new revision.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        /// <param name="id">The id of the workflow.</param>
        /// <returns>The ID of the created Workflow.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CreateWorkflowResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync([FromBody] WorkflowUpdateRequest request, [FromRoute] string id)
        {
            var workflow = request.Workflow;
            var originalName = request.OriginalWorkflowName;
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                _logger.LogDebug($"{nameof(UpdateAsync)} - Failed to validate {nameof(id)}");

                return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"/workflows/{id}", BadRequest);
            }

            _workflowValidator.OrignalName = originalName;

            try
            {
                var errors = await _workflowValidator.ValidateWorkflowAsync(workflow);

                if (errors.Count > 0)
                {
                    var validationErrors = WorkflowValidator.ErrorsToString(errors);
                    _logger.LogDebug($"{nameof(UpdateAsync)} - Failed to validate {nameof(workflow)}: {validationErrors}");

                    return Problem($"Failed to validate {nameof(workflow)}: {string.Join(", ", validationErrors)}", $"/workflows/{id}", BadRequest);
                }
            }
            catch (MonaiInternalServerException ex)
            {
                _logger.LogDebug($"{nameof(UpdateAsync)} - Internal server error while validating {nameof(workflow)}: {ex.InnerException}");
                return Problem($"Internal server error while validating {nameof(workflow)}", $"/workflows/{id}", InternalServerError);
            }

            try
            {
                var workflowId = await _workflowService.UpdateAsync(workflow, id, workflow.Name != originalName);

                if (workflowId == null)
                {
                    _logger.LogDebug($"{nameof(UpdateAsync)} - Failed to find workflow with Id: {id}");

                    return Problem($"Failed to find workflow with Id: {id}", $"/workflows/{id}", NotFound);
                }

                return StatusCode(StatusCodes.Status201Created, new CreateWorkflowResponse(workflowId));
            }
            catch (Exception e)
            {
                _logger.WorkflowUpdateAsyncError(id, e);
                return Problem($"Unexpected error occurred: {e.Message}", $"/workflows", InternalServerError);
            }
        }

        /// <summary>
        /// Soft deletes a workflow by the ID.
        /// </summary>
        /// <param name="id">The Workflow Id.</param>
        /// <returns>The specified workflow for a given Id.</returns>
        [Route("{id}")]
        [HttpDelete]
        [ProducesResponseType(typeof(WorkflowRevision), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
                _logger.WorkflowDeleteAsyncError(id, e);
                return Problem(
                    $"Unexpected error occurred: {e.Message}",
                    $"/workflows/{nameof(id)}",
                    InternalServerError);
            }
        }

        /// <summary>
        /// Get by AE Title.
        /// </summary>
        /// <param name="title">The AE title.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet("aetitle/{title}")]
        [ProducesResponseType(typeof(WorkflowRevision), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByAeTitle([FromRoute] string title, [FromQuery] PaginationFilter filter)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                _logger.LogDebug($"{nameof(GetByAeTitle)} - Failed to validate {nameof(title)}");

                return Problem($"Failed to validate {nameof(title)}, not a valid AE title", $"/workflows/aetitle/{title}", BadRequest);
            }

            try
            {
                var route = Request?.Path.Value ?? string.Empty;
                var pageSize = filter.PageSize ?? _options.Value.EndpointSettings.DefaultPageSize;
                var validFilter = new PaginationFilter(filter.PageNumber, pageSize, _options.Value.EndpointSettings.MaxPageSize);

                var pagedData = await _workflowService.GetByAeTitleAsync(
                    title,
                    (validFilter.PageNumber - 1) * validFilter.PageSize,
                    validFilter.PageSize);

                var dataTotal = await _workflowService.GetCountByAeTitleAsync(title);
                var pagedReponse = CreatePagedResponse(pagedData.ToList(), validFilter, dataTotal, _uriService, route);

                return Ok(pagedReponse);
            }
            catch (Exception e)
            {
                _logger.WorkflowGetAeTitleAsyncError(e);

                return Problem(
                    $"Unexpected error occurred: {e.Message}",
                    $"/workflows/aetitle",
                    InternalServerError);
            }
        }
    }
}

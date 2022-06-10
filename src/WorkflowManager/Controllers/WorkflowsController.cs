// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Contracts.Responses;
using Monai.Deploy.WorkflowManager.PayloadListener.Extensions;

namespace Monai.Deploy.WorkloadManager.Controllers;

/// <summary>
/// Workflows Controller
/// </summary>
[ApiController]
[Route("workflows")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;

    private readonly ILogger<WorkflowsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowsController"/> class.
    /// </summary>
    /// <param name="workflowService"></param>
    public WorkflowsController(IWorkflowService workflowService, ILogger<WorkflowsController> logger)
    {
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a list of all workflows.
    /// </summary>
    /// <returns>The ID of the created Workflow.</returns>
    [HttpGet]
    public IActionResult GetList()
    {
        try
        {
            var workflows = _workflowService.GetList();

            return Ok(workflows);
        }
        catch (Exception e)
        {
            return Problem($"Unexpected error occured: {e.Message}", $"/workflows", 500);
        }
    }

    /// <summary>
    /// Get a workflow by the ID.
    /// </summary>
    /// <param name="id">The Workflow Id</param>
    /// <returns>The specified workflow for a given Id.</returns>
    [Route("{id}")]
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromRoute] string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
        {
            _logger.LogDebug($"{nameof(GetAsync)} - Failed to validate {nameof(id)}");

            return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"/workflows/{id}", 400);
        }

        try
        {
            var workflow = await _workflowService.GetAsync(id);

            return Ok(workflow);
        }
        catch (Exception e)
        {
            return Problem($"Unexpected error occured: {e.Message}", $"/workflows/{nameof(id)}", 500);
        }
    }

    /// <summary>
    /// Create a workflow
    /// </summary>
    /// <param name="workflow">The Workflow.</param>
    /// <returns>The ID of the created Workflow.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] Workflow workflow)
    {
        if (!workflow.IsValid(out var validationErrors))
        {
            _logger.LogDebug($"{nameof(CreateAsync)} - Failed to validate {nameof(workflow)}: {validationErrors}");

            return Problem($"Failed to validate {nameof(workflow)}: {string.Join(", ", validationErrors)}", $"/workflows", 400);
        }

        try
        {
            var workflowId = await _workflowService.CreateAsync(workflow);

            return StatusCode(StatusCodes.Status201Created, new CreateWorkflowResponse(workflowId));
        }
        catch (Exception e)
        {
            return Problem($"Unexpected error occured: {e.Message}", $"/workflows", 500);
        }
    }

    /// <summary>
    /// Updates a workflow and creates a new revision
    /// </summary>
    /// <param name="workflow">The Workflow.</param>
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

            return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"/workflows/{id}", (int)HttpStatusCode.BadRequest);
        }

        if (!workflow.IsValid(out var validationErrors))
        {
            _logger.LogDebug($"{nameof(UpdateAsync)} - Failed to validate {nameof(workflow)}: {validationErrors}");

            return Problem($"Failed to validate {nameof(workflow)}: {string.Join(", ", validationErrors)}", $"/workflows/{id}", (int)HttpStatusCode.BadRequest);
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
            return Problem($"Unexpected error occured: {e.Message}", $"/workflows", (int)HttpStatusCode.InternalServerError);
        }
    }
}

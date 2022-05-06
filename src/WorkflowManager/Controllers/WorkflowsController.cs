// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Contracts.Responses;
using Monai.Deploy.WorkflowManager.PayloadListener.Extensions;

namespace Monai.Deploy.WorkloadManager.Controllers;

/// <summary>
/// Workflows Controller
/// </summary>
[ApiController]
[Route("[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowsController"/> class.
    /// </summary>
    /// <param name="workflowService"></param>
    public WorkflowsController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    /// <summary>
    /// Get a workflow by the ID
    /// </summary>
    /// <param name="id">The Workflow Id</param>
    /// <returns>The ID of the created Workflow.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromRoute] string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
        {
            return BadRequest();
        }

        var workflow = await _workflowService.GetAsync(id);

        return Ok(workflow);
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
            return BadRequest();
        }

        var workflowId = await _workflowService.CreateAsync(workflow);

        return StatusCode(StatusCodes.Status201Created, new CreateWorkflowResponse(workflowId));
    }
}

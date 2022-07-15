// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Controllers;

/// <summary>
/// Workflow Instances Controller.
/// </summary>
[ApiController]
[Route("workflowinstances")]
public class WorkflowInstanceController : ControllerBase
{
    private readonly IWorkflowInstanceService _workflowInstanceService;

    private readonly ILogger<WorkflowInstanceController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowInstanceController"/> class.
    /// </summary>
    /// <param name="workflowInstanceService"></param>
    public WorkflowInstanceController(
        IWorkflowInstanceService workflowInstanceService,
        ILogger<WorkflowInstanceController> logger)
    {
        _workflowInstanceService = workflowInstanceService ?? throw new ArgumentNullException(nameof(workflowInstanceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a list of workflowInstances.
    /// </summary>
    /// <param name="status">Workflow instance status filter.</param>
    /// <returns>A list of workflow instances.</returns>
    [HttpGet]
    public async Task<IActionResult> GetListAsync([FromQuery] string? status = null)
    {
        try
        {
            var workflowsInstances = await _workflowInstanceService.GetListAsync();
            if (!string.IsNullOrWhiteSpace(status))
            {
                workflowsInstances = workflowsInstances.Where(wf =>
                    wf.Status == Enum.Parse<Status>(status, true)).ToList();
            }

            return Ok(workflowsInstances);
        }
        catch (Exception e)
        {
            _logger.LogError($"{nameof(GetListAsync)} - Failed to get workflowInstances", e);

            return Problem($"Unexpected error occured: {e.Message}", $"/workflowinstances", (int)HttpStatusCode.InternalServerError);
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

            return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"/workflows/{id}", (int)HttpStatusCode.BadRequest);
        }

        try
        {
            var workflowInstance = await _workflowInstanceService.GetByIdAsync(id);

            if (workflowInstance is null)
            {
                _logger.LogDebug($"{nameof(GetByIdAsync)} - Failed to find workflow instance with Id: {id}");

                return NotFound($"Faild to find workflow instance with Id: {id}");
            }

            return Ok(workflowInstance);
        }
        catch (Exception e)
        {
            return Problem($"Unexpected error occured: {e.Message}", $"/workflowinstances/{nameof(id)}", (int)HttpStatusCode.InternalServerError);
        }
    }
}

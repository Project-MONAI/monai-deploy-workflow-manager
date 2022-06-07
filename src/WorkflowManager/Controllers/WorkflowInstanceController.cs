// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;

namespace Monai.Deploy.WorkloadManager.Controllers;

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
    /// <returns>A list of workflow instances.</returns>
    [HttpGet]
    public async Task<IActionResult> GetListAsync()
    {
        try
        {
            var workflowsInstances = await _workflowInstanceService.GetListAsync();

            return Ok(workflowsInstances);
        }
        catch (Exception e)
        {
            this._logger.LogError($"{nameof(GetListAsync)} - Failed to get workflowInstances", e);

            return Problem($"Unexpected error occured: {e.Message}", $"/workflowinstances", 500);
        }
    }
}

﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Filter;
using Monai.Deploy.WorkflowManager.Services;

namespace Monai.Deploy.WorkflowManager.Controllers;

/// <summary>
/// Workflow Instances Controller.
/// </summary>
[ApiController]
[Route("workflowinstances")]
public class WorkflowInstanceController : ApiControllerBase
{
    private readonly IWorkflowInstanceService _workflowInstanceService;

    private readonly ILogger<WorkflowInstanceController> _logger;
    private readonly IUriService _uriService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowInstanceController"/> class.
    /// </summary>
    /// <param name="workflowInstanceService">Workflow Instance Service.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="uriService">Uri Service.</param>
    public WorkflowInstanceController(
        IWorkflowInstanceService workflowInstanceService,
        ILogger<WorkflowInstanceController> logger,
        IUriService uriService)
    {
        _workflowInstanceService = workflowInstanceService ?? throw new ArgumentNullException(nameof(workflowInstanceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _uriService = uriService ?? throw new ArgumentNullException(nameof(uriService));
    }

    /// <summary>
    /// Get a list of workflowInstances.
    /// </summary>
    /// <param name="filter">Filters.</param>
    /// <returns>A list of workflow instances.</returns>
    [HttpGet]
    public async Task<IActionResult> GetListAsync([FromQuery] PaginationFilter filter)
    {
        try
        {
            var route = Request?.Path.Value ?? string.Empty;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            var pagedData = await _workflowInstanceService.GetAllAsync(
                (validFilter.PageNumber - 1) * validFilter.PageSize,
                validFilter.PageSize);

            var dataTotal = await _workflowInstanceService.CountAsync();
            var pagedReponse = CreatePagedReponse(pagedData.ToList(), validFilter, dataTotal, _uriService, route);

            return Ok(pagedReponse);
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

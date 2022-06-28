// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;

namespace Monai.Deploy.WorkflowManager.Controllers;

/// <summary>
/// Payloads Controller.
/// </summary>
[ApiController]
[Route("payload")]
public class PayloadController : ControllerBase
{
    private readonly IPayloadService _payloadService;

    private readonly ILogger<PayloadController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PayloadController"/> class.
    /// </summary>
    /// <param name="payloadService">paylod service to retrieve payloads.</param>
    /// <param name="logger">logger.</param>
    public PayloadController(IPayloadService payloadService, ILogger<PayloadController> logger)
    {
        _payloadService = payloadService ?? throw new ArgumentNullException(nameof(payloadService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a list of all workflows.
    /// </summary>
    /// <returns>The ID of the created Workflow.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        try
        {
            var payloads = await _payloadService.GetAllAsync();

            return Ok(payloads);
        }
        catch (Exception e)
        {
            return Problem($"Unexpected error occured: {e.Message}", $"/payload", 500);
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

            return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"/payload/{id}", 400);
        }

        try
        {
            var payload = await _payloadService.GetByIdAsync(id);

            if (payload is null)
            {
                _logger.LogDebug($"{nameof(GetAsync)} - Failed to find payload with payload id: {id}");

                return NotFound($"Faild to find payload with payload id: {id}");
            }

            return Ok(payload);
        }
        catch (Exception e)
        {
            return Problem($"Unexpected error occured: {e.Message}", $"/payload/{nameof(id)}", 500);
        }
    }
}

// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Filter;
using Monai.Deploy.WorkflowManager.Services;

namespace Monai.Deploy.WorkflowManager.Controllers;

/// <summary>
/// Payloads Controller.
/// </summary>
[ApiController]
[Route("payload")]
public class PayloadController : ApiControllerBase
{
    private readonly IOptions<WorkflowManagerOptions> _options;
    private readonly IPayloadService _payloadService;

    private readonly ILogger<PayloadController> _logger;
    private readonly IUriService _uriService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PayloadController"/> class.
    /// </summary>
    /// <param name="payloadService">paylod service to retrieve payloads.</param>
    /// <param name="logger">logger.</param>
    /// <param name="uriService">Uri Service.</param>
    public PayloadController(IPayloadService payloadService,
                             ILogger<PayloadController> logger,
                             IUriService uriService,
                             IOptions<WorkflowManagerOptions> options)
                            : base(options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _payloadService = payloadService ?? throw new ArgumentNullException(nameof(payloadService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _uriService = uriService ?? throw new ArgumentNullException(nameof(uriService));
    }

    /// <summary>
    /// Gets a paged response list of all workflows.
    /// </summary>
    /// <param name="filter">Filters.</param>
    /// <param name="patientId">Optional paient Id.</param>
    /// <param name="patientName">Optional patient name.</param>
    /// <returns>paged response of subset of all workflows.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] PaginationFilter filter, [FromQuery] string patientId = "", [FromQuery] string patientName = "")
    {
        try
        {
            var route = Request?.Path.Value ?? string.Empty;
            var pageSize = filter.PageSize ?? _options.Value.EndpointSettings.DefaultPageSize;
            var validFilter = new PaginationFilter(filter.PageNumber, pageSize, _options.Value.EndpointSettings.MaxPageSize);

            var pagedData = await _payloadService.GetAllAsync(
                (validFilter.PageNumber - 1) * validFilter.PageSize,
                validFilter.PageSize, 
                patientId, 
                patientName);

            var dataTotal = await _payloadService.CountAsync();
            var pagedReponse = CreatePagedReponse(pagedData.ToList(), validFilter, dataTotal, _uriService, route);

            return Ok(pagedReponse);
        }
        catch (Exception e)
        {
            return Problem($"Unexpected error occured: {e.Message}", $"/payload", InternalServerError);
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

            return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"/payload/{id}", BadRequest);
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
            return Problem($"Unexpected error occured: {e.Message}", $"/payload/{nameof(id)}", InternalServerError);
        }
    }
}

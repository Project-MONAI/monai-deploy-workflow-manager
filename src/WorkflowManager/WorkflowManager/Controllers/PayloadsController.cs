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
using Monai.Deploy.WorkflowManager.Common.Logging;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Exceptions;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Filter;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Common.ControllersShared
{
    /// <summary>
    /// Payloads Controller.
    /// </summary>
    [ApiController]
    [Route("payload")]
    public class PayloadsController : AuthenticatedApiControllerBase
    {
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IPayloadService _payloadService;

        private readonly ILogger<PayloadsController> _logger;
        private readonly IUriService _uriService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadsController"/> class.
        /// </summary>
        /// <param name="payloadService">payload service to retrieve payloads.</param>
        /// <param name="logger">logger.</param>
        /// <param name="uriService">Uri Service.</param>
        /// <param name="options">Workflow Manager options.</param>
        public PayloadsController(
            IPayloadService payloadService,
            ILogger<PayloadsController> logger,
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
        /// <param name="patientId">Optional patient Id.</param>
        /// <param name="patientName">Optional patient name.</param>
        /// <param name="accessionId">Optional accession Id.</param>
        /// <returns>paged response of subset of all workflows.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<List<Payload>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationFilter filter,
            [FromQuery] string patientId = null,
            [FromQuery] string patientName = null,
            [FromQuery] string accessionId = null)
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
                    patientName,
                    accessionId);

                var builder = Builders<Payload>.Filter;
                var dbFilter = builder.Empty;

                if (!string.IsNullOrEmpty(patientId))
                {
                    dbFilter &= builder.Regex(p => p.PatientDetails.PatientId, new BsonRegularExpression($"/{patientId}/i"));
                }

                if (!string.IsNullOrEmpty(patientName))
                {
                    dbFilter &= builder.Regex(p => p.PatientDetails.PatientName, new BsonRegularExpression($"/{patientName}/i"));
                }

                if (!string.IsNullOrWhiteSpace(accessionId))
                {
                    dbFilter &= builder.Regex(p => p.AccessionId, new BsonRegularExpression($"/{accessionId}/i"));
                }

                var dataTotal = await _payloadService.CountAsync(dbFilter);
                var pagedReponse = CreatePagedResponse(pagedData.ToList(), validFilter, dataTotal, _uriService, route);

                return Ok(pagedReponse);
            }
            catch (Exception e)
            {
                _logger.PayloadGetAllAsyncError(e);
                return Problem($"Unexpected error occurred: {e.Message}", $"/payload", InternalServerError);
            }
        }

        /// <summary>
        /// Get a payload by the ID.
        /// </summary>
        /// <param name="id">The payload Id.</param>
        /// <returns>The specified payload for a given Id.</returns>
        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(Payload), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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

                    return Problem($"Failed to find payload with payload id: {id}", $"/payload/{id}", NotFound);
                }

                return Ok(payload);
            }
            catch (Exception e)
            {
                _logger.PayloadGetAsyncError(id, e);
                return Problem($"Unexpected error occurred: {e.Message}", $"/payload/{nameof(id)}", InternalServerError);
            }
        }

        /// <summary>
        /// Delete a payload by ID.
        /// </summary>
        /// <param name="id">The payload id.</param>
        /// <returns>Boolean status of the success of the delete request.</returns>
        [Route("{id}")]
        [HttpDelete]
        [ProducesResponseType(typeof(bool), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                _logger.LogDebug($"{nameof(DeleteAsync)} - Failed to validate {nameof(id)}");

                return Problem($"Failed to validate {nameof(id)}, not a valid guid", $"/payload/{id}", BadRequest);
            }

            try
            {
                return Accepted(await _payloadService.DeletePayloadFromStorageAsync(id));
            }
            catch (MonaiBadRequestException e)
            {
                _logger.PayloadGetAsyncError(id, e);
                return Problem(e.Message, $"/payload/{nameof(id)}", BadRequest);
            }
            catch (MonaiNotFoundException e)
            {
                _logger.PayloadGetAsyncError(id, e);
                return Problem(e.Message, $"/payload/{nameof(id)}", NotFound);
            }
            catch (Exception e)
            {
                _logger.PayloadGetAsyncError(id, e);
                return Problem($"Unexpected error occurred: {e.Message}", $"/payload/{nameof(id)}", InternalServerError);
            }
        }
    }
}

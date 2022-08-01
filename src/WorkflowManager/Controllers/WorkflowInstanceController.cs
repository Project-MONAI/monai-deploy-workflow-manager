/*
 * Copyright 2022 MONAI Consortium
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
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Filter;
using Monai.Deploy.WorkflowManager.Services;

namespace Monai.Deploy.WorkflowManager.Controllers
{
    /// <summary>
    /// Workflow Instances Controller.
    /// </summary>
    [ApiController]
    [Route("workflowinstances")]
    public class WorkflowInstanceController : ApiControllerBase
    {
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IWorkflowInstanceService _workflowInstanceService;

        private readonly ILogger<WorkflowInstanceController> _logger;
        private readonly IUriService _uriService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowInstanceController"/> class.
        /// </summary>
        /// <param name="workflowInstanceService">Workflow Instance Service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="uriService">Uri Service.</param>
        /// <param name="options">Workflow Manager options.</param>
        public WorkflowInstanceController(
            IWorkflowInstanceService workflowInstanceService,
            ILogger<WorkflowInstanceController> logger,
            IUriService uriService,
            IOptions<WorkflowManagerOptions> options)
            : base(options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _workflowInstanceService = workflowInstanceService ?? throw new ArgumentNullException(nameof(workflowInstanceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uriService = uriService ?? throw new ArgumentNullException(nameof(uriService));
        }

        /// <summary>
        /// Get a list of workflowInstances.
        /// </summary>
        /// <param name="filter">Filters.</param>
        /// <param name="status">Workflow instance status filter.</param>
        /// <returns>A list of workflow instances.</returns>
        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] PaginationFilter filter, [FromQuery] string status = null)
        {
            try
            {
                var route = Request?.Path.Value ?? string.Empty;
                var pageSize = filter.PageSize ?? _options.Value.EndpointSettings.DefaultPageSize;
                Status? parsedStatus = status == null ? null : Enum.Parse<Status>(status, true);
                var validFilter = new PaginationFilter(filter.PageNumber, pageSize, _options.Value.EndpointSettings.MaxPageSize);

                var pagedData = await _workflowInstanceService.GetAllAsync(
                    (validFilter.PageNumber - 1) * validFilter.PageSize,
                    validFilter.PageSize,
                    parsedStatus);

                var dataTotal = await _workflowInstanceService.CountAsync();
                var pagedReponse = CreatePagedReponse<WorkflowInstance>(pagedData.ToList(), validFilter, dataTotal, _uriService, route);

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
}

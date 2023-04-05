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

using IdentityModel.OidcClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.ControllersShared;
using Monai.Deploy.WorkflowManager.Shared.Filter;
using Monai.Deploy.WorkflowManager.Shared.Services;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Monai.Deploy.WorkflowManager.TaskManager.Database;
using Monai.Deploy.WorkflowManager.TaskManager.Filter;
using MongoDB.Driver.Core.Clusters;

namespace Monai.Deploy.WorkflowManager.TaskManager.Controllers
{
    /// <summary>
    /// Execution stats endpoint.
    /// </summary>
    [ApiController]
    [Route("tasks")]
    public class TaskStatsController : ApiControllerBase
    {
        private readonly ILogger<TaskStatsController> _logger;
        private readonly IUriService _uriService;
        private readonly ITaskExecutionStatsRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskStatsController"/> class. for retreiving execution stats.
        /// </summary>
        /// <param name="options">The options set, in this case for the pagination settings.</param>
        /// <param name="uriService">DI service for uri manipulation.</param>
        /// <param name="logger">err, the logger.</param>
        /// <param name="repository">the repository used to store the execution stats.</param>
        /// <exception cref="ArgumentNullException">thrown if required arguments are null.</exception>
        public TaskStatsController(
            IOptions<WorkflowManagerOptions> options,
            IUriService uriService,
            ILogger<TaskStatsController> logger,
            ITaskExecutionStatsRepository repository)
            : base(options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uriService = uriService ?? throw new ArgumentNullException(nameof(uriService));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet("statsoverview")]
        public async Task<IActionResult> GetOverviewAsync([FromQuery] DateTime startTime, DateTime endTime)
        {
            if (endTime == default)
            {
                endTime = DateTime.Now;
            }

            if (startTime == default)
            {
                startTime = new DateTime(2023, 1, 1);
            }

            return Ok(new
            {
                PeriodStart = startTime,
                PeriodEnd = endTime,
                TotalExecutions = 99,
                TotalFailures = 2,
            });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStatsAsync([FromQuery] TimeFilter filter)
        {

            if (filter.EndTime == default)
            {
                filter.EndTime = DateTime.Now;
            }

            if (filter.StartTime == default)
            {
                filter.StartTime = new DateTime(2023, 1, 1);
            }

            var route = Request?.Path.Value ?? string.Empty;
            var pageSize = filter.PageSize ?? Options.Value.EndpointSettings?.DefaultPageSize ?? 10;
            var max = Options.Value.EndpointSettings?.MaxPageSize ?? 20;
            var validFilter = new PaginationFilter(filter.PageNumber, pageSize, max);

            var allStats = _repository.GetStatsAsync(filter.StartTime, filter.EndTime, pageSize, filter.PageNumber);
            var fails = _repository.GetStatsStatusNotEqualCountAsync(filter.StartTime, filter.EndTime, Messaging.Events.TaskExecutionStatus.Succeeded);
            var rangeCount = _repository.GetStatsCountAsync(filter.StartTime, filter.EndTime);
            await Task.WhenAll(allStats, fails, rangeCount);

            var statsDto = allStats.Result.OrderBy(a => a.Started).Select(s => new ExecutionStatDTO(s)).ToArray();

            var res = CreateStatsPagedReponse(statsDto, validFilter, rangeCount.Result, _uriService, route);

            res.PeriodStart = filter.StartTime;
            res.PeriodEnd = filter.EndTime;
            res.TotalExecutions = (int)rangeCount.Result;
            res.TotalFailures = (int)fails.Result;
            res.AverageTotalExecutionSeconds = 998;
            res.AverageArgoExecutionSeconds = 99;
            return Ok(res);
        }
    }
}

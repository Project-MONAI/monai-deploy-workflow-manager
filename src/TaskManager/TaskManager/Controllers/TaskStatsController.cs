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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.ControllersShared;
using Monai.Deploy.WorkflowManager.Shared.Filter;
using Monai.Deploy.WorkflowManager.Shared.Services;
using Monai.Deploy.WorkflowManager.Shared.Wrappers;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Monai.Deploy.WorkflowManager.TaskManager.Database;
using Monai.Deploy.WorkflowManager.TaskManager.Filter;
using Monai.Deploy.WorkflowManager.TaskManager.Logging;

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

        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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

            try
            {
                var fails = _repository.GetStatsStatusFailedCountAsync(startTime, endTime);
                var rangeCount = _repository.GetStatsCountAsync(startTime, endTime);
                var stats = _repository.GetAverageStats(startTime, endTime);

                await Task.WhenAll(fails, rangeCount, stats);
                return Ok(new
                {
                    PeriodStart = startTime,
                    PeriodEnd = endTime,
                    TotalExecutions = (int)rangeCount.Result,
                    TotalFailures = (int)fails.Result,
                    AverageTotalExecutionSeconds = Math.Round(stats.Result.avgTotalExecution, 2),
                    AverageArgoExecutionSeconds = Math.Round(stats.Result.avgArgoExecution, 2),
                });
            }
            catch (Exception e)
            {
                _logger.GetStatsOverviewAsyncError(e);
                return Problem($"Unexpected error occurred: {e.Message}", $"tasks/statsoverview", InternalServerError);
            }
        }

        [ProducesResponseType(typeof(StatsPagedResponse<List<ExecutionStatDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpGet("stats")]
        public async Task<IActionResult> GetStatsAsync([FromQuery] TimeFilter filter, string workflowId, string taskId)
        {

            if ((string.IsNullOrWhiteSpace(workflowId) && string.IsNullOrWhiteSpace(taskId)) is false
              && (string.IsNullOrWhiteSpace(workflowId) || string.IsNullOrWhiteSpace(taskId)))
            {
                // both not empty but one is !
                _logger.LogDebug($"{nameof(GetStatsAsync)} - Failed to validate WorkflowId or TaskId");
                return Problem($"Failed to validate ids, not a valid guid", $"tasks/stats/", BadRequest);
            }

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

            try
            {
                var allStats = _repository.GetStatsAsync(filter.StartTime, filter.EndTime, pageSize, filter.PageNumber, workflowId, taskId);
                var fails = _repository.GetStatsStatusFailedCountAsync(filter.StartTime, filter.EndTime, workflowId, taskId);
                var rangeCount = _repository.GetStatsCountAsync(filter.StartTime, filter.EndTime, workflowId, taskId);
                var stats = _repository.GetAverageStats(filter.StartTime, filter.EndTime, workflowId, taskId);

                await Task.WhenAll(allStats, fails, rangeCount, stats);

                ExecutionStatDTO[] statsDto;

                statsDto = allStats.Result
                    .OrderBy(a => a.StartedUTC)
                    .Select(s => new ExecutionStatDTO(s))
                    .ToArray();

                var res = CreateStatsPagedReponse(statsDto, validFilter, rangeCount.Result, _uriService, route);

                res.PeriodStart = filter.StartTime;
                res.PeriodEnd = filter.EndTime;
                res.TotalExecutions = rangeCount.Result;
                res.TotalFailures = fails.Result;
                res.AverageTotalExecutionSeconds = Math.Round(stats.Result.avgTotalExecution, 2);
                res.AverageArgoExecutionSeconds = Math.Round(stats.Result.avgArgoExecution, 2);
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.GetStatsAsyncError(e);
                return Problem($"Unexpected error occurred: {e.Message}", $"tasks/stats", InternalServerError);
            }

        }
    }
}

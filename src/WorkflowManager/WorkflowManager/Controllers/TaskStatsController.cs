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
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database;
using Monai.Deploy.WorkflowManager.Common.Logging;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Filter;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;

namespace Monai.Deploy.WorkflowManager.Common.ControllersShared
{
    /// <summary>
    /// Execution stats endpoint.
    /// </summary>
    [ApiController]
    [Route("tasks")]
    public class TaskStatsController : PaginationApiControllerBase
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

        /// <summary>
        /// Get an overview of execution stats for a given period.
        /// </summary>
        /// <param name="startTime">start time for stats.</param>
        /// <param name="endTime">endtime for stats.</param>
        /// <returns>an object with the execution stats.</returns>
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
                var successes = _repository.GetStatsStatusSucceededCountAsync(startTime, endTime);
                var fails = _repository.GetStatsStatusFailedCountAsync(startTime, endTime);
                var running = _repository.GetStatsStatusCountAsync(startTime, endTime, TaskExecutionStatus.Accepted.ToString());
                var rangeCount = _repository.GetStatsStatusCountAsync(startTime, endTime);
                var stats = _repository.GetAverageStats(startTime, endTime);

                await Task.WhenAll(fails, rangeCount, stats, running);
                return Ok(new
                {
                    PeriodStart = startTime,
                    PeriodEnd = endTime,
                    TotalExecutions = (int)rangeCount.Result,
                    TotalSucceeded = (int)successes.Result,
                    TotalFailures = (int)fails.Result,
                    TotalInprogress = running.Result,
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

        /// <summary>
        /// Get execution daily stats for a given time period.
        /// </summary>
        /// <param name="filter">TimeFiler defining start and end times, plus paging options.</param>
        /// <param name="workflowId">WorkflowId if you want stats just for a given workflow. (both workflowId and TaskId must be given, if you give one).</param>
        /// <returns>a paged obect with all the stat details.</returns>
        [ProducesResponseType(typeof(StatsPagedResponse<List<ExecutionStatDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpGet("dailystats")]
        public async Task<IActionResult> GetDailyStatsAsync([FromQuery] TimeFilter filter, string workflowId = "")
        {
            SetUpFilter(filter, out var route, out var pageSize, out var validFilter);

            try
            {
                var allStats = await _repository.GetAllStatsAsync(filter.StartTime, filter.EndTime, workflowId, string.Empty);
                var statsDto = allStats
                    .OrderBy(a => a.StartedUTC)
                    .GroupBy(s => s.StartedUTC.Date)
                    .Select(g => new ExecutionStatDayOverview
                    {
                        Date = DateOnly.FromDateTime(g.Key.Date),
                        TotalExecutions = g.Count(),
                        TotalFailures = g.Count(i => string.Compare(i.Status, "Failed", true) == 0 && i.Reason != FailureReason.TimedOut && i.Reason != FailureReason.Rejected),
                        TotalApprovals = g.Count(i => string.Compare(i.Status, "Succeeded", true) == 0 && i.Reason == FailureReason.None),
                        TotalRejections = g.Count(i => string.Compare(i.Status, "Failed", true) == 0 && i.Reason == FailureReason.Rejected),
                        TotalCancelled = g.Count(i => string.Compare(i.Status, "Failed", true) == 0 && i.Reason == FailureReason.TimedOut),

                        // TotalAwaitingReview = g.Count(i => string.Compare(i.Status, ApplicationReviewStatus.AwaitingReview.ToString(), true) == 0),
                        TotalAwaitingReview = g.Count(i => string.Compare(i.Status, TaskExecutionStatus.Accepted.ToString(), true) == 0),
                    });

                var pagedStats = statsDto.Skip((filter.PageNumber - 1) * pageSize).Take(pageSize);

                var res = CreateStatsPagedResponse(pagedStats, validFilter, statsDto.Count(), _uriService, route);
                var (avgTotalExecution, avgArgoExecution) = await _repository.GetAverageStats(filter.StartTime, filter.EndTime, workflowId, string.Empty);

                res.PeriodStart = filter.StartTime;
                res.PeriodEnd = filter.EndTime;
                res.TotalExecutions = allStats.Count();
                res.TotalSucceeded = statsDto.Sum(s => s.TotalApprovals);
                res.TotalFailures = statsDto.Sum(s => s.TotalFailures + s.TotalCancelled + s.TotalRejections);
                res.TotalInprogress = statsDto.Sum(s => s.TotalAwaitingReview);
                res.AverageTotalExecutionSeconds = Math.Round(avgTotalExecution, 2);
                res.AverageArgoExecutionSeconds = Math.Round(avgArgoExecution, 2);

                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.GetStatsAsyncError(e);
                return Problem($"Unexpected error occurred: {e.Message}", $"tasks/stats", InternalServerError);
            }
        }

        /// <summary>
        /// Get execution stats for a given time period.
        /// </summary>
        /// <param name="filter">TimeFiler defining start and end times, plus paging options.</param>
        /// <param name="workflowId">WorkflowId if you want stats just for a given workflow. (both workflowId and TaskId must be given, if you give one).</param>
        /// <param name="taskId">TaskId if you want stats for a given taskId.(both workflowId and TaskId must be given, if you give one).</param>
        /// <returns>a paged obect with all the stat details.</returns>
        [ProducesResponseType(typeof(StatsPagedResponse<List<ExecutionStatDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [HttpGet("stats")]
        public async Task<IActionResult> GetStatsAsync([FromQuery] TimeFilter filter, string workflowId = "", string taskId = "")
        {
            if ((string.IsNullOrWhiteSpace(workflowId) && string.IsNullOrWhiteSpace(taskId)) is false
              && (string.IsNullOrWhiteSpace(workflowId) || string.IsNullOrWhiteSpace(taskId)))
            {
                // both not empty but one is !
                _logger.LogDebug($"{nameof(GetStatsAsync)} - Failed to validate WorkflowId or TaskId");
                return Problem("Failed to validate ids, not a valid guid", "tasks/stats/", BadRequest);
            }

            SetUpFilter(filter, out var route, out var pageSize, out var validFilter);

            try
            {
                var allStats = await _repository.GetStatsAsync(filter.StartTime, filter.EndTime, pageSize, filter.PageNumber, workflowId, taskId);
                var statsDto = allStats
                    .OrderBy(a => a.StartedUTC)
                    .Select(s => new ExecutionStatDTO(s));

                var res = await GatherPagedStats(filter, workflowId, taskId, route, validFilter, statsDto);
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.GetStatsAsyncError(e);
                return Problem($"Unexpected error occurred: {e.Message}", $"tasks/stats", InternalServerError);
            }
        }

        private async Task<StatsPagedResponse<IEnumerable<T>>> GatherPagedStats<T>(TimeFilter filter, string workflowId, string taskId, string route, PaginationFilter validFilter, IEnumerable<T> statsDto)
        {
            workflowId ??= string.Empty;
            taskId ??= string.Empty;

            var successes = _repository.GetStatsStatusSucceededCountAsync(filter.StartTime, filter.EndTime, workflowId, taskId);

            var fails = _repository.GetStatsStatusFailedCountAsync(filter.StartTime, filter.EndTime, workflowId, taskId);

            var rangeCount = _repository.GetStatsTotalCompleteExecutionsCountAsync(filter.StartTime, filter.EndTime, workflowId, taskId);

            var stats = _repository.GetAverageStats(filter.StartTime, filter.EndTime, workflowId, taskId);

            var running = _repository.GetStatsStatusCountAsync(filter.StartTime, filter.EndTime, TaskExecutionStatus.Accepted.ToString(), workflowId, taskId);

            await Task.WhenAll(fails, rangeCount, stats, running);

            var res = CreateStatsPagedResponse(statsDto, validFilter, rangeCount.Result, _uriService, route);

            res.PeriodStart = filter.StartTime;
            res.PeriodEnd = filter.EndTime;
            res.TotalExecutions = rangeCount.Result;
            res.TotalSucceeded = successes.Result;
            res.TotalFailures = fails.Result;
            res.TotalInprogress = running.Result;
            res.AverageTotalExecutionSeconds = Math.Round(stats.Result.avgTotalExecution, 2);
            res.AverageArgoExecutionSeconds = Math.Round(stats.Result.avgArgoExecution, 2);
            return res;
        }

        private void SetUpFilter(TimeFilter filter, out string route, out int pageSize, out PaginationFilter validFilter)
        {
            if (filter.EndTime == default)
            {
                filter.EndTime = DateTime.Now;
            }

            if (filter.StartTime == default)
            {
                filter.StartTime = new DateTime(2023, 1, 1);
            }

            route = Request?.Path.Value ?? string.Empty;
            pageSize = filter.PageSize ?? Options.Value.EndpointSettings?.DefaultPageSize ?? 10;
            var max = Options.Value.EndpointSettings?.MaxPageSize ?? 20;
            validFilter = new PaginationFilter(filter.PageNumber, pageSize, max);
        }
    }
}

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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.Database
{
    public interface ITaskExecutionStatsRepository
    {
        /// <summary>
        /// Creates a task dispatch event in the database.
        /// </summary>
        /// <param name="taskExecutionInfo"></param>
        /// <param name="workflowId">workflow id.</param>
        /// <param name="correlationId">task id.</param>
        /// <returns></returns>
        Task CreateAsync(TaskExecution taskExecutionInfo, string workflowId, string correlationId);

        /// <summary>
        /// Updates status of a task dispatch event in the database.
        /// </summary>
        /// <param name="taskUpdateEvent"></param>
        /// <param name="workflowId">workflow id.</param>
        /// <param name="status">task id.</param>
        /// <returns></returns>
        Task UpdateExecutionStatsAsync(TaskExecution taskUpdateEvent, string workflowId, TaskExecutionStatus? status = null);

        /// <summary>
        /// Updates status of a task now its been canceled.
        /// </summary>
        /// <param name="taskCanceledEvent">A TaskCanceledException to update.</param>
        /// <param name="workflowId">workflow id.</param>
        /// <param name="correlationId">task id.</param>
        /// <returns></returns>
        Task UpdateExecutionStatsAsync(TaskCancellationEvent taskCanceledEvent, string workflowId, string correlationId);

        /// <summary>
        /// Returns all entries between the two given dates
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <param name="workflowId">optional workflow id.</param>
        /// <param name="taskId">optional task id.</param>
        /// <returns>a collections of stats</returns>
        Task<IEnumerable<ExecutionStats>> GetAllStatsAsync(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "");
        /// <summary>
        /// Returns paged entries between the two given dates
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="workflowId">optional workflow id.</param>
        /// <param name="taskId">optional task id.</param>
        /// <returns>a collections of stats</returns>
        Task<IEnumerable<ExecutionStats>> GetStatsAsync(DateTime startTime, DateTime endTime, int? pageSize = 10, int? pageNumber = 1, string workflowId = "", string taskId = "");

        /// <summary>
        /// Return the count of the entries with this status, or all if no status given.
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <param name="status">the status to get count of, or string.empty</param>
        /// <param name="workflowId">optional workflow id.</param>
        /// <param name="taskId">optional task id.</param>
        /// <returns>The count of all records in range</returns>
        Task<long> GetStatsStatusCountAsync(DateTime startTime, DateTime endTime, string status = "", string workflowId = "", string taskId = "");

        /// <summary>
        /// Returns all stats in Succeeded status.
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <param name="workflowId">optional workflow id.</param>
        /// <param name="taskId">optional task id.</param>
        /// <returns>All stats that succeeded</returns>
        Task<long> GetStatsStatusSucceededCountAsync(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "");

        /// <summary>
        /// Returns all stats in Failed or PartialFail status.
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <param name="workflowId">optional workflow id.</param>
        /// <param name="taskId">optional task id.</param>
        /// <returns>All stats that failed or partially failed</returns>
        Task<long> GetStatsStatusFailedCountAsync(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "");

        /// <summary>
        /// Returns total ran executions status that have ran to completion. (not dispatched, created, accepted)
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <param name="workflowId">optional workflow id.</param>
        /// <param name="taskId">optional task id.</param>
        /// <returns>All stats that failed or partially failed</returns>
        Task<long> GetStatsTotalCompleteExecutionsCountAsync(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "");


        /// <summary>
        /// Calculates the average execution time for the given range
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <param name="workflowId">optional workflow id.</param>
        /// <param name="taskId">optional task id.</param>
        /// <returns>the average execution times in the time range</returns>
        Task<(double avgTotalExecution, double avgArgoExecution)> GetAverageStats(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "");


        /// <summary>
        /// Return the total number of stats between the dates with optional status filter.
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <param name="statusFilter"></param>
        /// <param name="workflowId">optional workflow id.</param>
        /// <param name="taskId">optional task id.</param>
        /// <returns>The count of all records in range</returns>
        /// <summary>
        Task<long> GetStatsCountAsync(DateTime startTime, DateTime endTime, Expression<Func<ExecutionStats, bool>>? statusFilter = null, string workflowId = "", string taskId = "");
    }
}

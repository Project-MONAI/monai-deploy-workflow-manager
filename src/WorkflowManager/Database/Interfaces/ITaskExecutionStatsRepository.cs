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
using System.Threading.Tasks;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Database
{
    public interface ITaskExecutionStatsRepository
    {
        /// <summary>
        /// Creates a task dispatch event in the database.
        /// </summary>
        /// <param name="taskDispatchEvent">A TaskDispatchEvent to create.</param>
        /// <returns></returns>
        Task CreateAsync(TaskExecution TaskExecutionInfo, string workflowId, string correlationId);

        /// <summary>
        /// Updates status of a task dispatch event in the database.
        /// </summary>
        /// <param name="taskDispatchEvent">A TaskDispatchEvent to update.</param>
        /// <returns></returns>
        Task UpdateExecutionStatsAsync(TaskExecution taskUpdateEvent, string workflowId, TaskExecutionStatus? status = null);

        /// <summary>
        /// Updates status of a task now its been canceled.
        /// </summary>
        /// <param name="TaskCanceledException">A TaskCanceledException to update.</param>
        /// <returns></returns
        Task UpdateExecutionStatsAsync(TaskCancellationEvent taskCanceledEvent, string workflowId, string correlationId);

        /// <summary>
        /// Returns paged entries between the two given dates.
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <returns>a collections of stats</returns>
        Task<IEnumerable<ExecutionStats>> GetStatsAsync(DateTime startTime, DateTime endTime, int PageSize = 10, int PageNumber = 1, string workflowId = "", string taskId = "");

        /// <summary>
        /// Return the total number of stats between the dates
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <returns>The count of all records in range</returns>
        //Task<long> GetStatsCountAsync(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "");

        /// <summary>
        /// Return the count of the entries with this status, or all if no status given
        /// </summary>
        /// <param name="start">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <param name="status">the status to get count of, or string.empty</param>
        /// <returns>The count of all records in range</returns>
        Task<long> GetStatsStatusCountAsync(DateTime start, DateTime endTime, string status = "", string workflowId = "", string taskId = "");

        /// <summary>
        /// Returns all stats in Failed or PartialFail status.
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <returns>All stats that failed or partially failed</returns>
        Task<long> GetStatsStatusFailedCountAsync(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "");

        /// <summary>
        /// Calculates the average exection time for the given range
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <returns>the average exection times in the time range</returns>
        Task<(double avgTotalExecution, double avgArgoExecution)> GetAverageStats(DateTime startTime, DateTime endTime, string workflowId = "", string taskId = "");

    }
}

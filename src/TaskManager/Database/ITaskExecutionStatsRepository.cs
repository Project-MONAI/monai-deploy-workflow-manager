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

using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;

namespace Monai.Deploy.WorkflowManager.TaskManager.Database
{
    public interface ITaskExecutionStatsRepository
    {
        /// <summary>
        /// Creates a task dispatch event in the database.
        /// </summary>
        /// <param name="taskDispatchEvent">A TaskDispatchEvent to create.</param>
        /// <returns>Returns the created TaskDispatchEventInfo.</returns>
        Task CreateAsync(TaskDispatchEventInfo taskDispatchEventInfo);

        /// <summary>
        /// Updates user accounts of a task dispatch event in the database.
        /// </summary>
        /// <param name="taskDispatchEvent">A TaskDispatchEvent to update.</param>
        /// <returns>Returns the created TaskDispatchEventInfo.</returns>
        Task UpdateExecutionStatsAsync(TaskUpdateEvent taskUpdateEvent);

        /// <summary>
        /// Returns paged entries between the two given dates.
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <returns>a paged view of entried in range</returns>
        Task<IEnumerable<TaskExecutionStats>> GetStatsAsync(DateTime startTime, DateTime endTime, int PageSize = 10, int PageNumber = 1, string workflowInstanceId = "", string taskId = "");

        /// <summary>
        /// Return the total number of stats between the dates
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <returns>The count of all records in range</returns>
        Task<long> GetStatsCountAsync(DateTime startTime, DateTime endTime, string workflowInstanceId = "", string taskId = "");

        /// <summary>
        /// Returns all stats in Failed or PartialFail status.
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <returns>All stats NOT of that status</returns>
        Task<long> GetStatsStatusFailedCountAsync(DateTime startTime, DateTime endTime, string workflowInstanceId = "", string taskId = "");

        /// <summary>
        /// Calculates the average exection time for the given range
        /// </summary>
        /// <param name="startTime">start of the range.</param>
        /// <param name="endTime">end of the range.</param>
        /// <returns>the average exection times in the time range</returns>
        Task<(double avgTotalExecution, double avgArgoExecution)> GetAverageStats(DateTime startTime, DateTime endTime, string workflowInstanceId = "", string taskId = "");

    }
}

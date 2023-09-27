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

using Microsoft.Extensions.Logging;

namespace Monai.Deploy.WorkflowManager.Common.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 100000, Level = LogLevel.Error, Message = "Unexpected error occurred in GET /payload API.")]
        public static partial void PayloadGetAllAsyncError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 100001, Level = LogLevel.Error, Message = "Unexpected error occurred in GET /payload/{id} API.")]
        public static partial void PayloadGetAsyncError(this ILogger logger, string id, Exception ex);

        [LoggerMessage(EventId = 100002, Level = LogLevel.Error, Message = "Unexpected error occurred in GET /tasks/running API.")]
        public static partial void TasksGetRunningListAsyncError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 100003, Level = LogLevel.Error, Message = "Unexpected error occurred in GET /tasks/ API.")]
        public static partial void TasksGetAsyncError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 100004, Level = LogLevel.Error, Message = "Unexpected error occurred in GET /workflowinstances/ API.")]
        public static partial void WorkflowinstancesGetListAsyncError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 100005, Level = LogLevel.Error, Message = "Unexpected error occurred in GET /workflowinstances/{id} API.")]
        public static partial void WorkflowinstancesGetByIdAsyncError(this ILogger logger, string id, Exception ex);

        [LoggerMessage(EventId = 100006, Level = LogLevel.Error, Message = "Unexpected error occurred in GET /workflowinstances/failed API.")]
        public static partial void WorkflowinstancesGetFailedAsyncError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 100007, Level = LogLevel.Error, Message = "Unexpected error occurred in GET /workflowinstances/{id}/executions/{executionId}/acknowledge API.")]
        public static partial void WorkflowinstancesAcknowledgeTaskError(this ILogger logger, string id, string executionId, Exception ex);

        [LoggerMessage(EventId = 100008, Level = LogLevel.Error, Message = "Unexpected error occurred in GET /workflows/ API.")]
        public static partial void WorkflowGetListError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 100009, Level = LogLevel.Error, Message = "Unexpected error occurred in GET /workflows/{id} API.")]
        public static partial void WorkflowGetAsyncError(this ILogger logger, string id, Exception ex);

        [LoggerMessage(EventId = 100010, Level = LogLevel.Error, Message = "Unexpected error occurred in POST /workflows/ API.")]
        public static partial void WorkflowCreateAsyncError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 100011, Level = LogLevel.Error, Message = "Unexpected error occurred in PUT /workflows/{id} API.")]
        public static partial void WorkflowUpdateAsyncError(this ILogger logger, string id, Exception ex);

        [LoggerMessage(EventId = 100012, Level = LogLevel.Error, Message = "Unexpected error occurred in DELETE /workflows/{id} API.")]
        public static partial void WorkflowDeleteAsyncError(this ILogger logger, string id, Exception ex);

        [LoggerMessage(EventId = 100013, Level = LogLevel.Information, Message = "BYpass authentication.")]
        public static partial void BypassAuthentication(this ILogger logger);

        [LoggerMessage(EventId = 100014, Level = LogLevel.Error, Message = "Unexpected error occurred in get /workflows/aetitle API.")]
        public static partial void WorkflowGetAeTitleAsyncError(this ILogger logger, Exception ex);


        [LoggerMessage(EventId = 100015, Level = LogLevel.Error, Message = "Unexpected error occurred in GET tasks/statsoverview API.")]
        public static partial void GetStatsOverviewAsyncError(this ILogger logger, Exception ex);

        [LoggerMessage(EventId = 100016, Level = LogLevel.Error, Message = "Unexpected error occurred in GET tasks/stats API.")]
        public static partial void GetStatsAsyncError(this ILogger logger, Exception ex);

    }
}

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
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Common.Contracts.Models
{
    public class ExecutionStatDayOverview
    {
        [JsonProperty("date")]
        public DateOnly Date { get; set; }
        [JsonProperty("total_executions")]
        public int TotalExecutions { get; set; }
        [JsonProperty("total_failures")]
        public int TotalFailures { get; set; }
        [JsonProperty("total_approvals")]
        public int TotalApprovals { get; set; }
        [JsonProperty("total_rejections")]
        public int TotalRejections { get; set; }
        [JsonProperty("total_cancelled")]
        public int TotalCancelled { get; set; }
        [JsonProperty("total_awaiting_review")]
        public int TotalAwaitingReview { get; set; }
    }
}

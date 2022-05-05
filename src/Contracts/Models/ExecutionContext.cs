// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class ExecutionContext
    {
        [JsonProperty(PropertyName = "execution_id")]
        public string ExecutionId { get; set; }

        [JsonProperty(PropertyName = "task_id")]
        public string TaskId { get; set; }

        [JsonProperty(PropertyName = "input_dir")]
        public string InputDir { get; set; }

        [JsonProperty(PropertyName = "output_dir")]
        public string OutputDir { get; set; }

        [JsonProperty(PropertyName = "task")]
        public Dictionary<string, string> Task { get; set; }

        [JsonProperty(PropertyName = "start_time")]
        public decimal StartTime { get; set; }

        [JsonProperty(PropertyName = "end_time")]
        public decimal EndTime { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "error_msg")]
        public string ErrorMsg { get; set; }

        [JsonProperty(PropertyName = "result")]
        public Dictionary<string, string> Result { get; set; }
    }
}

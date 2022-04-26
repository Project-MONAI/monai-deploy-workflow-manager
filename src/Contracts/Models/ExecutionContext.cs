// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class ExecutionContext
    {
        [ConfigurationKeyName("execution_id")]
        public string ExecutionId { get; set; }

        [ConfigurationKeyName("task_id")]
        public string TaskId { get; set; }

        [ConfigurationKeyName("input_dir")]
        public string InputDir { get; set; }

        [ConfigurationKeyName("output_dir")]
        public string OutputDir { get; set; }

        [ConfigurationKeyName("task")]
        public Dictionary<string, string> Task { get; set; }

        [ConfigurationKeyName("start_time")]
        public decimal StartTime { get; set; }

        [ConfigurationKeyName("end_time")]
        public decimal EndTime { get; set; }

        [ConfigurationKeyName("status")]
        public string Status { get; set; }

        [ConfigurationKeyName("error_msg")]
        public string ErrorMsg { get; set; }

        [ConfigurationKeyName("result")]
        public Dictionary<string, string> Result { get; set; }
    }
}

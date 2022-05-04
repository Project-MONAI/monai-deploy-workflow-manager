// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class WorkflowInstance
    {
        [JsonIgnore]
        public string Id { get; set; }

        [ConfigurationKeyName("ae_title")]
        public string AeTitle { get; set; }

        [ConfigurationKeyName("workflow_id")]
        public string WorkflowId { get; set; }

        [ConfigurationKeyName("payload_id")]
        public string PayloadId { get; set; }

        [ConfigurationKeyName("start_time")]
        public DateTime StartTime { get; set; }

        [ConfigurationKeyName("status")]
        public Status Status { get; set; }

        [ConfigurationKeyName("bucket_id")]
        public string BucketId { get; set; }

        [ConfigurationKeyName("input_metadata")]
        public Dictionary<string, string> InputMetaData { get; set; }

        [ConfigurationKeyName("tasks")]
        public TaskExecution[] Tasks { get; set; }
    }
}

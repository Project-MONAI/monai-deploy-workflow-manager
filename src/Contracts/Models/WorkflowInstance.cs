// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class WorkflowInstance
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "ae_title")]
        public string AeTitle { get; set; }

        [JsonProperty(PropertyName = "workflow_id")]
        public string WorkflowId { get; set; }

        [JsonProperty(PropertyName = "payload_id")]
        public string PayloadId { get; set; }

        [JsonProperty(PropertyName = "start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        [JsonProperty(PropertyName = "bucket_id")]
        public string BucketId { get; set; }

        [JsonProperty(PropertyName = "input_metadata")]
        public Dictionary<string, string> InputMetaData { get; set; }

        [JsonProperty(PropertyName = "tasks")]
        public List<TaskExecution> Tasks { get; set; }
    }
}

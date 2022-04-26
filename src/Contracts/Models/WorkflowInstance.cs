// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using System;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class WorkflowInstance
    {
        [ConfigurationKeyName("id")]
        public Guid Id { get; set; }

        [ConfigurationKeyName("workflow_id")]
        public Guid WorkflowId { get; set; }

        [ConfigurationKeyName("payload_id")]
        public Guid PayloadId { get; set; }

        [ConfigurationKeyName("start_time")]
        public DateTime StartTime { get; set; }

        [ConfigurationKeyName("status")]
        public string Status { get; set; }

        [ConfigurationKeyName("bucket_id")]
        public string BucketId { get; set; }

        [ConfigurationKeyName("input_metadata")]
        public InputMataData InputMataData { get; set; }

        [ConfigurationKeyName("tasks")]
        public TaskExecution[] Tasks { get; set; }
    }
}

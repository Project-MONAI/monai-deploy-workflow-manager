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
using Monai.Deploy.WorkflowManager.Common.Contracts.Migrations;
using Mongo.Migration.Documents;
using Mongo.Migration.Documents.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Common.Contracts.Models
{
    [CollectionLocation("WorkflowInstances"), RuntimeVersion("1.0.0")]
    public class WorkflowInstance : IDocument
    {
        [JsonConverter(typeof(DocumentVersionConvert)), BsonSerializer(typeof(DocumentVersionConverBson))]
        public DocumentVersion Version { get; set; } = new DocumentVersion(1, 0, 0);

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "ae_title")]
        public string? AeTitle { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "workflow_name")]
        public string WorkflowName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "workflow_id")]
        public string WorkflowId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "payload_id")]
        public string PayloadId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        [JsonProperty(PropertyName = "bucket_id")]
        public string BucketId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "input_metadata")]
        public Dictionary<string, string> InputMetaData { get; set; } = new Dictionary<string, string>();

        [JsonProperty(PropertyName = "tasks")]
        public List<TaskExecution> Tasks { get; set; } = new List<TaskExecution>();

        [JsonProperty(PropertyName = "acknowledged_workflow_errors")]
        public DateTime? AcknowledgedWorkflowErrors { get; set; } = null;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

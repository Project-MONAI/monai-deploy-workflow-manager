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
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Migrations;
using Mongo.Migration.Documents;
using Mongo.Migration.Documents.Attributes;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Common.Contracts.Models
{
    [CollectionLocation("Payloads"), RuntimeVersion("1.0.7")]
    public class Payload : IDocument
    {
        [JsonConverter(typeof(DocumentVersionConvert)), BsonSerializer(typeof(DocumentVersionConverBson))]
        public DocumentVersion Version { get; set; } = new DocumentVersion(1, 0, 7);

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "payload_id")]
        public string PayloadId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "accession_id")]
        public string AccessionId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "workflows")]
        public IEnumerable<string> Workflows { get; set; } = [];

        [JsonProperty(PropertyName = "triggered_workflow_names")]
        public IEnumerable<string> TriggeredWorkflowNames { get; set; } = [];

        [JsonProperty(PropertyName = "workflow_instance_ids")]
        public IEnumerable<string> WorkflowInstanceIds { get; set; } = [];

        [JsonProperty(PropertyName = "file_count")]
        public int FileCount { get; set; }

        [JsonProperty(PropertyName = "correlation_id")]
        public string CorrelationId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "bucket")]
        public string Bucket { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty(PropertyName = "payload_deleted")]
        public PayloadDeleted PayloadDeleted { get; set; } = PayloadDeleted.No;

        [JsonProperty(PropertyName = "files")]
        public IList<BlockStorageInfo> Files { get; set; } = [];

        [JsonProperty(PropertyName = "patient_details")]
        public PatientDetails PatientDetails { get; set; } = new();

        public DataOrigin DataTrigger { get; set; } = new DataOrigin { DataService = DataService.DIMSE };

        [JsonProperty(PropertyName = "expires")]
        public DateTime? Expires { get; set; }

        [JsonProperty(PropertyName = "series_instance_uid")]
        public string? SeriesInstanceUid { get; set; }

    }

    public enum PayloadDeleted
    {
        No,
        Yes,
        InProgress,
        Failed
    }
}

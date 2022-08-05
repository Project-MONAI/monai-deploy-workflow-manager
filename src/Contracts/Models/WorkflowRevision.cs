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
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class WorkflowRevision : ISoftDeleteable
    {
        [BsonId]
        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }

        [JsonProperty(PropertyName = "workflow_id")]
        public string WorkflowId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "revision")]
        public int Revision { get; set; }

        [JsonProperty(PropertyName = "workflow")]
        public Workflow? Workflow { get; set; }

        [JsonIgnore]
        public DateTime? Deleted { get; set; } = null;

        [JsonIgnore]
        public bool IsDeleted { get => Deleted is not null; }
    }
}

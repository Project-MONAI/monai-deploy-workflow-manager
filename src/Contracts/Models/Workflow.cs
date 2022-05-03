// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class Workflow
    {
        [JsonIgnore]
        [BsonId]
        public string Id { get; set; }

        [JsonIgnore]
        public string WorkflowId { get; set; }

        [JsonIgnore]
        public int Revision { get; set; }

        [JsonIgnore]
        public WorkflowSpec WorkflowSpec { get; set; }
    }
}

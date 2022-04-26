// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class Workflow
    {
        [JsonIgnore]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [JsonIgnore]
        public Guid WorkflowId { get; set; }

        [JsonIgnore]
        public int Revision { get; set; }

        [JsonIgnore]
        public WorkflowSpec WorkflowSpec { get; set; }
    }
}

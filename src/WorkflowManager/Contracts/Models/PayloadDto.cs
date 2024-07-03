/*
 * Copyright 2023 MONAI Consortium
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

using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Common.Contracts.Models
{
    public class PayloadDto : Payload
    {
        public PayloadDto() { }
        public PayloadDto(Payload payload)
        {
            Version = payload.Version;
            Id = payload.Id;
            PayloadId = payload.PayloadId;
            Workflows = payload.Workflows;
            WorkflowInstanceIds = payload.WorkflowInstanceIds;
            FileCount = payload.FileCount;
            CorrelationId = payload.CorrelationId;
            Bucket = payload.Bucket;
            DataTrigger = payload.DataTrigger;
            Timestamp = payload.Timestamp;
            Files = payload.Files;
            PatientDetails = payload.PatientDetails;
            PayloadDeleted = payload.PayloadDeleted;
            SeriesInstanceUid = payload.SeriesInstanceUid;
            TriggeredWorkflowNames = payload.TriggeredWorkflowNames;
            AccessionId = payload.AccessionId;
        }

        [JsonProperty(PropertyName = "payload_status")]
        public PayloadStatus PayloadStatus { get; set; }
    }

    public enum PayloadStatus
    {
        InProgress,
        Complete
    }
}

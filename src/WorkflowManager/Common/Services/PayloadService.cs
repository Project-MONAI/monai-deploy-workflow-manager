/*
 * Copyright 2021-2022 MONAI Consortium
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

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Logging;
using Monai.Deploy.WorkflowManager.Storage.Services;

namespace Monai.Deploy.WorkflowManager.Common.Services
{
    public class PayloadService : IPayloadService
    {
        private readonly IPayloadRepsitory _payloadRepsitory;

        private readonly IDicomService _dicomService;

        private readonly ILogger<PayloadService> _logger;

        public PayloadService(IPayloadRepsitory payloadRepsitory, IDicomService dicomService, ILogger<PayloadService> logger)
        {
            _payloadRepsitory = payloadRepsitory ?? throw new ArgumentNullException(nameof(payloadRepsitory));
            _dicomService = dicomService ?? throw new ArgumentNullException(nameof(dicomService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Payload?> CreateAsync(WorkflowRequestEvent eventPayload)
        {
            Guard.Against.Null(eventPayload);

            try
            {
                var exists = await GetByIdAsync(eventPayload.PayloadId.ToString());

                if (exists is not null)
                {
                    _logger.PayloadAlreadyExists(eventPayload.PayloadId.ToString());
                    return exists;
                }

                var patientDetails = await _dicomService.GetPayloadPatientDetailsAsync(eventPayload.PayloadId.ToString(), eventPayload.Bucket);

                var payload = new Payload
                {
                    Id = Guid.NewGuid().ToString(),
                    PayloadId = eventPayload.PayloadId.ToString(),
                    Workflows = eventPayload.Workflows,
                    FileCount = eventPayload.FileCount,
                    CorrelationId = eventPayload.CorrelationId,
                    Bucket = eventPayload.Bucket,
                    CalledAeTitle = eventPayload.CalledAeTitle,
                    CallingAeTitle = eventPayload.CallingAeTitle,
                    Timestamp = eventPayload.Timestamp,
                    PatientDetails = patientDetails
                };

                if (await _payloadRepsitory.CreateAsync(payload))
                {
                    _logger.PayloadCreated(payload.Id);
                    return payload;
                }
                else
                {
                    _logger.FailedToCreatedPayload();
                }
            }
            catch (Exception e)
            {
                _logger.FailedToGetPatientDetails(eventPayload.PayloadId.ToString(), eventPayload.Bucket, e);
            }

            return null;
        }

        public async Task<Payload> GetByIdAsync(string payloadId)
        {
            Guard.Against.NullOrWhiteSpace(payloadId);

            return await _payloadRepsitory.GetByIdAsync(payloadId);
        }

        public async Task<IList<Payload>> GetAllAsync(int? skip = null,
                                                      int? limit = null,
                                                      string? patientId = "",
                                                      string? patientName = "")
            => await _payloadRepsitory.GetAllAsync(skip, limit, patientId, patientName);

        public async Task<IList<Payload>> GetAllAsync(int? skip = null, int? limit = null)
            => await _payloadRepsitory.GetAllAsync(skip, limit);

        public async Task<long> CountAsync() => await _payloadRepsitory.CountAsync();

        public async Task<bool> UpdateWorkflowInstanceIdsAsync(string payloadId, IEnumerable<string> workflowInstances)
        {
            if (await _payloadRepsitory.UpdateAssociatedWorkflowInstancesAsync(payloadId, workflowInstances))
            {
                _logger.PayloadUpdated(payloadId);
                return true;
            }
            else
            {
                _logger.PayloadUpdateFailed(payloadId);
                return false;
            }
        }
    }
}

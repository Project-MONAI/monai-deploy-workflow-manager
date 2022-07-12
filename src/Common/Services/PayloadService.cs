// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Logging.Logging;
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
                    return payload;
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

        public async Task<IList<Payload>> GetAllAsync(string? patientId = "", string? patientName = "")
            => await _payloadRepsitory.GetAllAsync(patientId, patientName);

        public async Task<bool> UpdateWorkflowInstanceIdsAsync(string payloadId, IEnumerable<string> workflowInstances)
            => await _payloadRepsitory.UpdateAssociatedWorkflowInstancesAsync(payloadId, workflowInstances);
    }
}

using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Storage.Services;

namespace Monai.Deploy.WorkflowManager.Common.Services
{
    public class PayloadService : IPayloadService
    {
        private readonly IPayloadRepsitory _payloadRepsitory;

        private readonly IDicomService _dicomService;

        public PayloadService(IPayloadRepsitory payloadRepsitory, IDicomService dicomService)
        {
            _payloadRepsitory = payloadRepsitory ?? throw new ArgumentNullException(nameof(payloadRepsitory));
            _dicomService = dicomService ?? throw new ArgumentNullException(nameof(dicomService));
        }

        public async Task<bool> CreateAsync(WorkflowRequestEvent eventPayload)
        {
            Guard.Against.Null(eventPayload);

            var patientDetails = await _dicomService.GetPayloadPatientDetails(eventPayload.PayloadId.ToString(), eventPayload.Bucket);

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

            return await _payloadRepsitory.CreateAsync(payload);
        }

        public async Task<Payload> GetByIdAsync(string payloadId)
        {
            Guard.Against.NullOrWhiteSpace(payloadId);

            return await _payloadRepsitory.GetByIdAsync(payloadId);
        }

        public async Task<IList<Payload>> GetAllAsync() => await _payloadRepsitory.GetAllAsync();
    }
}

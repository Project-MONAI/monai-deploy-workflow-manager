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

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.Exceptions;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Logging;
using Monai.Deploy.WorkflowManager.Storage.Services;

namespace Monai.Deploy.WorkflowManager.Common.Services
{
    public class PayloadService : IPayloadService
    {
        private readonly IPayloadRepository _payloadRepository;

        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;

        private readonly IDicomService _dicomService;

        private readonly IStorageService _storageService;

        private readonly ILogger<PayloadService> _logger;

        public PayloadService(
            IPayloadRepository payloadRepository,
            IDicomService dicomService,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<PayloadService> logger)
        {
            _payloadRepository = payloadRepository ?? throw new ArgumentNullException(nameof(payloadRepository));
            _workflowInstanceRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
            _dicomService = dicomService ?? throw new ArgumentNullException(nameof(dicomService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            var scope = _serviceScopeFactory.CreateScope();

            _storageService = scope.ServiceProvider.GetService<IStorageService>() ?? throw new ArgumentNullException(nameof(IStorageService));
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
                    PatientDetails = patientDetails,
                    PayloadDeleted = PayloadDeleted.No
                };

                if (await _payloadRepository.CreateAsync(payload))
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

            return await _payloadRepository.GetByIdAsync(payloadId);
        }

        public async Task<IList<PayloadDto>> GetAllAsync(int? skip = null,
                                                      int? limit = null,
                                                      string? patientId = "",
                                                      string? patientName = "")
            => await CreatePayloadsDto(
                await _payloadRepository.GetAllAsync(skip, limit, patientId, patientName)
            );

        public async Task<IList<PayloadDto>> GetAllAsync(int? skip = null, int? limit = null)
            => await CreatePayloadsDto(await _payloadRepository.GetAllAsync(skip, limit));

        private async Task<IList<PayloadDto>> CreatePayloadsDto(IList<Payload> payloads)
        {
            var dtos = new List<PayloadDto>();
            if (payloads is null || payloads.Count == 0)
            {
                return dtos;
            }

            var payloadIds = payloads.Select(payload => payload.Id).ToList();

            var workflowInstances =
                await _workflowInstanceRepository.GetByPayloadIdsAsync(payloadIds);

            foreach (var payload in payloads)
            {
                var payloadDto = new PayloadDto(payload);
                var wfs = workflowInstances?.Where(wf => wf.PayloadId == payload.Id);
                if (wfs == null || wfs.Any() is false)
                {
                    payloadDto.PayloadStatus = PayloadStatus.Complete;
                }
                else if (wfs.Any(wf => wf.Status == Status.Created))
                {
                    payloadDto.PayloadStatus = PayloadStatus.InProgress;
                }
                else if (wfs.All(wf => wf.Status != Status.Created))
                {
                    payloadDto.PayloadStatus = PayloadStatus.Complete;
                }
                dtos.Add(payloadDto);
            }

            return dtos;
        }

        public async Task<long> CountAsync() => await _payloadRepository.CountAsync();

        public async Task<bool> DeletePayloadFromStorageAsync(string payloadId)
        {
            Guard.Against.NullOrWhiteSpace(payloadId);

            var payload = await GetByIdAsync(payloadId);

            if (payload is null)
            {
                throw new MonaiNotFoundException($"Payload with ID: {payloadId} not found");
            }

            if (payload.PayloadDeleted == PayloadDeleted.InProgress || payload.PayloadDeleted == PayloadDeleted.Yes)
            {
                throw new MonaiBadRequestException($"Deletion of files for payload ID: {payloadId} already in progress or already deleted");
            }

            // update the payload to in progress before we request deletion from storage
            payload.PayloadDeleted = PayloadDeleted.InProgress;
            await _payloadRepository.UpdateAsync(payload);

            // run deletion in alternative thread so the user isn't held up
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                try
                {
                    // get all objects for the payload in storage to be deleted
                    var allPayloadObjects = await _storageService.ListObjectsAsync(payload.Bucket, payloadId, true);

                    if (allPayloadObjects.Any())
                    {
                        await _storageService.RemoveObjectsAsync(payload.Bucket, allPayloadObjects.Select(o => o.FilePath));
                    }

                    payload.PayloadDeleted = PayloadDeleted.Yes;
                }
                catch (Exception ex)
                {
                    _logger.PayloadDeleteFailed(payloadId, ex);
                    payload.PayloadDeleted = PayloadDeleted.Failed;
                }
                finally
                {
                    await _payloadRepository.UpdateAsync(payload);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return true;
        }

        public async Task<bool> UpdateWorkflowInstanceIdsAsync(string payloadId, IEnumerable<string> workflowInstances)
        {
            if (await _payloadRepository.UpdateAssociatedWorkflowInstancesAsync(payloadId, workflowInstances))
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

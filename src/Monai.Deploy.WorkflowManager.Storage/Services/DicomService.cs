// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.Common;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using Monai.Deploy.WorkflowManager.Storage.Constants;
using Newtonsoft.Json;
using Monai.Deploy.Storage.API;

namespace Monai.Deploy.WorkflowManager.Storage.Services
{
    public class DicomService : IDicomService
    {
        private readonly IStorageService _storageService;
        private readonly ILogger<DicomService> _logger;

        public DicomService(IStorageService storageService, ILogger<DicomService> logger)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PatientDetails> GetPayloadPatientDetails(string payloadId, string bucketName)
        {
            Guard.Against.NullOrWhiteSpace(bucketName);
            Guard.Against.NullOrWhiteSpace(payloadId);

            var items = _storageService.ListObjects(bucketName, $"{payloadId}/dcm", true);

            var patientDetails = new PatientDetails
            {
                PatientName = await GetFirstValue(items, payloadId, bucketName, DicomTagConstants.PatientNameTag),
                PatientId = await GetFirstValue(items, payloadId, bucketName, DicomTagConstants.PatientIdTag),
                PatientSex = await GetFirstValue(items, payloadId, bucketName, DicomTagConstants.PatientSexTag)
            };

            var dob = await GetFirstValue(items, payloadId, bucketName, DicomTagConstants.PatientDateOfBirthTag);

            if (DateTime.TryParse(dob, out var dateOfBirth))
            {
                patientDetails.PatientDob = dateOfBirth;
            }

            return patientDetails;
        }

        public async Task<string> GetFirstValue(IList<VirtualFileInfo> items, string payloadId, string bucketId, string keyId)
        {
            Guard.Against.NullOrWhiteSpace(bucketId);
            Guard.Against.NullOrWhiteSpace(payloadId);
            Guard.Against.NullOrWhiteSpace(keyId);

            try
            {
                var jsonStr = string.Empty;
                var value = new DicomValue();
                if (items is null || items.Count == 0)
                {
                    return string.Empty;
                }

                foreach (var item in items)
                {
                    if (!item.FilePath.EndsWith(".dcm.json"))
                    {
                        continue;
                    }

                    var stream = new MemoryStream();
                    await _storageService.GetObject(bucketId, $"{payloadId}/dcm/{item.Filename}", s => s.CopyTo(stream));
                    jsonStr = Encoding.UTF8.GetString(stream.ToArray());

                    var dict = JsonConvert.DeserializeObject<Dictionary<string, DicomValue>>(jsonStr);
                    dict.TryGetValue(keyId, out value);
                    if (value is null || value.Value is null)
                    {
                        continue;
                    }

                    var firstValue = value.Value.FirstOrDefault()?.ToString();

                    if (!string.IsNullOrWhiteSpace(firstValue))
                    {
                        return firstValue;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.FailedToGetDicomTag(payloadId, keyId, bucketId, e);
            }

            return string.Empty;
        }

        public IEnumerable<string> GetDicomPathsForTask(string outputDirectory, string bucketName)
        public async Task<IEnumerable<string>> GetDicomPathsForTask(string outputDirectory, string bucketName)
        {
            Guard.Against.NullOrWhiteSpace(outputDirectory);
            Guard.Against.NullOrWhiteSpace(bucketName);

            var files = await _storageService.ListObjectsAsync(bucketName, outputDirectory, true);

            var dicomFiles = files?.Where(f => f.FilePath.EndsWith(".dcm"));

            return dicomFiles?.Select(d => d.FilePath)?.ToList() ?? new List<string>();
        }
    }
}

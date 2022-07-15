// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
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

        public async Task<PatientDetails> GetPayloadPatientDetailsAsync(string payloadId, string bucketName)
        {
            Guard.Against.NullOrWhiteSpace(bucketName);
            Guard.Against.NullOrWhiteSpace(payloadId);

            var items = await _storageService.ListObjectsAsync(bucketName, $"{payloadId}/dcm", true);

            var patientDetails = new PatientDetails
            {
                PatientName = await GetFirstValueAsync(items, payloadId, bucketName, DicomTagConstants.PatientNameTag),
                PatientId = await GetFirstValueAsync(items, payloadId, bucketName, DicomTagConstants.PatientIdTag),
                PatientSex = await GetFirstValueAsync(items, payloadId, bucketName, DicomTagConstants.PatientSexTag),
                PatientAge = await GetFirstValueAsync(items, payloadId, bucketName, DicomTagConstants.PatientAgeTag),
                PatientHospitalId = await GetFirstValueAsync(items, payloadId, bucketName, DicomTagConstants.PatientHospitalIdTag)
            };

            var dob = await GetFirstValueAsync(items, payloadId, bucketName, DicomTagConstants.PatientDateOfBirthTag);

            if (DateTime.TryParse(dob, out var dateOfBirth))
            {
                patientDetails.PatientDob = dateOfBirth;
            }

            return patientDetails;
        }

        public async Task<string> GetFirstValueAsync(IList<VirtualFileInfo> items, string payloadId, string bucketId, string keyId)
        {
            Guard.Against.NullOrWhiteSpace(bucketId);
            Guard.Against.NullOrWhiteSpace(payloadId);
            Guard.Against.NullOrWhiteSpace(keyId);

            try
            {
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

                    var stream = await _storageService.GetObjectAsync(bucketId, item.FilePath);
                    var jsonStr = Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());

                    var dict = JsonConvert.DeserializeObject<Dictionary<string, DicomValue>>(jsonStr);
                    if (dict is not null)
                    {
                        dict.TryGetValue(keyId, out var value);
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
            }
            catch (Exception e)
            {
                _logger.FailedToGetDicomTag(payloadId, keyId, bucketId, e);
            }

            return string.Empty;
        }

        public async Task<IEnumerable<string>> GetDicomPathsForTaskAsync(string outputDirectory, string bucketName)
        {
            Guard.Against.NullOrWhiteSpace(outputDirectory);
            Guard.Against.NullOrWhiteSpace(bucketName);

            var files = await _storageService.ListObjectsAsync(bucketName, outputDirectory, true);

            var dicomFiles = files?.Where(f => f.FilePath.EndsWith(".dcm"));

            return dicomFiles?.Select(d => d.FilePath)?.ToList() ?? new List<string>();
        }

        public async Task<string> GetAnyValueAsync(string keyId, string payloadId, string bucketId)
        {
            Guard.Against.NullOrWhiteSpace(keyId);
            Guard.Against.NullOrWhiteSpace(payloadId);
            Guard.Against.NullOrWhiteSpace(bucketId);

            var path = $"{payloadId}/dcm";
            var listOfFiles = await _storageService.ListObjectsAsync(bucketId, path, true);
            var listOfJsonFiles = listOfFiles.Where(file => file.Filename.EndsWith(".json")).ToList();
            var fileCount = listOfJsonFiles.Count;

            for (int i = 0; i < fileCount; i++)
            {
                var matchValue = await GetDcmJsonFileValueAtIndexAsync(i, path, bucketId, keyId, listOfJsonFiles);

                if (matchValue != null)
                {
                    return matchValue;
                }
            }

            return string.Empty;
        }

        public async Task<string> GetAllValueAsync(string keyId, string payloadId, string bucketId)
        {
            Guard.Against.NullOrWhiteSpace(keyId);
            Guard.Against.NullOrWhiteSpace(payloadId);
            Guard.Against.NullOrWhiteSpace(bucketId);

            var path = $"{payloadId}/dcm";
            var listOfFiles = await _storageService.ListObjectsAsync(bucketId, path, true);
            var listOfJsonFiles = listOfFiles.Where(file => file.Filename.EndsWith(".json")).ToList();
            var matchValue = await GetDcmJsonFileValueAtIndexAsync(0, path, bucketId, keyId, listOfJsonFiles);
            var fileCount = listOfJsonFiles.Count;

            for (int i = 0; i < fileCount; i++)
            {
                if (listOfJsonFiles[i].Filename.EndsWith(".dcm"))
                {
                    var currentValue = await GetDcmJsonFileValueAtIndexAsync(i, path, bucketId, keyId, listOfJsonFiles);
                    if (currentValue != matchValue)
                    {
                        return string.Empty;
                    }
                }
            }

            return matchValue;
        }

        /// <summary>
        /// Gets file at position
        /// </summary>
        /// <param name="index"></param>
        /// <param name="path"></param>
        /// <param name="bucketId"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public async Task<string> GetDcmJsonFileValueAtIndexAsync(int index,
                                                              string path,
                                                              string bucketId,
                                                              string keyId,
                                                              List<VirtualFileInfo> items)
        {
            Guard.Against.NullOrWhiteSpace(bucketId);
            Guard.Against.NullOrWhiteSpace(path);
            Guard.Against.NullOrWhiteSpace(keyId);
            Guard.Against.Null(items);

            if (index > items.Count)
            {
                return string.Empty;
            }

            var stream = await _storageService.GetObjectAsync(bucketId, items[index].FilePath);
            var jsonStr = Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());

            var dict = JsonConvert.DeserializeObject<Dictionary<string, DicomValue>>(jsonStr);

            if (dict is null)
            {
                return string.Empty;
            }

            dict.TryGetValue(keyId, out var value);

            if (value is not null && value.Value is not null)
            {
                var str = value?.Value.Cast<string>();
                if (str is not null)
                {
                    return string.Concat(str);
                }
            }

            return string.Empty;
        }
    }
}

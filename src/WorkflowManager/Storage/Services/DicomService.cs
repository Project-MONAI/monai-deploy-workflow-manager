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

using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Logging;
using Monai.Deploy.WorkflowManager.Common.Storage.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Monai.Deploy.WorkflowManager.Common.Storage.Services
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

        private static readonly Dictionary<string, string> SupportedTypes = new()
        {
            { "CS", "Code String" },
            { "DA", "Date" },
            { "DS", "Decimal String" },
            { "IS", "Integer String" },
            { "LO", "Long String" },
            { "SH", "Short String" },
            { "UI", "Unique Identifier (UID)" },
            { "UL", "Unsigned Long" },
            { "US", "Unsigned Short" },
        };

        private static readonly Dictionary<string, string> UnsupportedTypes = new()
        {
            { "CS", "Code String" },
            { "DA", "Date" },
            { "DS", "Decimal String" },
            { "IS", "Integer String" },
            { "LO", "Long String" },
            { "SH", "Short String" },
            { "UI", "Unique Identifier (UID)" },
            { "UL", "Unsigned Long" },
            { "US", "Unsigned Short" },
        };

        public async Task<PatientDetails> GetPayloadPatientDetailsAsync(string payloadId, string bucketName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(bucketName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(payloadId);

            var dict = await GetMetaData(payloadId, bucketName);

            var patientDetails = new PatientDetails
            {
                PatientName = GetFirstValueAsync(dict, DicomTagConstants.PatientNameTag),
                PatientId = GetFirstValueAsync(dict, DicomTagConstants.PatientIdTag),
                PatientSex = GetFirstValueAsync(dict, DicomTagConstants.PatientSexTag),
                PatientAge = GetFirstValueAsync(dict, DicomTagConstants.PatientAgeTag),
                PatientHospitalId = GetFirstValueAsync(dict, DicomTagConstants.PatientHospitalIdTag)
            };

            var dob = GetFirstValueAsync(dict, DicomTagConstants.PatientDateOfBirthTag);

            if (DateTime.TryParseExact(dob, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth))
            {
                patientDetails.PatientDob = dateOfBirth;
            }

            return patientDetails;
        }

        private string? GetFirstValueAsync(Dictionary<string, DicomValue>? dict, string keyId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(keyId);
            if (dict is null)
            {
                return null;
            }

            try
            {
                var value = GetValue(dict, keyId);

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
                return null;
            }
            catch (Exception e)
            {
                _logger.FailedToGetDicomTagFromDictoionary(keyId, e);
            }

            return null;
        }

        public async Task<Dictionary<string, DicomValue>?> GetMetaData(string payloadId, string bucketId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(bucketId);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(payloadId);
            var items = await _storageService.ListObjectsAsync(bucketId, $"{payloadId}/dcm", true);
            var dict = new Dictionary<string, DicomValue>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (items is null || items.Any() is false)
                {
                    return null;
                }

                foreach (var filePath in items.Select(item => item.FilePath))
                {
                    if (filePath.EndsWith(".dcm.json") is false)
                    {
                        continue;
                    }

                    var stream = await _storageService.GetObjectAsync(bucketId, filePath);
                    var jsonStr = Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());

                    var dictCurrent = new Dictionary<string, DicomValue>(StringComparer.OrdinalIgnoreCase);
                    JsonConvert.PopulateObject(jsonStr, dictCurrent);


                    // merge the two dictionaries
                    foreach (var (key, value) in dictCurrent)
                    {
                        dict.TryAdd(key, value);
                    }
                }
                return dict;
            }
            catch (Exception e)
            {
                _logger.FailedToGetDicomMetadataFromBucket(payloadId, bucketId, e);
            }

            return null;
        }

        public async Task<IEnumerable<string>> GetDicomPathsForTaskAsync(string outputDirectory, string bucketName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(outputDirectory, nameof(outputDirectory));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(bucketName, nameof(bucketName));

            var files = await _storageService.ListObjectsAsync(bucketName, outputDirectory, true);

            var dicomFiles = files?.Where(f => f.FilePath.EndsWith(".dcm"));

            return dicomFiles?.Select(d => d.FilePath) ?? [];
        }

        public async Task<string> GetAnyValueAsync(string keyId, string payloadId, string bucketId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(keyId, nameof(keyId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(payloadId, nameof(payloadId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(bucketId, nameof(bucketId));

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
            ArgumentNullException.ThrowIfNullOrWhiteSpace(keyId, nameof(keyId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(payloadId, nameof(payloadId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(bucketId, nameof(bucketId));

            var path = $"{payloadId}/dcm";
            var listOfFiles = await _storageService.ListObjectsAsync(bucketId, path, true);
            var listOfJsonFiles = listOfFiles.Where(file => file.Filename.EndsWith(".json")).ToList();
            var matchValue = await GetDcmJsonFileValueAtIndexAsync(0, path, bucketId, keyId, listOfJsonFiles);
            var fileCount = listOfJsonFiles.Count;

            for (var i = 0; i < fileCount; i++)
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
            ArgumentNullException.ThrowIfNullOrWhiteSpace(bucketId, nameof(bucketId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(path, nameof(path));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(keyId, nameof(keyId));
            ArgumentNullException.ThrowIfNull(items, nameof(items));

            if (index > items.Count)
            {
                return string.Empty;
            }

            var stream = await _storageService.GetObjectAsync(bucketId, items[index].FilePath);
            var jsonStr = Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());

            var dict = new Dictionary<string, DicomValue>(StringComparer.OrdinalIgnoreCase);
            JsonConvert.PopulateObject(jsonStr, dict);
            return GetValue(dict, keyId);
        }

        public string GetValue(Dictionary<string, DicomValue> dict, string keyId)
        {
            var result = string.Empty;

            if (dict.TryGetValue(keyId, out var value))
            {
                if (string.Equals(keyId, DicomTagConstants.PatientNameTag) || value.Vr.ToUpperInvariant() == "PN")
                {
                    result = GetPatientName(value.Value);
                    _logger.GetPatientName(result);
                    return result;
                }
                var jsonString = DecodeComplexString(value);
                if (SupportedTypes.TryGetValue(value.Vr.ToUpperInvariant(), out var vrFullString))
                {
                    result = TryGetValueAndLogSupported(vrFullString, value, jsonString);
                }
                else if (UnsupportedTypes.TryGetValue(value.Vr.ToUpperInvariant(), out vrFullString))
                {
                    result = TryGetValueAndLogSupported(vrFullString, value, jsonString);
                }
                else
                {
                    result = TryGetValueAndLogUnSupported("Unknown Dicom Type", value, jsonString);
                }
            }
            return result;
        }

        public string? GetSeriesInstanceUID(Dictionary<string, DicomValue>? dict)
        {
            if (dict is null)
            {
                return null;
            }

            if (dict.TryGetValue(DicomTagConstants.SeriesInstanceUIDTag, out var value))
            {
                return JsonConvert.SerializeObject(value.Value);
            }
            return null;
        }

        public string? GetAccessionID(Dictionary<string, DicomValue>? dict)
        {
            if (dict is null)
            {
                return null;
            }

            if (dict.TryGetValue(DicomTagConstants.AccessionNumberTag, out var value))
            {
                var accession = JsonConvert.SerializeObject(value.Value);
                accession = accession.Replace("[\"", "").Replace("\"]", "");
                return accession;
            }
            return null;
        }

        private string TryGetValueAndLogSupported(string vrFullString, DicomValue value, string jsonString)
        {
            var result = TryGetValue(value);
            _logger.SupportedType(value.Vr, vrFullString, jsonString, result);
            return result;
        }

        private string TryGetValueAndLogUnSupported(string vrFullString, DicomValue value, string jsonString)
        {
            var result = TryGetValue(value);
            _logger.UnsupportedType(value.Vr, vrFullString, jsonString, result);
            return result;
        }

        private string TryGetValue(DicomValue value)
        {
            var result = string.Empty;
            foreach (var val in value.Value)
            {
                try
                {
                    if (double.TryParse(val.ToString(), out var dbl))
                    {
                        result = ConcatResult(result, dbl);
                    }
                    else
                    {
                        result = ConcatResult(result, val);
                    }
                }
                catch (Exception ex)
                {
                    _logger.UnableToCastDicomValueToString(DecodeComplexString(value), ex);
                }
            }
            if (value.Value.Length > 1)
            {
                return $"[{result}]";
            }
            return result;
        }

        private static string ConcatResult(string result, dynamic str)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                result = string.Concat(result, $"{str}");
            }
            else
            {
                result = string.Concat(result, $", {str}");
            }

            return result;
        }

        private static string DecodeComplexString(DicomValue dicomValue)
        {
            return JsonConvert.SerializeObject(dicomValue.Value);
        }

        private static string GetPatientName(object[] values)
        {

            var resultStr = new List<string>();

            foreach (var value in values)
            {
                var valueStr = JObject.FromObject(value)?
                    .GetValue("Alphabetic", StringComparison.OrdinalIgnoreCase)?
                    .Value<string>();

                if (valueStr is not null)
                {
                    resultStr.Add(valueStr);
                }
            }

            if (resultStr.Any() is true)
            {
                return string.Concat(resultStr);
            }

            return string.Empty;
        }
    }
}

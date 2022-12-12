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
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Logging;
using Monai.Deploy.WorkflowManager.Storage.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            if (DateTime.TryParseExact(dob, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth))
            {
                patientDetails.PatientDob = dateOfBirth;
            }

            return patientDetails;
        }

        public async Task<string?> GetFirstValueAsync(IList<VirtualFileInfo> items, string payloadId, string bucketId, string keyId)
        {
            Guard.Against.NullOrWhiteSpace(bucketId);
            Guard.Against.NullOrWhiteSpace(payloadId);
            Guard.Against.NullOrWhiteSpace(keyId);

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

                    var dict = new Dictionary<string, DicomValue>(StringComparer.OrdinalIgnoreCase);
                    JsonConvert.PopulateObject(jsonStr, dict);

                    var value = GetValue(dict, keyId);

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.FailedToGetDicomTagFromPayload(payloadId, keyId, bucketId, e);
            }

            return null;
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

            var dict = new Dictionary<string, DicomValue>(StringComparer.OrdinalIgnoreCase);
            JsonConvert.PopulateObject(jsonStr, dict);

            return GetValue(dict, keyId);
        }

        public string GetValue(Dictionary<string, DicomValue> dict, string keyId)
        {
            if (dict.Any() is false)
            {
                return string.Empty;
            }
            dict.TryGetValue(keyId, out var value);

            var result = string.Empty;
            if (string.Equals(keyId, DicomTagConstants.PatientNameTag))
            {
                result = GetPatientName(value.Value);
                _logger.GetPatientName(result);
                return result;
            }

            if (value is not null && value.Value is not null)
            {
                var jsonString = DecodeComplexString(value);
#pragma warning disable S1479 // "switch" statements should not have too many "case" clauses - complicity of this switch statement will help future developers when implementing  support for  other data types in future hopefully.
                switch (value.Vr.ToUpperInvariant())
                {
                    case "CS":/* supported */
                        result = TryGetValueAndLogSupported("Code String", value, jsonString);
                        break;
                    case "DA":/* supported */
                        result = TryGetValueAndLogSupported("Date", value, jsonString);
                        break;
                    case "DS":/* supported */
                        result = TryGetValueAndLogSupported("Decimal String", value, jsonString);
                        break;
                    case "IS":/* supported */
                        result = TryGetValueAndLogSupported("Integer String", value, jsonString);
                        break;
                    case "LO":/* supported */
                        result = TryGetValueAndLogSupported("Long String", value, jsonString);
                        break;
                    case "SH":/* supported */
                        result = TryGetValueAndLogSupported("Short String", value, jsonString);
                        break;
                    case "UI": /* supported */
                        result = TryGetValueAndLogSupported("Unique Identifier (UID)", value, jsonString);
                        break;
                    case "UL":/* supported */
                        result = TryGetValueAndLogSupported("Unsigned Long", value, jsonString);
                        break;
                    case "US":/* supported */
                        result = TryGetValueAndLogSupported("Unsigned Short", value, jsonString);
                        break;
                    case "PN":
                        result = GetPatientName(value.Value);
                        _logger.GetPatientName(result);
                        break;
                    case "AE":
                        result = TryGetValueAndLogUnSupported("Application Entity", value, jsonString);
                        break;
                    case "AS":
                        result = TryGetValueAndLogUnSupported("Age String", value, jsonString);
                        break;
                    case "AT":
                        result = TryGetValueAndLogUnSupported("Attribute Tag", value, jsonString);
                        break;
                    case "DT":
                        result = TryGetValueAndLogUnSupported("Date Time", value, jsonString);
                        break;
                    case "FL":
                        result = TryGetValueAndLogUnSupported("Floating Point Single", value, jsonString);
                        break;
                    case "FD":
                        result = TryGetValueAndLogUnSupported("Floating Point Double", value, jsonString);
                        break;
                    case "LT":
                        result = TryGetValueAndLogUnSupported("Long Text", value, jsonString);
                        break;
                    case "OB":
                        result = TryGetValueAndLogUnSupported("Other Byte", value, jsonString);
                        break;
                    case "OD":
                        result = TryGetValueAndLogUnSupported("Other Double", value, jsonString);
                        break;
                    case "OF":
                        result = TryGetValueAndLogUnSupported("Other Float", value, jsonString);
                        break;
                    case "OL":
                        result = TryGetValueAndLogUnSupported("Other Long", value, jsonString);
                        break;
                    case "OV":
                        result = TryGetValueAndLogUnSupported("Other 64-bit Very Long", value, jsonString);
                        break;
                    case "OW":
                        result = TryGetValueAndLogUnSupported("Other Word", value, jsonString);
                        break;
                    case "SL":
                        result = TryGetValueAndLogUnSupported("Signed Long", value, jsonString);
                        break;
                    case "SQ":
                        result = TryGetValueAndLogUnSupported("Sequence of Items", value, jsonString);
                        break;
                    case "SS":
                        result = TryGetValueAndLogUnSupported("Signed Short", value, jsonString);
                        break;
                    case "ST":
                        result = TryGetValueAndLogUnSupported("Short Text", value, jsonString);
                        break;
                    case "SV":
                        result = TryGetValueAndLogUnSupported("Signed 64-bit Very Long", value, jsonString);
                        break;
                    case "TM":
                        result = TryGetValueAndLogUnSupported("Time", value, jsonString);
                        break;
                    case "UC":
                        result = TryGetValueAndLogUnSupported("Unlimited Characters", value, jsonString);
                        break;
                    case "UN":
                        result = TryGetValueAndLogUnSupported("Unknown", value, jsonString);
                        break;
                    case "UR":
                        result = TryGetValueAndLogUnSupported("Universal Resource Identifier or Universal Resource Locator (URI/URL)", value, jsonString);
                        break;
                    case "UT":
                        result = TryGetValueAndLogUnSupported("Unlimited Text", value, jsonString);
                        break;
                    case "UV":
                        result = TryGetValueAndLogUnSupported("Unsigned 64-bit Very Long", value, jsonString);
                        break;
                    default:
                        result = TryGetValueAndLogUnSupported("Unknown Dicom Type", value, jsonString);
                        break;
                }
#pragma warning restore S1479
            }
            return result;
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
            try
            {
                var strs = value.Value.Cast<string>();
                if (strs is not null)
                {
                    return string.Concat(strs);
                }
            }
            catch (Exception ex)
            {
                _logger.UnableToCastDicomValueToString(DecodeComplexString(value), ex);
            }
            return string.Empty;
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

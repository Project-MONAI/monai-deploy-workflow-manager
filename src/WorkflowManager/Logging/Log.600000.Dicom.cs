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

using Microsoft.Extensions.Logging;

namespace Monai.Deploy.WorkflowManager.Common.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 600000, Level = LogLevel.Error, Message = "Failed to get DICOM tag {dicomTag} in bucket {bucketId}. Payload: {payloadId}")]
        public static partial void FailedToGetDicomTagFromPayload(this ILogger logger, string payloadId, string dicomTag, string bucketId, Exception ex);

        [LoggerMessage(EventId = 600001, Level = LogLevel.Information, Message = "Attempted to retrieve Patient Name from DCM file, result: {name}")]
        public static partial void GetPatientName(this ILogger logger, string name);

        [LoggerMessage(EventId = 600002, Level = LogLevel.Information, Message = "Unsupported Type '{vr}' {vrFull} with value: {value} result: '{result}'")]
        public static partial void UnsupportedType(this ILogger logger, string vr, string vrFull, string value, string result);

        [LoggerMessage(EventId = 600003, Level = LogLevel.Information, Message = "Decoding supported type '{vr}' {vrFull} with value: {value} result: '{result}'")]
        public static partial void SupportedType(this ILogger logger, string vr, string vrFull, string value, string result);

        [LoggerMessage(EventId = 600004, Level = LogLevel.Error, Message = "Failed trying to cast Dicom Value to string {value}")]
        public static partial void UnableToCastDicomValueToString(this ILogger logger, string value, Exception ex);

        [LoggerMessage(EventId = 600005, Level = LogLevel.Debug, Message = "Dicom export marked as succeeded with {fileStatusCount} files marked as exported.")]
        public static partial void DicomExportSucceeded(this ILogger logger, string fileStatusCount);

        [LoggerMessage(EventId = 600006, Level = LogLevel.Debug, Message = "Dicom export marked as failed with {fileStatusCount} files marked as exported.")]
        public static partial void DicomExportFailed(this ILogger logger, string fileStatusCount);

        [LoggerMessage(EventId = 600007, Level = LogLevel.Error, Message = "Failed to get DICOM metadata from bucket {bucketId}. Payload: {payloadId}")]
        public static partial void FailedToGetDicomMetadataFromBucket(this ILogger logger, string payloadId, string bucketId, Exception ex);

        [LoggerMessage(EventId = 600008, Level = LogLevel.Error, Message = "Failed to get DICOM tag {dicomTag} from dictionary")]
        public static partial void FailedToGetDicomTagFromDictoionary(this ILogger logger, string dicomTag, Exception ex);
    }
}

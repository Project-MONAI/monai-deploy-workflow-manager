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

using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Storage.Services
{
    public interface IDicomService
    {
        /// <summary>
        /// Gets a list of paths of all dicom images in the output directory.
        /// </summary>
        /// <param name="outputDirectory">Output dir of the task.</param>
        /// <param name="bucketName">Name of the bucket.</param>
        Task<IEnumerable<string>> GetDicomPathsForTaskAsync(string outputDirectory, string bucketName);

        /// <summary>
        /// Gets patient details from the dicom metadata.
        /// </summary>
        /// <param name="payloadId">Payload id.</param>
        /// <param name="bucketName">Name of the bucket.</param>
        Task<PatientDetails> GetPayloadPatientDetailsAsync(string payloadId, string bucketName);

        /// <summary>
        /// If series contains given value
        /// if all values exist for given key example 0010 0040 then and
        /// they are all same value return that value otherwise return
        /// 'null'
        /// </summary>
        /// <param name="keyId"></param>
        /// <param name="matchValue"></param>
        /// <param name="workflowInstance"></param>
        /// <returns></returns>
        Task<string> GetAllValueAsync(string keyId, string payloadId, string bucketId);

        /// <summary>
        /// If any keyid exists return first occurance
        /// if no matchs return 'null'
        /// </summary>
        /// <param name="keyId">example of keyId 00100040</param>
        /// <param name="workflowInstance"></param>
        /// <returns></returns>
        Task<string> GetAnyValueAsync(string keyId, string payloadId, string bucketId);
    }
}

// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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

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
        IEnumerable<string> GetDicomPathsForTask(string outputDirectory, string bucketName);

        PatientDetails GetPayloadPatientDetails(string payloadId, string bucketName);
        Task<IEnumerable<string>> GetDicomPathsForTask(string outputDirectory, string bucketName);
        Task<PatientDetails> GetPayloadPatientDetails(string payloadId, string bucketName);
    }
}

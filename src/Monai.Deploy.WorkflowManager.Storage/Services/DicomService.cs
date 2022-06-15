// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Monai.Deploy.Storage;

namespace Monai.Deploy.WorkflowManager.Storage.Services
{
    public class DicomService : IDicomService
    {
        private IStorageService StorageService { get; set; }

        public DicomService(IStorageService storageService)
        {
            StorageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        public IEnumerable<string> GetDicomPathsForTask(string outputDirectory, string bucketName)
        {
            Guard.Against.NullOrWhiteSpace(outputDirectory);
            Guard.Against.NullOrWhiteSpace(bucketName);

            var files = StorageService.ListObjects(bucketName, outputDirectory, true);

            var dicomFiles = files?.Where(f => f.FilePath.EndsWith(".dcm"));

            return dicomFiles?.Select(d => d.FilePath)?.ToList() ?? new List<string>();
        }
    }
}

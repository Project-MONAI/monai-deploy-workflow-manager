// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Storage;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Common
{
    public interface IDicomStore
    {
        IStorageService StorageService { get; }

        string GetAllValue(string keyId, string payloadId, string bucketId);
        string GetAnyValue(string keyId, string payloadId, string bucketId);
        string GetFirstValue(string payloadId, string bucketId, string keyId);
        string GetDcmFileValueAtIndex(int index, string path, string bucketId, string keyId);
    }
}

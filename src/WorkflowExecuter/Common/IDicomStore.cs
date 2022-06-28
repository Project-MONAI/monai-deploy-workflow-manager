// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Storage.API;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Common
{
    public interface IDicomStore
    {
        IStorageService StorageService { get; }

        Task<string> GetAllValueAsync(string keyId, string payloadId, string bucketId);
        Task<string> GetAnyValueAsync(string keyId, string payloadId, string bucketId);
        Task<string> GetDcmJsonFileValueAtIndexAsync(int index, string path, string bucketId, string keyId, List<VirtualFileInfo> items);
        Task<string> GetFirstValueAsync(IList<VirtualFileInfo> items, string payloadId, string bucketId, string keyId);
    }
}
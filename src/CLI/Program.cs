// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SecurityToken.Model;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.Common;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Common;

namespace CLI
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var fileSystem = new Lazy<IStorageService>(() => new FileSystem());
            DicomStore dicomStore = new DicomStore(fileSystem);
            Console.WriteLine("Hello World!");
        }
    }

    class FileSystem : IStorageService
    {
        public string Name => throw new NotImplementedException();

        public Task GetObject(string bucketName, string objectName, Action<Stream> callback, CancellationToken cancellationToken = default)
        {
            return File.ReadAllTextAsync($"{bucketName}{objectName}");
        }

        public IList<VirtualFileInfo> ListObjects(string bucketName, string prefix = "", bool recursive = false, CancellationToken cancellationToken = default)
        {
            var items = Directory.GetFiles("C:\\Users\\LillieDae\\OneDrive - Answer Digital\\Desktop\\id\\dcm");

            var result = new List<VirtualFileInfo>();
            foreach (var item in items)
            {
                var filename = Path.GetFileName(item);
                result.Add(new VirtualFileInfo(filename, "null", "null", 5));
            }
            return result;
        }





        public Task CopyObject(string sourceBucketName, string sourceObjectName, string destinationBucketName, string destinationObjectName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task CopyObjectWithCredentials(string sourceBucketName, string sourceObjectName, string destinationBucketName, string destinationObjectName, Credentials credentials, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task CreateFolder(string bucketName, string folderPath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task CreateFolderWithCredentials(string bucketName, string folderPath, Credentials credentials, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Credentials> CreateTemporaryCredentials(string bucketName, string folderName, int durationSeconds = 3600, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task GetObjectWithCredentials(string bucketName, string objectName, Credentials credentials, Action<Stream> callback, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public IList<VirtualFileInfo> ListObjectsWithCredentials(string bucketName, Credentials credentials, string prefix = "", bool recursive = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task PutObject(string bucketName, string objectName, Stream data, long size, string contentType, Dictionary<string, string> metadata, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task PutObjectWithCredentials(string bucketName, string objectName, Stream data, long size, string contentType, Dictionary<string, string> metadata, Credentials credentials, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task RemoveObject(string bucketName, string objectName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task RemoveObjects(string bucketName, IEnumerable<string> objectNames, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task RemoveObjectsWithCredentials(string bucketName, IEnumerable<string> objectNames, Credentials credentials, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task RemoveObjectWithCredentials(string bucketName, string objectName, Credentials credentials, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public KeyValuePair<string, string> VerifyObjectExists(string bucketName, KeyValuePair<string, string> objectPair) => throw new NotImplementedException();
        public Dictionary<string, string> VerifyObjectsExist(string bucketName, Dictionary<string, string> objectDict) => throw new NotImplementedException();
    }
}

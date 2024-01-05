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

using System.Reactive.Linq;
using Minio;
using Minio.DataModel.Args;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.POCO;
using Polly;
using Polly.Retry;
using File = System.IO.File;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support
{
    public class MinioClientUtil
    {
        private AsyncRetryPolicy RetryPolicy { get; set; }
        private IMinioClient Client { get; set; }

        public MinioClientUtil()
        {
            Client = new MinioClient()
                .WithEndpoint(TestExecutionConfig.MinioConfig.Endpoint)
                .WithCredentials(
                    TestExecutionConfig.MinioConfig.AccessKey,
                    TestExecutionConfig.MinioConfig.AccessToken
                ).Build();

            RetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
        }

        public async Task CreateBucket(string bucketName)
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    if (await Client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName)))
                    {
                        try
                        {
                            var listOfKeys = new List<string>();
                            var listArgs = new ListObjectsArgs()
                                .WithBucket(bucketName)
                                .WithPrefix("")
                                .WithRecursive(true);

                            var objs = await Client.ListObjectsAsync(listArgs).ToList();
                            foreach (var obj in objs)
                            {
                                await Client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(bucketName).WithObject(obj.Key));
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        await Client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Bucket]  Exception: {e}");
                    if (e.Message != "MinIO API responded with message=Your previous request to create the named bucket succeeded and you already own it.")
                    {
                        throw;
                    }
                }
            });
        }

        public async Task AddFileToStorage(string localPath, string folderPath)
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    FileAttributes fileAttributes = File.GetAttributes(localPath);
                    if (fileAttributes.HasFlag(FileAttributes.Directory))
                    {
                        var files = Directory.GetFiles($"{localPath}", "*.*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            var relativePath = $"{folderPath}{Path.GetRelativePath(localPath, file)}";
                            var fileName = Path.GetFileName(file);
                            var bs = File.ReadAllBytes(file);
                            using (var filestream = new MemoryStream(bs))
                            {
                                var fileInfo = new FileInfo(file);
                                var metaData = new Dictionary<string, string>
                                {
                                            { "Test-Metadata", "Test  Test" }
                                };
                                await Client.PutObjectAsync(
                                new PutObjectArgs()
                                    .WithBucket(TestExecutionConfig.MinioConfig.Bucket)
                                    .WithObject(relativePath)
                                    .WithFileName(file)
                                    .WithContentType("application/octet-stream")
                                    .WithHeaders(metaData));
                            }
                        }
                    }
                    else
                    {
                        var bs = File.ReadAllBytes(localPath);
                        using (MemoryStream filestream = new MemoryStream(bs))
                        {
                            FileInfo fileInfo = new FileInfo(localPath);
                            var metaData = new Dictionary<string, string>
                        {
                                    { "Test-Metadata", "Test  Test" }
                        };
                            await Client.PutObjectAsync(
                                new PutObjectArgs()
                                .WithBucket(TestExecutionConfig.MinioConfig.Bucket)
                                .WithObject(folderPath)
                                .WithFileName(localPath)
                                .WithContentType("application/octet-stream")
                                .WithHeaders(metaData));
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"[Bucket]  Exception: {e}");
                }
            });
        }

        public async Task GetFile(string bucketName, string objectName, string fileName)
        {
            await Client.GetObjectAsync(new GetObjectArgs().WithBucket(bucketName).WithObject(objectName).WithFile(fileName));
        }

        public async Task<bool> CheckFileExists(string bucketName, string objectName)
        {
            var args = new StatObjectArgs()
                            .WithBucket(bucketName)
                            .WithObject(objectName);
            try
            {
                await Client.StatObjectAsync(args);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public async Task DeleteBucket(string bucketName)
        {
            bool found = await Client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (found)
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    await Client.RemoveBucketAsync(new RemoveBucketArgs().WithBucket(bucketName));
                });
            }
        }

        public async Task RemoveObjects(string bucketName, string objectName)
        {
            if (await Client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName)))
            {
                await Client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(bucketName).WithObject(objectName));
            }
        }

        public async Task<IList<VirtualFileInfo>> ListFilesFromDir(string bucketName, string objectName)
        {
            var cancellationToken = default(CancellationToken);
            return await Task.Run(() =>
            {
                var files = new List<VirtualFileInfo>();
                var listArgs = new ListObjectsArgs()
                    .WithBucket(bucketName)
                    .WithPrefix(objectName)
                    .WithRecursive(true);

                var objservable = Client.ListObjectsAsync(listArgs, cancellationToken);
                var completedEvent = new ManualResetEventSlim(false);
                objservable.Subscribe(item =>
                {
                    if (!item.IsDir)
                    {
                        files.Add(new VirtualFileInfo(Path.GetFileName(item.Key), item.Key, item.ETag, item.Size)
                        {
                            LastModifiedDateTime = item.LastModifiedDateTime
                        });
                    }
                },
                error =>
                {
                },
                () => completedEvent.Set(), cancellationToken);

                completedEvent.Wait(cancellationToken);
                return files;
            }).ConfigureAwait(false);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}

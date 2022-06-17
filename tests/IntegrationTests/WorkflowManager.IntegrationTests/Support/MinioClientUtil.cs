using Minio;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public class MinioClientUtil
    {
        private AsyncRetryPolicy RetryPolicy { get; set; }
        private MinioClient Client { get; set; }

        public MinioClientUtil()
        {
            Client = new MinioClient(
                TestExecutionConfig.MinioConfig.Endpoint,
                TestExecutionConfig.MinioConfig.AccessKey,
                TestExecutionConfig.MinioConfig.AccessToken);
            RetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
        }

        public async Task CreateBucket(string bucketName)
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    await Client.MakeBucketAsync(bucketName);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Bucket]  Exception: {e}");
                }
            });
        }

        public async Task AddFileToStorage(string fileLocation, string bucketName, string objectName)
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    byte[] bs = File.ReadAllBytes(fileLocation);
                    using (MemoryStream filestream = new MemoryStream(bs))
                    {
                        FileInfo fileInfo = new FileInfo(fileLocation);
                        var metaData = new Dictionary<string, string>
                    {
                                { "Test-Metadata", "Test  Test" }
                    };
                        await Client.PutObjectAsync(
                            bucketName,
                            objectName,
                            fileLocation,
                            "application/octet-stream",
                            metaData);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Bucket]  Exception: {e}");
                }
            });
        }

        public async Task GetFile(string bucketName, string objectName, string fileName)
        {
            await Client.GetObjectAsync(bucketName, objectName, fileName);
        }

        public async Task DeleteBucket(string bucketName)
        {
            bool found = await Client.BucketExistsAsync(bucketName);
            if (found)
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    await Client.RemoveBucketAsync(bucketName);
                });
            }
        }

        public async Task RemoveObjects(string bucketName, string objectName)
        {
            bool found = await Client.BucketExistsAsync(bucketName);
            if (found)
            {
                await Client.RemoveObjectAsync(bucketName, objectName);
            }
        }
    }
}

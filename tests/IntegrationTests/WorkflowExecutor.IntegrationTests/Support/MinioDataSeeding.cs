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

using System.Reflection;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.POCO;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support;
using TechTalk.SpecFlow.Infrastructure;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.Support
{
    public class MinioDataSeeding
    {
        private MinioClientUtil MinioClient { get; set; }

        private DataHelper DataHelper { get; set; }

        private ISpecFlowOutputHelper OutputHelper { get; set; }

        public MinioDataSeeding(MinioClientUtil minioClient, DataHelper dataHelper, ISpecFlowOutputHelper outputHelper)
        {
            MinioClient = minioClient;
            DataHelper = dataHelper;
            OutputHelper = outputHelper;
        }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task SeedArtifactRepo(string payloadId, string? folderName = null)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

        }

        public async Task SeedWorkflowInputArtifacts(string payloadId, string? folderName = null)
        {
            string localPath;

            if (string.IsNullOrEmpty(folderName))
            {
                OutputHelper.WriteLine($"folderName not specified. Seeding Minio with objects from **/DICOMs/full_patient_metadata/dcm");

                localPath = Path.Combine(GetDirectory() ?? "", "DICOMs", "full_patient_metadata", "dcm");
            }
            else
            {
                OutputHelper.WriteLine($"Seeding Minio with artifacts from **/DICOMs/{folderName}/dcm");

                localPath = Path.Combine(GetDirectory() ?? "", "DICOMs", folderName, "dcm");
            }

            OutputHelper.WriteLine($"Seeding objects to {TestExecutionConfig.MinioConfig.Bucket}/{payloadId}/dcm");
            await MinioClient.AddFileToStorage(localPath, $"{payloadId}/dcm");
            OutputHelper.WriteLine($"Objects seeded");
        }

        public async Task SeedArtifactRecieviedArtifact(string payloadId)
        {
            var localPath = Path.Combine(GetDirectory() ?? "", "DICOMs", "full_patient_metadata", "dcm");

            await MinioClient.AddFileToStorage(localPath, $"path");
        }

        public async Task SeedTaskOutputArtifacts(string payloadId, string workflowInstanceId, string executionId, string? folderName = null)
        {
            string localPath;

            if (string.IsNullOrEmpty(folderName))
            {
                OutputHelper.WriteLine($"folderName not specified. Seeding Minio with objects from **/DICOMs/output_metadata/dcm");

                localPath = Path.Combine(GetDirectory() ?? "", "DICOMs", "output_metadata", "dcm");
            }
            else
            {
                OutputHelper.WriteLine($"Seeding Minio with objects from **/DICOMs/{folderName}/dcm");

                localPath = Path.Combine(GetDirectory() ?? "", "DICOMs", folderName, "dcm");
            }

            OutputHelper.WriteLine($"Seeding objects to {TestExecutionConfig.MinioConfig.Bucket}/{payloadId}/workflows/{workflowInstanceId}/{executionId}/");
            await MinioClient.AddFileToStorage(localPath, $"{payloadId}/workflows/{workflowInstanceId}/{executionId}/");
            OutputHelper.WriteLine($"Objects seeded");
        }

        private string? GetDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}

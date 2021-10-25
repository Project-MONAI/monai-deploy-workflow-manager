// Copyright 2021 MONAI Consortium
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkloadManager.Contracts.Grpc;
using Monai.Deploy.WorkloadManager.Core.Services.Http;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkloadManager.Core.Test.Services.Http
{
    [Collection(TestServerFixture.ApiIntegration)]
    public class FileServiceTest
    {
        private readonly GrpcChannel _channel;
        private readonly Mock<ILogger<FileService>> _logger;
        private readonly Fixture _fixture;

        public FileServiceTest(TestServerFixture testServerFixture)
        {
            _channel = testServerFixture.GrpcChannel;
            _logger = new Mock<ILogger<FileService>>();
            _fixture = new Fixture();
        }

        [Fact(DisplayName = "FileServiceTest - Constructor Test")]
        public void ConstructorTest()
        {
            Assert.Throws<ArgumentNullException>(() => new FileService(null));
        }

        [Fact(DisplayName = "Download - Unimplemented")]
        public async Task Download_Unimplemented()
        {
            var request = _fixture.Create<FileDownloadRequest>();
            var client = new File.FileClient(_channel);

            await Assert.ThrowsAsync<RpcException>(async () =>
            {
                var call = client.Download(request);

                while (await call.ResponseStream.MoveNext())
                {
                    var response = call.ResponseStream.Current;
                }
            });
        }

        [Fact(DisplayName = "Upload - Unimplemented")]
        public async Task Upload_Unimplemented()
        {
            var requests = _fixture.Create<List<FileUploadRequest>>();
            var client = new File.FileClient(_channel);

            await Assert.ThrowsAsync<RpcException>(async () =>
            {
                using (var call = client.Upload())
                {
                    foreach (var request in requests)
                    {
                        await call.RequestStream.WriteAsync(request);
                    }
                    await call.RequestStream.CompleteAsync();
                    var response = await call.ResponseAsync;
                }
            });
        }
    }
}

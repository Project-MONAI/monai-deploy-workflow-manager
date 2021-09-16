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

using AutoFixture;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkloadManager.Contracts.Grpc;
using Monai.Deploy.WorkloadManager.Services.Http;
using Monai.Deploy.WorkloadManager.Test.Services.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Monai.Deploy.WorkloadManager.Test.Services.Http
{
    [Collection(TestServerFixture.ApiIntegration)]
    public class PayloadServiceTest
    {
        private readonly GrpcChannel channel;
        private readonly Mock<ILogger<PayloadService>> logger;
        private readonly Fixture fixture;

        public PayloadServiceTest(TestServerFixture testServerFixture)
        {
            this.channel = testServerFixture.GrpcChannel;
            this.logger = new Mock<ILogger<PayloadService>>();
            this.fixture = new Fixture();
        }

        [Fact(DisplayName = "PayloadServiceTest - Constructor Test")]
        public void ConstructorTest()
        {
            Assert.Throws<ArgumentNullException>(() => new PayloadService(null));
        }

        [Fact(DisplayName = "Download - Unimplemented")]
        public async Task Download_Unimplemented()
        {
            var request = fixture.Create<PayloadDownloadRequest>();
            var client = new Payload.PayloadClient(this.channel);

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
            var requests = fixture.Create<List<PayloadUploadRequest>>();
            var client = new Payload.PayloadClient(this.channel);

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

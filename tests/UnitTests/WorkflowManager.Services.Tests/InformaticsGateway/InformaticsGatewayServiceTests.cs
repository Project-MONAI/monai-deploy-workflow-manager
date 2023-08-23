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

using System.Net;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Exceptions;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Services.InformaticsGateway;
using Moq;
using Moq.Protected;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.Services.Tests.InformaticsGateway
{
    public class InformaticsGatewayServiceTests
    {
        private IInformaticsGatewayService InformaticsGatewayService { get; }

        private readonly Mock<HttpMessageHandler> _httpMessageHandler;

        private readonly IOptions<InformaticsGatewayConfiguration> _options;

        public InformaticsGatewayServiceTests()
        {
            var httpClientFactory = new Mock<IHttpClientFactory>();

            _httpMessageHandler = new Mock<HttpMessageHandler>();

            var httpClient = new Mock<HttpClient>(_httpMessageHandler.Object);

            httpClientFactory.Setup(w => w.CreateClient(It.IsAny<string>())).Returns(httpClient.Object);

            _options = Options.Create(new InformaticsGatewayConfiguration() { ApiHost = "https://localhost:5010", Username = "username", Password = "password" });

            InformaticsGatewayService = new InformaticsGatewayService(httpClientFactory.Object, _options);
        }

        [Fact]
        public async Task OriginsExist_InvalidSource_ReturnsFalse()
        {
            var source = "invalid_source";

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                });

            var result = await InformaticsGatewayService.OriginExists(source);

            var expected = false;

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task OriginsExist_ExternalServiceError_ThrowsMonaiInternalErrorException()
        {
            var source = "any_source";

            await Assert.ThrowsAsync<MonaiInternalServerException>(async () => await InformaticsGatewayService.OriginExists(source));
        }

        [Fact]
        public async Task OriginsExist_ValidSource_ReturnsTrue()
        {
            var source = "valid_source";

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                });

            var result = await InformaticsGatewayService.OriginExists(source);

            var expected = true;

            Assert.Equal(expected, result);
        }
    }
}

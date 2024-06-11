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

using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.Tests
{
    public class ArgoProviderTest
    {
        [Fact(DisplayName = "Generates Argo client with API Token")]
        public async Task GeneratesArgoClientWithApiToken()
        {
            var logger = new Mock<ILogger<ArgoProvider>>();
            var httpFactory = new Mock<IHttpClientFactory>();
            var baseUri = "http://some-uri/";
            var version = new Version
            {
                BuildDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                Compiler = "compiler",
                GitCommit = "commit",
                GitTag = "tag",
                GitTreeState = "state",
                GoVersion = "go",
                Platform = "platform",
                Version1 = "version1"
            };
            var token = Guid.NewGuid().ToString();

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(version)) });

            var httpClient = new HttpClient(handlerMock.Object);

            logger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            httpFactory.Setup(p => p.CreateClient(It.IsAny<string>())).Returns(httpClient);
            var argo = new ArgoProvider(logger.Object, httpFactory.Object, new Mock<ILoggerFactory>().Object);

            var client = argo.CreateClient(baseUri, token) as ArgoClient;

            Assert.NotNull(client);
            Assert.Equal(baseUri.ToString(), client!.BaseUrl);

            _ = await client!.Argo_GetVersionAsync().ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1),
               ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get && CheckToken(req.Headers.Authorization, token)),
               ItExpr.IsAny<CancellationToken>());
        }

        private static bool CheckToken(AuthenticationHeaderValue? authorization, string token)
        {
            return authorization is not null &&
                authorization.Scheme.Equals("Bearer", StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(authorization.Parameter) &&
                authorization.Parameter.Equals(token, StringComparison.Ordinal);
        }
    }
}

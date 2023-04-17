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

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.Tests
{
    public class ArgoClientTest
    {
        private readonly Mock<HttpClient> _mockHttpClient;
        ArgoClient ArgoClient { get; set; }

        public ArgoClientTest()
        {
            _mockHttpClient = new Mock<HttpClient>();
            ArgoClient = new ArgoClient(_mockHttpClient.Object);
        }

        [Fact(DisplayName = "Argo_DeleteWorkflowTemplateAsync - Calls Send Async With Delete")]
        public async Task Argo_DeleteWorkflowTemplateAsync()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            var result = await ArgoClient.Argo_DeleteWorkflowTemplateAsync("test", "test", new CancellationToken(false));
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Delete)));

            Assert.True(result);
        }

        [Fact(DisplayName = "Argo_GetVersionAsync - Calls Send Async With Get")]
        public async Task Argo_GetVersionAsync()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            var result = await ArgoClient.Argo_GetVersionAsync();
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get)));

            Assert.NotNull(result);
        }
    }
}

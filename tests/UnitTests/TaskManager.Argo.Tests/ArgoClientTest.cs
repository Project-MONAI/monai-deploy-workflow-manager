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

using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.Tests
{
    public class ArgoClientTest
    {
        [Fact(DisplayName = "Argo_DeleteWorkflowTemplateAsync - Calls Send Async With Delete")]
        public async Task Argo_DeleteWorkflowTemplateAsync()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            var httpclient = new HttpClient(mockHttpMessageHandler.Object);

            ArgoClient ArgoClient = new(httpclient);

            var result = await ArgoClient.Argo_DeleteWorkflowTemplateAsync("test", "test", new CancellationToken(false));

            Assert.True(result);
        }

        [Fact(DisplayName = "Argo_GetVersionAsync - Calls Send Async With Get")]
        public async Task Argo_GetVersionAsync()
        {
            var ver = new Argo.Version()
            {
                BuildDate = "date",
                Compiler = "compilse",
                GitCommit = "commit",
                GitTag = "tag",
                GitTreeState = "state",
                GoVersion = "gover",
            };
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(ver);
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StreamContent(stream) });

            var httpclient = new HttpClient(mockHttpMessageHandler.Object);

            ArgoClient ArgoClient = new(httpclient);

            var result = await ArgoClient.Argo_GetVersionAsync();

            Assert.NotNull(result);
            Assert.Equal(ver.GitCommit, result.GitCommit);
            Assert.Equal(ver.BuildDate, result.BuildDate);
            Assert.Equal(ver.Compiler, result.Compiler);
            Assert.Equal(ver.GitTag, result.GitTag);
            Assert.Equal(ver.GitCommit, result.GitCommit);
            Assert.Equal(ver.GitTreeState, result.GitTreeState);
        }
    }
}

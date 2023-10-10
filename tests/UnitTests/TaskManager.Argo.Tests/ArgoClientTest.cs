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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Argo;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.Tests
{
    public class ArgoClientTest
    {
        private readonly Mock<ILoggerFactory> _loggerMock = new Mock<ILoggerFactory>();

        public ArgoClientTest()
        {
            _loggerMock.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);
        }

        [Fact(DisplayName = "Argo_DeleteWorkflowTemplateAsync - Calls Send Async With Delete")]
        public async Task Argo_DeleteWorkflowTemplateAsync()
        {
            _loggerMock.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            var httpclient = new HttpClient(mockHttpMessageHandler.Object);

            ArgoClient argoClient = new(httpclient, _loggerMock.Object);

            var result = await argoClient.Argo_DeleteWorkflowTemplateAsync("test", "test", new CancellationToken(false));

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

            ArgoClient argoClient = new(httpclient, _loggerMock.Object);

            var result = await argoClient.Argo_GetVersionAsync();

            Assert.NotNull(result);
            Assert.Equal(ver.GitCommit, result.GitCommit);
            Assert.Equal(ver.BuildDate, result.BuildDate);
            Assert.Equal(ver.Compiler, result.Compiler);
            Assert.Equal(ver.GitTag, result.GitTag);
            Assert.Equal(ver.GitCommit, result.GitCommit);
            Assert.Equal(ver.GitTreeState, result.GitTreeState);
        }

        [Fact(DisplayName = "Argo_CreateWorkflowAsync")]
        public async Task Argo_CreateWorkflowAsync()
        {
            var data = "{ metadata:{}, spec: {} }";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StreamContent(stream) });

            var httpclient = new HttpClient(mockHttpMessageHandler.Object);

            ArgoClient argoClient = new(httpclient, _loggerMock.Object);

            var result = await argoClient.Argo_CreateWorkflowAsync(
                "argo",
                new WorkflowCreateRequest { Namespace = "argo", Workflow = new Workflow() },
                CancellationToken.None);

            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Argo_GetWorkflowAsync")]
        public async Task Argo_GetWorkflowAsync()
        {
            var data = "{ metadata:{}, spec: {} }";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StreamContent(stream) });

            var httpclient = new HttpClient(mockHttpMessageHandler.Object);

            ArgoClient argoClient = new(httpclient, _loggerMock.Object);

            var result = await argoClient.Argo_GetWorkflowAsync("argo", "name", "version", "fields",
                CancellationToken.None);

            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Argo_StopWorkflowAsync")]
        public async Task Argo_StopWorkflowAsync()
        {
            var data = "{ metadata:{}, spec: {} }";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StreamContent(stream) });

            var httpclient = new HttpClient(mockHttpMessageHandler.Object);

            ArgoClient argoClient = new(httpclient, _loggerMock.Object);

            var result = await argoClient.Argo_StopWorkflowAsync("argo", "name",
                new WorkflowStopRequest { Namespace = "argo" });

            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Argo_TerminateWorkflowAsync")]
        public async Task Argo_TerminateWorkflowAsync()
        {
            var data = "{ metadata:{}, spec: {} }";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StreamContent(stream) });

            var httpclient = new HttpClient(mockHttpMessageHandler.Object);

            ArgoClient argoClient = new(httpclient, _loggerMock.Object);

            var result = await argoClient.Argo_TerminateWorkflowAsync("argo", "name",
                new WorkflowTerminateRequest { Namespace = "argo" });

            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Argo_GetWorkflowTemplateAsync")]
        public async Task Argo_GetWorkflowTemplateAsync()
        {
            var data = "{ metadata:{}, spec: {} }";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StreamContent(stream) });

            var httpclient = new HttpClient(mockHttpMessageHandler.Object);

            ArgoClient argoClient = new(httpclient, _loggerMock.Object);

            var result = await argoClient.Argo_GetWorkflowTemplateAsync("argo", "name", "options");

            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Argo_Get_WorkflowLogsAsync")]
        public async Task Argo_Get_WorkflowLogsAsync()
        {
            var data = "{ metadata:{}, spec: {} }";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StreamContent(stream) });

            var httpclient = new HttpClient(mockHttpMessageHandler.Object);

            ArgoClient argoClient = new(httpclient, _loggerMock.Object);

            var result = await argoClient.Argo_Get_WorkflowLogsAsync("argo", "name", "pod", "options");

            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Argo_CreateWorkflowTemplateAsync")]
        public async Task Argo_CreateWorkflowTemplateAsync()
        {
            var data = "{ metadata:{}, spec: {} }";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StreamContent(stream) });

            var httpclient = new HttpClient(mockHttpMessageHandler.Object);

            ArgoClient argoClient = new(httpclient, _loggerMock.Object);

            var result = await argoClient.Argo_CreateWorkflowTemplateAsync("argo",
                new WorkflowTemplateCreateRequest { Namespace = "argo", Template = new WorkflowTemplate() },
                CancellationToken.None);

            Assert.NotNull(result);
        }

        [Fact(DisplayName = "DecodeLogs")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task DecodeLogs()
        {
            var result = BaseArgoClient.DecodeLogs("");

            Assert.NotNull(result);
        }

        [Fact(DisplayName = "ConvertToString")]
        public async Task ConvertToString()
        {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            //bool
            var result = ArgoClient.ConvertToString(true, CultureInfo.InvariantCulture);
            Assert.Equal("true", result);

            //enum
            result = ArgoClient.ConvertToString(MyEnum.Zero, CultureInfo.InvariantCulture);
            Assert.Equal("0", result);

            //enum
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            result = ArgoClient.ConvertToString(null, CultureInfo.InvariantCulture);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Equal("", result);


            //byte array
            var data = new byte[3];
            data[0] = byte.MinValue;
            data[1] = 0;
            data[2] = byte.MaxValue;
            result = ArgoClient.ConvertToString(data, CultureInfo.InvariantCulture);
            Assert.Equal(Convert.ToBase64String(data), result);

            //string array
            var data2 = new string[3];
            data2[0] = "1";
            data2[1] = "2";
            data2[2] = "3";
            result = ArgoClient.ConvertToString(data2, CultureInfo.InvariantCulture);
            Assert.Equal(string.Join(",", data2), result);
        }

        enum MyEnum
        {
            Zero
        }
    }
}


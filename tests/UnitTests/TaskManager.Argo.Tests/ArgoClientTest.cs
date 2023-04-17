using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
        public async void Argo_DeleteWorkflowTemplateAsync()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            bool result = await ArgoClient.Argo_DeleteWorkflowTemplateAsync("test", "test", new CancellationToken(false));
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Delete)));

            Assert.True(result);
        }

        [Fact(DisplayName = "Argo_GetVersionAsync - Calls Send Async With Get")]
        public async void Argo_GetVersionAsync()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            var result = await ArgoClient.Argo_GetVersionAsync();
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Delete);
        }

    }
}

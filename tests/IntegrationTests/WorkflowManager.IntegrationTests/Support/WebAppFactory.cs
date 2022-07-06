// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{

    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override IHostBuilder CreateHostBuilder() =>
            base.CreateHostBuilder()
            .ConfigureHostConfiguration(
                config => config.AddCommandLine(new[] { "--test=yes" }));
    }

    public static class WebAppFactory
    {
        public static HttpClient SetupWorkflowManger()
        {
            var webApplicationFactory = new CustomWebApplicationFactory<Program>();

            return webApplicationFactory.CreateClient();
        }

        public static async Task<HttpResponseMessage> GetConsumers()
        {
            var httpClient = new HttpClient();

            var svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(TestExecutionConfig.RabbitConfig.User + ":" + TestExecutionConfig.RabbitConfig.Password));

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", svcCredentials);

            return await httpClient.GetAsync($"http://{TestExecutionConfig.RabbitConfig.Host}:{TestExecutionConfig.RabbitConfig.Port}/api/consumers");
        }
    }
}

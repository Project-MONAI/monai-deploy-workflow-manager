// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public static class WebAppFactory
    {
        public static void SetupWorkflowManger()
        {
            var webApplicationFactory = new WebApplicationFactory<Program>();

            _ = webApplicationFactory.CreateClient();
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

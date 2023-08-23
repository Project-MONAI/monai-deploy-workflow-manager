// 
// Copyright 2023 Guy’s and St Thomas’ NHS Foundation Trust
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.POCO;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support
{
    public static class WebAppFactory
    {
        public static WebApplicationFactory<Program> GetWebApplicationFactory() =>
            new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Test.json"));
                    });
                });

        public static async Task<HttpResponseMessage> GetQueueStatus(HttpClient httpClient, string vhost, string queue)
        {
            var svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(TestExecutionConfig.RabbitConfig.User + ":" + TestExecutionConfig.RabbitConfig.Password));

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", svcCredentials);

            return await httpClient.GetAsync($"http://{TestExecutionConfig.RabbitConfig.Host}:{TestExecutionConfig.RabbitConfig.WebPort}/api/queues/{vhost}/{queue}");
        }
    }
}

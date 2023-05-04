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
using Monai.Deploy.WorkflowManager.Configuration;

namespace Monai.Deploy.WorkflowManager.Services.InformaticsGateway
{
    public class InformaticsGatewayService : IInformaticsGatewayService
    {
        public InformaticsGatewayService(
            IHttpClientFactory httpClientFactory,
            IOptions<InformaticsGatewayConfiguration> options
        )
        {
            _httpClient = httpClientFactory?.CreateClient("Informatics-Gateway") ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _httpClient.BaseAddress = new Uri(_options.Value.ApiHost);
        }

        private readonly HttpClient _httpClient;

        private readonly IOptions<InformaticsGatewayConfiguration> _options;

        public async Task<bool> OriginExists(string name)
        {
            var response = await _httpClient.GetAsync($"/config/source/{name}");

            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}

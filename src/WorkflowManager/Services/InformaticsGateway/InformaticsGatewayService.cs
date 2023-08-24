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
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Exceptions;
using Monai.Deploy.WorkflowManager.Common.Configuration;

namespace Monai.Deploy.WorkflowManager.Common.Services.InformaticsGateway
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

            var authenticationString = $"{_options.Value.Username}:{_options.Value.Password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        }

        private readonly HttpClient _httpClient;

        private readonly IOptions<InformaticsGatewayConfiguration> _options;

        public async Task<bool> OriginExists(string aetitle)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/config/source/aetitle/{aetitle}");

                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                throw new MonaiInternalServerException($"An error occured when checking if the origin '{aetitle}' existed.", ex);
            }
        }
    }
}

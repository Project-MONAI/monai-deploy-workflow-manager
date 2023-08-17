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

using Ardalis.GuardClauses;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Monai.Deploy.TaskManager.Argo.Logging;

namespace Monai.Deploy.TaskManager.Argo
{
#pragma warning disable CA1054 // URI-like parameters should not be strings

    public class ArgoProvider : IArgoProvider
    {
        private readonly ILogger<ArgoProvider> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ArgoProvider(ILogger<ArgoProvider> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public IArgoClient CreateClient(string baseUrl, string? apiToken, bool allowInsecure = true)
        {
            Guard.Against.NullOrWhiteSpace(baseUrl, nameof(baseUrl));

            _logger.CreatingArgoClient(baseUrl);

            var ClientName = allowInsecure is true ? "Argo-Insecure" : "Argo";

            var httpClient = _httpClientFactory.CreateClient(ClientName);

            Guard.Against.Null(httpClient, nameof(httpClient));

            if (apiToken is not null)
            {
                httpClient.SetBearerToken(apiToken);
            }
            return new ArgoClient(httpClient) { BaseUrl = baseUrl };
        }
    }

#pragma warning restore CA1054 // URI-like parameters should not be strings
}

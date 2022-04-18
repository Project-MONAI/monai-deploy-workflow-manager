// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Argo;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Logging;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
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

        public IArgoClient CreateClient(string baseUrl, string? apiToken)
        {
            Guard.Against.NullOrWhiteSpace(baseUrl, nameof(baseUrl));

            _logger.CreatingArgoClient(baseUrl);
            var httpClient = _httpClientFactory.CreateClient();
            if (apiToken is not null)
            {
                httpClient.SetBearerToken(apiToken);
            }
            return new ArgoClient(httpClient) { BaseUrl = baseUrl };
        }
    }
#pragma warning restore CA1054 // URI-like parameters should not be strings
}

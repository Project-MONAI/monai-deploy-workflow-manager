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
        private readonly IHttpClientFactory _clientFactory;

        public ArgoProvider(ILogger<ArgoProvider> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientFactory = httpClientFactory;
        }

        public IArgoClient CreateClient(string baseUrl, string? apiToken, bool allowInsecure = true)
        {
            Guard.Against.NullOrWhiteSpace(baseUrl, nameof(baseUrl));

            _logger.CreatingArgoClient(baseUrl);

            var ClientName = allowInsecure is true ? "Argo-Insecure" : "Argo";

            var httpClient = _clientFactory.CreateClient(ClientName);

            Guard.Against.Null(httpClient);

            if (apiToken is not null)
            {
                httpClient.SetBearerToken(apiToken);
            }
            return new ArgoClient(httpClient) { BaseUrl = baseUrl };
        }


    }

#pragma warning restore CA1054 // URI-like parameters should not be strings
}

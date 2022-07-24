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
        private readonly HttpClientHandler? _handler;
        private readonly IHttpClientFactory _clientFactory;

        public ArgoProvider(ILogger<ArgoProvider> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientFactory = httpClientFactory;
        }

        public IArgoClient CreateClient(string baseUrl, string? apiToken, bool allowInsecure = true)
        {
            Guard.Against.NullOrWhiteSpace(baseUrl, nameof(baseUrl));
            Guard.Against.Null(_handler);

            _logger.CreatingArgoClient(baseUrl);

            if (allowInsecure)
            {
                _handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                _handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };
            }

            var httpClient = new HttpClient(_handler);
            if (apiToken is not null)
            {
                httpClient.SetBearerToken(apiToken);
            }
            return new ArgoClient(httpClient) { BaseUrl = baseUrl };
        }


    }

#pragma warning restore CA1054 // URI-like parameters should not be strings
}

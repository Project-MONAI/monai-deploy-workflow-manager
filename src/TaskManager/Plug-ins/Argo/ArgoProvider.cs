// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Argo;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Logging;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    public class ArgoProvider : IArgoProvider
    {
        private readonly ILogger<ArgoProvider> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ArgoProvider(ILogger<ArgoProvider> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public ArgoClient CreateClient(Uri baseUrl)
        {
            _logger.CreatingArgoClient(baseUrl);
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = baseUrl;
            return new ArgoClient(httpClient);
        }
    }
}

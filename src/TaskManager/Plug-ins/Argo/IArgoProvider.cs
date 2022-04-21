// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Argo;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    public interface IArgoProvider
    {
        /// <summary>
        /// Creates an instance of the <see cref="ArgoClient" />
        /// </summary>
        /// <param name="baseUrl">The base URL of the Argo service.</param>
        /// <param name="apiToken">Token for accessing Argo API.</param>
        /// <returns></returns>
#pragma warning disable CA1054 // URI-like parameters should not be strings

        IArgoClient CreateClient(string baseUrl, string? apiToken);

#pragma warning restore CA1054 // URI-like parameters should not be strings
    }
}

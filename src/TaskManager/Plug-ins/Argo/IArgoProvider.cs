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

        IArgoClient CreateClient(string baseUrl, string? apiToken, bool allowInsecure = true);

#pragma warning restore CA1054 // URI-like parameters should not be strings
    }
}

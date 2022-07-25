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

namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    public interface IMetadataRepository : IDisposable
    {
        /// <summary>
        /// Retrieves metadata for the current plugin.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="Task<Dictionary<string, object>>"/>metadata dictionary</returns>
        Task<Dictionary<string, object>> RetrieveMetadata(CancellationToken cancellationToken = default);
    }
}

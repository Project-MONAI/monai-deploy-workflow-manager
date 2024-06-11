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

using System.Collections.Generic;
using System.Threading.Tasks;
using Artifact = Monai.Deploy.Messaging.Common.Artifact;

namespace Monai.Deploy.WorkflowManager.Common.Database.Repositories
{
    public interface IArtifactsRepository
    {
        /// <summary>
        /// Gets All ArtifactsReceivedItems by workflowInstance and taskId.
        /// </summary>
        /// <param name="workflowInstance"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task<List<ArtifactReceivedItems>?> GetAllAsync(string workflowInstance, string taskId);

        /// <summary>
        /// Adds an item to the ArtifactsReceivedItems collection.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task AddItemAsync(ArtifactReceivedItems item);

        /// <summary>
        /// Adds an item to the ArtifactsReceivedItems collection.
        /// </summary>
        /// <param name="workflowInstanceId"></param>
        /// <param name="taskId"></param>
        /// <param name="artifactsOutputs"></param>
        /// <returns></returns>
        Task AddItemAsync(string workflowInstanceId, string taskId, List<Artifact> artifactsOutputs);

        Task AddOrUpdateItemAsync(string workflowInstanceId, string taskId,
            IEnumerable<Artifact> artifactsOutputs);
    }
}

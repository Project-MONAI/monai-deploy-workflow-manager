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

namespace Monai.Deploy.WorkflowManager.Common.Services.InformaticsGateway
{
    public interface IInformaticsGatewayService
    {
        /// <summary>
        /// Checks if a data origin exists with the informatics gateway.
        /// </summary>
        /// <param name="name">Name of the source.</param>
        /// <returns>bool based on success status on the informatics gateway request.</returns>
        Task<bool> OriginExists(string name);
    }
}

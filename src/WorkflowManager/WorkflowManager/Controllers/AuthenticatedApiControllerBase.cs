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

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Configuration;

namespace Monai.Deploy.WorkflowManager.Common.ControllersShared
{
    /// <summary>
    /// Base authenticated api controller base.
    /// </summary>
    [Authorize]
    public class AuthenticatedApiControllerBase : PaginationApiControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatedApiControllerBase"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public AuthenticatedApiControllerBase(IOptions<WorkflowManagerOptions> options)
            : base(options)
        {
        }
    }
}

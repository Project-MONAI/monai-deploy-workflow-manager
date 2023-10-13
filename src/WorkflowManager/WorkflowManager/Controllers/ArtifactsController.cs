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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.ControllersShared
{
    /// <summary>
    /// Artifacts Controller
    /// </summary>
    [ApiController]
    [Route("artifacts/")]
    public class ArtifactsController : ApiControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactsController"/> class.
        /// </summary>
        public ArtifactsController()
        {
        }

        /// <summary>
        /// Get Artifact Types
        /// </summary>
        /// <returns>List of supported artifact types.</returns>
        [HttpGet]
        [Route("types")]
        [ProducesResponseType(typeof(List<Payload>), StatusCodes.Status200OK)]
        public IActionResult GetArtifactTypes()
        {
            return Ok(ArtifactTypes.ListOfModularity);
        }
    }
}

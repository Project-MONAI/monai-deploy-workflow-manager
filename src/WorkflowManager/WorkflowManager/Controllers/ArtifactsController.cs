using System.Collections.Generic;
using System.Linq;
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

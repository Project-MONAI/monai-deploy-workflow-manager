using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.ControllersShared;
using Newtonsoft.Json;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.Test.Controllers
{
    public class ArtifactsControllerTests
    {
        private ArtifactsController ArtifactsController { get; }

        public ArtifactsControllerTests()
        {
            ArtifactsController = new ArtifactsController();
        }

        [Fact]
        public void GetArtifactTypesTest()
        {
            var result = ArtifactsController.GetArtifactTypes();
            Assert.NotNull(result);
            var ok = Assert.IsType<OkObjectResult>(result);
            var json = JsonConvert.SerializeObject(ok.Value);

            ok.Value.Should().BeEquivalentTo(ArtifactTypes.ListOfModularity);
        }
    }
}

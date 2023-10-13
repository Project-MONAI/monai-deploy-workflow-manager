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


using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Monai.Deploy.Messaging.Common;
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

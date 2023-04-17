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

using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.TaskManager.Argo;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Controllers;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Tests;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Test.Controllers
{
    public class TemplateControllerTests : ArgoPluginTestBase
    {
        private readonly Mock<ILogger<TemplateController>> _tempLogger;
        private readonly Mock<ILogger<ArgoPlugin>> _argoLogger;

        private TemplateController TemplateController { get; set; }

        public TemplateControllerTests()
        {
            _tempLogger = new Mock<ILogger<TemplateController>>();
            _argoLogger = new Mock<ILogger<ArgoPlugin>>();
        }

        [Fact(DisplayName = "CreateArgoTemplate - ReturnsOk")]
        public async Task CreateArgoTemplate_Controller_ReturnsOk()
        {
            var data = "{}";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = stream;
            httpContext.Request.ContentLength = stream.Length;
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            TemplateController = new TemplateController(
                ServiceScopeFactory.Object,
                _tempLogger.Object,
                _argoLogger.Object,
                Options)
            {
                ControllerContext = controllerContext
            };

            var result = await TemplateController.CreateArgoTemplate();

            Assert.IsType<ActionResult<Argo.WorkflowTemplate>>(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        }

        [Fact(DisplayName = "DeleteArgoTemplate - ReturnsOk")]
        public async Task DeleteArgoTemplate_Controller_ReturnsOk()
        {
            TemplateController = new TemplateController(
                ServiceScopeFactory.Object,
                _tempLogger.Object,
                _argoLogger.Object,
                Options);

            var result = await TemplateController.DeleteArgoTemplate("template");

            Assert.IsType<ActionResult<bool>>(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        }
    }
}

﻿/*
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
using System.Text;
using Argo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Configuration;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("argo/[controller]")]
    public class TemplateController : ControllerBase
    {
        private readonly ArgoPlugin _argoPlugin;
        private readonly ILogger<TemplateController> _tempLogger;

        public TemplateController(
            IServiceScopeFactory scopeFactory,
            ILogger<TemplateController> tempLogger,
            ILogger<ArgoPlugin> argoLogger,
            IOptions<WorkflowManagerOptions> options)
        {
            _tempLogger = tempLogger;

            _argoPlugin = new ArgoPlugin(scopeFactory, argoLogger, options, new Messaging.Events.TaskDispatchEvent());


        }

        [HttpPost]
        public async Task<ActionResult<WorkflowTemplate>> CreateArgoTemplate()
        {
            using StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8);

            var value2 = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(value2))
            {
                return BadRequest("No file recieved");
            }
            WorkflowTemplate? workflowTemplate = null;
            try
            {
                workflowTemplate = await _argoPlugin.CreateArgoTemplate(value2);
            }
            catch (Exception)
            {
                return BadRequest("message: Argo unable to process template");
            }


            return Ok(workflowTemplate);
        }

        [Route("{name}")]
        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteArgoTemplate(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("No name parameter provided");
            }

            try
            {
                return Ok(await _argoPlugin.DeleteArgoTemplate(name));
            }
            catch (Exception)
            {
                return BadRequest("message: Argo unable to process template");
            }
        }
    }
}

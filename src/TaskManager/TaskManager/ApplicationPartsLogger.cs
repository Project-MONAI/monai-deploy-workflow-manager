/*
 * Copyright 2023 MONAI Consortium
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

using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Monai.Deploy.WorkflowManager.TaskManager
{
    public class ApplicationPartsLogger : IHostedService
    {
        private readonly ILogger<ApplicationPartsLogger> _logger;
        private readonly ApplicationPartManager _partManager;

        public ApplicationPartsLogger(ILogger<ApplicationPartsLogger> logger, ApplicationPartManager partManager)
        {
            _logger = logger;
            _partManager = partManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Get the names of all the application parts. This is the short assembly name for AssemblyParts
            var applicationParts = _partManager.ApplicationParts.Select(x => x.Name);

            // Create a controller feature, and populate it from the application parts
            var controllerFeature = new ControllerFeature();
            _partManager.PopulateFeature(controllerFeature);

            // Get the names of all of the controllers
            var controllers = controllerFeature.Controllers.Select(x => x.Name);

            // Log the application parts and controllers
            _logger.LogInformation(
                "Found the following application parts: '{ApplicationParts}' with the following controllers: '{Controllers}'",
                string.Join(", ", applicationParts),
                string.Join(", ", controllers));

            return Task.CompletedTask;
        }

        // Required by the interface
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

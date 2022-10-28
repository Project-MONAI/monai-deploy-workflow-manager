/*
 * Copyright 2021-2022 MONAI Consortium
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

using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Services;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManger.Common.Tests.Services
{
    public class WorkflowServiceTests
    {
        private IWorkflowService WorkflowService { get; set; }

        private readonly Mock<IWorkflowRepository> _workflowRepository;
        private readonly Mock<ILogger<WorkflowService>> _logger;

        public WorkflowServiceTests()
        {
            _workflowRepository = new Mock<IWorkflowRepository>();
            _logger = new Mock<ILogger<WorkflowService>>();

            WorkflowService = new WorkflowService(_workflowRepository.Object, _logger.Object);
        }

        [Fact]
        public async Task WorkflowService_NullWorkflow_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => WorkflowService.UpdateAsync(null, null));
        }

        [Fact]
        public async Task WorkflowService_NoExistingWorkflow_ReturnsNull()
        {
            var result = await WorkflowService.UpdateAsync(new Workflow(), Guid.NewGuid().ToString());

            Assert.Null(result);
        }

        [Fact]
        public async Task WorkflowService_WorkflowExists_ReturnsWorkflowId()
        {
            var workflowRevision = new WorkflowRevision
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                Revision = 1,
                Workflow = new Workflow
                {
                    Name = "Workflowname1",
                    Description = "Workflowdesc1",
                    Version = "1",
                    InformaticsGateway = new InformaticsGateway
                    {
                        AeTitle = "aetitle"
                    },
                    Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                }
            };

            _workflowRepository.Setup(w => w.GetByWorkflowIdAsync(workflowRevision.WorkflowId)).ReturnsAsync(workflowRevision);
            _workflowRepository.Setup(w => w.UpdateAsync(It.IsAny<Workflow>(), workflowRevision)).ReturnsAsync(workflowRevision.WorkflowId);

            var result = await WorkflowService.UpdateAsync(new Workflow(), workflowRevision.WorkflowId);

            Assert.Equal(workflowRevision.WorkflowId, result);
        }
    }
}

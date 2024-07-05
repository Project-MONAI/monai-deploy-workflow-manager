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

using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Moq;
using Xunit;
using MongoDB.Driver;

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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            await Assert.ThrowsAsync<ArgumentNullException>(() => WorkflowService.UpdateAsync(null, null));
        }

        [Fact]
        public async Task WorkflowService_NoExistingWorkflow_ReturnsNull()
        {
            var result = await WorkflowService.UpdateAsync(new Workflow(), Guid.NewGuid().ToString());

            Assert.Null(result);
        }

        [Fact]
        public async Task WorkflowService_GetAsync_With_Empty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => WorkflowService.GetAsync(string.Empty));
        }

        [Fact]
        public async Task WorkflowService_GetByNameAsync_With_Empty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => WorkflowService.GetByNameAsync(string.Empty));
        }

        [Fact]
        public async Task WorkflowService_CreateAsync_With_Empty()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => WorkflowService.CreateAsync(null));
        }

        [Fact]
        public async Task WorkflowService_CreateAsync_With_ValidReturn()
        {
            var expectedResult = "1";
            _workflowRepository.Setup(w => w.CreateAsync(It.IsAny<Workflow>())).ReturnsAsync(expectedResult);
            var tasks = new TaskObject[] { new TaskObject() };
            var result = await WorkflowService.CreateAsync(new Workflow() { Name = "workflow1", Tasks = tasks });

            Assert.Equal(expectedResult, result);
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

            var result = await WorkflowService.UpdateAsync(new Workflow(), workflowRevision.WorkflowId, true);

            Assert.Equal(workflowRevision.WorkflowId, result);
        }

        [Fact]
        public async Task WorkflowService_DeleteWorkflow_With_Empty()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => WorkflowService.DeleteWorkflowAsync(null));
        }

        [Fact]
        public async Task WorkflowService_DeleteWorkflow_Calls_SoftDelete()
        {
            var result = await WorkflowService.DeleteWorkflowAsync(new WorkflowRevision());
            _workflowRepository.Verify(r => r.SoftDeleteWorkflow(It.IsAny<WorkflowRevision>()), Times.Once());
        }

        [Fact]
        public async Task WorkflowService_Count_Calls_Count()
        {
            var result = await WorkflowService.CountAsync();
            _workflowRepository.Verify(r => r.CountAsync(Builders<WorkflowRevision>.Filter.Empty), Times.Once());
        }

        [Fact]
        public async Task WorkflowService_GetCountByAeTitleAsync_Calls_Count()
        {
            var result = await WorkflowService.GetCountByAeTitleAsync("string");
            _workflowRepository.Verify(r => r.GetCountByAeTitleAsync(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task WorkflowService_GetAllAsync_Calls_GetAllAsync()
        {
            var result = await WorkflowService.GetAllAsync(1, 2);
            _workflowRepository.Verify(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

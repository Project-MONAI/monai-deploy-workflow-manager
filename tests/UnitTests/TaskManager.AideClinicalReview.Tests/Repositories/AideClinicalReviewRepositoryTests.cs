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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Repositories;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Tests.Repositories
{
    public class AideClinicalReviewMetadataRepositoryTests
    {
        private readonly Mock<ILogger<AideClinicalReviewMetadataRepository>> _logger;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
        private readonly Mock<IServiceScope> _serviceScope;
        private readonly Mock<IStorageService> _storageService;

        public AideClinicalReviewMetadataRepositoryTests()
        {
            _logger = new Mock<ILogger<AideClinicalReviewMetadataRepository>>();
            _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            _storageService = new Mock<IStorageService>();
            _serviceScope = new Mock<IServiceScope>();

            _serviceScopeFactory.Setup(p => p.CreateScope()).Returns(_serviceScope.Object);

            _logger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        }

        [Fact(DisplayName = "Throws when missing required metadata arguments")]
        public void ArgoRepository_ThrowsWhenMissingPluginArguments()
        {
            var taskDispatch = GenerateTaskDispatchEvent();
            var taskCallback = GenerateTaskCallbackEvent();

            taskDispatch.PayloadId = "";

            Assert.Throws<ArgumentException>(() => new AideClinicalReviewMetadataRepository(_serviceScopeFactory.Object, _storageService.Object, _logger.Object, taskDispatch, taskCallback));
        }

        [Fact(DisplayName = "No metadata returns empty dictionary")]
        public async Task ArgoRepository_MetadataNotFound_ReturnsEmptyDict()
        {
            var taskDispatch = GenerateTaskDispatchEvent();
            var taskCallback = GenerateTaskCallbackEvent();

            var repository = new AideClinicalReviewMetadataRepository(_serviceScopeFactory.Object, _storageService.Object, _logger.Object, taskDispatch, taskCallback);

            var metadata = await repository.RetrieveMetadata();

            Assert.Equal(new Dictionary<string, object>(), metadata);
        }

        private static TaskCallbackEvent GenerateTaskCallbackEvent()
        {
            var message = new TaskCallbackEvent
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ExecutionId = Guid.NewGuid().ToString(),
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                TaskId = Guid.NewGuid().ToString(),
                Metadata = new Dictionary<string, object>(),
                Identity = Guid.NewGuid().ToString(),
            };
            message.Outputs.Add(new Messaging.Common.Storage
            {
                Name = "output",
                Endpoint = Guid.NewGuid().ToString(),
                Credentials = new Messaging.Common.Credentials
                {
                    AccessKey = Guid.NewGuid().ToString(),
                    AccessToken = Guid.NewGuid().ToString()
                },
                Bucket = Guid.NewGuid().ToString(),
                RelativeRootPath = Guid.NewGuid().ToString(),
            });
            return message;
        }

        private static TaskDispatchEvent GenerateTaskDispatchEvent()
        {
            var message = new TaskDispatchEvent
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ExecutionId = Guid.NewGuid().ToString(),
                TaskPluginType = Guid.NewGuid().ToString(),
                PayloadId = Guid.NewGuid().ToString(),
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                TaskId = Guid.NewGuid().ToString(),
                IntermediateStorage = new Messaging.Common.Storage
                {
                    Name = Guid.NewGuid().ToString(),
                    Endpoint = Guid.NewGuid().ToString(),
                    Credentials = new Messaging.Common.Credentials
                    {
                        AccessKey = Guid.NewGuid().ToString(),
                        AccessToken = Guid.NewGuid().ToString()
                    },
                    Bucket = Guid.NewGuid().ToString(),
                    RelativeRootPath = Guid.NewGuid().ToString(),
                }
            };
            message.Inputs.Add(new Messaging.Common.Storage
            {
                Name = "input-dicom",
                Endpoint = Guid.NewGuid().ToString(),
                Credentials = new Messaging.Common.Credentials
                {
                    AccessKey = Guid.NewGuid().ToString(),
                    AccessToken = Guid.NewGuid().ToString()
                },
                Bucket = Guid.NewGuid().ToString(),
                RelativeRootPath = Guid.NewGuid().ToString(),
            });
            message.Inputs.Add(new Messaging.Common.Storage
            {
                Name = "input-ehr",
                Endpoint = Guid.NewGuid().ToString(),
                Credentials = new Messaging.Common.Credentials
                {
                    AccessKey = Guid.NewGuid().ToString(),
                    AccessToken = Guid.NewGuid().ToString()
                },
                Bucket = Guid.NewGuid().ToString(),
                RelativeRootPath = Guid.NewGuid().ToString(),
            });
            message.Outputs.Add(new Messaging.Common.Storage
            {
                Name = "output",
                Endpoint = Guid.NewGuid().ToString(),
                Credentials = new Messaging.Common.Credentials
                {
                    AccessKey = Guid.NewGuid().ToString(),
                    AccessToken = Guid.NewGuid().ToString()
                },
                Bucket = Guid.NewGuid().ToString(),
                RelativeRootPath = Guid.NewGuid().ToString(),
            });
            return message;
        }
    }
}

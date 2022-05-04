// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Common;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Services
{
    public class WorkflowExecuterService : IWorkflowExecuterService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        private readonly IStorageService _storageService;
        private readonly IMessageBrokerPublisherService _messageBrokerPublisherService;
        private readonly StorageServiceConfiguration _storageConfiguration;

        private string TaskDispatchRoutingKey { get; }

        public WorkflowExecuterService(
            IOptions<WorkflowManagerOptions> configuration,
            IOptions<StorageServiceConfiguration> storageConfiguration,
            IWorkflowRepository workflowRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IMessageBrokerPublisherService messageBrokerPublisherService,
            IStorageService storageService)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (storageConfiguration is null)
            {
                throw new ArgumentNullException(nameof(storageConfiguration));
            }

            _storageConfiguration = storageConfiguration.Value;

            TaskDispatchRoutingKey = configuration.Value.Messaging.Topics.TaskDispatchRequest;

            _workflowRepository = workflowRepository ?? throw new ArgumentNullException(nameof(workflowRepository));
            _workflowInstanceRepository = workflowInstanceRepository ?? throw new ArgumentNullException(nameof(workflowInstanceRepository));
            _messageBrokerPublisherService = messageBrokerPublisherService ?? throw new ArgumentNullException(nameof(messageBrokerPublisherService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        public async Task<bool> ProcessPayload(WorkflowRequestEvent message)
        {
            var processed = true;
            var workflows = new List<Workflow>();

            workflows = message.Workflows?.Any() != true ?
                await _workflowRepository.GetWorkflowsByAeTitleAsync(message.CalledAeTitle) as List<Workflow> :
                await _workflowRepository.GetByWorkflowsIdsAsync(message.Workflows) as List<Workflow>;

            var workflowInstances = new List<WorkflowInstance>();
            workflows.ForEach((workflow) => workflowInstances.Add(CreateWorkFlowIntsance(message, workflow)));

            var existingInstances = await _workflowInstanceRepository.GetByWorkflowsIdsAsync(workflowInstances.Select(w => w.WorkflowId).ToList());
            workflowInstances.RemoveAll(i => existingInstances.ToList().Exists(e => e.WorkflowId == i.WorkflowId && e.PayloadId == i.PayloadId));

            processed &= await _workflowInstanceRepository.CreateAsync(workflowInstances);

            if (processed)
            {
                foreach (var workflowInstance in workflowInstances)
                {
                    var taskDispatchEvent = EventMapper.ToTaskDispatchEvent(workflowInstance.Tasks.FirstOrDefault(), workflowInstance.WorkflowId, _storageConfiguration);
                    var jsonMesssage = new JsonMessage<TaskDispatchEvent>(taskDispatchEvent, MessageBrokerConfiguration.WorkflowManagerApplicationId, taskDispatchEvent.CorrelationId, Guid.NewGuid().ToString());

                    await _messageBrokerPublisherService.Publish(TaskDispatchRoutingKey, jsonMesssage.ToMessage());
                }
            }

            return processed;
        }

        private WorkflowInstance CreateWorkFlowIntsance(WorkflowRequestEvent message, Workflow workflow)
        {
            var workflowInstance = new WorkflowInstance()
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflow.WorkflowId,
                PayloadId = message.PayloadId.ToString(),
                StartTime = DateTime.UtcNow,
                Status = Status.Created,
                AeTitle = workflow.WorkflowSpec?.InformaticsGateway?.AeTitle,
                BucketId = $"{message.Bucket}/{workflow.Id}",
                InputMetaData = { } //Functionality to be added later
            };

            var tasks = new List<TaskExecution>();
            // part of this ticket just take the first task
            if (workflow.WorkflowSpec.Tasks.Length > 0)
            {
                var firstTask = workflow.WorkflowSpec.Tasks.FirstOrDefault();

                // check if template exists merge args.

                //if (!string.IsNullOrEmpty(firstTask.Ref))
                //{
                //    var template = workflow.TaskTemplates.Where(x => x.Id == firstTask.Ref).FirstOrDefault();
                //    firstTask = template ?? firstTask;
                //}

                var exceutionId = Guid.NewGuid();

                tasks.Add(new TaskExecution()
                {
                    ExecutionId = exceutionId,
                    TaskType = firstTask.Type,
                    TaskPluginArguments = firstTask.Args,
                    TaskId = firstTask.Id,
                    Status = Status.Created,
                    InputArtifacts = firstTask.Artifacts?.Input?.ToDictionary(),
                    OutputDirectory = $"{message.Bucket}/{workflow.Id}/{exceutionId}",
                    Metadata = { }
                });
            }

            workflowInstance.Tasks = tasks.ToArray();

            return workflowInstance;
        }
    }
}

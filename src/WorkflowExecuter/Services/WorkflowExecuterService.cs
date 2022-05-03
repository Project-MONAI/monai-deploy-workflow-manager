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

            if (message.Workflows?.Any() != true)
            {
                workflows = await _workflowRepository.GetWorkflowsByAeTitleAsync(message.CalledAeTitle) as List<Workflow>;
            }
            else
            {
                workflows = await _workflowRepository.GetByWorkflowsIdsAsync(message.Workflows) as List<Workflow>;
            }

            var workflowInstances = new List<WorkflowInstance>();
            var taskDispatches = new List<JsonMessage<TaskDispatchEvent>>();

            foreach (var workflow in workflows)
            {
                var workflowIntance = CreateWorkFlowIntsance(message, workflow);
                workflowInstances.Add(workflowIntance);

                await _storageService.CreateFolder(message.Bucket, workflow.WorkflowId.ToString());

                var taskDispatchEvent = EventMapper.ToTaskDispatchEvent(workflowIntance.Tasks.FirstOrDefault(), workflow.WorkflowId.ToString(), _storageConfiguration);

                taskDispatches.Add(new JsonMessage<TaskDispatchEvent>(taskDispatchEvent, MessageBrokerConfiguration.WorkflowManagerApplicationId, taskDispatchEvent.CorrelationId, Guid.NewGuid().ToString()));
            }

            processed &= await _workflowInstanceRepository.CreateAsync(workflowInstances);

            if (processed)
            {
                taskDispatches.ForEach(async (t) => await _messageBrokerPublisherService.Publish(TaskDispatchRoutingKey, t.ToMessage()));
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
                BucketId = $"{message.Bucket}/{workflow.Id}",
                InputMataData = null//????? need to speak to victor
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
                    Status = Status.Created,    /// task.ref is reference to template if exist use that template
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

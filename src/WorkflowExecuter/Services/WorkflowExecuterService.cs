// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Services
{
    public class WorkflowExecuterService : IWorkflowExecuterService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        private readonly IStorageService _storageService;
        private readonly IMessageBrokerPublisherService _messageBrokerPublisherService;

        private string TaskDispatchRoutingKey { get; }

        public WorkflowExecuterService(
            IOptions<WorkflowManagerOptions> configuration,
            IWorkflowRepository workflowRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IMessageBrokerPublisherService messageBrokerPublisherService,
            IStorageService storageService)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

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

            foreach (var workflow in workflows)
            {
                var workflowIntance = await CreateWorkFlowIntsance(message, workflow);
                workflowInstances.Add(workflowIntance);
                //_storageService.CreateFolder(message.Bucket, workflow.Id);
                //var credentials = _storageService.CreateTemporaryCredentials(message.Bucket, workflow.Id);

                //_messageBrokerPublisherService.Publish(TaskDispatchRoutingKey, new Message());
            }

            processed &= await _workflowInstanceRepository.CreateAsync(workflowInstances);

            return processed;
        }

        private async Task<WorkflowInstance> CreateWorkFlowIntsance(WorkflowRequestEvent message, Workflow workflow)
        {
            var workflowInstance = new WorkflowInstance()
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.WorkflowId,
                PayloadId = message.PayloadId,
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
                    InputArtifacts = new InputArtifacts()  //
                    {
                        // task.Artifacts.Input. in workflow it is an array workflow instance not an array

                        //value is long string convert to minao path  either be an empty or orignal payload path
                    },
                    OutputDirectory = $"{message.Bucket}/{workflow.Id}/{exceutionId}",
                    Metadata = { }
                });
            }

            workflowInstance.Tasks = tasks.ToArray();

            return workflowInstance;
        }
    }
}

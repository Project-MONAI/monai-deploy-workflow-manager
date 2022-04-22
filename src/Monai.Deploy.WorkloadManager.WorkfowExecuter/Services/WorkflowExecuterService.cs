using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Models;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Services
{
    public class WorkflowExecuterService : IWorkflowExecuterService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;


        public WorkflowExecuterService(IWorkflowRepository workflowRepository, IWorkflowInstanceRepository workflowInstanceRepository)
        {
            _workflowRepository = workflowRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
        }
        public async Task<bool> ProcessPayload(PayloadReceived message)
        {
            var processed = true;
            var workflows = new List<Workflow>();

            if (message.Workflows?.Any() != true)
            {
                workflows = await _workflowRepository.GetListByAeTitleAsync(message.CalledAeTitle);
            }
            else
            {
                workflows = await _workflowRepository.GetByWorkflowsIdsAsync(message.Workflows);

            }

            var workflowInstances = new List<WorkflowInstance>();
            foreach (var workflow in workflows)
            {
                var workflowIntance = await CreateWorkFlowIntsance(message, workflow);
                workflowInstances.Add(workflowIntance);
            }

            processed &= await _workflowInstanceRepository.CreateAsync(workflowInstances);
            return processed;
        }

        private async Task<WorkflowInstance> CreateWorkFlowIntsance(PayloadReceived message, Workflow workflow)
        {
            var workflowInstance = new WorkflowInstance()
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                PayloadId = message.PayloadId,
                StartTime = DateTime.Now,
                BucketId = $"{message.Bucket}/{workflow.Id}",
                InputMataData = null//????? need to speak to victor
            };

            var tasks = new List<WorkflowTask>();
            // part of this ticket just take the first task
            if (workflow.Tasks.Length > 0)
            {
                var firstTask = workflow.Tasks.FirstOrDefault();

                // check if template exists

                if (!string.IsNullOrEmpty(firstTask.Ref))
                {
                    var template = workflow.TaskTemplates.Where(x => x.Id == firstTask.Ref).FirstOrDefault();
                    firstTask = template ?? firstTask;

                }
                var exceutionId = Guid.NewGuid();

                tasks.Add(new WorkflowTask()
                {
                    ExecutionId = Guid.NewGuid(),
                    TaskType = firstTask.Type,
                    TaskPluginArguments = new TaskPluginArguments(), // dictionary    args
                    TaskId = firstTask.Id,
                    Status = "created",    /// task.ref is reference to template if exist use that template
                    InputArtifacts = new InputArtifacts()  //
                    {
                        // task.Artifacts.Input. in workflow it is an array workflow instance not an array

                        //value is long string convert to minao path  either be an empty or orignal payload path
                    },
                    OutputDirectory = $"{message.Bucket}/{workflow.Id}/$execution_id",
                    Metadata = { }
                });
            }
         //   workflowInstance.Tasks = tasks.ToArray();

            return workflowInstance;
        }
    }
}

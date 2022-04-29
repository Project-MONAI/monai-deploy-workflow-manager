using Amazon.SecurityToken.Model;
using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Common
{
    public static class EventMapper
    {
        public static TaskDispatchEvent ToTaskDispatchEvent(TaskExecution task, string workflowId, Credentials credentials, StorageServiceConfiguration configuration)
        {
            Guard.Against.Null(task, nameof(task));
            Guard.Against.Null(configuration, nameof(configuration));

            Guard.Against.Null(credentials, nameof(credentials));
            Guard.Against.Null(credentials.SecretAccessKey, nameof(credentials.SecretAccessKey));
            Guard.Against.Null(credentials.AccessKeyId, nameof(credentials.AccessKeyId));
            Guard.Against.Null(credentials.SessionToken, nameof(credentials.SessionToken));

            var inputs = new List<Messaging.Common.Storage>();

            foreach (var inArtifact in task.InputArtifacts)
            {
                inputs.Add(new Messaging.Common.Storage
                {
                    Credentials = new Messaging.Common.Credentials
                    {
                        AccessKey = credentials.AccessKeyId,
                        AccessToken = credentials.SecretAccessKey,
                        SessionToken = credentials.SessionToken
                    },
                    SecuredConnection = bool.Parse(configuration.Settings["securedConnection"]),
                    Endpoint = configuration.Settings["endpoint"],
                    Bucket = configuration.Settings["bucket"],
                    RelativeRootPath = inArtifact.Value,
                    Name = inArtifact.Value
                });
            }

            return new TaskDispatchEvent
            {
                WorkflowId = workflowId,
                TaskId = task.TaskId,
                ExecutionId = task.ExecutionId.ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                TaskPluginArguments = task.TaskPluginArguments,
                Inputs = inputs,
                Metadata = task.Metadata
            };
        }
    }
}

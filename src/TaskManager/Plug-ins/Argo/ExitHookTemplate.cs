// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Linq;
using System.Text;
using Ardalis.GuardClauses;
using Argo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Logging;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    internal sealed class ExitHookTemplate
    {
        private readonly TaskDispatchEvent _taskDispatchEvent;
        private readonly MessageBrokerServiceConfiguration _messageBrokerServiceConfiguration;
        private readonly Guid _messageId;
        private readonly string _messageFileName;

        public ExitHookTemplate(TaskDispatchEvent taskDispatchEvent, MessageBrokerServiceConfiguration messageBrokerServiceConfiguration)
        {
            _taskDispatchEvent = taskDispatchEvent ?? throw new ArgumentNullException(nameof(taskDispatchEvent));
            _messageBrokerServiceConfiguration = messageBrokerServiceConfiguration ?? throw new ArgumentNullException(nameof(messageBrokerServiceConfiguration));
            _messageId = Guid.NewGuid();
            _messageFileName = $"{_messageId}.json";
        }

        public Template2 GenerateMessageTemplate(S3Artifact2 artifact)
        {
            var taskUpdateEvent = GenerateTaskCallbackEvent();
            var taskUpdateEventJson = JsonConvert.SerializeObject(taskUpdateEvent);
            var message = GenerateTaskCallbackMessage();
            var messageJson = JsonConvert.SerializeObject(JsonConvert.SerializeObject(message)); // serialize the message 2x

            return new Template2()
            {
                Name = Strings.ExitHookTemplateGenerateTemplateName,
                Inputs = new Inputs
                {
                    Parameters = new List<Parameter>()
                    {
                        new Parameter { Name = Strings.ExitHookParameterEvent, Value = taskUpdateEventJson },
                        new Parameter { Name = Strings.ExitHookParameterMessage, Value = messageJson }
                    }
                },
                Container = new Container2
                {
                    Image = Strings.ExitHookGenerateMessageContainerImage,
                    Command = new List<string> { "/bin/sh", "-c" },
                    Args = new List<string> { $"echo \"{{{{inputs.parameters.message}}}}\" > {Strings.ExitHookOutputPath}{_messageFileName}; cat {Strings.ExitHookOutputPath}{_messageFileName};" }
                },
                Outputs = new Outputs
                {
                    Artifacts = new List<Artifact>()
                    {
                        new Artifact
                        {
                            Name = Strings.ExitHookOutputArtifactName,
                            Path  = Strings.ExitHookOutputPath,
                            Archive = new ArchiveStrategy
                            {
                                None = new NoneStrategy()
                            },
                            S3 = artifact
                        }
                    }
                }
            };
        }

        private TaskCallbackEvent GenerateTaskCallbackEvent() =>
            new TaskCallbackEvent
            {
                WorkflowId = _taskDispatchEvent.WorkflowId,
                TaskId = _taskDispatchEvent.TaskId,
                ExecutionId = _taskDispatchEvent.ExecutionId,
                CorrelationId = _taskDispatchEvent.CorrelationId,
                Identity = "{{workflow.name}}"
            };

        private object GenerateTaskCallbackMessage() =>
             new
             {
                 ContentType = "application/json",
                 CorrelationID = _taskDispatchEvent.CorrelationId,
                 MessageID = Guid.NewGuid().ToString(),
                 Type = "TaskCallbackEvent",
                 AppID = "argo",
                 Exchange = _messageBrokerServiceConfiguration.PublisherSettings["exchange"],
                 RoutingKey = "md.tasks.callback",
                 DeliveryMode = 2,
                 Body = "{{=sprig.b64enc(inputs.parameters.event)}}"
             };

        public Template2 GenerateSendTemplate(S3Artifact2 artifact)
        {
            string username = _messageBrokerServiceConfiguration.PublisherSettings["username"];
            string password = _messageBrokerServiceConfiguration.PublisherSettings["password"];
            string endpoint = _messageBrokerServiceConfiguration.PublisherSettings["endpoint"];
            string virtualHost = _messageBrokerServiceConfiguration.PublisherSettings["virtualHost"];
            string exchange = _messageBrokerServiceConfiguration.PublisherSettings["exchange"];

            var copyOfArtifact = new S3Artifact2
            {
                AccessKeySecret = artifact.AccessKeySecret,
                Bucket = artifact.Bucket,
                Endpoint = artifact.Endpoint,
                Insecure = artifact.Insecure,
                Key = $"{artifact.Key}/{_messageFileName}",
                SecretKeySecret = artifact.SecretKeySecret,
            };

            return new Template2()
            {
                Name = Strings.ExitHookTemplateSendTemplateName,
                Inputs = new Inputs
                {
                    Artifacts = new List<Artifact>()
                    {
                        new Artifact
                        {
                            Name = "message",
                            Path  = $"{Strings.ExitHookOutputPath}{_messageFileName}",
                            Archive = new ArchiveStrategy
                            {
                                None = new NoneStrategy()
                            },
                            S3 = copyOfArtifact
                        }
                    }
                },
                Container = new Container2
                {
                    Image = Strings.ExitHookSendMessageContainerImage,
                    Command = new List<string> { "/rabtap" },
                    Args = new List<string> {
                        "pub",
                        $"--uri=amqp://{username}:{password}@{endpoint}/{virtualHost}",
                        "--format=json",
                        $"{Strings.ExitHookOutputPath}{_messageFileName}",
                        "--delay=0s",
                        "--confirms",
                        "--mandatory" }
                }
            };
        }
    }

}

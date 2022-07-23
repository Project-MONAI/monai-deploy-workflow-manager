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

using Argo;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.StaticValues;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    internal sealed class ExitHookTemplate
    {
        private readonly TaskDispatchEvent _taskDispatchEvent;
        private readonly string _messagingEndpoint;
        private readonly string _messagingUsername;
        private readonly string _messagingPassword;
        private readonly string _messagingTopic;
        private readonly string _messagingExchange;
        private readonly Guid _messageId;
        private readonly string _messageFileName;

        public ExitHookTemplate(TaskDispatchEvent taskDispatchEvent)
        {
            _taskDispatchEvent = taskDispatchEvent ?? throw new ArgumentNullException(nameof(taskDispatchEvent));
            _messagingEndpoint = taskDispatchEvent.TaskPluginArguments[Keys.MessagingEnddpoint];
            _messagingUsername = taskDispatchEvent.TaskPluginArguments[Keys.MessagingUsername];
            _messagingPassword = taskDispatchEvent.TaskPluginArguments[Keys.MessagingPassword];
            _messagingTopic = taskDispatchEvent.TaskPluginArguments[Keys.MessagingTopic];
            _messagingExchange = taskDispatchEvent.TaskPluginArguments[Keys.MessagingExchange];
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
            new()
            {
                WorkflowInstanceId = _taskDispatchEvent.WorkflowInstanceId,
                TaskId = _taskDispatchEvent.TaskId,
                ExecutionId = _taskDispatchEvent.ExecutionId,
                CorrelationId = _taskDispatchEvent.CorrelationId,
                Identity = "{{workflow.name}}",
                Outputs = _taskDispatchEvent.Outputs ?? new List<Messaging.Common.Storage>()
            };

        private object GenerateTaskCallbackMessage() =>
             new
             {
                 ContentType = Strings.ContentTypeJson,
                 CorrelationID = _taskDispatchEvent.CorrelationId,
                 MessageID = _messageId.ToString(),
                 Type = nameof(TaskCallbackEvent),
                 AppID = Strings.ApplicationId,
                 Exchange = _messagingExchange,
                 RoutingKey = _messagingTopic,
                 DeliveryMode = 2,
                 Body = "{{=sprig.b64enc(inputs.parameters.event)}}"
             };

        public Template2 GenerateSendTemplate(S3Artifact2 artifact)
        {
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
                        $"--uri=amqp://{_messagingUsername}:{_messagingPassword}@{_messagingEndpoint}",
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

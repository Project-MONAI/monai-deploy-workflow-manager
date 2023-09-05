/*
 * Copyright 2023 MONAI Consortium
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
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    internal sealed class ExitHookTemplate
    {
        private readonly TaskDispatchEvent _taskDispatchEvent;
        private readonly WorkflowManagerOptions _options;
        private readonly string _messagingEndpoint;
        private readonly string _messagingUsername;
        private readonly string _messagingPassword;
        private readonly string _messagingTopic;
        private readonly string _messagingExchange;
        private readonly Guid _messageId;
        private readonly string _messageFileName;
        private readonly string _messagingVhost;

        public ExitHookTemplate(WorkflowManagerOptions options, TaskDispatchEvent taskDispatchEvent)
        {
            _taskDispatchEvent = taskDispatchEvent ?? throw new ArgumentNullException(nameof(taskDispatchEvent));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _messagingEndpoint = options.Messaging.ArgoCallback.ArgoCallbackOverrideEnabled ?
                options.Messaging.ArgoCallback.ArgoRabbitOverrideEndpoint :
                options.Messaging.PublisherSettings[ArgoParameters.MessagingEndpoint];

            _messagingUsername = options.Messaging.PublisherSettings[ArgoParameters.MessagingUsername];
            _messagingPassword = options.Messaging.PublisherSettings[ArgoParameters.MessagingPassword];
            _messagingTopic = options.Messaging.Topics.TaskCallbackRequest;
            _messagingExchange = options.Messaging.PublisherSettings[ArgoParameters.MessagingExchange];
            _messagingVhost = options.Messaging.PublisherSettings[ArgoParameters.MessagingVhost];
            _messageId = Guid.NewGuid();
            _messageFileName = $"{_messageId}.json";
        }

        public Template2 GenerateCallbackMessageTemplate(S3Artifact2 artifact)
        {
            var taskUpdateEvent = GenerateTaskCallbackEvent();
            var taskUpdateEventJson = JsonConvert.SerializeObject(taskUpdateEvent);

            return new Template2()
            {
                Name = Strings.ExitHookTemplateSendTemplateName,
                Inputs = new Inputs
                {
                    Parameters = new List<Parameter>()
                    {
                        new Parameter { Name = Strings.ExitHookParameterEvent, Value = taskUpdateEventJson },
                    }
                },
                Container = new Container2
                {
                    Image = _options.TaskManager.ArgoExitHookSendMessageContainerImage,
                    Resources = new ResourceRequirements
                    {
                        Limits = new Dictionary<string, string>
                        {
                            {"cpu", _options.TaskManager.ArgoPluginArguments.MessageGeneratorContainerCpuLimit},
                            {"memory", _options.TaskManager.ArgoPluginArguments.MessageGeneratorContainerMemoryLimit}
                        }
                    },
                    Command = new List<string> { "python" },
                    Args = new List<string> {
                        "/app/app.py",
                        "--host", _messagingEndpoint,
                        "--username", _messagingUsername,
                        "--password", _messagingPassword,
                        "--vhost", _messagingVhost,
                        "--exchange", _messagingExchange,
                        "--topic", _messagingTopic,
                        "--correlationId", _taskDispatchEvent.CorrelationId,
                        "--message", "{{inputs.parameters.event}}"
                        }
                },
                PodSpecPatch = "{\"initContainers\":[{\"name\":\"init\",\"resources\":{\"limits\":{\"cpu\":\"" + _options.TaskManager.ArgoPluginArguments.InitContainerCpuLimit + "\",\"memory\": \"" + _options.TaskManager.ArgoPluginArguments.InitContainerMemoryLimit + "\"},\"requests\":{\"cpu\":\"0\",\"memory\":\"0Mi\"}}}],\"containers\":[{\"name\":\"wait\",\"resources\":{\"limits\":{\"cpu\":\"" + _options.TaskManager.ArgoPluginArguments.WaitContainerCpuLimit + "\",\"memory\":\"" + _options.TaskManager.ArgoPluginArguments.WaitContainerMemoryLimit + "\"},\"requests\":{\"cpu\":\"0\",\"memory\":\"0Mi\"}}}]}",
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
    }
}

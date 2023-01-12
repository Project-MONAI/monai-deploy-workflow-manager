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

using System.Reflection;
using System.Text;
using Ardalis.GuardClauses;
using Argo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.TaskManager.API;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.API.Extensions;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Logging;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.StaticValues;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    public sealed class ArgoPlugin : TaskPluginBase, IAsyncDisposable
    {
        private readonly Dictionary<string, string> _secretStores;
        private readonly Dictionary<string, Messaging.Common.Storage> _intermediaryArtifactStores;
        private readonly IServiceScope _scope;
        private readonly IKubernetesProvider _kubernetesProvider;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IArgoProvider _argoProvider;
        private readonly ILogger<ArgoPlugin> _logger;
        private readonly ITaskDispatchEventService _taskDispatchEventService;
        private int? _activeDeadlineSeconds;
        private string _namespace;
        private string _baseUrl = null!;
        private bool _allowInsecure = true;
        private string? _apiToken;

        public ArgoPlugin(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ArgoPlugin> logger,
            IOptions<WorkflowManagerOptions> options,
            TaskDispatchEvent taskDispatchEvent)
            : base(taskDispatchEvent)
        {
            Guard.Against.Null(serviceScopeFactory, nameof(serviceScopeFactory));

            _secretStores = new Dictionary<string, string>();
            _intermediaryArtifactStores = new Dictionary<string, Messaging.Common.Storage>();
            _scope = serviceScopeFactory.CreateScope();

            _taskDispatchEventService = _scope.ServiceProvider.GetService<ITaskDispatchEventService>() ?? throw new ServiceNotFoundException(nameof(ITaskDispatchEventService));
            _kubernetesProvider = _scope.ServiceProvider.GetRequiredService<IKubernetesProvider>() ?? throw new ServiceNotFoundException(nameof(IKubernetesProvider));
            _argoProvider = _scope.ServiceProvider.GetRequiredService<IArgoProvider>() ?? throw new ServiceNotFoundException(nameof(IArgoProvider));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            ValidateEvent();
            Initialize();
        }

        private void Initialize()
        {
            if (Event.TaskPluginArguments.ContainsKey(Keys.TimeoutSeconds) &&
                int.TryParse(Event.TaskPluginArguments[Keys.TimeoutSeconds], out var result))
            {
                _activeDeadlineSeconds = result;
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.ArgoApiToken))
            {
                _apiToken = Event.TaskPluginArguments[Keys.ArgoApiToken];
            }

            bool updateEvent = false;

            if (Event.TaskPluginArguments.ContainsKey(Keys.Namespace))
            {
                _namespace = Event.TaskPluginArguments[Keys.Namespace];
            }
            else
            {
                _namespace = Strings.DefaultNamespace;
                Event.TaskPluginArguments.Add(Keys.Namespace, _namespace);
                updateEvent = true;
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.AllowInsecureseUrl))
            {
                _allowInsecure = string.Compare("true", Event.TaskPluginArguments[Keys.AllowInsecureseUrl], true) == 0;
            }
            else
            {
                _allowInsecure = true;
                Event.TaskPluginArguments.Add(Keys.AllowInsecureseUrl, "true");
                updateEvent = true;
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.BaseUrl))
            {
                _baseUrl = Event.TaskPluginArguments[Keys.BaseUrl];
            }
            else
            {
                _baseUrl = _options.Value.TaskManager.ArgoPluginArguments.ServerUrl;
                Event.TaskPluginArguments.Add(Keys.BaseUrl, _baseUrl);
                updateEvent = true;
            }

            if (updateEvent)
            {
                var eventInfo = new TaskDispatchEventInfo(Event);
                Task.Run(() => _taskDispatchEventService.UpdateTaskPluginArgsAsync(eventInfo, Event.TaskPluginArguments));
            }

            _logger.Initialized(_namespace, _baseUrl, _activeDeadlineSeconds, (!string.IsNullOrWhiteSpace(_apiToken)));
        }

        private void ValidateEvent()
        {
            if (Event.TaskPluginArguments is null || Event.TaskPluginArguments.Count == 0)
            {
                throw new InvalidTaskException($"Required parameters to execute Argo workflow are missing: {string.Join(',', Keys.RequiredParameters)}");
            }

            foreach (var key in Keys.RequiredParameters)
            {
                if (!Event.TaskPluginArguments.ContainsKey(key))
                {
                    throw new InvalidTaskException($"Required parameter to execute Argo workflow is missing: {key}");
                }
            }

            foreach (var key in Keys.RequiredSettings)
            {
                if (!_options.Value.Messaging.PublisherSettings.ContainsKey(key))
                {
                    throw new ConfigurationException($"Required message publisher setting to execute Argo workflow is missing: {key}");
                }
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.BaseUrl) && !Uri.IsWellFormedUriString(Event.TaskPluginArguments[Keys.BaseUrl], UriKind.Absolute))
            {
                throw new InvalidTaskException($"The value '{Event.TaskPluginArguments[Keys.BaseUrl]}' provided for '{Keys.BaseUrl}' is not a valid URI.");
            }
        }

        public override async Task<ExecutionStatus> ExecuteTask(CancellationToken cancellationToken = default)
        {
            using var loggingScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["correlationId"] = Event.CorrelationId,
                ["workflowInstanceId"] = Event.WorkflowInstanceId,
                ["taskId"] = Event.TaskId,
                ["executionId"] = Event.ExecutionId,
                ["argoNamespace"] = _namespace
            });

            Workflow workflow;
            try
            {
                workflow = await BuildWorkflowWrapper(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.ErrorGeneratingWorkflow(ex);
                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError, Errors = ex.Message };
            }

            try
            {
                var client = _argoProvider.CreateClient(_baseUrl, _apiToken, _allowInsecure);
                _logger.CreatingArgoWorkflow(workflow.Metadata.GenerateName);
                var result = await client.Argo_CreateWorkflowAsync(_namespace, new WorkflowCreateRequest { Namespace = _namespace, Workflow = workflow }, cancellationToken).ConfigureAwait(false);
                _logger.ArgoWorkflowCreated(result.Metadata.Name);
                return new ExecutionStatus
                {
                    Status = TaskExecutionStatus.Accepted,
                    FailureReason = FailureReason.None,
                    Stats = new Dictionary<string, string> { { Strings.IdentityKey, result.Metadata.Name } }
                };
            }
            catch (Exception ex)
            {
                _logger.ErrorCreatingWorkflow(ex);
                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError, Errors = ex.Message };
            }
        }

        public override async Task<ExecutionStatus> GetStatus(string identity, TaskCallbackEvent callbackEvent, CancellationToken cancellationToken = default)
        {
            Guard.Against.NullOrWhiteSpace(identity, nameof(identity));
            Task? logTask = null;
            try
            {
                var client = _argoProvider.CreateClient(_baseUrl, _apiToken, _allowInsecure);
                var workflow = await client.Argo_GetWorkflowAsync(_namespace, identity, null, null, cancellationToken).ConfigureAwait(false);

                // it take sometime for the Argo job to be in the final state after emitting the callback event.
                var retryCount = 30;
                while (workflow.Status.Phase.Equals(Strings.ArgoPhaseRunning, StringComparison.OrdinalIgnoreCase) && retryCount-- > 0)
                {
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                    workflow = await client.Argo_GetWorkflowAsync(_namespace, identity, null, null, cancellationToken).ConfigureAwait(false);
                }
                logTask = PipeExecutionLogs(client, identity);

                var stats = GetExecutuionStats(workflow);
                if (stats is null)
                {
                    stats = new Dictionary<string, string>();
                }
                if (Strings.ArgoFailurePhases.Contains(workflow.Status.Phase, StringComparer.OrdinalIgnoreCase))
                {
                    return new ExecutionStatus
                    {
                        Status = TaskExecutionStatus.Failed,
                        FailureReason = FailureReason.ExternalServiceError,
                        Errors = workflow.Status.Message,
                        Stats = stats
                    };
                }
                else if (workflow.Status.Phase.Equals(Strings.ArgoPhaseSucceeded, StringComparison.OrdinalIgnoreCase))
                {
                    return new ExecutionStatus
                    {
                        Status = TaskExecutionStatus.Succeeded,
                        FailureReason = FailureReason.None,
                        Stats = stats
                    };
                }
                else
                {
                    return new ExecutionStatus
                    {
                        Status = TaskExecutionStatus.Failed,
                        FailureReason = FailureReason.Unknown,
                        Errors = $"Argo status = '{workflow.Status.Phase}'. Messages = '{workflow.Status.Message}'.",
                        Stats = stats
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorCreatingWorkflow(ex);
                return new ExecutionStatus
                {
                    Status = TaskExecutionStatus.Failed,
                    FailureReason = FailureReason.PluginError,
                    Errors = ex.Message
                };
            }
            finally
            {
                if (logTask is not null)
                {
                    await logTask;
                }
            }
        }

        private Dictionary<string, string> GetExecutuionStats(Workflow workflow)
        {
            Guard.Against.Null(workflow);

            TimeSpan? duration = null;
            if (workflow.Status?.StartedAt is not null && workflow.Status?.FinishedAt is not null)
            {
                duration = workflow.Status?.FinishedAt - workflow.Status?.StartedAt;
            }
            var stats = new Dictionary<string, string>
            {
                { "workflowInstanceId", Event.WorkflowInstanceId },
                { "duration", duration.HasValue ? duration.Value.TotalMilliseconds.ToString() : string.Empty },
                { "startedAt", workflow.Status?.StartedAt.ToString() ?? string.Empty  },
                { "finishedAt", workflow.Status?.FinishedAt.ToString() ?? string.Empty  }
            };

            if (workflow.Status is null)
            {
                return stats;
            }

            if (workflow.Status.ResourcesDuration is not null)
            {
                foreach (var item in workflow.Status.ResourcesDuration)
                {
                    stats.Add($"resourceDuration.{item.Key}", item.Value.ToString());
                }
            }

            if (workflow.Status.Nodes is not null)
            {
                foreach (var item in workflow.Status.Nodes)
                {
                    var json = JsonConvert.SerializeObject(item.Value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    stats.Add($"nodes.{item.Key}", json);
                }
            }

            return stats;
        }

        private async Task PipeExecutionLogs(IArgoClient client, string identity)
        {
            try
            {
#pragma warning disable CA2254 // Template should be a static expression
                var logs = await client.Argo_Get_WorkflowLogsAsync(_namespace, identity, null, "init");
                _logger.ArgoLog(logs);
                logs = await client.Argo_Get_WorkflowLogsAsync(_namespace, identity, null, "wait");
                _logger.ArgoLog(logs);
                logs = await client.Argo_Get_WorkflowLogsAsync(_namespace, identity, null, "main");
                _logger.ArgoLog(logs);
#pragma warning restore CA2254 // Template should be a static expression
            }
            catch (Exception ex)
            {
                int we = 0;
            }
        }

        private async Task<Workflow> BuildWorkflowWrapper(CancellationToken cancellationToken)
        {
            _logger.GeneratingArgoWorkflow();
            var workflow = new Workflow
            {
                ApiVersion = Strings.ArgoApiVersion,
                Kind = Strings.KindWorkflow,
                Metadata = new ObjectMeta()
                {
                    GenerateName = $"md-{Event.TaskPluginArguments![Keys.WorkflowTemplateName]}-",
                    Labels = new Dictionary<string, string>
                    {
                        { Strings.TaskIdLabelSelectorName, Event.TaskId! },
                        { Strings.WorkflowIdLabelSelectorName, Event.WorkflowInstanceId! },
                        { Strings.CorrelationIdLabelSelectorName, Event.CorrelationId! },
                        { Strings.ExecutionIdLabelSelectorName, Event.ExecutionId }
                    }
                },
                Spec = new WorkflowSpec()
                {
                    ActiveDeadlineSeconds = _activeDeadlineSeconds,
                    Entrypoint = Strings.WorkflowEntrypoint,
                    Hooks = new Dictionary<string, LifecycleHook>
                    {
                        { Strings.ExitHook, new LifecycleHook(){ Template = Strings.ExitHookTemplateName }}
                    }
                }
            };

            workflow.Spec.Templates = new List<Template2>();
            // Add the main workflow template
            await AddMainWorkflowTemplate(workflow, cancellationToken).ConfigureAwait(false);

            // Add the exit template for the exit hook
            await AddExitHookTemplate(workflow, cancellationToken).ConfigureAwait(false);

            ProcessTaskPluginArguments(workflow);

            _logger.ArgoWorkflowTemplateGenerated(workflow.Metadata.GenerateName);
            var workflowJson = JsonConvert.SerializeObject(workflow, Formatting.Indented);
            workflowJson = workflowJson.Replace(_options.Value.Messaging.PublisherSettings[Keys.MessagingPassword], "*****");

            _logger.ArgoWorkflowTemplateJson(workflow.Metadata.GenerateName, workflowJson);

            return workflow;
        }

        /// <summary>
        /// Adds limits and requests from task plugin arguments to templates inside given workflow.
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="workflow"></param>
        /// <param name="cancellationToken"></param>
        private void ProcessTaskPluginArguments(Workflow workflow)
        {
            Guard.Against.Null(workflow);

            var resources = Event.GetTaskPluginArgumentsParameter<Dictionary<string, string>>(Keys.ArgoResource);
            var priorityClassName = Event.GetTaskPluginArgumentsParameter(Keys.TaskPriorityClassName);
            var argoParameters = Event.GetTaskPluginArgumentsParameter<Dictionary<string, string>>(Keys.ArgoParameters);

            if (argoParameters is not null)
            {
                foreach (var item in argoParameters)
                {
                    if (workflow.Spec.Arguments is null)
                    {
                        workflow.Spec.Arguments = new Arguments();
                    }

                    if (workflow.Spec.Arguments.Parameters is null)
                    {
                        workflow.Spec.Arguments.Parameters = new List<Parameter>();
                    }

                    workflow.Spec.Arguments.Parameters.Add(new Parameter() { Name = item.Key, Value = item.Value });
                }
            }

            foreach (var template in workflow.Spec.Templates)
            {
                AddLimit(resources, template, ResourcesKeys.CpuLimit);
                AddLimit(resources, template, ResourcesKeys.MemoryLimit);
                AddRequest(resources, template, ResourcesKeys.CpuReservation);
                AddRequest(resources, template, ResourcesKeys.MemoryReservation);
                AddRequest(resources, template, ResourcesKeys.GpuLimit);

                if (priorityClassName is not null)
                {
                    template.PriorityClassName = priorityClassName;
                }
            }
        }

        private static void AddLimit(Dictionary<string, string>? resources, Template2 template, ResourcesKey key)
        {
            Guard.Against.Null(template);
            Guard.Against.Null(key);
            if (template.Container is null || resources is null || !resources.TryGetValue(key.TaskKey, out var value))
            {
                return;
            }
            if (template.Container.Resources is null)
            {
                template.Container.Resources = new ResourceRequirements();
            }
            if (template.Container.Resources.Limits is null)
            {
                template.Container.Resources.Limits = new Dictionary<string, string>();
            }

            template.Container.Resources.Limits.Add(key.ArgoKey, value);
        }

        private static void AddRequest(Dictionary<string, string>? resources, Template2 template, ResourcesKey key)
        {
            Guard.Against.Null(template);
            Guard.Against.Null(key);
            if (template.Container is null || resources is null || !resources.TryGetValue(key.TaskKey, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (template.Container.Resources is null)
            {
                template.Container.Resources = new ResourceRequirements();
            }
            if (template.Container.Resources.Requests is null)
            {
                template.Container.Resources.Requests = new Dictionary<string, string>();
            }

            template.Container.Resources.Requests.Add(key.ArgoKey, value);
        }

        private async Task AddMainWorkflowTemplate(Workflow workflow, CancellationToken cancellationToken)
        {
            Guard.Against.Null(workflow, nameof(workflow));

            var workflowTemplate = await LoadWorkflowTemplate(Event.TaskPluginArguments![Keys.WorkflowTemplateName]).ConfigureAwait(false);

            if (workflowTemplate is null)
            {
                throw new TemplateNotFoundException(Event.TaskPluginArguments![Keys.WorkflowTemplateName]);
            }
            var mainTemplateSteps = new Template2()
            {
                Name = Strings.WorkflowEntrypoint,
                Steps = new List<ParallelSteps>()
                {
                    new ParallelSteps() {
                        new WorkflowStep()
                        {
                            Name = Strings.WorkflowEntrypoint,
                            Template = workflowTemplate.Spec.Entrypoint
                        }
                    }
                }
            };

            await CopyWorkflowTemplateToWorkflow(workflowTemplate, workflowTemplate.Spec.Entrypoint, workflow, cancellationToken).ConfigureAwait(false);
            workflow.Spec.Templates.Add(mainTemplateSteps);

            await ConfigureInputArtifactStoreForTemplates(workflow.Spec.Templates, cancellationToken).ConfigureAwait(false);
            await ConfigureOuputArtifactStoreForTemplates(workflow.Spec.Templates, cancellationToken).ConfigureAwait(false);
        }

        private async Task AddExitHookTemplate(Workflow workflow, CancellationToken cancellationToken)
        {
            Guard.Against.Null(workflow, nameof(workflow));

            var temporaryStore = Event.IntermediateStorage.Clone() as Messaging.Common.Storage;
            temporaryStore!.RelativeRootPath = $"{temporaryStore.RelativeRootPath}/messaging";

            var exitTemplateSteps = new Template2()
            {
                Name = Strings.ExitHookTemplateName,
                Steps = new List<ParallelSteps>()
                {
                    new ParallelSteps()
                    {
                        new WorkflowStep()
                        {
                            Name = Strings.ExitHookTemplateGenerateTemplateName,
                            Template = Strings.ExitHookTemplateGenerateTemplateName,
                        }
                    },

                    new ParallelSteps()
                    {
                        new WorkflowStep()
                        {
                            Name = Strings.ExitHookTemplateSendTemplateName,
                            Template = Strings.ExitHookTemplateSendTemplateName,
                        }
                    }
                }
            };

            workflow.Spec.Templates.Add(exitTemplateSteps);

            var artifact = await CreateArtifact(temporaryStore, cancellationToken).ConfigureAwait(false);

            var exitHookTemplate = new ExitHookTemplate(_options.Value, Event);
            workflow.Spec.Templates.Add(exitHookTemplate.GenerateMessageTemplate(artifact));
            workflow.Spec.Templates.Add(exitHookTemplate.GenerateSendTemplate(artifact));
        }

        private async Task<WorkflowTemplate> LoadWorkflowTemplate(string workflowTemplateName)
        {
            Guard.Against.NullOrWhiteSpace(workflowTemplateName, nameof(workflowTemplateName));

            try
            {
                var client = _argoProvider.CreateClient(_baseUrl, _apiToken, _allowInsecure);
                return await client.Argo_GetWorkflowTemplateAsync(_namespace, workflowTemplateName, null).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.ErrorLoadingWorkflowTemplate(workflowTemplateName, ex);
                throw;
            }
        }

        private async Task CopyWorkflowTemplateToWorkflow(WorkflowTemplate workflowTemplate, string name, Workflow workflow, CancellationToken cancellationToken)
        {
            Guard.Against.Null(workflowTemplate, nameof(workflowTemplate));
            Guard.Against.Null(workflowTemplate.Spec, nameof(workflowTemplate.Spec));
            Guard.Against.Null(workflowTemplate.Spec.Templates, nameof(workflowTemplate.Spec.Templates));
            Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Guard.Against.Null(workflow, nameof(workflow));

            var template = workflowTemplate.Spec.Templates.FirstOrDefault(p => p.Name == name);
            if (template is null)
            {
                throw new TemplateNotFoundException(workflowTemplate.Metadata.Name, name);
            }

            workflow.Spec.Templates.Add(template);

            await CopyTemplateSteps(template.Steps, workflowTemplate, name, workflow, cancellationToken).ConfigureAwait(false);
            await CopyTemplateDags(template.Dag, workflowTemplate, name, workflow, cancellationToken).ConfigureAwait(false);
            AddAllTopLevelAttrs(workflowTemplate, workflow);
            SetArgoTtlStratergy(workflow);
        }

        private static void AddAllTopLevelAttrs(WorkflowTemplate workflowTemplate, Workflow workflow)
        {
            var props = workflow.Spec.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in props.Where(p => p.CanWrite))
            {
                var p = property;
                if (p.GetValue(workflowTemplate.Spec) is not null && p.GetValue(workflow.Spec) is null)
                {
                    p.SetValue(workflow.Spec, p.GetValue(workflowTemplate.Spec));
                }
            }
        }

        private void SetArgoTtlStratergy(Workflow workflow)
        {
            if (workflow.Spec.TtlStrategy is null)
            {
                workflow.Spec.TtlStrategy = new TTLStrategy { SecondsAfterCompletion = _options.Value.ArgoTtlStatergySeconds };
            }
            else
            {
                if (workflow.Spec.TtlStrategy.SecondsAfterCompletion.HasValue)
                    workflow.Spec.TtlStrategy.SecondsAfterCompletion = Math.Max(_options.Value.MinArgoTtlStatergySeconds, workflow.Spec.TtlStrategy.SecondsAfterCompletion.Value);
                if (workflow.Spec.TtlStrategy.SecondsAfterSuccess.HasValue)
                    workflow.Spec.TtlStrategy.SecondsAfterSuccess = Math.Max(_options.Value.MinArgoTtlStatergySeconds, workflow.Spec.TtlStrategy.SecondsAfterSuccess.Value);
                if (workflow.Spec.TtlStrategy.SecondsAfterFailure.HasValue)
                    workflow.Spec.TtlStrategy.SecondsAfterFailure = Math.Max(_options.Value.MinArgoTtlStatergySeconds, workflow.Spec.TtlStrategy.SecondsAfterFailure.Value);
            }
            RemovePodGCStratergy(workflow);
        }

        private static void RemovePodGCStratergy(Workflow workflow)
        {
            workflow.Spec.PodGC = null;
        }

        /// <summary>
        /// Configures input artifact store for all templates.
        /// For dag & steps, if an argument with S3 Key is set to <see cref="Strings.InputRepositoryToken"/>
        /// with a matching name in <see cref="TaskDispatchEvent.Inputs"/>, then the connection information is used.
        /// For all other template types, if a matching name in <see cref="TaskDispatchEvent.Inputs"/>, then the connection information is used.
        /// Otherwise, the ArgoPlugin assume that a connection is provided.
        /// </summary>
        /// <param name="templates"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ConfigureInputArtifactStoreForTemplates(ICollection<Template2> templates, CancellationToken cancellationToken)
        {
            Guard.Against.Null(templates, nameof(templates));

            foreach (var template in templates)
            {
                if (template.Dag is not null)
                {
                    await ConfigureInputArtifactStore(template.Name, templates, template.Dag.Tasks.Where(p => p.Arguments is not null && p.Arguments.Artifacts is not null).SelectMany(p => p.Arguments.Artifacts), true, cancellationToken).ConfigureAwait(false);
                }
                else if (template.Steps is not null)
                {
                    foreach (var step in template.Steps)
                    {
                        await ConfigureInputArtifactStore(template.Name, templates, step.Where(p => p.Arguments is not null && p.Arguments.Artifacts is not null).SelectMany(p => p.Arguments.Artifacts), true, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (template.Inputs is not null)
                {
                    await ConfigureInputArtifactStore(template.Name, templates, template.Inputs.Artifacts, false, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task ConfigureInputArtifactStore(string templateName, ICollection<Template2> templates, IEnumerable<Artifact> artifacts, bool isDagOrStep, CancellationToken cancellationToken)
        {
            Guard.Against.NullOrWhiteSpace(templateName, nameof(templateName));
            Guard.Against.Null(templates, nameof(templates));

            if (artifacts is null || !artifacts.Any())
            {
                return;
            }

            foreach (var artifact in artifacts)
            {
                if (!isDagOrStep && IsInputConfiguredInStepOrDag(templates, templateName, artifact.Name))
                {
                    continue;
                }

                var storageInfo = Event.Inputs.FirstOrDefault(p => p.Name.Equals(artifact.Name, StringComparison.Ordinal));
                if (storageInfo is null)
                {
                    _logger.NoInputArtifactStorageConfigured(artifact.Name, templateName);
                    return;
                }
                artifact.S3 = await CreateArtifact(storageInfo, cancellationToken).ConfigureAwait(false);
            }
        }

        private bool IsInputConfiguredInStepOrDag(ICollection<Template2> templates, string referencedTemplateName, string referencedArtifactName)
        {
            Guard.Against.Null(templates, nameof(templates));
            Guard.Against.NullOrWhiteSpace(referencedTemplateName, nameof(referencedTemplateName));
            Guard.Against.NullOrWhiteSpace(referencedArtifactName, nameof(referencedArtifactName));

            List<Artifact> artifacts = new List<Artifact>();
            foreach (var template in templates)
            {
                if (template.Dag is not null)
                {
                    artifacts.AddRange(template.Dag.Tasks
                        .Where(p => p.Template.Equals(referencedTemplateName, StringComparison.Ordinal) && p.Arguments is not null)
                        .SelectMany(p => p.Arguments.Artifacts));
                }
                else if (template.Steps is not null)
                {
                    foreach (var step in template.Steps)
                    {
                        artifacts.AddRange(step.Where(p => p.Template.Equals(referencedTemplateName, StringComparison.OrdinalIgnoreCase) && p.Arguments is not null)
                           .SelectMany(p => p.Arguments.Artifacts));
                    }
                }
            }

            return artifacts.Any(p => p.Name.Equals(referencedArtifactName, StringComparison.Ordinal));
        }

        /// <summary>
        /// Configures output artifact store for non-dag & non-steps templates.
        /// If a matching output name in the template is found in <see cref="TaskDispatchEvent.Outputs"/>, the connection information is used and is assumed to be a task output.
        /// Otherwise, the <see cref="TaskDispatchEvent.IntermediateStorage"/> output store is used and a subdirectory is created & mapped into the container.
        /// </summary>
        /// <param name="templates"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ConfigureOuputArtifactStoreForTemplates(ICollection<Template2> templates, CancellationToken cancellationToken)
        {
            Guard.Against.Null(templates, nameof(templates));

            foreach (var template in templates)
            {
                if (template.Dag is not null || template.Steps is not null)
                {
                    continue;
                }

                await SetupOutputArtifactStoreForTemplate(template, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SetupOutputArtifactStoreForTemplate(Template2 template, CancellationToken cancellationToken)
        {
            Guard.Against.Null(template, nameof(template));

            if (template.Outputs is null || template.Outputs.Artifacts is null)
            {
                return;
            }

            foreach (var artifact in template.Outputs.Artifacts)
            {
                var storageInfo = Event.Outputs.FirstOrDefault(p => p.Name.Equals(artifact.Name, StringComparison.Ordinal));

                if (storageInfo is null)
                {
                    storageInfo = GenerateIntermediaryArtifactStore(artifact.Name);
                    _logger.UseIntermediaryArtifactStorage(artifact.Name, template.Name);
                }

                artifact.S3 = await CreateArtifact(storageInfo, cancellationToken).ConfigureAwait(false);
                artifact.Archive = new ArchiveStrategy
                {
                    None = new NoneStrategy()
                };
            }
        }

        private Messaging.Common.Storage GenerateIntermediaryArtifactStore(string artifactName)
        {
            if (_intermediaryArtifactStores.ContainsKey(artifactName))
            {
                return _intermediaryArtifactStores[artifactName];
            }

            var storageInfo = Event.IntermediateStorage.Clone() as Messaging.Common.Storage;
            storageInfo!.RelativeRootPath = $"{storageInfo.RelativeRootPath}/{artifactName}";

            _intermediaryArtifactStores.Add(artifactName, storageInfo);

            return storageInfo;
        }

        private async Task<S3Artifact2> CreateArtifact(Messaging.Common.Storage storageInfo, CancellationToken cancellationToken)
        {
            Guard.Against.Null(storageInfo, nameof(storageInfo));

            if (!_secretStores.TryGetValue(storageInfo.Name!, out var secret))
            {
                secret = await GenerateK8sSecretFrom(storageInfo, cancellationToken).ConfigureAwait(false);
            }

            return new S3Artifact2()
            {
                Bucket = storageInfo.Bucket,
                Key = storageInfo.RelativeRootPath,
                Insecure = !storageInfo.SecuredConnection,
                Endpoint = storageInfo.Endpoint,
                AccessKeySecret = new SecretKeySelector { Name = secret, Key = Strings.SecretAccessKey },
                SecretKeySecret = new SecretKeySelector { Name = secret, Key = Strings.SecretSecretKey },
            };
        }

        private async Task CopyTemplateDags(DAGTemplate dag, WorkflowTemplate workflowTemplate, string name, Workflow workflow, CancellationToken cancellationToken)
        {
            Guard.Against.Null(workflowTemplate, nameof(workflowTemplate));
            Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Guard.Against.Null(workflow, nameof(workflow));

            if (dag is not null)
            {
                foreach (var task in dag.Tasks)
                {
                    await CopyWorkflowTemplateToWorkflow(workflowTemplate, task.Template, workflow, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task CopyTemplateSteps(ICollection<ParallelSteps> steps, WorkflowTemplate workflowTemplate, string name, Workflow workflow, CancellationToken cancellationToken)
        {
            Guard.Against.Null(workflowTemplate, nameof(workflowTemplate));
            Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Guard.Against.Null(workflow, nameof(workflow));

            if (steps is not null)
            {
                foreach (var pStep in steps)
                {
                    foreach (var step in pStep)
                    {
                        await CopyWorkflowTemplateToWorkflow(workflowTemplate, step.Template, workflow, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<string> GenerateK8sSecretFrom(Messaging.Common.Storage storage, CancellationToken cancellationToken)
        {
            Guard.Against.Null(storage, nameof(storage));
            Guard.Against.Null(storage.Credentials, nameof(storage.Credentials));
            Guard.Against.NullOrWhiteSpace(storage.Name, nameof(storage.Name));
            Guard.Against.NullOrWhiteSpace(storage.Credentials.AccessKey, nameof(storage.Credentials.AccessKey));
            Guard.Against.NullOrWhiteSpace(storage.Credentials.AccessToken, nameof(storage.Credentials.AccessToken));

            var client = _kubernetesProvider.CreateClient();
            var secret = new k8s.Models.V1Secret
            {
                Metadata = new k8s.Models.V1ObjectMeta
                {
                    Name = $"{storage.Name.ToLowerInvariant()}-{DateTime.UtcNow.Ticks}",
                    Labels = new Dictionary<string, string>
                    {
                        { Strings.LabelCreator, Strings.LabelCreatorValue }
                    }
                },
                Type = Strings.SecretTypeOpaque,
                Data = new Dictionary<string, byte[]>
                {
                    { Strings.SecretAccessKey, Encoding.UTF8.GetBytes(storage.Credentials.AccessKey) },
                    { Strings.SecretSecretKey, Encoding.UTF8.GetBytes(storage.Credentials.AccessToken) }
                }
            };

            _logger.GeneratingArtifactSecret(storage.Name);
            var result = await client.CoreV1.CreateNamespacedSecretWithHttpMessagesAsync(secret, _namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
            result.Response.EnsureSuccessStatusCode();
            _secretStores.Add(storage.Name, result.Body.Metadata.Name);
            return result.Body.Metadata.Name;
        }

        private async Task RemoveKubenetesSecrets()
        {
            if (_secretStores.Any())
            {
                var client = _kubernetesProvider.CreateClient();

                foreach (var secret in _secretStores.Values)
                {
                    try
                    {
                        await client.CoreV1.DeleteNamespacedSecretWithHttpMessagesAsync(secret, _namespace).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorDeletingKubernetesSecret(secret, ex);
                    }
                }
                _secretStores.Clear();
            }
        }

        ~ArgoPlugin() => Dispose(disposing: false);

        protected override void Dispose(bool disposing)
        {
            if (!DisposedValue && disposing)
            {
                _scope.Dispose();
            }

            base.Dispose(disposing);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore()
        {
            await RemoveKubenetesSecrets().ConfigureAwait(false);
        }

        public override async Task HandleTimeout(string identity)
        {
            var client = _argoProvider.CreateClient(_baseUrl, _apiToken, _allowInsecure);

            await client.Argo_StopWorkflowAsync(_namespace, identity, new WorkflowStopRequest
            {
                Namespace = _namespace,
                Name = identity,
            });

            await client.Argo_TerminateWorkflowAsync(_namespace, identity, new WorkflowTerminateRequest
            {
                Name = identity,
                Namespace = _namespace
            });
        }
    }
}

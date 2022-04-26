// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text;
using Ardalis.GuardClauses;
using Argo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Logging;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    public sealed class ArgoPlugin : RunnerBase, IAsyncDisposable
    {
        private readonly Dictionary<string, string> _secretStores;
        private readonly IServiceScope _scope;
        private readonly IKubernetesProvider _kubernetesProvider;
        private readonly IArgoProvider _argoProvider;
        private readonly IOptions<MessageBrokerServiceConfiguration> _options;
        private readonly ILogger<ArgoPlugin> _logger;
        private int? _activeDeadlineSeconds;
        private string _namespace;
        private string _baseUrl = null!;
        private string? _apiToken;

        public ArgoPlugin(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<MessageBrokerServiceConfiguration> options,
            ILogger<ArgoPlugin> logger,
            TaskDispatchEvent taskDispatchEvent)
            : base(taskDispatchEvent)
        {
            Guard.Against.Null(serviceScopeFactory, nameof(serviceScopeFactory));

            _secretStores = new Dictionary<string, string>();
            _scope = serviceScopeFactory.CreateScope();

            _kubernetesProvider = _scope.ServiceProvider.GetRequiredService<IKubernetesProvider>() ?? throw new ServiceNotFoundException(nameof(IKubernetesProvider));
            _argoProvider = _scope.ServiceProvider.GetRequiredService<IArgoProvider>() ?? throw new ServiceNotFoundException(nameof(IArgoProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
            _namespace = Strings.DefaultNamespace;

            ValidateEventAndInit();
            Initialize();
        }

        private void Initialize()
        {
            if (Event.TaskPluginArguments.ContainsKey(Keys.TimeoutSeconds) &&
                int.TryParse(Event.TaskPluginArguments[Keys.TimeoutSeconds], out var result))
            {
                _activeDeadlineSeconds = result;
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.Namespace))
            {
                _namespace = Event.TaskPluginArguments[Keys.Namespace];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.ArgoApiToken))
            {
                _apiToken = Event.TaskPluginArguments[Keys.ArgoApiToken];
            }

            _baseUrl = Event.TaskPluginArguments[Keys.BaseUrl];

            _logger.Initialized(_namespace, _baseUrl, _activeDeadlineSeconds, (!string.IsNullOrWhiteSpace(_apiToken)));
        }

        private void ValidateEventAndInit()
        {
            if (Event.TaskPluginArguments is null || Event.TaskPluginArguments.Count == 0)
            {
                throw new ValidationException($"Required parameters to execute Argo workflow are missing: {string.Join(',', Keys.RequiredParameters)}");
            }

            foreach (var key in Keys.RequiredParameters)
            {
                if (!Event.TaskPluginArguments.ContainsKey(key))
                {
                    throw new ValidationException($"Required parameter to execute Argo workflow is missing: {key}");
                }
            }

            if (!Uri.IsWellFormedUriString(Event.TaskPluginArguments[Keys.BaseUrl], UriKind.Absolute))
            {
                throw new ValidationException($"The value '{Event.TaskPluginArguments[Keys.BaseUrl]}' provided for '{Keys.BaseUrl}' is not a valid URI.");
            }
        }

        public override async Task<ExecutionStatus> ExecuteTask(CancellationToken cancellationToken = default)
        {
            using var loggerScope = _logger.BeginScope($"Workflow ID={Event.WorkflowId}, Task ID={Event.TaskId}, Execution ID={Event.ExecutionId}, Argo namespace={_namespace}");

            Workflow workflow;
            try
            {
                workflow = await BuildWorkflowWrapper(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.ErrorGeneratingWorkflow(ex);
                return new ExecutionStatus { Status = Messaging.Events.TaskStatus.Failed, FailureReason = FailureReason.PluginError, Errors = ex.Message };
            }

            try
            {
                var client = _argoProvider.CreateClient(_baseUrl, _apiToken);
                _logger.CreatingArgoWorkflow(workflow.Metadata.GenerateName);
                var result = await client.WorkflowService_CreateWorkflowAsync(_namespace, new WorkflowCreateRequest { Namespace = _namespace, Workflow = workflow }, cancellationToken).ConfigureAwait(false);
                _logger.ArgoWorkflowCreated(result.Metadata.Name);
                return new ExecutionStatus { Status = Messaging.Events.TaskStatus.Accepted, FailureReason = FailureReason.None };
            }
            catch (Exception ex)
            {
                _logger.ErrorCreatingWorkflow(ex);
                return new ExecutionStatus { Status = Messaging.Events.TaskStatus.Failed, FailureReason = FailureReason.PluginError, Errors = ex.Message };
            }
        }

        public override async Task<ExecutionStatus> GetStatus(string identity, CancellationToken cancellationToken = default)
        {
            Guard.Against.NullOrWhiteSpace(identity, nameof(identity));

            var client = _argoProvider.CreateClient(_baseUrl, _apiToken);

            try
            {
                var workflow = await client.WorkflowService_GetWorkflowAsync(_namespace, identity, null, null, cancellationToken).ConfigureAwait(false);
                if (Strings.ArgoFailurePhases.Contains(workflow.Status.Phase, StringComparer.OrdinalIgnoreCase))
                {
                    return new ExecutionStatus { Status = Messaging.Events.TaskStatus.Failed, FailureReason = FailureReason.ExternalServiceError, Errors = workflow.Status.Message };
                }
                else if (workflow.Status.Phase.Equals(Strings.ArgoPhaseSucceeded, StringComparison.OrdinalIgnoreCase))
                {
                    return new ExecutionStatus { Status = Messaging.Events.TaskStatus.Succeeded, FailureReason = FailureReason.None };
                }
                else
                {
                    return new ExecutionStatus { Status = Messaging.Events.TaskStatus.Unknown, FailureReason = FailureReason.Unknown, Errors = $"Argo status = {workflow.Status.Phase}. Messages = {workflow.Status.Message}." };
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorCreatingWorkflow(ex);
                return new ExecutionStatus { Status = Messaging.Events.TaskStatus.Failed, FailureReason = FailureReason.PluginError, Errors = ex.Message };
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
                        { Strings.WorkflowIdLabelSelectorName, Event.WorkflowId! },
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

            _logger.ArgoWorkflowTemplateGenerated(workflow.Metadata.GenerateName);
            _logger.ArgoWorkflowTemplateJson(workflow.Metadata.GenerateName, JsonConvert.SerializeObject(workflow, Formatting.Indented));

            return workflow;
        }

        private async Task AddMainWorkflowTemplate(Workflow workflow, CancellationToken cancellationToken)
        {
            Guard.Against.Null(workflow, nameof(workflow));

            var workflowTemplate = await LoadWorkflowTemplate(Event.TaskPluginArguments![Keys.WorkflowTemplateName]).ConfigureAwait(false);

            if(workflowTemplate is null)
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
                            Template = Event.TaskPluginArguments![Keys.WorkflowTemplateTemplateRefName]
                        }
                    }
                }
            };

            await CopyWorkflowTemplateToWorkflow(workflowTemplate, Event.TaskPluginArguments![Keys.WorkflowTemplateTemplateRefName], workflow, cancellationToken).ConfigureAwait(false);

            workflow.Spec.Templates.Add(mainTemplateSteps);
        }

        private async Task AddExitHookTemplate(Workflow workflow, CancellationToken cancellationToken)
        {
            Guard.Against.Null(workflow, nameof(workflow));

            var temporaryStore = Event.Outputs.FirstOrDefault(p => p.Name!.Equals(Strings.ExitHookOutputStorageName, StringComparison.Ordinal));

            if (temporaryStore is null)
            {
                throw new ArtifactMappingNotFoundException(Strings.ExitHookOutputStorageName);
            }

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

            var exitHookTemplate = new ExitHookTemplate(Event, _options.Value);
            workflow.Spec.Templates.Add(exitHookTemplate.GenerateMessageTemplate(artifact));
            workflow.Spec.Templates.Add(exitHookTemplate.GenerateSendTemplate(artifact));
        }

        private async Task<WorkflowTemplate> LoadWorkflowTemplate(string workflowTemplateName)
        {
            Guard.Against.NullOrWhiteSpace(workflowTemplateName, nameof(workflowTemplateName));

            try
            {
                var client = _argoProvider.CreateClient(_baseUrl, _apiToken);
                return await client.WorkflowTemplateService_GetWorkflowTemplateAsync(_namespace, workflowTemplateName, null).ConfigureAwait(false);
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
            await AddArtifacts(template?.Inputs?.Artifacts, Event.Inputs, cancellationToken).ConfigureAwait(false);
            await AddArtifacts(template?.Outputs?.Artifacts, Event.Outputs, cancellationToken).ConfigureAwait(false);
        }

        private async Task AddArtifacts(ICollection<Artifact>? artifacts, List<Messaging.Common.Storage> storageInfos, CancellationToken cancellationToken)
        {
            Guard.Against.Null(storageInfos, nameof(storageInfos));

            if (artifacts is null || artifacts.Count == 0)
            {
                return;
            }

            foreach (var artifact in artifacts)
            {
                var storageInfo = storageInfos.FirstOrDefault(p => p.Name!.Equals(artifact.Name, StringComparison.Ordinal));
                if (storageInfo is null)
                {
                    throw new ArtifactMappingNotFoundException(artifact.Name);
                }

                artifact.S3 = await CreateArtifact(storageInfo, cancellationToken).ConfigureAwait(false);
            }
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
            var result = await client.CreateNamespacedSecretWithHttpMessagesAsync(secret, _namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
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
                        await client.DeleteNamespacedSecretWithHttpMessagesAsync(secret, _namespace).ConfigureAwait(false);
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
    }
}

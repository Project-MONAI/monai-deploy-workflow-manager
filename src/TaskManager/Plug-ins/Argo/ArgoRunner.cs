// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text;
using Ardalis.GuardClauses;
using Argo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Logging;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    public class ArgoRunner : AsynchronousRunnerBase, IAsyncDisposable, IDisposable
    {
        private readonly List<string> _secretStores;
        private readonly IServiceScope _scope;
        private readonly IKubernetesProvider _kubernetesProvider;
        private readonly IArgoProvider _argoProvider;
        private readonly ILogger<ArgoRunner> _logger;
        private int? _activeDeadlineSeconds;
        private string _namespace;
        private bool _disposedValue;
        private Uri _baseUrl;

        public ArgoRunner(IServiceScopeFactory serviceScopeFactory, TaskDispatchEvent taskDispatchEvent, ILogger<ArgoRunner> logger)
            : base(taskDispatchEvent)
        {
            Guard.Against.Null(serviceScopeFactory, nameof(serviceScopeFactory));

            _secretStores = new List<string>();
            _scope = serviceScopeFactory.CreateScope();

            _kubernetesProvider = _scope.ServiceProvider.GetRequiredService<IKubernetesProvider>() ?? throw new ServiceNotFoundException(nameof(IKubernetesProvider));
            _argoProvider = _scope.ServiceProvider.GetRequiredService<IArgoProvider>() ?? throw new ServiceNotFoundException(nameof(IArgoProvider));

            _logger = logger;

            _namespace = Strings.DefaultNamespace;

            ValidateEventAndSetup();
        }

        private void ValidateEventAndSetup()
        {
            if (Event.TaskPluginArguments is null)
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

            if (Event.TaskPluginArguments.ContainsKey(Keys.TimeoutSeconds) &&
                int.TryParse(Event.TaskPluginArguments[Keys.TimeoutSeconds], out var result))
            {
                _activeDeadlineSeconds = result;
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.Namespace))
            {
                _namespace = Event.TaskPluginArguments[Keys.Namespace];
            }

            if (Uri.IsWellFormedUriString(Event.TaskPluginArguments[Keys.BaseUrl], UriKind.Absolute))
            {
                throw new ValidationException($"The value '{Event.TaskPluginArguments[Keys.BaseUrl]}' provided for '{Keys.BaseUrl}' is not a valid URI.");
            }

            _baseUrl = new Uri(Event.TaskPluginArguments[Keys.BaseUrl]);
        }

        public override async Task<ExecutionStatus> ExecuteTask()
        {
            using var loggerScope = _logger.BeginScope($"Workflow ID={Event.WorkflowId}, Task ID={Event.TaskId}, Execution ID={Event.ExecutionId}, Argo namespace={_namespace}");
            var workflow = await BuildWorkflowWrapper().ConfigureAwait(false);
            var client = _argoProvider.CreateClient(_baseUrl);

            try
            {
                _ = await client.WorkflowService_CreateWorkflowAsync(_namespace, new WorkflowCreateRequest { Namespace = _namespace, Workflow = workflow }).ConfigureAwait(false);
                return new ExecutionStatus { Status = Messaging.Events.TaskStatus.Accepted, FailureReason = FailureReason.None };
            }
            catch (Exception ex)
            {
                _logger.ErrorCreatingWorkflow(ex);
                return new ExecutionStatus { Status = Messaging.Events.TaskStatus.Failed, FailureReason = FailureReason.Unknown, Errors = ex.Message };
            }

        }

        public override async Task<ExecutionStatus> GetStatus(string identity)
        {
            Guard.Against.NullOrWhiteSpace(identity, nameof(identity));

            var client = _argoProvider.CreateClient(_baseUrl);

            try
            {
                var workflow = await client.WorkflowService_GetWorkflowAsync(_namespace, identity, null, null).ConfigureAwait(false);
                if (Strings.ArgoFailurePhases.Contains(workflow.Status.Phase, StringComparer.OrdinalIgnoreCase))
                {
                    return new ExecutionStatus { Status = Messaging.Events.TaskStatus.Failed, FailureReason = FailureReason.ExternalServiceError, Errors = workflow.Status.Message };
                }
                else
                {
                    return new ExecutionStatus { Status = Messaging.Events.TaskStatus.Succeeded, FailureReason = FailureReason.None };
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorCreatingWorkflow(ex);
                return new ExecutionStatus { Status = Messaging.Events.TaskStatus.Failed, FailureReason = FailureReason.Unknown, Errors = ex.Message };
            }

        }

        private async Task<Workflow> BuildWorkflowWrapper()
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

            // Add the exit template for the exit hook
            AddExitHookTemplate(workflow);

            // Add the main workflow template
            await AddMainWorkflowTemplate(workflow).ConfigureAwait(false);

            _logger.ArgoWorkflowTemplateGenerated(workflow.Metadata.GenerateName);
            _logger.ArgoWorkflowTemplateJson(workflow.Metadata.GenerateName, JsonConvert.SerializeObject(workflow, Formatting.Indented));

            return workflow;
        }

        private async Task AddMainWorkflowTemplate(Workflow workflow)
        {
            Guard.Against.Null(workflow, nameof(workflow));

            var mainTemplateStep = new WorkflowStep()
            {
                Name = Strings.ExitHookTemplateTemplateName,
                TemplateRef = new TemplateRef()
                {
                    Name = Event.TaskPluginArguments![Keys.WorkflowTemplateName],
                    Template = Event.TaskPluginArguments![Keys.WorkflowTemplateTemplateName]
                },
                Arguments = new Arguments()
                {
                    Artifacts = new List<Artifact>(),
                }
            };

            await AddArtifacts(mainTemplateStep, Event.Inputs).ConfigureAwait(false);
            await AddArtifacts(mainTemplateStep, Event.Outputs).ConfigureAwait(false);

            workflow.Spec.Templates.Add(new Template2()
            {
                Name = Strings.WorkflowEntrypoint,
                Steps = new List<ParallelSteps>() { new ParallelSteps() { mainTemplateStep } }
            });
        }

        private void AddExitHookTemplate(Workflow workflow)
        {
            Guard.Against.Null(workflow, nameof(workflow));

            workflow.Spec.Templates.Add(new Template2()
            {
                Name = Strings.ExitHookTemplateName,
                Steps = new List<ParallelSteps>()
                {
                     new ParallelSteps()
                     {
                          new WorkflowStep()
                          {
                                Name = Strings.ExitHookTemplateTemplateName,
                                TemplateRef= new TemplateRef()
                                {
                                    Name = Event.TaskPluginArguments![Keys.ExitWorkflowTemplateName],
                                    Template = Event.TaskPluginArguments[Keys.ExitWorkflowTemplateTemplateName]
                                }
                          }
                     }
                }
            });
        }

        private async Task AddArtifacts(WorkflowStep workflowStep, List<Messaging.Common.Storage> storageList)
        {
            Guard.Against.Null(workflowStep, nameof(workflowStep));
            Guard.Against.Null(storageList, nameof(storageList));

            foreach (var storage in storageList)
            {
                var secret = await GenerateK8sSecretFrom(storage).ConfigureAwait(false);
                workflowStep.Arguments.Artifacts.Add(new Artifact
                {
                    Name = storage.Name,
                    S3 = new S3Artifact2()
                    {
                        Bucket = storage.Bucket,
                        Key = storage.RelativeRootPath,
                        Insecure = !storage.SecuredConnection,
                        Endpoint = storage.Endpoint,
                        AccessKeySecret = new SecretKeySelector { Key = secret, Name = Strings.SecretAccessKey },
                        SecretKeySecret = new SecretKeySelector { Key = secret, Name = Strings.SecretSecretKey },
                    }
                });
            }
        }

        // TODO: we may need to generate a set of temporary credentials from the storage service.
        private async Task<string> GenerateK8sSecretFrom(Messaging.Common.Storage storage)
        {
            Guard.Against.Null(storage, nameof(storage));
            Guard.Against.Null(storage.Credentials, nameof(storage.Credentials));
            Guard.Against.NullOrWhiteSpace(storage.Name, nameof(storage.Name));
            Guard.Against.NullOrWhiteSpace(storage.Credentials.AccessKey, nameof(storage.Credentials.AccessKey));
            Guard.Against.NullOrWhiteSpace(storage.Credentials.AccessToken, nameof(storage.Credentials.AccessToken));

            var client = _kubernetesProvider.CreateClient();
            var secret = new k8s.Models.V1Secret();
            secret.Metadata.Name = $"{storage.Name.ToLowerInvariant()}{Strings.SecretNamePostfix}";
            secret.Type = Strings.SecretTypeOpaque;
            secret.Data = new Dictionary<string, byte[]>
            {
                { Strings.SecretAccessKey, Encoding.UTF8.GetBytes(storage.Credentials.AccessKey) },
                { Strings.SecretSecretKey, Encoding.UTF8.GetBytes(storage.Credentials.AccessToken) }
            };

            _logger.GeneratingArtifactSecret(storage.Name);
            var result = await client.CreateNamespacedSecretWithHttpMessagesAsync(secret, _namespace).ConfigureAwait(false);
            result.Response.EnsureSuccessStatusCode();
            _secretStores.Add(result.Body.Metadata.Name);
            return result.Body.Metadata.Name;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _scope.Dispose();
                }

                _disposedValue = true;
            }
        }

        private async Task RemoveKubenetesSecrets()
        {
            if (_secretStores.Any())
            {
                var client = _kubernetesProvider.CreateClient();

                foreach (var secret in _secretStores)
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

        ~ArgoRunner()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            await RemoveKubenetesSecrets().ConfigureAwait(false);
        }
    }
}

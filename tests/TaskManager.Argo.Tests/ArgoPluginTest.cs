// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Argo;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.SharedTest;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Moq;
using Moq.Language.Flow;
using Xunit;
using TaskStatus = Monai.Deploy.Messaging.Events.TaskStatus;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo;

public class ArgoPluginTest
{
    private readonly Mock<ILogger<ArgoPlugin>> _logger;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
    private readonly Mock<IServiceScope> _serviceScope;
    private readonly Mock<IKubernetesProvider> _kubernetesProvider;
    private readonly Mock<IArgoProvider> _argoProvider;
    private readonly Mock<IArgoClient> _argoClient;
    private readonly Mock<IKubernetes> _kubernetesClient;
    private readonly IOptions<MessageBrokerServiceConfiguration> _options;

    public ArgoPluginTest()
    {
        _logger = new Mock<ILogger<ArgoPlugin>>();
        _serviceScopeFactory = new Mock<IServiceScopeFactory>();
        _serviceScope = new Mock<IServiceScope>();
        _kubernetesProvider = new Mock<IKubernetesProvider>();
        _argoProvider = new Mock<IArgoProvider>();
        _argoClient = new Mock<IArgoClient>();
        _kubernetesClient = new Mock<IKubernetes>();
        _options = Options.Create(new MessageBrokerServiceConfiguration());
        _options.Value.PublisherSettings.Add("username", "username");
        _options.Value.PublisherSettings.Add("password", "password");
        _options.Value.PublisherSettings.Add("endpoint", "endpoint");
        _options.Value.PublisherSettings.Add("virtualHost", "virtualHost");
        _options.Value.PublisherSettings.Add("exchange", "exchange");

        _serviceScopeFactory.Setup(p => p.CreateScope()).Returns(_serviceScope.Object);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(x => x.GetService(typeof(IKubernetesProvider)))
            .Returns(_kubernetesProvider.Object);
        serviceProvider
            .Setup(x => x.GetService(typeof(IArgoProvider)))
            .Returns(_argoProvider.Object);

        _serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

        _logger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _argoProvider.Setup(p => p.CreateClient(It.IsAny<string>(), It.IsAny<string?>())).Returns(_argoClient.Object);
        _kubernetesProvider.Setup(p => p.CreateClient()).Returns(_kubernetesClient.Object);
    }

    [Fact(DisplayName = "Throws when missing required plug-in arguments")]
    public void ArgoPlugin_ThrowsWhenMissingPluginArguments()
    {
        var message = GenerateTaskDispatchEvent();
        Assert.Throws<InvalidTaskException>(() => new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, message));

        foreach (var key in Keys.RequiredParameters.Take(Keys.RequiredParameters.Count - 1))
        {
            message.TaskPluginArguments.Add(key, Guid.NewGuid().ToString());
            Assert.Throws<InvalidTaskException>(() => new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, message));
        }
        message.TaskPluginArguments[Keys.RequiredParameters.Last()] = Guid.NewGuid().ToString();
        Assert.Throws<InvalidTaskException>(() => new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, message));

        message.TaskPluginArguments[Keys.BaseUrl] = "/api";
        Assert.Throws<InvalidTaskException>(() => new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, message));
    }

    [Fact(DisplayName = "Initializes values")]
    public void ArgoPlugin_InitializesValues()
    {
        var message = GenerateTaskDispatchEventWithValidArguments();

        _ = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, message);
        _logger.VerifyLogging($"Argo plugin initialized: namespace=namespace, base URL=http://api-endpoint/, activeDeadlineSeconds=50, apiToken configured=True.", LogLevel.Information, Times.Once());
    }

    [Fact(DisplayName = "ExecuteTask - returns ExecutionStatus on failure")]
    public async Task ArgoPlugin_ExecuteTask_ReturnsExecutionStatusOnFailure()
    {
        var message = GenerateTaskDispatchEventWithValidArguments();

        _argoClient.Setup(p => p.WorkflowTemplateService_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(GenerateWorkflowTemplate(message));
        _argoClient.Setup(p => p.WorkflowService_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception("error"));

        SetupKubbernetesSecrets()
            .Returns((V1Secret body, string ns, string dr, string fm, string fv, bool? pretty, IReadOnlyDictionary<string, IReadOnlyList<string>> headers, CancellationToken ct) =>
            {
                return Task.FromResult(new HttpOperationResponse<V1Secret> { Body = body, Response = new HttpResponseMessage { } });
            });
        SetupKubernetesDeleteSecret();

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal("error", result.Errors);

        _argoClient.Verify(p => p.WorkflowService_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        _kubernetesClient.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));

        await runner.DisposeAsync().ConfigureAwait(false);
        _kubernetesClient.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<V1DeleteOptions>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<bool?>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "ExecuteTask - returns ExecutionStatus when failed to generate K8s secrets")]
    public async Task ArgoPlugin_ExecuteTask_ReturnsExecutionStatusWhenFailedToGenerateSecrets()
    {
        var message = GenerateTaskDispatchEventWithValidArguments();

        _argoClient.Setup(p => p.WorkflowTemplateService_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(GenerateWorkflowTemplate(message));

        SetupKubbernetesSecrets()
            .Throws(new Exception("error"));
        SetupKubernetesDeleteSecret();

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal("error", result.Errors);

        _argoClient.Verify(p => p.WorkflowService_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Never());
        _kubernetesClient.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Once());

        await runner.DisposeAsync().ConfigureAwait(false);
        _kubernetesClient.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<V1DeleteOptions>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<bool?>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact(DisplayName = "ExecuteTask - returns ExecutionStatus on success")]
    public async Task ArgoPlugin_ExecuteTask_ReturnsExecutionStatusOnSuccess()
    {
        var message = GenerateTaskDispatchEventWithValidArguments();
        _argoClient.Setup(p => p.WorkflowTemplateService_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(GenerateWorkflowTemplate(message));
        _argoClient.Setup(p => p.WorkflowService_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string ns, WorkflowCreateRequest body, CancellationToken cancellationToken) =>
            {
                return new Workflow { Metadata = new ObjectMeta { Name = "workflow" } };
            });

        SetupKubbernetesSecrets()
            .ReturnsAsync((V1Secret body, string ns, string dr, string fm, string fv, bool? pretty, IReadOnlyDictionary<string, IReadOnlyList<string>> headers, CancellationToken ct) =>
            {
                return new HttpOperationResponse<V1Secret> { Body = body, Response = new HttpResponseMessage { } };
            });
        SetupKubernetesDeleteSecret();


        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskStatus.Accepted, result.Status);
        Assert.Equal(FailureReason.None, result.FailureReason);
        Assert.Empty(result.Errors);

        _argoClient.Verify(p => p.WorkflowService_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        _kubernetesClient.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));

        await runner.DisposeAsync().ConfigureAwait(false);
        _kubernetesClient.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<V1DeleteOptions>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<bool?>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Theory(DisplayName = "GetStatus - returns ExecutionStatus on success")]
    [InlineData(Strings.ArgoPhaseSucceeded)]
    [InlineData(Strings.ArgoPhaseFailed)]
    [InlineData(Strings.ArgoPhaseError)]
    [InlineData(Strings.ArgoPhaseSkipped)]
    [InlineData(Strings.ArgoPhasePending)]
    [InlineData(Strings.ArgoPhaseRunning)]
    public async Task ArgoPlugin_GetStatus_ReturnsExecutionStatusOnSuccess(string phase)
    {
        _argoClient.Setup(p => p.WorkflowService_GetWorkflowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string ns, string name, string version, string fields, CancellationToken cancellationToken) =>
            {
                return new Workflow
                {
                    Status = new WorkflowStatus
                    {
                        Phase = phase,
                        Message = phase == Strings.ArgoPhaseSucceeded ? string.Empty : "error"
                    }
                };
            });

        var message = GenerateTaskDispatchEventWithValidArguments();

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, message);
        var result = await runner.GetStatus("identity", CancellationToken.None).ConfigureAwait(false);

        if (phase == Strings.ArgoPhaseSucceeded)
        {
            Assert.Equal(TaskStatus.Succeeded, result.Status);
            Assert.Equal(FailureReason.None, result.FailureReason);
            Assert.Empty(result.Errors);
        }
        else if (Strings.ArgoFailurePhases.Contains(phase, StringComparer.OrdinalIgnoreCase))
        {
            Assert.Equal(TaskStatus.Failed, result.Status);
            Assert.Equal(FailureReason.ExternalServiceError, result.FailureReason);
            Assert.Equal("error", result.Errors);
        }
        else
        {
            Assert.Equal(TaskStatus.Unknown, result.Status);
            Assert.Equal(FailureReason.Unknown, result.FailureReason);
            Assert.Equal($"Argo status = '{phase}'. Messages = 'error'.", result.Errors);
        }

        _argoClient.Verify(p => p.WorkflowService_GetWorkflowAsync(It.Is<string>(p => p.Equals("namespace", StringComparison.OrdinalIgnoreCase)), It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = "GetStatus - returns ExecutionStatus on failure")]
    public async Task ArgoPlugin_GetStatus_ReturnsExecutionStatusOnFailure()
    {
        _argoClient.Setup(p => p.WorkflowService_GetWorkflowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception("error"));

        var message = GenerateTaskDispatchEventWithValidArguments();

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, message);
        var result = await runner.GetStatus("identity", CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal("error", result.Errors);

        _argoClient.Verify(p => p.WorkflowService_GetWorkflowAsync(It.Is<string>(p => p.Equals("namespace", StringComparison.OrdinalIgnoreCase)), It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    private static TaskDispatchEvent GenerateTaskDispatchEventWithValidArguments()
    {
        var message = GenerateTaskDispatchEvent();
        message.TaskPluginArguments[Keys.BaseUrl] = "http://api-endpoint/";
        message.TaskPluginArguments[Keys.WorkflowTemplateName] = "workflowTemplate";
        message.TaskPluginArguments[Keys.WorkflowTemplateEntrypoint] = "workflowTemplate-template";
        message.TaskPluginArguments[Keys.MessagingEnddpoint] = "1.2.2.3/virtualhost";
        message.TaskPluginArguments[Keys.MessagingUsername] = "username";
        message.TaskPluginArguments[Keys.MessagingPassword] = "password";
        message.TaskPluginArguments[Keys.MessagingExchange] = "exchange";
        message.TaskPluginArguments[Keys.MessagingTopic] = "topic";
        message.TaskPluginArguments[Keys.Namespace] = "namespace";
        message.TaskPluginArguments[Keys.TimeoutSeconds] = "50";
        message.TaskPluginArguments[Keys.ArgoApiToken] = "token";
        return message;
    }

    private static TaskDispatchEvent GenerateTaskDispatchEvent()
    {
        var message = new TaskDispatchEvent
        {
            CorrelationId = Guid.NewGuid().ToString(),
            ExecutionId = Guid.NewGuid().ToString(),
            TaskAssemblyName = Guid.NewGuid().ToString(),
            WorkflowId = Guid.NewGuid().ToString(),
            TaskId = Guid.NewGuid().ToString()
        };
        message.Inputs.Add(new Messaging.Common.Storage
        {
            Name = Guid.NewGuid().ToString(),
            Endpoint = Guid.NewGuid().ToString(),
            Credentials = new Messaging.Common.Credentials
            {
                AccessKey = Guid.NewGuid().ToString(),
                AccessToken = Guid.NewGuid().ToString()
            },
            Bucket = Guid.NewGuid().ToString(),
            RelativeRootPath = Guid.NewGuid().ToString(),
        });
        message.Outputs.Add(new Messaging.Common.Storage
        {
            Name = Strings.ExitHookOutputStorageName,
            Endpoint = Guid.NewGuid().ToString(),
            Credentials = new Messaging.Common.Credentials
            {
                AccessKey = Guid.NewGuid().ToString(),
                AccessToken = Guid.NewGuid().ToString()
            },
            Bucket = Guid.NewGuid().ToString(),
            RelativeRootPath = Guid.NewGuid().ToString(),
        });
        return message;
    }

    private WorkflowTemplate GenerateWorkflowTemplate(TaskDispatchEvent taskDispatchEvent) => new()
    {
        Kind = "WorkflowTemplate",
        Metadata = new ObjectMeta()
        {
            Name = taskDispatchEvent.TaskPluginArguments[Keys.WorkflowTemplateName],
        },
        Spec = new WorkflowSpec
        {
            Templates = new List<Template2>
            {
                new Template2
                {
                     Name = taskDispatchEvent.TaskPluginArguments[Keys.WorkflowTemplateEntrypoint],
                     Steps = new List<ParallelSteps>
                     {
                        new ParallelSteps
                        {
                             new WorkflowStep
                             {
                                  Name = "T1.Step1a",
                                  Template = "T1.Step1a"
                             },
                             new WorkflowStep
                             {
                                  Name = "T1.Step1b",
                                  Template = "T1.Step1b"
                             }
                        },
                        new ParallelSteps
                        {
                             new WorkflowStep
                             {
                                  Name = "T1.Step2a",
                                  Template = "T1.Step2a"
                             },
                             new WorkflowStep
                             {
                                  Name = "T1.Step2b",
                                  Template = "T1.Step2b"
                             }
                        }
                     },
                     Dag = new DAGTemplate
                     {
                          Tasks = new List<DAGTask>
                          {
                              new DAGTask
                              {
                                    Name = "Dag.Task1",
                                    Template = "Dag.Task1",
                              },
                              new DAGTask
                              {
                                    Name = "Dag.Task2",
                                    Template = "Dag.Task2",
                              }
                          }
                     }
                },
                new Template2
                {
                    Name = "T1.Step1a",
                    Inputs = new Inputs
                    {
                         Artifacts = new List<Artifact>
                         {
                             new Artifact{ Name = taskDispatchEvent.Inputs[0].Name }
                         }
                    },
                    Outputs = new Outputs
                    {
                        Artifacts = new List<Artifact>
                        {
                            new Artifact { Name = taskDispatchEvent.Outputs[0].Name }
                        }
                    }
                },
                new Template2
                {
                    Name = "T1.Step1b",
                },
                new Template2
                {
                    Name = "T1.Step2a",
                },
                new Template2
                {
                    Name = "T1.Step2b",
                },
                new Template2
                {
                    Name = "Dag.Task1",
                },
                new Template2
                {
                    Name = "Dag.Task2",
                }
            }
        }
    };

    private ISetup<IKubernetes, Task<HttpOperationResponse<V1Secret>>> SetupKubbernetesSecrets() => _kubernetesClient.Setup(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
                        It.IsAny<V1Secret>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool?>(),
                        It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
                        It.IsAny<CancellationToken>()));

    private void SetupKubernetesDeleteSecret() => _kubernetesClient.Setup(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<V1DeleteOptions>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
                    It.IsAny<CancellationToken>()));
}

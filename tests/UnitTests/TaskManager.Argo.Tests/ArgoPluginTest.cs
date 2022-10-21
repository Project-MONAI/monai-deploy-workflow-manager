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

using System;
using System.Collections.Generic;
using System.IO;
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
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.SharedTest;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.StaticValues;
using Moq;
using Moq.Language.Flow;
using Newtonsoft.Json;
using Xunit;
using YamlDotNet.Serialization;
using Options = Microsoft.Extensions.Options.Options;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.Tests;

public class ArgoPluginTest
{
    private readonly Mock<ILogger<ArgoPlugin>> _logger;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
    private readonly Mock<IServiceScope> _serviceScope;
    private readonly Mock<IKubernetesProvider> _kubernetesProvider;
    private readonly Mock<IArgoProvider> _argoProvider;
    private readonly Mock<IArgoClient> _argoClient;
    private readonly Mock<IKubernetes> _kubernetesClient;
    private readonly Mock<ICoreV1Operations> _k8sCoreOperations;
    private readonly IOptions<WorkflowManagerOptions> _options;
    private Workflow? _submittedArgoTemplate;
    private readonly int _argoTtlStatergySeconds = 360;
    private readonly int _minAgoTtlStatergySeconds = 30;

    public ArgoPluginTest()
    {
        _logger = new Mock<ILogger<ArgoPlugin>>();
        _serviceScopeFactory = new Mock<IServiceScopeFactory>();
        _serviceScope = new Mock<IServiceScope>();
        _kubernetesProvider = new Mock<IKubernetesProvider>();
        _argoProvider = new Mock<IArgoProvider>();
        _argoClient = new Mock<IArgoClient>();
        _kubernetesClient = new Mock<IKubernetes>();
        _k8sCoreOperations = new Mock<ICoreV1Operations>();

        _options = Options.Create(new WorkflowManagerOptions());
        _options.Value.Messaging.PublisherSettings.Add("endpoint", "1.2.2.3/virtualhost");
        _options.Value.Messaging.PublisherSettings.Add("username", "username");
        _options.Value.Messaging.PublisherSettings.Add("password", "password");
        _options.Value.Messaging.PublisherSettings.Add("exchange", "exchange");
        _options.Value.Messaging.PublisherSettings.Add("virtualHost", "vhost");
        _options.Value.Messaging.Topics.TaskCallbackRequest = "md.tasks.callback";
        _options.Value.ArgoTtlStatergySeconds = _argoTtlStatergySeconds;
        _options.Value.MinArgoTtlStatergySeconds = _minAgoTtlStatergySeconds;

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
        _argoProvider.Setup(p => p.CreateClient(It.IsAny<string>(), It.IsAny<string?>(), true)).Returns(_argoClient.Object);
        _kubernetesProvider.Setup(p => p.CreateClient()).Returns(_kubernetesClient.Object);
        _kubernetesClient.SetupGet<ICoreV1Operations>(p => p.CoreV1).Returns(_k8sCoreOperations.Object);
    }

    [Fact(DisplayName = "Throws when missing required plug-in arguments")]
    public void ArgoPlugin_ThrowsWhenMissingPluginArguments()
    {
        var message = GenerateTaskDispatchEvent();
        Assert.Throws<InvalidTaskException>(() => new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));

        foreach (var key in Keys.RequiredParameters.Take(Keys.RequiredParameters.Count - 1))
        {
            message.TaskPluginArguments.Add(key, Guid.NewGuid().ToString());
            Assert.Throws<InvalidTaskException>(() => new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));
        }
        message.TaskPluginArguments[Keys.RequiredParameters[Keys.RequiredParameters.Count - 1]] = Guid.NewGuid().ToString();
        Assert.Throws<InvalidTaskException>(() => new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));

        message.TaskPluginArguments[Keys.BaseUrl] = "/api";
        Assert.Throws<InvalidTaskException>(() => new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));
    }

    [Fact(DisplayName = "Throws when missing required settings")]
    public void ArgoPlugin_ThrowsWhenMissingRequiredSettings()
    {
        var message = GenerateTaskDispatchEventWithValidArguments();

        _options.Value.Messaging.PublisherSettings.Remove("password");
        Assert.Throws<ConfigurationException>(() => new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));

        foreach (var key in Keys.RequiredSettings.Take(Keys.RequiredSettings.Count - 1))
        {
            message.TaskPluginArguments.Add(key, Guid.NewGuid().ToString());
            Assert.Throws<ConfigurationException>(() => new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));
        }
        message.TaskPluginArguments[Keys.RequiredSettings[Keys.RequiredSettings.Count - 1]] = Guid.NewGuid().ToString();
        Assert.Throws<ConfigurationException>(() => new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message));
    }

    [Fact(DisplayName = "Initializes values")]
    public void ArgoPlugin_InitializesValues()
    {
        var message = GenerateTaskDispatchEventWithValidArguments();

        _ = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
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

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskExecutionStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal("error", result.Errors);

        _argoClient.Verify(p => p.WorkflowService_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        _k8sCoreOperations.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(3));

        await runner.DisposeAsync().ConfigureAwait(false);
        _k8sCoreOperations.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<V1DeleteOptions>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<bool?>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(3));
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

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskExecutionStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal("error", result.Errors);

        _argoClient.Verify(p => p.WorkflowService_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Never());
        _k8sCoreOperations.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Once());

        await runner.DisposeAsync().ConfigureAwait(false);
        _k8sCoreOperations.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
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

    [Fact(DisplayName = "ExecuteTask - returns ExecutionStatus when failed to load WorkflowTemplate")]
    public async Task ArgoPlugin_ExecuteTask_ReturnsExecutionStatusWhenFailedToLoadWorkflowTemplate()
    {
        var message = GenerateTaskDispatchEventWithValidArguments();

        _argoClient.Setup(p => p.WorkflowTemplateService_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("error"));

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskExecutionStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal("error", result.Errors);

        _argoClient.Verify(p => p.WorkflowService_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Never());
        _k8sCoreOperations.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Never());

        await runner.DisposeAsync().ConfigureAwait(false);
        _k8sCoreOperations.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
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

    [Fact(DisplayName = "ExecuteTask - returns ExecutionStatus when failed to locate entrypoint template")]
    public async Task ArgoPlugin_ExecuteTask_ReturnsExecutionStatusWhenFailedToLocateTemplate()
    {
        var message = GenerateTaskDispatchEventWithValidArguments();

        var argoTemplate = GenerateWorkflowTemplate(message);
        argoTemplate.Spec.Entrypoint = "missing-template";
        _argoClient.Setup(p => p.WorkflowTemplateService_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(argoTemplate);

        SetupKubbernetesSecrets().Throws(new Exception("error"));
        SetupKubernetesDeleteSecret();

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskExecutionStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal($"Template '{argoTemplate.Spec.Entrypoint}' cannot be found in the referenced WorkflowTmplate '{message.TaskPluginArguments[Keys.WorkflowTemplateName]}'.", result.Errors);

        _argoClient.Verify(p => p.WorkflowService_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Never());
        _k8sCoreOperations.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Never());

        await runner.DisposeAsync().ConfigureAwait(false);
        _k8sCoreOperations.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
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

    [Theory(DisplayName = "ExecuteTask - WorkflowTemplate test")]
    [InlineData("SimpleTemplate.yml", 3)]
    [InlineData("DagWithIntermediateArtifacts.yml", 3)]
    public async Task ArgoPlugin_ExecuteTask_WorkflowTemplates(string filename, int secretsCreated)
    {
        var argoTemplate = LoadArgoTemplate(filename);
        Assert.NotNull(argoTemplate);

        var message = GenerateTaskDispatchEventWithValidArguments();
        message.TaskPluginArguments["resources"] = "{\"memory_reservation\": \"string\",\"cpu_reservation\": \"string\",\"gpu_limit\": 1,\"memory_limit\": \"string\",\"cpu_limit\": \"string\"}";
        message.TaskPluginArguments["priorityClass"] = "Helo";
        Workflow? submittedArgoTemplate = null;

        _argoClient.Setup(p => p.WorkflowTemplateService_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(argoTemplate);
        _argoClient.Setup(p => p.WorkflowService_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()))
            .Callback((string ns, WorkflowCreateRequest body, CancellationToken cancellationToken) =>
            {
                submittedArgoTemplate = body.Workflow;
            })
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

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
        Assert.Equal(FailureReason.None, result.FailureReason);
        Assert.Empty(result.Errors);

        _argoClient.Verify(p => p.WorkflowService_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        _k8sCoreOperations.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(secretsCreated));

        await runner.DisposeAsync().ConfigureAwait(false);
        _k8sCoreOperations.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<V1DeleteOptions>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<bool?>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(secretsCreated));

        Assert.NotNull(submittedArgoTemplate);
        if (filename == "SimpleTemplate.yml") ValidateSimpleTemplate(message, submittedArgoTemplate!);
        else if (filename == "DagWithIntermediateArtifacts.yml") ValidateDagWithIntermediateArtifacts(message, submittedArgoTemplate!);
        else
            Assert.True(false);
    }

    private static void ValidateDagWithIntermediateArtifacts(TaskDispatchEvent message, Workflow workflow)
    {
        var template = workflow.Spec.Templates.FirstOrDefault(p => p.Name.Equals("my-entrypoint", StringComparison.Ordinal));
        Assert.NotNull(template!);
        Assert.Equal(message.Inputs.First(p => p.Name.Equals("input-dicom")).RelativeRootPath, template!.Dag.Tasks.ElementAt(0).Arguments.Artifacts.First().S3.Key);
        Assert.Equal(message.Inputs.First(p => p.Name.Equals("input-dicom")).RelativeRootPath, template!.Dag.Tasks.ElementAt(1).Arguments.Artifacts.First().S3.Key);
        Assert.Null(template.Dag.Tasks.ElementAt(2).Arguments.Artifacts.ElementAt(0).S3);
        Assert.Null(template.Dag.Tasks.ElementAt(2).Arguments.Artifacts.ElementAt(1).S3);

        template = workflow.Spec.Templates.FirstOrDefault(p => p.Name.Equals("segmentation", StringComparison.Ordinal));
        Assert.NotNull(template!);
        Assert.Null(template!.Inputs.Artifacts.First().S3);
        Assert.Equal($"{message.IntermediateStorage.RelativeRootPath}/{template.Outputs.Artifacts.First().Name}", template.Outputs.Artifacts.First().S3.Key);

        template = workflow.Spec.Templates.FirstOrDefault(p => p.Name.Equals("inference", StringComparison.Ordinal));
        Assert.NotNull(template!);
        Assert.Null(template!.Inputs.Artifacts.First().S3);
        Assert.Equal($"{message.IntermediateStorage.RelativeRootPath}/{template.Outputs.Artifacts.First().Name}", template.Outputs.Artifacts.First().S3.Key);

        template = workflow.Spec.Templates.FirstOrDefault(p => p.Name.Equals("generate-report", StringComparison.Ordinal));
        Assert.NotNull(template!);
        Assert.Null(template!.Inputs.Artifacts.ElementAt(0).S3);
        Assert.Null(template.Inputs.Artifacts.ElementAt(1).S3);
        Assert.Equal(message.Outputs.First(p => p.Name.Equals("output")).RelativeRootPath, template.Outputs.Artifacts.First().S3.Key);
    }

    private static void ValidateSimpleTemplate(TaskDispatchEvent message, Workflow workflow)
    {
        var firstTemplate = workflow.Spec.Templates.FirstOrDefault(p => p.Name.Equals("my-entrypoint", StringComparison.Ordinal));

        foreach (var template in workflow.Spec.Templates.Where(w => w.Container is not null))
        {
            Assert.True(template.Container.Resources is not null);
            Assert.True(template.Container.Resources?.Limits is not null);
            Assert.True(template.Container.Resources?.Requests is not null);
            var value = "";

            Assert.True(template.Container.Resources?.Requests?.TryGetValue("requests.memory", out value));
            Assert.True(value == "string");
            Assert.True(template.Container.Resources?.Requests?.TryGetValue("requests.cpu", out value));
            Assert.True(value == "string");
            Assert.True(template.Container.Resources?.Limits?.TryGetValue("limits.memory", out value));
            Assert.True(value == "string");
            Assert.True(template.Container.Resources?.Limits?.TryGetValue("limits.cpu", out value));
            Assert.True(value == "string");
            Assert.True(template.Container.Resources?.Requests?.TryGetValue("nvidia.com/gpu", out value));
            Assert.True(value == "1");

            Assert.True(template.PriorityClassName == "Helo");
        }

        Assert.NotNull(firstTemplate);

        Assert.Equal(message.Inputs.First(p => p.Name.Equals("input-dicom")).RelativeRootPath, firstTemplate!.Inputs.Artifacts.ElementAt(0).S3.Key);
    }

    [Fact(DisplayName = "ExecuteTask - Waits until Succeeded Phase")]
    public async Task ArgoPlugin_GetStatus_WaitUntilSucceededPhase()
    {
        var tryCount = 0;
        _argoClient.Setup(p => p.WorkflowService_GetWorkflowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string ns, string name, string version, string fields, CancellationToken cancellationToken) =>
            {
                if (tryCount++ < 2)
                {
                    return new Workflow
                    {
                        Status = new WorkflowStatus
                        {
                            Phase = Strings.ArgoPhaseRunning,
                            Message = string.Empty
                        }
                    };
                }

                return new Workflow
                {
                    Status = new WorkflowStatus
                    {
                        Phase = Strings.ArgoPhaseSucceeded,
                        Message = Strings.ArgoPhaseSucceeded,
                    }
                };
            });

        var message = GenerateTaskDispatchEventWithValidArguments();

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.GetStatus("identity", CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskExecutionStatus.Succeeded, result.Status);
        Assert.Equal(FailureReason.None, result.FailureReason);
        Assert.Empty(result.Errors);

        _argoClient.Verify(p => p.WorkflowService_GetWorkflowAsync(It.Is<string>(p => p.Equals("namespace", StringComparison.OrdinalIgnoreCase)), It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact(DisplayName = "GetStatus - Stats contains info")]
    public async Task ArgoPlugin_GetStatus_HasStatsInfo()
    {
        var tryCount = 0;
        _argoClient.Setup(p => p.WorkflowService_GetWorkflowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string ns, string name, string version, string fields, CancellationToken cancellationToken) =>
            {
                if (tryCount++ < 2)
                {
                    return new Workflow
                    {
                        Status = new WorkflowStatus
                        {
                            Phase = Strings.ArgoPhaseRunning,
                            Message = string.Empty
                        }
                    };
                }

                return new Workflow
                {
                    Status = new WorkflowStatus
                    {
                        Phase = Strings.ArgoPhaseSucceeded,
                        Message = Strings.ArgoPhaseSucceeded,
                        Nodes = new Dictionary<string, NodeStatus>
                        {
                            { "first", new NodeStatus { Id = "firstId" } },
                            { "second", new NodeStatus { Id = "secondId" } },
                            { "third", new NodeStatus { Id = "thirdId" } },
                        }
                    }
                };
            });

        var message = GenerateTaskDispatchEventWithValidArguments();

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.GetStatus("identity", CancellationToken.None).ConfigureAwait(false);

        var objNodeInfo = result?.Stats;
        Assert.NotNull(objNodeInfo);
#pragma warning disable CS8604 // Possible null reference argument.
        var nodeInfo = ValiateCanConvertToDictionary(objNodeInfo);
#pragma warning restore CS8604 // Possible null reference argument.

        Assert.Equal(7, nodeInfo.Values.Count);
        Assert.Equal("{\"id\":\"firstId\"}", nodeInfo["nodes.first"]);
        Assert.Empty(result?.Errors);

        _argoClient.Verify(p => p.WorkflowService_GetWorkflowAsync(It.Is<string>(p => p.Equals("namespace", StringComparison.OrdinalIgnoreCase)), It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    public static Dictionary<string, string> ValiateCanConvertToDictionary(object obj)
    {
        var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
#pragma warning disable CS8603 // Possible null reference return.
        return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
#pragma warning restore CS8603 // Possible null reference return.
    }


    [Theory(DisplayName = "GetStatus - returns ExecutionStatus on success")]
    [InlineData(Strings.ArgoPhaseSucceeded)]
    [InlineData(Strings.ArgoPhaseFailed)]
    [InlineData(Strings.ArgoPhaseError)]
    [InlineData(Strings.ArgoPhaseSkipped)]
    [InlineData(Strings.ArgoPhasePending)]
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

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.GetStatus("identity", CancellationToken.None).ConfigureAwait(false);

        if (phase == Strings.ArgoPhaseSucceeded)
        {
            Assert.Equal(TaskExecutionStatus.Succeeded, result.Status);
            Assert.Equal(FailureReason.None, result.FailureReason);
            Assert.Empty(result.Errors);
        }
        else if (Strings.ArgoFailurePhases.Contains(phase, StringComparer.OrdinalIgnoreCase))
        {
            Assert.Equal(TaskExecutionStatus.Failed, result.Status);
            Assert.Equal(FailureReason.ExternalServiceError, result.FailureReason);
            Assert.Equal("error", result.Errors);
        }
        else
        {
            Assert.Equal(TaskExecutionStatus.Failed, result.Status);
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

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.GetStatus("identity", CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskExecutionStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal("error", result.Errors);

        _argoClient.Verify(p => p.WorkflowService_GetWorkflowAsync(It.Is<string>(p => p.Equals("namespace", StringComparison.OrdinalIgnoreCase)), It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = "ImagePullSecrets - get copied accross")]
    public async Task ArgoPlugin_Copies_ImagePullSecrets()
    {
        var argoTemplate = LoadArgoTemplate("SimpleTemplate.yml");
        Assert.NotNull(argoTemplate);

        var secret = new LocalObjectReference() { Name = "ImagePullSecret" };
        argoTemplate.Spec.ImagePullSecrets = new List<LocalObjectReference>() { secret };

        var message = GenerateTaskDispatchEventWithValidArguments();

        SetUpSimpleArgoWorkFlow(argoTemplate);

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
        Assert.Equal(secret, _submittedArgoTemplate?.Spec.ImagePullSecrets.First());
    }

    [Fact(DisplayName = "TTL gets added if not pressent")]
    public async Task ArgoPlugin_Ensures_TTL_Added_If_Not_pressent()
    {
        var argoTemplate = LoadArgoTemplate("SimpleTemplate.yml");
        Assert.NotNull(argoTemplate);

        SetUpSimpleArgoWorkFlow(argoTemplate);

        var message = GenerateTaskDispatchEventWithValidArguments();

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
        Assert.Equal(_argoTtlStatergySeconds, _submittedArgoTemplate?.Spec.TtlStrategy?.SecondsAfterCompletion);
    }

    [Theory(DisplayName = "TTL gets extended if too short")]
    [InlineData(31, 31, 29)]
    [InlineData(1, null, null)]
    [InlineData(null, 31, 3)]
    public async Task ArgoPlugin_Ensures_TTL_Extended_If_Too_Short(int? secondsAfterCompletion, int? secondsAfterSuccess, int? secondsAfterFailure)
    {
        var argoTemplate = LoadArgoTemplate("SimpleTemplate.yml");
        Assert.NotNull(argoTemplate);

        argoTemplate.Spec.TtlStrategy = new TTLStrategy();
        argoTemplate.Spec.TtlStrategy.SecondsAfterCompletion = secondsAfterCompletion;
        argoTemplate.Spec.TtlStrategy.SecondsAfterSuccess = secondsAfterSuccess;
        argoTemplate.Spec.TtlStrategy.SecondsAfterFailure = secondsAfterFailure;

        SetUpSimpleArgoWorkFlow(argoTemplate);

        var message = GenerateTaskDispatchEventWithValidArguments();

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
        if (secondsAfterCompletion is not null)
        {
            Assert.Equal(Math.Max(_minAgoTtlStatergySeconds, secondsAfterCompletion.Value), _submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterCompletion);
        }
        else
        {
            Assert.Null(_submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterCompletion);
        }

        if (secondsAfterSuccess is not null)
        {
            Assert.Equal(Math.Max(_minAgoTtlStatergySeconds, secondsAfterSuccess.Value), _submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterSuccess);
        }
        else
        {
            Assert.Null(_submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterSuccess);
        }

        if (secondsAfterFailure is not null)
        {
            Assert.Equal(Math.Max(_minAgoTtlStatergySeconds, secondsAfterFailure.Value), _submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterFailure);
        }
        else
        {
            Assert.Null(_submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterFailure);
        }
    }

    [Theory(DisplayName = "TTL gets left as is if longer")]
    [InlineData(31, 31, 31)]
    [InlineData(9999999, 31, 31)]
    [InlineData(31, null, null)]
    [InlineData(31, 31, null)]
    public async Task ArgoPlugin_Ensures_TTL_Remains(int? secondsAfterCompletion, int? secondsAfterSuccess, int? secondsAfterFailure)
    {
        var argoTemplate = LoadArgoTemplate("SimpleTemplate.yml");
        Assert.NotNull(argoTemplate);

        argoTemplate.Spec.TtlStrategy = new TTLStrategy();
        argoTemplate.Spec.TtlStrategy.SecondsAfterCompletion = secondsAfterCompletion;
        argoTemplate.Spec.TtlStrategy.SecondsAfterSuccess = secondsAfterSuccess;
        argoTemplate.Spec.TtlStrategy.SecondsAfterFailure = secondsAfterFailure;

        SetUpSimpleArgoWorkFlow(argoTemplate);

        var message = GenerateTaskDispatchEventWithValidArguments();

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
        Assert.Equal(secondsAfterCompletion, _submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterCompletion);
        Assert.Equal(secondsAfterSuccess, _submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterSuccess);
        Assert.Equal(secondsAfterFailure, _submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterFailure);
    }

    [Fact(DisplayName = "pocGC gets removed if pressent")]
    public async Task ArgoPlugin_Ensures_podGC_is_removed()
    {
        var argoTemplate = LoadArgoTemplate("SimpleTemplate.yml");
        Assert.NotNull(argoTemplate);

        argoTemplate.Spec.PodGC = new PodGC { Strategy = "OnePodSeccess" };

        SetUpSimpleArgoWorkFlow(argoTemplate);

        var message = GenerateTaskDispatchEventWithValidArguments();

        var runner = new ArgoPlugin(_serviceScopeFactory.Object, _logger.Object, _options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
        Assert.Null(_submittedArgoTemplate?.Spec.PodGC);

    }
    private void SetUpSimpleArgoWorkFlow(WorkflowTemplate argoTemplate)
    {
        Assert.NotNull(argoTemplate);

        _argoClient.Setup(p => p.WorkflowTemplateService_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(argoTemplate);

        _argoClient.Setup(p => p.WorkflowService_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()))
            .Callback((string ns, WorkflowCreateRequest body, CancellationToken cancellationToken) =>
            {
                _submittedArgoTemplate = body.Workflow;
            })
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
    }

    private static TaskDispatchEvent GenerateTaskDispatchEventWithValidArguments()
    {
        var message = GenerateTaskDispatchEvent();
        message.TaskPluginArguments[Keys.BaseUrl] = "http://api-endpoint/";
        message.TaskPluginArguments[Keys.WorkflowTemplateName] = "workflowTemplate";
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
            TaskPluginType = Guid.NewGuid().ToString(),
            WorkflowInstanceId = Guid.NewGuid().ToString(),
            TaskId = Guid.NewGuid().ToString(),
            IntermediateStorage = new Messaging.Common.Storage
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
            }
        };
        message.Inputs.Add(new Messaging.Common.Storage
        {
            Name = "input-dicom",
            Endpoint = Guid.NewGuid().ToString(),
            Credentials = new Messaging.Common.Credentials
            {
                AccessKey = Guid.NewGuid().ToString(),
                AccessToken = Guid.NewGuid().ToString()
            },
            Bucket = Guid.NewGuid().ToString(),
            RelativeRootPath = Guid.NewGuid().ToString(),
        });
        message.Inputs.Add(new Messaging.Common.Storage
        {
            Name = "input-ehr",
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
            Name = "output",
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

    private static WorkflowTemplate GenerateWorkflowTemplate(TaskDispatchEvent taskDispatchEvent) => new()
    {
        Kind = "WorkflowTemplate",
        Metadata = new ObjectMeta()
        {
            Name = taskDispatchEvent.TaskPluginArguments[Keys.WorkflowTemplateName],
        },
        Spec = new WorkflowSpec
        {
            Entrypoint = "EntrypointTemplate",
            Templates = new List<Template2>
            {
                new Template2
                {
                     Name = "EntrypointTemplate",
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
                                  Template = "T1.Step1b",
                                  Arguments = new Arguments
                                  {
                                      Artifacts = new List<Artifact>
                                      {
                                          new Artifact()
                                          {
                                              Name = "T1.Step1b.Artifact1"
                                          }
                                      }
                                  }
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
                                  Name = "T1.Dag",
                                  Template = "T1.Dag"
                             }
                        }
                     }
                },

                new Template2
                {
                     Name= "T1.Dag",
                     Dag = new DAGTemplate
                     {
                          Tasks = new List<DAGTask>
                          {
                              new DAGTask
                              {
                                    Name = "T1.Dag.Task1",
                                    Template = "T1.Dag.Task1",
                                    Arguments = new Arguments
                                    {
                                        Artifacts = new List<Artifact>
                                        {
                                            new Artifact()
                                            {
                                                  Name = taskDispatchEvent.Inputs[0].Name
                                            }
                                        }
                                    }
                              },
                              new DAGTask
                              {
                                    Name = "T1.Dag.Task2",
                                    Template = "T1.Dag.Task2",
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
                    Inputs= new Inputs
                    {
                         Artifacts = new List<Artifact>
                         {
                             new Artifact
                             {
                                 Name = "T1.Step1b.Artifact1"
                             },
                             new Artifact
                             {
                                 Name = taskDispatchEvent.Inputs.First().Name,
                             }
                         },
                    }
                },
                new Template2
                {
                    Name = "T1.Step2a",
                },
                new Template2
                {
                    Name = "T1.Dag.Task1",
                    Inputs= new Inputs
                    {
                         Artifacts = new List<Artifact>
                         {
                             new Artifact
                             {
                                 Name = "T1.Dag.Task1.Artifact1"
                             },
                             new Artifact
                             {
                                 Name = "T1.Dag.Task1.Artifact2"
                             }
                         },
                    }
                },
                new Template2
                {
                    Name = "T1.Dag.Task2",
                    Inputs = new Inputs
                    {
                        Artifacts = new List<Artifact>()
                        {
                            new Artifact
                            {
                                Name = "missing.storage.info"
                            }
                        }
                    },
                    Outputs= new Outputs
                    {
                        Artifacts = new List<Artifact>()
                        {
                            new Artifact
                            {
                                Name = "missing.storage.info"
                            }
                        }
                    },
                }
            }
        }
    };

    private static WorkflowTemplate? LoadArgoTemplate(string filename)
    {
        var templateString = File.ReadAllText($"Templates/{filename}");

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<WorkflowTemplate>(templateString);
    }

    private ISetup<ICoreV1Operations, Task<HttpOperationResponse<V1Secret>>> SetupKubbernetesSecrets() => _k8sCoreOperations.Setup(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
                           It.IsAny<V1Secret>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<bool?>(),
                           It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
                           It.IsAny<CancellationToken>()));

    private void SetupKubernetesDeleteSecret() => _k8sCoreOperations.Setup(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
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

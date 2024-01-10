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
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.SharedTest;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Moq;
using Moq.Language.Flow;
using Newtonsoft.Json;
using Xunit;
using YamlDotNet.Serialization;
using Options = Microsoft.Extensions.Options.Options;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.Tests;

public class ArgoPluginTest : ArgoPluginTestBase
{
    private readonly Mock<ILogger<ArgoPlugin>> _logger;
    private Workflow? _submittedArgoTemplate;

    public ArgoPluginTest()
    {
        _logger = new Mock<ILogger<ArgoPlugin>>();

        _logger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    }

    [Fact(DisplayName = "Throws when missing required plug-in arguments")]
    public void ArgoPlugin_ThrowsWhenMissingPluginArguments()
    {
        var message = GenerateTaskDispatchEvent();
        Assert.Throws<InvalidTaskException>(() => new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message));
    }

    [Fact(DisplayName = "Throws when missing required settings")]
    public void ArgoPlugin_ThrowsWhenMissingRequiredSettings()
    {
        var message = GenerateTaskDispatchEventWithValidArguments();

        Options.Value.Messaging.PublisherSettings.Remove("password");
        Assert.Throws<ConfigurationException>(() => new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message));

        foreach (var key in ArgoParameters.RequiredSettings.Take(ArgoParameters.RequiredSettings.Count - 1))
        {
            message.TaskPluginArguments.Add(key, Guid.NewGuid().ToString());
            Assert.Throws<ConfigurationException>(() => new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message));
        }
        message.TaskPluginArguments[ArgoParameters.RequiredSettings[ArgoParameters.RequiredSettings.Count - 1]] = Guid.NewGuid().ToString();
        Assert.Throws<ConfigurationException>(() => new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message));
    }

    [Fact(DisplayName = "Initializes values")]
    public void ArgoPlugin_InitializesValues()
    {
        var message = GenerateTaskDispatchEventWithValidArguments();

        _ = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        _logger.VerifyLogging($"Argo plugin initialized: namespace=namespace, base URL=http://api-endpoint/, activeDeadlineSeconds=50, apiToken configured=true. allowInsecure=true", LogLevel.Information, Times.Once());
    }

    [Fact(DisplayName = "ExecuteTask - returns ExecutionStatus on failure")]
    public async Task ArgoPlugin_ExecuteTask_ReturnsExecutionStatusOnFailure()
    {
        var message = GenerateTaskDispatchEventWithValidArguments();

        ArgoClient.Setup(p => p.Argo_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(GenerateWorkflowTemplate(message));
        ArgoClient.Setup(p => p.Argo_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception("error"));

        SetupKubbernetesSecrets()
            .Returns((V1Secret body, string ns, string dr, string fm, string fv, bool? pretty, IReadOnlyDictionary<string, IReadOnlyList<string>> headers, CancellationToken ct) =>
            {
                return Task.FromResult(new HttpOperationResponse<V1Secret> { Body = body, Response = new HttpResponseMessage { } });
            });
        SetupKubernetesDeleteSecret();

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal("error", result.Errors);

        ArgoClient.Verify(p => p.Argo_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        K8sCoreOperations.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(3));

        await runner.DisposeAsync();
        K8sCoreOperations.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
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

        ArgoClient.Setup(p => p.Argo_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(GenerateWorkflowTemplate(message));

        SetupKubbernetesSecrets()
            .Throws(new Exception("error"));
        SetupKubernetesDeleteSecret();

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal("error", result.Errors);

        ArgoClient.Verify(p => p.Argo_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Never());
        K8sCoreOperations.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Once());

        await runner.DisposeAsync();
        K8sCoreOperations.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
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

        ArgoClient.Setup(p => p.Argo_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("error"));

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal("error", result.Errors);

        ArgoClient.Verify(p => p.Argo_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Never());
        K8sCoreOperations.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Never());

        await runner.DisposeAsync();
        K8sCoreOperations.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
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
        ArgoClient.Setup(p => p.Argo_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(argoTemplate);

        SetupKubbernetesSecrets().Throws(new Exception("error"));
        SetupKubernetesDeleteSecret();

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal($"Template '{argoTemplate.Spec.Entrypoint}' cannot be found in the referenced WorkflowTmplate '{message.TaskPluginArguments[ArgoParameters.WorkflowTemplateName]}'.", result.Errors);

        ArgoClient.Verify(p => p.Argo_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Never());
        K8sCoreOperations.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Never());

        await runner.DisposeAsync();
        K8sCoreOperations.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
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
    [InlineData("SimpleTemplate.yml", 3, true)]
    [InlineData("DagWithIntermediateArtifacts.yml", 3)]
    public async Task ArgoPlugin_ExecuteTask_WorkflowTemplates(string filename, int secretsCreated, bool withoutDefaultArguments = false)
    {
        var argoTemplate = LoadArgoTemplate(filename);
        Assert.NotNull(argoTemplate);

        var message = GenerateTaskDispatchEventWithValidArguments(withoutDefaultArguments);
        message.TaskPluginArguments["gpu_required"] = "true";
        message.TaskPluginArguments["memory"] = "1";
        message.TaskPluginArguments["cpu"] = "1";
        message.TaskPluginArguments["priority"] = "Helo";
        Workflow? submittedArgoTemplate = null;

        ArgoClient.Setup(p => p.Argo_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(argoTemplate);
        ArgoClient.Setup(p => p.Argo_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()))
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

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
        Assert.Equal(FailureReason.None, result.FailureReason);
        Assert.Empty(result.Errors);

        ArgoClient.Verify(p => p.Argo_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        K8sCoreOperations.Verify(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
            It.IsAny<V1Secret>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool?>(),
            It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(secretsCreated));

        await runner.DisposeAsync();
        K8sCoreOperations.Verify(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
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
            var value = "";

            Assert.True(template.Container.Resources?.Limits?.TryGetValue("memory", out value));
            Assert.True(value == "1");
            Assert.True(template.Container.Resources?.Limits?.TryGetValue("cpu", out value));
            Assert.True(value == "1");
            Assert.True(template.Container.Resources?.Limits?.TryGetValue("nvidia.com/gpu", out value));
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
        ArgoClient.Setup(p => p.Argo_GetWorkflowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.GetStatus("identity", new TaskCallbackEvent(), CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Succeeded, result.Status);
        Assert.Equal(FailureReason.None, result.FailureReason);
        Assert.Empty(result.Errors);

        ArgoClient.Verify(p => p.Argo_GetWorkflowAsync(It.Is<string>(p => p.Equals("namespace", StringComparison.OrdinalIgnoreCase)), It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact(DisplayName = "GetStatus - Stats contains info")]
    public async Task ArgoPlugin_GetStatus_HasStatsInfo()
    {
        var tryCount = 0;
        ArgoClient.Setup(p => p.Argo_GetWorkflowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.GetStatus("identity", new TaskCallbackEvent(), CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        var objNodeInfo = result?.Stats;
        Assert.NotNull(objNodeInfo);
#pragma warning disable CS8604 // Possible null reference argument.
        var nodeInfo = ValiateCanConvertToDictionary(objNodeInfo);

        Assert.Equal(7, nodeInfo.Values.Count);
        Assert.Equal("{\"id\":\"firstId\"}", nodeInfo["nodes.first"]);
        Assert.Empty(result?.Errors);

        ArgoClient.Verify(p => p.Argo_GetWorkflowAsync(It.Is<string>(p => p.Equals("namespace", StringComparison.OrdinalIgnoreCase)), It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact(DisplayName = "GetStatus - Stats contains info")]
    public async Task ArgoPlugin_GetStatus_Argregates_stats()
    {
        var tryCount = 0;
        ArgoClient.Setup(p => p.Argo_GetWorkflowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
                            { "first", new NodeStatus { Id = "firstId" ,Type="Pod" ,Name="", StartedAt = DateTime.SpecifyKind(new DateTime(2023,3,3), DateTimeKind.Utc) , FinishedAt = DateTime.SpecifyKind(new DateTime(2023,3,3,8,0,32), DateTimeKind.Utc)} },
                            { "second", new NodeStatus { Id = "secondId",Type="Pod" ,Name = $"node-{Strings.ExitHookTemplateSendTemplateName}"  , StartedAt = DateTime.SpecifyKind(new DateTime(2023,3,4), DateTimeKind.Utc) , FinishedAt = DateTime.SpecifyKind(new DateTime(2023,3,4,8,0,32), DateTimeKind.Utc)} },
                            { "third", new NodeStatus { Id = "thirdId" ,Type="Pod" ,Name="", StartedAt = DateTime.SpecifyKind(new DateTime(2023,3,4) , DateTimeKind.Utc), FinishedAt = DateTime.SpecifyKind(new DateTime(2023,3,4,8,0,32), DateTimeKind.Utc)}  },
                        }
                    }
                };
            });

        var message = GenerateTaskDispatchEventWithValidArguments();

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.GetStatus("identity", new TaskCallbackEvent(), CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        var objNodeInfo = result?.Stats;
        Assert.NotNull(objNodeInfo);
#pragma warning disable CS8604 // Possible null reference argument.
        var nodeInfo = ValiateCanConvertToDictionary(objNodeInfo);
#pragma warning restore CS8604 // Possible null reference argument.

        Assert.True(nodeInfo.ContainsKey("podStartTime0"));
        Assert.True(nodeInfo.ContainsKey("podFinishTime0"));
        Assert.True(nodeInfo.ContainsKey("send-messagepodFinishTime1"));
        Assert.Equal("2023-03-03 08:00:32Z", nodeInfo["podFinishTime0"]);

        ArgoClient.Verify(p => p.Argo_GetWorkflowAsync(It.Is<string>(p => p.Equals("namespace", StringComparison.OrdinalIgnoreCase)), It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
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
        ArgoClient.Setup(p => p.Argo_GetWorkflowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.GetStatus("identity", new TaskCallbackEvent(), CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

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

        ArgoClient.Verify(p => p.Argo_GetWorkflowAsync(It.Is<string>(p => p.Equals("namespace", StringComparison.OrdinalIgnoreCase)), It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = "GetStatus - returns ExecutionStatus on failure")]
    public async Task ArgoPlugin_GetStatus_ReturnsExecutionStatusOnFailure()
    {
        ArgoClient.Setup(p => p.Argo_GetWorkflowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception("error"));

        var message = GenerateTaskDispatchEventWithValidArguments();

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.GetStatus("identity", new TaskCallbackEvent(), CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Failed, result.Status);
        Assert.Equal(FailureReason.PluginError, result.FailureReason);
        Assert.Equal("error", result.Errors);

        ArgoClient.Verify(p => p.Argo_GetWorkflowAsync(It.Is<string>(p => p.Equals("namespace", StringComparison.OrdinalIgnoreCase)), It.Is<string>(p => p.Equals("identity", StringComparison.OrdinalIgnoreCase)), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
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

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
        Assert.Equal(secret, _submittedArgoTemplate?.Spec.ImagePullSecrets.First());
    }

    [Fact(DisplayName = "TTL gets added if not present")]
    public async Task ArgoPlugin_Ensures_TTL_Added_If_Not_present()
    {
        var argoTemplate = LoadArgoTemplate("SimpleTemplate.yml");
        Assert.NotNull(argoTemplate);

        SetUpSimpleArgoWorkFlow(argoTemplate);

        var message = GenerateTaskDispatchEventWithValidArguments();

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
        Assert.Equal(ArgoTtlStatergySeconds, _submittedArgoTemplate?.Spec.TtlStrategy?.SecondsAfterSuccess);
    }

    [Fact(DisplayName = "Argo Plugin adds required resource limits")]
    public async Task ArgoPlugin_Adds_Container_Resource_Restrictions_Based_On_Configured_Values()
    {
        var argoTemplate = LoadArgoTemplate("SimpleTemplate.yml");
        Assert.NotNull(argoTemplate);

        SetUpSimpleArgoWorkFlow(argoTemplate);

        var message = GenerateTaskDispatchEventWithValidArguments();

        var expectedPodSpecPatch = "{\"initContainers\":[{\"name\":\"init\",\"resources\":{\"limits\":{\"cpu\":\"" + InitContainerCpuLimit + "\",\"memory\": \"" +
                                                   InitContainerMemoryLimit +
                                                   "\"},\"requests\":{\"cpu\":\"0\",\"memory\":\"0Mi\"}}}],\"containers\":[{\"name\":\"wait\",\"resources\":{\"limits\":{\"cpu\":\"" +
                                                   WaitContainerCpuLimit + "\",\"memory\":\"" + WaitContainerMemoryLimit +
                                                   "\"},\"requests\":{\"cpu\":\"0\",\"memory\":\"0Mi\"}}}]}";

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        Assert.Equal(MessageGeneratorContainerCpuLimit, _submittedArgoTemplate?.Spec.Templates.FirstOrDefault(p => p.Name == Strings.ExitHookTemplateSendTemplateName).Container.Resources.Limits["cpu"]);
        Assert.Equal(MessageGeneratorContainerMemoryLimit, _submittedArgoTemplate?.Spec.Templates.FirstOrDefault(p => p.Name == Strings.ExitHookTemplateSendTemplateName).Container.Resources.Limits["memory"]);
        Assert.Equal(expectedPodSpecPatch, _submittedArgoTemplate?.Spec.Templates.FirstOrDefault(p => p.Name == Strings.ExitHookTemplateSendTemplateName).PodSpecPatch);
    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
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

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
        if (secondsAfterCompletion is not null)
        {
            Assert.Equal(Math.Max(MinAgoTtlStatergySeconds, secondsAfterCompletion.Value), _submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterCompletion);
        }
        else
        {
            Assert.Null(_submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterCompletion);
        }

        if (secondsAfterSuccess is not null)
        {
            Assert.Equal(Math.Max(MinAgoTtlStatergySeconds, secondsAfterSuccess.Value), _submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterSuccess);
        }
        else
        {
            Assert.Null(_submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterSuccess);
        }

        if (secondsAfterFailure is not null)
        {
            Assert.Equal(Math.Max(MinAgoTtlStatergySeconds, secondsAfterFailure.Value), _submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterFailure);
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

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
        Assert.Equal(secondsAfterCompletion, _submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterCompletion);
        Assert.Equal(secondsAfterSuccess, _submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterSuccess);
        Assert.Equal(secondsAfterFailure, _submittedArgoTemplate?.Spec.TtlStrategy.SecondsAfterFailure);
    }

    [Fact(DisplayName = "pocGC gets removed if present")]
    public async Task ArgoPlugin_Ensures_podGC_is_removed()
    {
        var argoTemplate = LoadArgoTemplate("SimpleTemplate.yml");
        Assert.NotNull(argoTemplate);

        argoTemplate.Spec.PodGC = new PodGC { Strategy = "OnePodSeccess" };

        SetUpSimpleArgoWorkFlow(argoTemplate);

        var message = GenerateTaskDispatchEventWithValidArguments();

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.Equal(TaskExecutionStatus.Accepted, result.Status);
        Assert.Null(_submittedArgoTemplate?.Spec.PodGC);
    }
    [Fact]
    public async Task ArgoPlugin_CreateArgoTemplate_Invalid_json_Throws_JsonSerializationException()
    {
        var template = "\"name\":\"fred\"";

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, new Messaging.Events.TaskDispatchEvent());

        await Assert.ThrowsAsync<JsonSerializationException>(async () => await runner.CreateArgoTemplate(template).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext));
    }

    [Fact]
    public async Task ArgoPlugin_CreateArgoTemplate_Invalid_Object_Throws()
    {
        var template = "@";

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, new Messaging.Events.TaskDispatchEvent());

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await runner.CreateArgoTemplate(template).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext));
    }

    [Fact]
    public async Task ArgoPlugin_CreateArgoTemplate_Valid_json_Calls_Client()
    {
        ArgoClient.Setup(a =>
        a.Argo_CreateWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<WorkflowTemplateCreateRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new WorkflowTemplate()));

        var template = "{\"name\":\"fred\"}";

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, new Messaging.Events.TaskDispatchEvent());
        await runner.CreateArgoTemplate(template).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        ArgoClient.Verify(a =>
            a.Argo_CreateWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<WorkflowTemplateCreateRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "podPriorityClassName gets set if not given in workflow")]
    public async Task ArgoPlugin_Ensures_podPriorityClassName_is_set()
    {
        var argoTemplate = LoadArgoTemplate("SimpleTemplate.yml");
        Assert.NotNull(argoTemplate);

        SetUpSimpleArgoWorkFlow(argoTemplate);

        var message = GenerateTaskDispatchEventWithValidArguments();

        WorkflowCreateRequest? requestMade = default;
        ArgoClient.Setup(a => a.Argo_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()))
         .Callback<string, WorkflowCreateRequest, CancellationToken>((name, request, token) =>
             {
                 requestMade = request;
             });

        var defaultClassName = "standard";
        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.NotNull(requestMade);
        Assert.Equal(defaultClassName, requestMade.Workflow.Spec.PodPriorityClassName);

        foreach (var template in requestMade.Workflow.Spec.Templates)
        {
            Assert.Equal(defaultClassName, template.PriorityClassName);
        }
    }

    [Fact(DisplayName = "podPriorityClassName gets set if given in workflow")]
    public async Task ArgoPlugin_Ensures_podPriorityClassName_is_set_as_given()
    {
        var argoTemplate = LoadArgoTemplate("SimpleTemplate.yml");
        Assert.NotNull(argoTemplate);

        SetUpSimpleArgoWorkFlow(argoTemplate);

        var message = GenerateTaskDispatchEventWithValidArguments();

        var givenClassName = "fred";
        message.TaskPluginArguments.Add("priority", givenClassName);

        WorkflowCreateRequest? requestMade = default;
        ArgoClient.Setup(a => a.Argo_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()))
         .Callback<string, WorkflowCreateRequest, CancellationToken>((name, request, token) =>
         {
             requestMade = request;
         });

        var runner = new ArgoPlugin(ServiceScopeFactory.Object, _logger.Object, Options, message);
        var result = await runner.ExecuteTask(CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        Assert.NotNull(requestMade);
        Assert.Equal(givenClassName, requestMade.Workflow.Spec.PodPriorityClassName);

        foreach (var template in requestMade.Workflow.Spec.Templates)
        {
            Assert.Equal(givenClassName, template.PriorityClassName);
        }
    }

    private void SetUpSimpleArgoWorkFlow(WorkflowTemplate argoTemplate)
    {
        Assert.NotNull(argoTemplate);

        ArgoClient.Setup(p => p.Argo_GetWorkflowTemplateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(argoTemplate);

        ArgoClient.Setup(p => p.Argo_CreateWorkflowAsync(It.IsAny<string>(), It.IsAny<WorkflowCreateRequest>(), It.IsAny<CancellationToken>()))
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

    private static TaskDispatchEvent GenerateTaskDispatchEventWithValidArguments(bool withoutDefaultProperties = false)
    {
        var message = GenerateTaskDispatchEvent();
        message.TaskPluginArguments[ArgoParameters.WorkflowTemplateName] = "workflowTemplate";
        message.TaskPluginArguments[ArgoParameters.TimeoutSeconds] = "50";
        message.TaskPluginArguments[ArgoParameters.ArgoApiToken] = "token";

        if (withoutDefaultProperties is false)
        {
            message.TaskPluginArguments[ArgoParameters.BaseUrl] = "http://api-endpoint/";
            message.TaskPluginArguments[ArgoParameters.Namespace] = "namespace";
            message.TaskPluginArguments[ArgoParameters.AllowInsecureseUrl] = "true";
        }

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
            Name = taskDispatchEvent.TaskPluginArguments[ArgoParameters.WorkflowTemplateName],
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

    private ISetup<ICoreV1Operations, Task<HttpOperationResponse<V1Secret>>> SetupKubbernetesSecrets() => K8sCoreOperations.Setup(p => p.CreateNamespacedSecretWithHttpMessagesAsync(
                           It.IsAny<V1Secret>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<bool?>(),
                           It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
                           It.IsAny<CancellationToken>()));

    private void SetupKubernetesDeleteSecret() => K8sCoreOperations.Setup(p => p.DeleteNamespacedSecretWithHttpMessagesAsync(
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
#pragma warning restore CS8604 // Possible null reference argument.

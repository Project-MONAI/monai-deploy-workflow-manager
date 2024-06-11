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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Events;
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Logging;
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Models;
using Monai.Deploy.WorkflowManager.TaskManager.API;

namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview
{
    public class AideClinicalReviewPlugin : TaskPluginBase
    {
        private const string TaskManagerApplicationId = "4c9072a1-35f5-4d85-847d-dafca22244a8";
        private readonly IServiceScope _scope;
        private readonly ILogger<AideClinicalReviewPlugin> _logger;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IMessageBrokerPublisherService _messageBrokerPublisherService;

        private string? _patientId;
        private string? _patientName;
        private string? _patientSex;
        private string? _patientDob;
        private string? _patientAge;
        private string? _patientHospitalId;
        private string? _queueName = null;
        private string? _workflowName;
        private bool _notifications;
        private string? _reviewedTaskId;
        private string? _applicationName;
        private string? _mode;
        private string? _applicationVersion;
        private string? _reviewedExecutionId;
        private string[] _reviewerRoles = Array.Empty<string>();

        public AideClinicalReviewPlugin(
            IServiceScopeFactory serviceScopeFactory,
            IMessageBrokerPublisherService messageBrokerPublisherService,
            IOptions<WorkflowManagerOptions> options,
            ILogger<AideClinicalReviewPlugin> logger,
            TaskDispatchEvent taskDispatchEvent)
            : base(taskDispatchEvent)
        {
            ArgumentNullException.ThrowIfNull(serviceScopeFactory, nameof(serviceScopeFactory));

            _scope = serviceScopeFactory.CreateScope();

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _messageBrokerPublisherService = messageBrokerPublisherService ?? throw new ArgumentNullException(nameof(messageBrokerPublisherService));

            ValidateEventAndInit();
            Initialize();
        }

        private void Initialize()
        {
            InitializePatientDetails();

            if (Event.TaskPluginArguments.ContainsKey(Keys.QueueName))
            {
                _queueName = Event.TaskPluginArguments[Keys.QueueName];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.WorkflowName))
            {
                _workflowName = Event.TaskPluginArguments[Keys.WorkflowName];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.Notifications) && Boolean.TryParse(Event.TaskPluginArguments[Keys.Notifications], out var result))
            {
                _notifications = result;
            }
            else
            {
                _notifications = true;
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.ReviewedExecutionId))
            {
                _reviewedExecutionId = Event.TaskPluginArguments[Keys.ReviewedExecutionId];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.ReviewedTaskId))
            {
                _reviewedTaskId = Event.TaskPluginArguments[Keys.ReviewedTaskId];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.ApplicationName))
            {
                _applicationName = Event.TaskPluginArguments[Keys.ApplicationName];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.ApplicationVersion))
            {
                _applicationVersion = Event.TaskPluginArguments[Keys.ApplicationVersion];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.Mode))
            {
                _mode = Event.TaskPluginArguments[Keys.Mode];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.ReviewerRoles))
            {
                var reviewerRoles = Event.TaskPluginArguments[Keys.ReviewerRoles];
                var reviewerRolesSplit = Array.ConvertAll(reviewerRoles.Split(','), p => p.Trim());

                if (reviewerRolesSplit.Any() && reviewerRolesSplit.Any(r => string.IsNullOrWhiteSpace(r)) is false)
                {
                    _reviewerRoles = reviewerRolesSplit;

                    return;
                }
            }

            _reviewerRoles = new string[] { "clinician" };
        }

        private void InitializePatientDetails()
        {
            if (Event.TaskPluginArguments.ContainsKey(PatientKeys.PatientId))
            {
                _patientId = Event.TaskPluginArguments[PatientKeys.PatientId];
            }

            if (Event.TaskPluginArguments.ContainsKey(PatientKeys.PatientName))
            {
                _patientName = Event.TaskPluginArguments[PatientKeys.PatientName];
            }

            if (Event.TaskPluginArguments.ContainsKey(PatientKeys.PatientSex))
            {
                _patientSex = Event.TaskPluginArguments[PatientKeys.PatientSex];
            }

            if (Event.TaskPluginArguments.ContainsKey(PatientKeys.PatientDob))
            {
                _patientDob = Event.TaskPluginArguments[PatientKeys.PatientDob];
            }

            if (Event.TaskPluginArguments.ContainsKey(PatientKeys.PatientAge))
            {
                _patientAge = Event.TaskPluginArguments[PatientKeys.PatientAge];
            }

            if (Event.TaskPluginArguments.ContainsKey(PatientKeys.PatientHospitalId))
            {
                _patientHospitalId = Event.TaskPluginArguments[PatientKeys.PatientHospitalId];
            }
        }

        private void ValidateEventAndInit()
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
        }

        public override async Task<ExecutionStatus> ExecuteTask(CancellationToken cancellationToken = default)
        {
            using var loggingScope = _logger.BeginScope(new LoggingDataDictionary<string, object>
            {
                ["correlationId"] = Event.CorrelationId,
                ["workflowInstanceId"] = Event.WorkflowInstanceId,
                ["taskId"] = Event.TaskId,
                ["executionId"] = Event.ExecutionId
            });

            try
            {
                var reviewEvent = GenerateClinicalReviewRequestEventMessage();
                await SendClinicalReviewRequestEvent(reviewEvent).ConfigureAwait(false);

                return new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None, Stats = new Dictionary<string, string> { { Strings.IdentityKey, reviewEvent.Body.ExecutionId } } };
            }
            catch (Exception ex)
            {
                _logger.ErrorSendingMessage(_queueName ?? "no queue name", ex);

                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError, Errors = ex.Message };
            }
        }

        private JsonMessage<ClinicalReviewRequestEvent> GenerateClinicalReviewRequestEventMessage()
        {
            return new JsonMessage<ClinicalReviewRequestEvent>(new ClinicalReviewRequestEvent
            {
                CorrelationId = Event.CorrelationId,
                ExecutionId = Event.ExecutionId,
                WorkflowInstanceId = Event.WorkflowInstanceId,
                TaskId = Event.TaskId,
                ReviewedTaskId = _reviewedTaskId ?? string.Empty,
                ReviewedExecutionId = _reviewedExecutionId ?? string.Empty,
                WorkflowName = _workflowName ?? string.Empty,
                Notifications = _notifications,
                Files = Event.Inputs,
                ReviewerRoles = _reviewerRoles,
                PatientMetadata = new PatientMetadata
                {
                    PatientId = _patientId,
                    PatientSex = _patientSex,
                    PatientName = _patientName,
                    PatientDob = _patientDob,
                    PatientAge = _patientAge,
                    PatientHospitalId = _patientHospitalId
                },
                ApplicationMetadata = new Dictionary<string, string>
                {
                    { Keys.ApplicationName, _applicationName ?? string.Empty },
                    { Keys.ApplicationVersion, _applicationVersion ?? string.Empty},
                    { Keys.Mode, _mode ?? string.Empty },
                }
            }, TaskManagerApplicationId, Event.CorrelationId);
        }

        private async Task SendClinicalReviewRequestEvent(JsonMessage<ClinicalReviewRequestEvent> message)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            var queue = _queueName ?? _options.Value.Messaging.Topics.AideClinicalReviewRequest;
            _logger.SendClinicalReviewRequestMessage(queue, _workflowName ?? string.Empty);
            await _messageBrokerPublisherService.Publish(queue, message.ToMessage()).ConfigureAwait(false);
            _logger.SendClinicalReviewRequestMessageSent(queue);
        }

        public override async Task<ExecutionStatus> GetStatus(string identity, TaskCallbackEvent callbackEvent, CancellationToken cancellationToken = default)
        {
            var executionStatus = TaskExecutionStatus.Succeeded;

            // metadata properties
            var userId = string.Empty;
            var message = "N/A";
            var reason = "N/A";

            if (callbackEvent.Metadata.TryGetValue(Keys.MetadataAcceptance, out var acceptance))
            {
                executionStatus = (bool)acceptance ?
                        TaskExecutionStatus.Succeeded :
                        TaskExecutionStatus.PartialFail;
            }

            if (callbackEvent.Metadata.TryGetValue(Keys.MetadataMessage, out var metadataMessage))
            {
                message = (string)metadataMessage;
            }

            if (callbackEvent.Metadata.TryGetValue(Keys.MetadataUserId, out var metadataUserId))
            {
                userId = (string)metadataUserId;
            }

            if (callbackEvent.Metadata.TryGetValue(Keys.MetadataReason, out var metadataReason))
            {
                reason = (string)metadataReason;
            }

            _logger.RecordTaskDecision(
                Event.TaskId,
                executionStatus == TaskExecutionStatus.Succeeded ? "Accepted" : "Rejected",
                DateTime.UtcNow.ToLongDateString(),
                userId,
                _applicationName ?? string.Empty,
                reason,
                message);

            return await Task.Run(() => new ExecutionStatus
            {
                Status = executionStatus,
                FailureReason = FailureReason.None,
            });
        }

        ~AideClinicalReviewPlugin() => Dispose(disposing: false);

        protected override void Dispose(bool disposing)
        {
            if (!DisposedValue && disposing)
            {
                _scope.Dispose();
            }

            base.Dispose(disposing);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async ValueTask DisposeAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        public override Task HandleTimeout(string identity)
        {
            var message = GenerateCancelationMessage(identity);

            var queue = _queueName ?? _options.Value.Messaging.Topics.AideClinicalReviewCancelation;
            _logger.SendClinicalReviewRequestMessage(queue, _workflowName ?? string.Empty);
            return _messageBrokerPublisherService.Publish(queue, message.ToMessage());
        }

        private JsonMessage<TaskCancellationEvent> GenerateCancelationMessage(string identity)
        {
            return new JsonMessage<TaskCancellationEvent>(new TaskCancellationEvent
            {
                ExecutionId = identity,
                WorkflowInstanceId = Event.WorkflowInstanceId,
                TaskId = Event.TaskId,
                Reason = FailureReason.TimedOut,
                Identity = identity,
                Message = $"{FailureReason.TimedOut} {DateTime.UtcNow}"
            }, TaskManagerApplicationId, Event.CorrelationId);
        }
    }
}

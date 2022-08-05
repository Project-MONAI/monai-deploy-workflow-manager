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

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Events;
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Logging;
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Models;
using Monai.Deploy.WorkflowManager.TaskManager.API;

namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview
{
    public class AideClinicalReviewPlugin : TaskPluginBase, IAsyncDisposable
    {
        private const string TaskManagerApplicationId = "4c9072a1-35f5-4d85-847d-dafca22244a8";
        private readonly IServiceScope _scope;
        private readonly ILogger<AideClinicalReviewPlugin> _logger;
        private readonly IMessageBrokerPublisherService _messageBrokerPublisherService;

        private string _patientId;
        private string _patientName;
        private string _patientSex;
        private string _patientDob;
        private string _patientAge;
        private string _patientHospitalId;
        private string _queueName;
        private string _workflowName;
        private string _reviewedTaskId;
        private string _reviewedExecutionId;

        public AideClinicalReviewPlugin(
            IServiceScopeFactory serviceScopeFactory,
            IMessageBrokerPublisherService messageBrokerPublisherService,
            ILogger<AideClinicalReviewPlugin> logger,
            TaskDispatchEvent taskDispatchEvent)
            : base(taskDispatchEvent)
        {
            Guard.Against.Null(serviceScopeFactory, nameof(serviceScopeFactory));

            _scope = serviceScopeFactory.CreateScope();

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageBrokerPublisherService = messageBrokerPublisherService ?? throw new ArgumentNullException(nameof(messageBrokerPublisherService));

            ValidateEventAndInit();
            Initialize();
        }

        private void Initialize()
        {
            if (Event.TaskPluginArguments.ContainsKey(Keys.PatientId))
            {
                _patientId = Event.TaskPluginArguments[Keys.PatientId];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.PatientName))
            {
                _patientName = Event.TaskPluginArguments[Keys.PatientName];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.PatientSex))
            {
                _patientSex = Event.TaskPluginArguments[Keys.PatientSex];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.PatientDob))
            {
                _patientDob = Event.TaskPluginArguments[Keys.PatientDob];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.PatientAge))
            {
                _patientAge = Event.TaskPluginArguments[Keys.PatientAge];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.PatientHospitalId))
            {
                _patientHospitalId = Event.TaskPluginArguments[Keys.PatientHospitalId];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.QueueName))
            {
                _queueName = Event.TaskPluginArguments[Keys.QueueName];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.WorkflowName))
            {
                _workflowName = Event.TaskPluginArguments[Keys.WorkflowName];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.ReviewedExecutionId))
            {
                _reviewedExecutionId = Event.TaskPluginArguments[Keys.ReviewedExecutionId];
            }

            if (Event.TaskPluginArguments.ContainsKey(Keys.ReviewedTaskId))
            {
                _reviewedTaskId = Event.TaskPluginArguments[Keys.ReviewedTaskId];
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
            try
            {
                var reviewEvent = GenerateClinicalReviewRequestEventMessage();
                await SendClinicalReviewRequestEvent(reviewEvent).ConfigureAwait(false);

                return new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None };
            }
            catch (Exception ex)
            {
                _logger.ErrorSendingMessage(_queueName, ex);

                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError, Errors = ex.Message };
            }
        }

        private JsonMessage<ClinicalReviewRequestEvent> GenerateClinicalReviewRequestEventMessage()
        {
            return new JsonMessage<ClinicalReviewRequestEvent>(new ClinicalReviewRequestEvent
            {
                CorrelationId = Event.CorrelationId,
                ExecutionId = Event.ExecutionId,
                TaskId = Event.TaskId,
                ReviewedTaskId = _reviewedTaskId,
                ReviewedExecutionId = _reviewedExecutionId,
                WorkflowName = _workflowName,
                Files = Event.Inputs,
                PatientMetadata = new PatientMetadata
                {
                    PatientId = _patientId,
                    PatientSex = _patientSex,
                    PatientName = _patientName,
                    PatientDob = _patientDob,
                    PatientAge = _patientAge,
                    PatientHospitalId = _patientHospitalId
                }
            }, TaskManagerApplicationId, Event.CorrelationId);
        }

        private async Task SendClinicalReviewRequestEvent(JsonMessage<ClinicalReviewRequestEvent> message)
        {
            Guard.Against.Null(message, nameof(message));

            _logger.SendClinicalReviewRequestMessage(_queueName, _workflowName);
            await _messageBrokerPublisherService.Publish(_queueName, message.ToMessage()).ConfigureAwait(false);
            _logger.SendClinicalReviewRequestMessageSent(_queueName);
        }

        public override async Task<ExecutionStatus> GetStatus(string identity, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => new ExecutionStatus { Status = TaskExecutionStatus.Succeeded });
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

        public override Task HandleTimeout(string identity) { return Task.CompletedTask; } // not implemented
    }
}

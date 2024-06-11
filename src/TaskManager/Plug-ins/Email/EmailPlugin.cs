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


using System.Net.Mail;
using FellowOakDicom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.API;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Microsoft.Extensions.DependencyInjection;

namespace Monai.Deploy.WorkflowManager.TaskManager.Email
{
    public class EmailPlugin : TaskPluginBase
    {
        private const string TaskManagerApplicationId = "4c9072a1-35f5-4d85-847d-dafca22244a8";
        private readonly ILogger<EmailPlugin> _logger;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IMessageBrokerPublisherService _messageBrokerPublisherService;
        private readonly IStorageService _storageService;
        private readonly string _requestQueue;
        private readonly string _cancelationQueue;
        private readonly IServiceScope _scope;

        private string[]? _recipientRoles;
        private string[]? _recipientAddresses;
        private string[]? _includeMetadata;

        public EmailPlugin(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<EmailPlugin> logger,
            IOptions<WorkflowManagerOptions> options,
            TaskDispatchEvent taskDispatchEvent)
            : base(taskDispatchEvent)
        {

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            ArgumentNullException.ThrowIfNull(serviceScopeFactory, nameof(serviceScopeFactory));
            _scope = serviceScopeFactory.CreateScope();

            _messageBrokerPublisherService = _scope.ServiceProvider.GetService<IMessageBrokerPublisherService>() ?? throw new ServiceNotFoundException(nameof(IMessageBrokerPublisherService));
            _storageService = _scope.ServiceProvider.GetService<IStorageService>() ?? throw new ServiceNotFoundException(nameof(IStorageService)); ;

            _requestQueue = _options.Value.Messaging.Topics.NotificationEmailRequest;
            _cancelationQueue = _options.Value.Messaging.Topics.NotificationEmailCancelation;

            ValidateEventAndInit();
        }

        private void ValidateEventAndInit()
        {
            if (Event.TaskPluginArguments is null || Event.TaskPluginArguments.Count == 0)
            {
                throw new InvalidTaskException($"Required parameters to execute Argo workflow are missing: {string.Join(',', ValidationConstants.RecipientRoles, ValidationConstants.RecipientEmails)}");
            }

            if (Event.TaskPluginArguments.ContainsKey(ValidationConstants.RecipientRoles))
            {
                var recipientRoles = Event.TaskPluginArguments[ValidationConstants.RecipientRoles];
                var recipientRolesSplit = recipientRoles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (recipientRolesSplit.Length != 0)
                {
                    _recipientRoles = recipientRolesSplit;
                }
            }

            if (Event.TaskPluginArguments.ContainsKey(ValidationConstants.RecipientEmails))
            {
                var recipients = Event.TaskPluginArguments[ValidationConstants.RecipientEmails];
                var recipientsSplit = recipients.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (recipientsSplit.Length != 0)
                {
                    _recipientAddresses = recipientsSplit;
                }
                if (recipientsSplit.Any(r => ValidateEmailAddress(r) is false))
                {
                    throw new InvalidTaskException($"Invalid email address parameter given");
                }
            }

            if (_recipientRoles is null && _recipientAddresses is null)
            {
                throw new InvalidTaskException($"Required parameter to execute Argo workflow is missing: {ValidationConstants.RecipientEmails} or {ValidationConstants.RecipientRoles} must be specified");
            }

            if (Event.TaskPluginArguments.ContainsKey(ValidationConstants.MetadataValues))
            {
                _includeMetadata = Event.TaskPluginArguments[ValidationConstants.MetadataValues].Split(new char[] { ',' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
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
                var metadata = new Dictionary<string, List<string>>();
                if (Event.Inputs.Any())
                {
                    foreach (var input in Event.Inputs)
                    {
                        metadata = await AddRawMetaFromFile(metadata, $"{input.RelativeRootPath}", input.Bucket);
                    }
                }

                var emailRequest = GenerateEmailRequestEventMessage(FlattenMeta(metadata));
                await SendEmailRequestEvent(emailRequest).ConfigureAwait(false);

                return new ExecutionStatus { Status = TaskExecutionStatus.Accepted, FailureReason = FailureReason.None, Stats = new Dictionary<string, string> { { "IdentityKey", emailRequest.Body.Id.ToString() } } };
            }
            catch (Exception ex)
            {
                _logger.ErrorSendingMessage(_requestQueue, ex);

                return new ExecutionStatus { Status = TaskExecutionStatus.Failed, FailureReason = FailureReason.PluginError, Errors = ex.Message };
            }
        }

        private static Dictionary<string, string> FlattenMeta(Dictionary<string, List<string>> input)
        {
            var values = new Dictionary<string, string>();
            foreach (var key in input.Keys)
            {
                values.Add(key, string.Join(",", input[key].Distinct()));
            }
            return values;
        }

        private async Task<Dictionary<string, List<string>>> AddRawMetaFromFile(Dictionary<string, List<string>> metadata, string path, string bucketName)
        {
            if (_includeMetadata is null || _includeMetadata.Count() == 0)
            {
                _logger.NoMetaDataRequested();
                return metadata;
            }
            List<VirtualFileInfo> allFiles;
            try
            {
                allFiles = (List<VirtualFileInfo>)await _storageService.ListObjectsAsync(bucketName, path, true);
            }
            catch (Exception ex)
            {
                var mess = ex.Message;
                throw;
            }

            foreach (var file in allFiles)
            {
                if (file.FilePath.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase)) continue;
                ArgumentNullException.ThrowIfNullOrWhiteSpace(bucketName, nameof(bucketName));
                ArgumentNullException.ThrowIfNullOrWhiteSpace(path, nameof(path));

                // load file from Minio !
                var fileStream = await _storageService.GetObjectAsync(bucketName, $"{file.FilePath}");
                try
                {
                    var dcmFile = DicomFile.Open(fileStream);

                    foreach (var item in _includeMetadata)
                    {
                        DicomTag? tag = null;
                        try
                        {
                            tag = DicomDictionary.Default[item];
                        }
                        catch (Exception)
                        {
                            try
                            {
                                tag = DicomTag.Parse(item);
                            }
                            catch (Exception)
                            {
                                //empty on purpose
                            }
                        }
                        if (tag is not null)
                        {
                            var value = dcmFile.Dataset.GetString(tag).Trim();
                            if (metadata.ContainsKey(item))
                            {
                                metadata[item].Add(value);
                            }
                            else
                            {
                                metadata.Add(item, new List<string> { value });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.ErrorGettingMetaData($"{file.FilePath}/{file.Filename}", ex.Message);
                }
            }

            return metadata;
        }

        private JsonMessage<EmailRequestEvent> GenerateEmailRequestEventMessage(Dictionary<string, string>? metadata)
        {

            return new JsonMessage<EmailRequestEvent>(new Monai.Deploy.Messaging.Events.EmailRequestEvent
            {
                WorkflowName = Event.TaskPluginArguments.ContainsKey(ValidationConstants.WorkflowName) ? Event.TaskPluginArguments[ValidationConstants.WorkflowName] : string.Empty,
                WorkflowInstanceId = Event.WorkflowInstanceId,
                TaskId = Event.TaskId,
                Roles = _recipientRoles is not null ? string.Join(",", _recipientRoles) : string.Empty,
                Emails = _recipientAddresses is not null ? string.Join(",", _recipientAddresses) : string.Empty,
                Metadata = metadata ?? new()

            }, TaskManagerApplicationId, Event.CorrelationId);
        }

        private bool ValidateEmailAddress(string email)
        {
            return MailAddress.TryCreate(email, out _);
        }

        private async Task SendEmailRequestEvent(JsonMessage<EmailRequestEvent> message)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            _logger.SendEmailRequestMessage(_requestQueue);
            await _messageBrokerPublisherService.Publish(_requestQueue, message.ToMessage()).ConfigureAwait(false);
            _logger.SendEmailRequestMessageSent(_requestQueue);
        }

        public override Task<ExecutionStatus> GetStatus(string identity, TaskCallbackEvent callbackEvent, CancellationToken cancellationToken = default)
        {
            var result = new ExecutionStatus() { Status = TaskExecutionStatus.Succeeded };
            return Task.FromResult(result);
        }
        public override Task HandleTimeout(string identity) => Task.CompletedTask;
    }
}

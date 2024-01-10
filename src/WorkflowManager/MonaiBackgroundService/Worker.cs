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

using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Logging;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Common;
using Monai.Deploy.WorkflowManager.MonaiBackgroundService.Logging;
using Monai.Deploy.WorkflowManager.Common.Database.Interfaces;
using Monai.Deploy.Storage.API;

namespace Monai.Deploy.WorkflowManager.Common.MonaiBackgroundService
{
    public class Worker : BackgroundService
    {
        private const string JobIdentity = "JobIdentity";
        private readonly ILogger<Worker> _logger;
        private readonly ITasksService _tasksService;
        private readonly IMessageBrokerPublisherService _publisherService;
        private readonly IOptions<WorkflowManagerOptions> _options;
        private readonly IPayloadRepository _payloadRepository;
        private readonly IStorageService _storageService;
        public bool IsRunning { get; set; } = false;

        public Worker(
            ILogger<Worker> logger,
            ITasksService tasksService,
            IMessageBrokerPublisherService publisherService,
            IPayloadRepository payloadRepository,
            IStorageService storageService,
            IOptions<WorkflowManagerOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tasksService = tasksService ?? throw new ArgumentNullException(nameof(tasksService));
            _publisherService = publisherService ?? throw new ArgumentNullException(nameof(publisherService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _payloadRepository = payloadRepository ?? throw new ArgumentNullException(nameof(payloadRepository));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        public static string ServiceName => "Monai Background Service";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                IsRunning = true;
                var time = DateTime.UtcNow;
                _logger.ServiceStarted(ServiceName);

                await DoWork();

                _logger.ServiceCompleted(ServiceName, (int)(DateTime.UtcNow - time).TotalMilliseconds);
                await Task.Delay(_options.Value.BackgroundServiceSettings.BackgroundServiceDelay, stoppingToken);
            }

            _logger.ServiceStopping(ServiceName);
            IsRunning = false;
        }

        public async Task DoWork()
        {
            await ProcessTimedoutTasks().ConfigureAwait(false);
            await ProcessExpiredPayloads().ConfigureAwait(false);
        }

        private async Task ProcessTimedoutTasks()
        {
            try
            {
                var (tasks, _) = await _tasksService.GetAllAsync();
                foreach (var task in tasks.Where(t => t.TimeoutInterval != 0 && t.Timeout < DateTime.UtcNow))
                {
                    task.ResultMetadata.TryGetValue(JobIdentity, out var identity);

                    var correlationId = Guid.NewGuid().ToString();

                    await PublishTimeoutUpdateEvent(task, correlationId, task.WorkflowInstanceId).ConfigureAwait(false); // -> task manager

                    await PublishCancellationEvent(task, correlationId, identity as string ?? task.ExecutionId, task.WorkflowInstanceId).ConfigureAwait(false); // -> workflow executor
                }
            }
            catch (Exception e)
            {
                _logger.WorkerException(e.Message);
            }
        }

        private async Task ProcessExpiredPayloads()
        {
            var payloads = new List<Payload>();
            try
            {
                payloads = (await _payloadRepository.GetPayloadsToDelete(DateTime.UtcNow).ConfigureAwait(false)).ToList();

                if (payloads.Count != 0)
                {
                    var ids = payloads.Select(p => p.PayloadId).ToList();

                    await _payloadRepository.MarkDeletedState(ids, PayloadDeleted.InProgress).ConfigureAwait(false);
                }

            }
            catch (Exception e)
            {
                _logger.WorkerException(e.Message);
            }

            try
            {
                await RemoveStoredFiles(payloads.ToList());
            }
            catch (Exception e)
            {

                _logger.WorkerException(e.Message);
            }
        }

        private async Task RemoveStoredFiles(List<Payload> payloads)
        {
            var tasks = new List<Task>();

            foreach (var payload in payloads)
            {
                var filepaths = (payload.Files.Select(f => f.Path)).ToList();

                var all = await _storageService.ListObjectsAsync(payload.Bucket, payload.PayloadId, true);

                filepaths.AddRange(all.Select(f => f.FilePath));

                foreach (var filepath in filepaths)
                {
                    await _storageService.RemoveObjectAsync(payload.Bucket, filepath);
                }

                tasks.Add(_payloadRepository.MarkDeletedState(new List<string> { payload.PayloadId }, PayloadDeleted.Yes));
            }
            await Task.WhenAll(tasks);
        }

        private async Task PublishCancellationEvent(TaskExecution task, string correlationId, string identity, string workflowInstanceId)
        {
            _logger.TimingOutTaskCancellationEvent(identity, task.WorkflowInstanceId);

            var cancellationEvent = EventMapper.GenerateTaskCancellationEvent(
                identity,
                task.ExecutionId,
                workflowInstanceId,
                task.TaskId,
                FailureReason.TimedOut,
                $"{DateTime.UtcNow}");

            cancellationEvent.Validate();

            var message = EventMapper.ToJsonMessage(cancellationEvent, ServiceName, correlationId);

            await _publisherService!.Publish(_options.Value.Messaging.Topics.TaskCancellationRequest, message.ToMessage()).ConfigureAwait(false);
        }

        private async Task PublishTimeoutUpdateEvent(TaskExecution task, string correlationId, string workflowInstanceId)
        {
            var timeoutString = $"{task.TaskStartTime.ToShortDateString()} {task.TaskStartTime.ToShortTimeString()}";
            var duration = DateTime.UtcNow - task.TaskStartTime;
            var durationString = $"{duration.TotalSeconds} Seconds";

            _logger.TimingOutTask(task.TaskId, timeoutString, durationString, task.ExecutionId, correlationId);

            var updateEvent = EventMapper.GenerateTaskUpdateEvent(new GenerateTaskUpdateEventParams
            {
                CorrelationId = correlationId,
                ExecutionId = task.ExecutionId,
                WorkflowInstanceId = workflowInstanceId,
                TaskId = task.TaskId,
                TaskExecutionStatus = TaskExecutionStatus.Failed,
                FailureReason = FailureReason.TimedOut,
                Stats = task.ExecutionStats
            });

            updateEvent.Validate();

            var message = EventMapper.ToJsonMessage(updateEvent, ServiceName, updateEvent.CorrelationId);

            await _publisherService!.Publish(_options.Value.Messaging.Topics.TaskUpdateRequest, message.ToMessage()).ConfigureAwait(false); // to task manager
        }
    }
}

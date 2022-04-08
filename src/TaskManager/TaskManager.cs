// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common;
using Monai.Deploy.WorkflowManager.Contracts.Rest;
using Monai.Deploy.WorkflowManager.TaskManager.Logging;

namespace TaskManager
{
    public class TaskManager : IHostedService, IDisposable, IMonaiService
    {
        private bool _disposedValue;
        private readonly ILogger<TaskManager> _logger;

        public ServiceStatus Status { get; set; } = ServiceStatus.Unknown;

        public string ServiceName => "MONAI Deploy Task Manager";

        public TaskManager(ILogger<TaskManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var task = Task.Run(() =>
            {
                BackgroundProcessing(cancellationToken);
            }, CancellationToken.None);

            Status = ServiceStatus.Running;
            _logger.ServiceStarted(ServiceName);

            if (task.IsCompleted)
                return task;
            return Task.CompletedTask;

        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.ServiceStopping(ServiceName);
            Status = ServiceStatus.Stopped;

            return Task.CompletedTask;
        }

        private void BackgroundProcessing(CancellationToken cancellationToken)
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

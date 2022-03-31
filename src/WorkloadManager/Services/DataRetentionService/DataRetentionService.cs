// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkloadManager.Contracts.Rest;
using Monai.Deploy.WorkloadManager.Logging;
using Monai.Deploy.WorkloadManager.Services.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Monai.Deploy.WorkloadManager.Services.DataRetentionService
{
    internal class DataRetentionService : IHostedService, IDisposable, IMonaiService
    {
        private readonly ILogger<DataRetentionService> _logger;
        private bool _disposedValue;

        public ServiceStatus Status { get; set; } = ServiceStatus.Unknown;

        public string ServiceName => "Data Retention Service";

        public DataRetentionService(ILogger<DataRetentionService> logger)
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
            _logger.ServiceStarting(ServiceName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Status = ServiceStatus.Disposed;

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

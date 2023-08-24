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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Logging;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;

namespace Monai.Deploy.WorkflowManager.Common.Services.DataRetentionService
{
    public class DataRetentionService : IHostedService, IDisposable, IMonaiService
    {
        private readonly ILogger<DataRetentionService> _logger;

        private bool _disposedValue;

        public DataRetentionService(ILogger<DataRetentionService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ServiceStatus Status { get; set; } = ServiceStatus.Unknown;

        public string ServiceName => "Data Retention Service";

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var task = Task.Run(() => BackgroundProcessing(cancellationToken), CancellationToken.None);

            Status = ServiceStatus.Running;
            if (task.IsCompleted)
            {
                return task;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.ServiceStopping(ServiceName);
            Status = ServiceStatus.Stopped;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // Do not change
            // code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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

        private void BackgroundProcessing(CancellationToken cancellationToken)
        {
            _logger.ServiceStarting(ServiceName);
        }
    }
}

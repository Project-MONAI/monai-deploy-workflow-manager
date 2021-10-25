// Copyright 2021 MONAI Consortium
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkloadManager.Common;
using Monai.Deploy.WorkloadManager.Contracts.Rest;

namespace Monai.Deploy.WorkloadManager.Core.Services.JobSchedulingService
{
    internal class JobSchedulingService : IHostedService, IDisposable, IMonaiService
    {
        private readonly ILogger<JobSchedulingService> _logger;
        public ServiceStatus Status { get; set; } = ServiceStatus.Unknown;

        public string ServiceName => "Job Scheduling Service";

        public JobSchedulingService(ILogger<JobSchedulingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Dispose()
        {
            Status = ServiceStatus.Disposed;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var task = Task.Run(() =>
            {
                BackgroundProcessing(cancellationToken);
            }, cancellationToken);

            Status = ServiceStatus.Running;
            if (task.IsCompleted)
                return task;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{ServiceName} Service is stopping.");
            Status = ServiceStatus.Stopped;
            return Task.CompletedTask;
        }

        private void BackgroundProcessing(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{ServiceName} Service is starting.");
        }
    }
}

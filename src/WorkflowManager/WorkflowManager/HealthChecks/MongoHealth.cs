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
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Database.Options;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.HealthChecks
{
    /// <summary>
    /// MongoDb Health Checks.
    /// </summary>
    public class MongoHealth : IHealthCheck
    {
        private readonly IMongoClient _client;
        private readonly IOptions<WorkloadManagerDatabaseSettings> _databaseSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoHealth"/> class.
        /// </summary>
        /// <param name="client"></param>
        public MongoHealth(IMongoClient client, IOptions<WorkloadManagerDatabaseSettings> databaseSettings)
        {
            _client = client;
            _databaseSettings = databaseSettings;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            _client.GetDatabase(_databaseSettings.Value.DatabaseName);
            return new HealthCheckResult(HealthStatus.Unhealthy, "desc", new Exception("boom"));
        }
    }
}

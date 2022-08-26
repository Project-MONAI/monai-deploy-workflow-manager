/*
 * Copyright 2021-2022 MONAI Consortium
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

namespace Monai.Deploy.WorkflowManager.HealthChecks
{
    /// <summary>
    /// Settings related to health check endpoints.
    /// </summary>
    public static class HealthCheckSettings
    {
        /// <summary>
        /// Database name in health checks endpoints.
        /// </summary>
        public static readonly string DatabaseHealthCheckName = "Database";

        /// <summary>
        /// Subscriber queue name in health checks endpoints.
        /// </summary>
        public static readonly string SubscriberQueueHealthCheckName = "Subscriber-Queue";

        /// <summary>
        /// Publisher queue name in health checks endpoints.
        /// </summary>
        public static readonly string PublisherQueueHealthCheckName = "Publisher-Queue";

        /// <summary>
        /// Database timeout health checks endpoints.
        /// </summary>
        public static readonly TimeSpan DatabaseHealthCheckTimeout = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Subscriber queue timeout for health checks endpoints.
        /// </summary>
        public static readonly TimeSpan SubscriberQueueHealthCheckTimeout = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Publisher queue timeout for health checks endpoints.
        /// </summary>
        public static readonly TimeSpan PublisherQueueHealthCheckTimeout = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Gets database Tags that show on health check endpoints.
        /// </summary>
        public static IEnumerable<string> DatabaseTags { get; } = new[] { "services", "database", "mongodb" };

        /// <summary>
        /// Gets SubscriberQueue Tags that show on health check endpoints.
        /// </summary>
        public static IEnumerable<string> SubscriberQueueTags { get; } = new[] { "services", "queue", "subscriber", "rabbitmq" };

        /// <summary>
        /// Gets PublisherQueue Tags that show on health check endpoints.
        /// </summary>
        public static IEnumerable<string> PublisherQueueTags { get; } = new[] { "services", "queue", "publisher", "rabbitmq" };
    }
}

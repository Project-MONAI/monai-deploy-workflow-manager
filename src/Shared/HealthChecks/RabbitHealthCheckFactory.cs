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

using System.Net.Security;
using RabbitMQ.Client;

namespace Monai.Deploy.WorkflowManager.HealthChecks
{
    /// <summary>
    /// Rabbit Factory used to check health status of RabbitMq queue.
    /// </summary>
    public static class RabbitHealthCheckFactory
    {
        /// <summary>
        /// Creates connection factory for RabbitMq used in the health check.
        /// </summary>
        /// <param name="settings">Rabbit Settings.</param>
        /// <returns>IConnectionFactory.</returns>
        public static IConnectionFactory Create(Dictionary<string, string> settings)
        {
            var keys = new
            {
                endpoint = "endpoint",
                username = "username",
                password = "password",
                virtualHost = "virtualHost",
                exchange = "exchange",
                deadLetterExchange = "deadLetterExchange",
                deliveryLimit = "deliveryLimit",
                requeueDelay = "requeueDelay",
                useSSL = "useSSL",
                port = "port",
            };

            var portNumber = settings[keys.port];

            settings.TryGetValue(keys.useSSL, out var useSsl);
            if (!bool.TryParse(useSsl, out var sslEnabled))
            {
                sslEnabled = false;
            }

            if (!int.TryParse(portNumber, out var port))
            {
                port = sslEnabled ? 5671 : 5672; // 5671 is default port for SSL/TLS , 5672 is default port for PLAIN.
            }

            var sslOptions = new SslOption
            {
                Enabled = sslEnabled,
                ServerName = settings[keys.endpoint],
                AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNotAvailable,
            };

            return new ConnectionFactory()
            {
                HostName = settings[keys.endpoint],
                UserName = settings[keys.username],
                Password = settings[keys.password],
                Port = port,
                VirtualHost = settings[keys.virtualHost],
                Ssl = sslOptions,
            };
        }
    }
}

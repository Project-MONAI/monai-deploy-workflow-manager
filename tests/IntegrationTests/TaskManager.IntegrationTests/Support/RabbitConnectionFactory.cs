// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using RabbitMQ.Client;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests
{
    public static class RabbitConnectionFactory
    {
        public static ConnectionFactory GetConnectionFactory()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = TestExecutionConfig.RabbitConfig.Host,
                UserName = TestExecutionConfig.RabbitConfig.User,
                Password = TestExecutionConfig.RabbitConfig.Password,
                VirtualHost = TestExecutionConfig.RabbitConfig.VirtualHost
            };

            return connectionFactory;
        }
    }
}

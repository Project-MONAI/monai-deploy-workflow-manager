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

using Monai.Deploy.WorkflowManager.Common.IntegrationTests.POCO;
using RabbitMQ.Client;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support
{
    public static class RabbitConnectionFactory
    {
        private static IModel? Channel { get; set; }

        public static IModel GetRabbitConnection()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = TestExecutionConfig.RabbitConfig.Host,
                UserName = TestExecutionConfig.RabbitConfig.User,
                Password = TestExecutionConfig.RabbitConfig.Password,
                VirtualHost = TestExecutionConfig.RabbitConfig.VirtualHost,
                Port = TestExecutionConfig.RabbitConfig.Port
            };

            Channel = connectionFactory.CreateConnection().CreateModel();

            return Channel;
        }

        public static void DeleteQueue(string queueName)
        {
            if (Channel is null)
            {
                GetRabbitConnection();
            }

            Channel?.QueueDelete(queueName);
        }

        public static void PurgeQueue(string queueName)
        {
            if (Channel is null)
            {
                GetRabbitConnection();
            }

            Channel?.QueuePurge(queueName);
        }

        public static void DeleteAllQueues()
        {
            DeleteQueue(TestExecutionConfig.RabbitConfig.WorkflowRequestQueue);
            DeleteQueue(TestExecutionConfig.RabbitConfig.TaskDispatchQueue);
            DeleteQueue(TestExecutionConfig.RabbitConfig.TaskCallbackQueue);
            DeleteQueue(TestExecutionConfig.RabbitConfig.TaskUpdateQueue);
            DeleteQueue(TestExecutionConfig.RabbitConfig.ExportCompleteQueue);
            DeleteQueue(TestExecutionConfig.RabbitConfig.ExportRequestQueue);
            DeleteQueue(TestExecutionConfig.RabbitConfig.ArtifactsRequestQueue);
            DeleteQueue($"{TestExecutionConfig.RabbitConfig.WorkflowRequestQueue}-dead-letter");
            DeleteQueue($"{TestExecutionConfig.RabbitConfig.TaskDispatchQueue}-dead-letter");
            DeleteQueue($"{TestExecutionConfig.RabbitConfig.TaskCallbackQueue}-dead-letter");
            DeleteQueue($"{TestExecutionConfig.RabbitConfig.TaskUpdateQueue}-dead-letter");
            DeleteQueue($"{TestExecutionConfig.RabbitConfig.ExportCompleteQueue}-dead-letter");
            DeleteQueue($"{TestExecutionConfig.RabbitConfig.ExportRequestQueue}-dead-letter");
            DeleteQueue($"{TestExecutionConfig.RabbitConfig.ArtifactsRequestQueue}-dead-letter");
            DeleteQueue("-dead-letter");
        }

        public static void PurgeAllQueues()
        {
            PurgeQueue(TestExecutionConfig.RabbitConfig.WorkflowRequestQueue);
            PurgeQueue(TestExecutionConfig.RabbitConfig.TaskDispatchQueue);
            PurgeQueue(TestExecutionConfig.RabbitConfig.TaskUpdateQueue);
            PurgeQueue(TestExecutionConfig.RabbitConfig.ExportCompleteQueue);
            PurgeQueue(TestExecutionConfig.RabbitConfig.ExportRequestQueue);
            PurgeQueue($"{TestExecutionConfig.RabbitConfig.WorkflowRequestQueue}-dead-letter");
            PurgeQueue($"{TestExecutionConfig.RabbitConfig.TaskUpdateQueue}-dead-letter");
            PurgeQueue($"{TestExecutionConfig.RabbitConfig.ExportCompleteQueue}-dead-letter");
        }
    }
}

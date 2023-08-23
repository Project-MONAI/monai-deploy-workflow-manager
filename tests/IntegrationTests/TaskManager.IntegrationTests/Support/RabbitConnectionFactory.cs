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

using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.POCO;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support
{
    public static class RabbitConnectionFactory
    {
        public static IConnection? Connection { get; set; }

        public static IList<string> QueueNames = new List<string>()
        {
            $"{TestExecutionConfig.RabbitConfig.TaskDispatchQueue}",
            $"{TestExecutionConfig.RabbitConfig.TaskUpdateQueue}",
            $"{TestExecutionConfig.RabbitConfig.TaskCallbackQueue}",
            $"{TestExecutionConfig.RabbitConfig.ClinicalReviewQueue}",
            $"{TestExecutionConfig.RabbitConfig.EmailQueue}",
            $"{TestExecutionConfig.RabbitConfig.TaskCancellationQueue}",
            $"{TestExecutionConfig.RabbitConfig.TaskDispatchQueue}-dead-letter",
            $"{TestExecutionConfig.RabbitConfig.TaskUpdateQueue}-dead-letter",
            $"{TestExecutionConfig.RabbitConfig.TaskCallbackQueue}-dead-letter",
            $"{TestExecutionConfig.RabbitConfig.ClinicalReviewQueue}-dead-letter",
            $"{TestExecutionConfig.RabbitConfig.EmailQueue}-dead-letter",
            $"{TestExecutionConfig.RabbitConfig.TaskCancellationQueue}-dead-letter"
        };

        public static void SetRabbitConnection()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = TestExecutionConfig.RabbitConfig.Host,
                UserName = TestExecutionConfig.RabbitConfig.User,
                Password = TestExecutionConfig.RabbitConfig.Password,
                VirtualHost = TestExecutionConfig.RabbitConfig.VirtualHost,
                Port = TestExecutionConfig.RabbitConfig.Port,
            };

            Connection = connectionFactory.CreateConnection();
        }

        public static void DeleteQueue(string queueName)
        {
            using (var channel = Connection?.CreateModel())
            {
                channel?.QueueDelete(queueName);
            }
        }

        public static void PurgeQueue(string queueName)
        {
            using (var channel = Connection?.CreateModel())
            {
                channel?.QueuePurge(queueName);
            }
        }

        public static void DeleteAllQueues()
        {
            foreach (var name in QueueNames)
            {
                try
                {
                    DeleteQueue(name);
                }
                catch (OperationInterruptedException ex)
                {
                    Console.WriteLine($"Unable to delete Queue:{name} please see Message:{ex.Message}");
                }
            }
        }

        public static void PurgeAllQueues()
        {
            foreach (var name in QueueNames)
            {
                try
                {
                    PurgeQueue(name);
                }
                catch (OperationInterruptedException ex)
                {
                    Console.WriteLine($"Unable to purge Queue:{name} please see Message:{ex.Message}");
                }
            }
        }
    }
}

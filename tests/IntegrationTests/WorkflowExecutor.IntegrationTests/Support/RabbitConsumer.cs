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

using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support
{
    public class RabbitConsumer
    {
        public RabbitConsumer(IModel channel, string exchange, string routingKey)
        {
            var arguments = new Dictionary<string, object>()
                {
                    { "x-queue-type", "quorum" },
                    { "x-delivery-limit", Deliverylimit },
                    { "x-dead-letter-exchange", DeadLetterExchange }
                };

            var deadLetterQueue = $"{RoutingKey}-dead-letter";

            var deadLetterExists = QueueExists(deadLetterQueue);
            if (deadLetterExists.exists == false)
            {
                channel.QueueDeclare(queue: deadLetterQueue, durable: true, exclusive: false, autoDelete: false);
            }

            Exchange = exchange;
            RoutingKey = routingKey;
            Channel = channel;
            Queue = Channel.QueueDeclare(queue: routingKey, durable: true, exclusive: false, autoDelete: false, arguments);
            Channel.QueueBind(Queue.QueueName, Exchange, RoutingKey);
            if (!string.IsNullOrEmpty(deadLetterQueue))
            {
                channel.QueueBind(deadLetterQueue, DeadLetterExchange, RoutingKey);
            }

            Channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
        }

        private QueueDeclareOk Queue { get; set; }

        private string Exchange { get; set; }

        private string RoutingKey { get; set; }

        private IModel Channel { get; set; }

        private string DeadLetterExchange { get; set; } = "monaideploy-dead-letter";

        private int Deliverylimit { get; set; } = 3;

        public T? GetMessage<T>()
        {
            var basicGetResult = Channel.BasicGet(Queue.QueueName, true);

            if (basicGetResult != null)
            {
                var byteArray = basicGetResult.Body.ToArray();

                var str = Encoding.Default.GetString(byteArray);

                return JsonConvert.DeserializeObject<T>(str);
            }

            return default;
        }

        public void CloseConnection()
        {
            Channel.Close();
        }
        private (bool exists, bool accessable) QueueExists(string queueName)
        {
            var testChannel = RabbitConnectionFactory.GetRabbitConnection();

            try
            {
                var testRun = testChannel!.QueueDeclarePassive(queue: queueName);
            }
            catch (OperationInterruptedException operationInterruptedException)
            {
                ///RabbitMQ node that hosts the previously created dead-letter queue is unavailable
                if (operationInterruptedException.Message.Contains("down or inaccessible"))
                {
                    return (true, false);
                }
                else
                {
                    return (false, true);
                }
            }
            return (true, true);
        }
    }
}

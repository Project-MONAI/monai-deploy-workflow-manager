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

namespace Monai.Deploy.TaskManager.IntegrationTests.Support
{
    public class RabbitConsumer
    {
        public RabbitConsumer(string exchange, string routingKey)
        {
            Exchange = exchange;
            RoutingKey = routingKey;
        }

        private string Exchange { get; set; }

        private string RoutingKey { get; set; }
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        public T? GetMessage<T>()
        {
            using (var channel = RabbitConnectionFactory.Connection?.CreateModel())
            {
                var queue = channel.QueueDeclare(queue: RoutingKey, durable: true);
                channel.QueueBind(queue.QueueName, Exchange, RoutingKey);
                channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);

                var basicGetResult = channel.BasicGet(queue.QueueName, true);

                if (basicGetResult != null)
                {
                    var byteArray = basicGetResult.Body.ToArray();

                    var str = Encoding.Default.GetString(byteArray);

                    return JsonConvert.DeserializeObject<T>(str);
                }
            }

            return default;
        }
    }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
}

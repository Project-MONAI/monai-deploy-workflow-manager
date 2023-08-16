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

namespace Monai.Deploy.Common.IntegrationTests.Support
{
    public class RabbitConsumer
    {
        public RabbitConsumer(IModel channel, string exchange, string routingKey)
        {
            Exchange = exchange;
            RoutingKey = routingKey;
            Channel = channel;
            Queue = Channel.QueueDeclare(queue: routingKey, durable: true, exclusive: false, autoDelete: false);
            Channel.QueueBind(Queue.QueueName, Exchange, RoutingKey);
            Channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
        }

        private QueueDeclareOk Queue { get; set; }

        private string Exchange { get; set; }

        private string RoutingKey { get; set; }

        private IModel Channel { get; set; }

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
    }
}

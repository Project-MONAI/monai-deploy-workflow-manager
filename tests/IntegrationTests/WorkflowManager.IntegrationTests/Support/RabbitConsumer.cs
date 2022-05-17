// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public class RabbitConsumer
    {
        public RabbitConsumer(ConnectionFactory connectionFactory, string exchange, string routingKey)
        {
            Exchange = exchange;
            RoutingKey = routingKey;
            var connection = connectionFactory.CreateConnection();
            Channel = connection.CreateModel();
            Queue = Channel.QueueDeclare(queue: string.Empty, durable: true, exclusive: false, autoDelete: false);
            Channel.QueueBind(Queue.QueueName, Exchange, RoutingKey);
            Channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
        }

        private QueueDeclareOk Queue { get; set; }

        private string Exchange { get; set; }

        private string RoutingKey { get; set; }

        private IModel Channel { get; set; }

        public T GetMessage<T>()
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

// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using RabbitMQ.Client;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    [Binding]
    public class MessageReceiver : DefaultBasicConsumer
    {
        private readonly IModel _channel;

        public MessageReceiver(IModel channel)
        {
            _channel = channel;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            // TO DO Assertions
            _channel.BasicAck(deliveryTag, false);
        }
    }
}

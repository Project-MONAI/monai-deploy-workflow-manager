using RabbitMQ.Client;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Support
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
            _channel.BasicAck(deliveryTag, false);
        }
    }
}

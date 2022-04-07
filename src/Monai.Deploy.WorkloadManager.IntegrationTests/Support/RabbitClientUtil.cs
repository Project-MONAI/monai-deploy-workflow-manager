using System.Text;
using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
using RabbitMQ.Client;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Support
{
    public class RabbitClientUtil
    {
        public RabbitClientUtil()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = TestExecutionConfig.RabbitConfig.Host,
                UserName = TestExecutionConfig.RabbitConfig.User,
                Password = TestExecutionConfig.RabbitConfig.Password
            };

            var connection = connectionFactory.CreateConnection();
            Channel = connection.CreateModel();
            MessageReceiver = new MessageReceiver(Channel);
            Properties = Channel.CreateBasicProperties();
            Properties.Persistent = false; // is this needed
        }

        private IModel Channel { get; set; }

        private MessageReceiver MessageReceiver { get; set; }

        private IBasicProperties Properties { get; set; }

        public void CreateQueue(string queueName)
        {
            Channel.QueueDeclare(queueName, true, false, false, null);
        }

        public void PurgeQueue(string queueName)
        {
            Channel.QueuePurge(queueName);
        }

        public void DeleteQueue(string queueName)
        {
            Channel.QueueDelete(queueName);
        }

        public void PublishMessage(string message, string queueName)
        {
            var messageBuffer = Encoding.Default.GetBytes(message);
            Channel.BasicPublish("",
                queueName,
                false,
                Properties,
                messageBuffer);
        }

        public void ConsumeMessage(string queueName)
        {
            Channel.BasicConsume(
                queueName,
                false,
                MessageReceiver);
        }

        public string ReturnMessagesFromQueue(string queueName)
        {
            var data = Channel.BasicGet(queueName, false);
            var message = data.Body.ToArray();
            var str = Encoding.Default.GetString(message);
            return str;
        }

        public void CloseConnection()
        {
            Channel.Close();
        }
    }
}

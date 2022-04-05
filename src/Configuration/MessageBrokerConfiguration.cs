using Monai.Deploy.MessageBroker.Common;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.Configuration
{
    public class MessageBrokerConfiguration : MessageBrokerServiceConfiguration
    {
        public static readonly string WorkflowManagerApplicationId = "16988a78-87b5-4168-a5c3-2cfc2bab8e54";

        /// <summary>
        /// Gets or sets retry options relate to the message broker services.
        /// </summary>
        [JsonProperty(PropertyName = "reties")]
        public RetryConfiguration Retries { get; set; } = new RetryConfiguration();

        /// <summary>
        /// Gets or sets the topics for events published/subscribed by Informatics Gateway
        /// </summary>
        [JsonProperty(PropertyName = "topics")]
        public MessageBrokerConfigurationKeys Topics { get; set; } = new MessageBrokerConfigurationKeys();
    }
}

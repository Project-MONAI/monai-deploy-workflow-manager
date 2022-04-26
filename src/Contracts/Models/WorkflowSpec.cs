using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class WorkflowSpec
    {
        [ConfigurationKeyName("name")]
        public string Name { get; set; }

        [ConfigurationKeyName("version")]
        public string Version { get; set; }

        [ConfigurationKeyName("description")]
        public string Description { get; set; }

        [ConfigurationKeyName("informatics_gateway")]
        public InformaticsGateway InformaticsGateway { get; set; }

        public TaskObject[] Tasks { get; set; }
    }
}

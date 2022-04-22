using System;

using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class Workflow
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "informatics_gateway")]
        public InformaticsGateway InformaticsGateway { get; set; }

        public TaskObject[] Tasks { get; set; }

        public TaskObject[] TaskTemplates { get; set; }
    }
}

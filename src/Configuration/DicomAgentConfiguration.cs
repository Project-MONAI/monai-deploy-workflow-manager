using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Configuration
{
    public class DicomAgentConfiguration
    {
        /// <summary>
        /// Gets or sets the agent name for the dicom web protocol.
        /// Defaults to `monaidicomweb`.
        /// </summary>
        [ConfigurationKeyName("dicomWebAgentName")]
        public string DicomWebAgentName { get; set; } = "monaidicomweb";

        /// <summary>
        /// Gets or sets the agent name for monaiscu.
        /// Defaults to `monaiscu`.
        /// </summary>
        [ConfigurationKeyName("scuAgentName")]
        public string ScuAgentName { get; set; } = "monaiscu";
    }
}

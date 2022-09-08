using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Configuration
{
    public class ArgoCallbackConfiguration
    {
        /// <summary>
        /// Gets or sets the rabbit override for agro callback.
        /// Defaults to `false`.
        /// </summary>
        [ConfigurationKeyName("argoRabbitOverrideEnabled")]
        public bool ArgoCallbackOverrideEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the rabbit override endpoint for agro callback.
        /// </summary>
        [ConfigurationKeyName("argoRabbitOverrideEndpoint")]
        public string ArgoRabbitOverrideEndpoint { get; set; } = string.Empty;
    }
}

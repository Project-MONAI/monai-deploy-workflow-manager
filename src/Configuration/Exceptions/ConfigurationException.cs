using System;
using System.Runtime.Serialization;

namespace Monai.Deploy.WorkloadManager.Configuration.Exceptions
{
    /// <summary>
    /// Represnets an exception based upon invalid configuration.
    /// </summary>
    [Serializable]
    public class ConfigurationException : Exception
    {
        public ConfigurationException()
        {
        }

        public ConfigurationException(string message) : base(message)
        {
        }

        public ConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

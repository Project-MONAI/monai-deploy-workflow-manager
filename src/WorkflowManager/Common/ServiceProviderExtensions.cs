using System;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkloadManager.Configuration.Exceptions;
using Monai.Deploy.WorkloadManager.Logging.Logging;

namespace Monai.Deploy.WorkloadManager.Common
{
    internal static class IServiceProviderExtensions
    {
        internal static TInterface LocateService<TInterface>(this IServiceProvider serviceProvider, ILogger<Program> logger, string fullyQualifiedTypeString)
        {
            var type = Type.GetType(fullyQualifiedTypeString);
            if (type is null)
            {
                logger.TypeNotFound(fullyQualifiedTypeString);
                throw new ConfigurationException($"Type '{fullyQualifiedTypeString}' cannot be found.");
            }

            var instance = serviceProvider.GetService(type);
            if (instance is null)
            {
                logger.InstanceOfTypeNotFound(fullyQualifiedTypeString);
                throw new ConfigurationException($"Instance of '{fullyQualifiedTypeString}' cannot be found.");
            }

            return (TInterface)instance;
        }
    }
}

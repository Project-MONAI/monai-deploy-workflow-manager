// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Configuration.Exceptions;
using Monai.Deploy.WorkflowManager.Logging.Logging;

namespace Monai.Deploy.WorkflowManager.Common
{
    internal static class ServiceProviderExtensions
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

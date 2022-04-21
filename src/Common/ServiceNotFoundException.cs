// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Globalization;
using System.Runtime.Serialization;
using Ardalis.GuardClauses;

namespace Monai.Deploy.WorkflowManager.Common
{
    public static class ServiceNotFoundExceptionGuardExtension
    {
        public static void NullService<T>(this IGuardClause guardClause, T service, string parameterName)
        {
            Guard.Against.Null(guardClause, nameof(guardClause));
            Guard.Against.NullOrWhiteSpace(parameterName, nameof(parameterName));

            if (service is null)
            {
                throw new ServiceNotFoundException(parameterName);
            }
        }
    }

    [Serializable]
    public class ServiceNotFoundException : Exception
    {
        private static readonly string MessageFormat = "Required service '{0}' cannot be found or cannot be initialized.";

        public ServiceNotFoundException(string serviceName)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, serviceName))
        {
        }

        public ServiceNotFoundException(string serviceName, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, serviceName), innerException)
        {
        }

        private ServiceNotFoundException()
        {
        }

        protected ServiceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

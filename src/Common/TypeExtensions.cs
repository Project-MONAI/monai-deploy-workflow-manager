// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-FileCopyrightText: © 2019-2020 NVIDIA Corporation
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;

namespace Monai.Deploy.WorkflowManager.Common
{
    public static class TypeExtensions
    {
        public static T CreateInstance<T>(this Type type, IServiceProvider serviceProvider, params object[] parameters)
        {
            Guard.Against.Null(type, nameof(type));
            Guard.Against.Null(serviceProvider, nameof(serviceProvider));

            return (T)ActivatorUtilities.CreateInstance(serviceProvider, type, parameters);
        }

        public static T CreateInstance<T>(this Type interfaceType, IServiceProvider serviceProvider, string typeString, params object[] parameters)
        {
            Guard.Against.Null(interfaceType, nameof(interfaceType));
            Guard.Against.Null(serviceProvider, nameof(serviceProvider));
            Guard.Against.NullOrWhiteSpace(typeString, nameof(typeString));

            var type = interfaceType.GetType(typeString);
            var processor = ActivatorUtilities.CreateInstance(serviceProvider, type, parameters);

            if (interfaceType.IsAssignableFrom(type))
            {
                return (T)processor;
            }
            else
            {
                throw new NotSupportedException($"'{typeString}' must implement '{interfaceType.Name}' interface");
            }
        }

        public static Type GetType(this Type interfaceType, string typeString)
        {
            Guard.Against.Null(interfaceType, nameof(interfaceType));
            Guard.Against.NullOrWhiteSpace(typeString, nameof(typeString));

            var type = Type.GetType(
                      typeString,
                      (name) =>
                      {
                          return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(z => !string.IsNullOrWhiteSpace(z.FullName) && z.FullName.StartsWith(name.FullName));
                      },
                      null,
                      true);

            if (type is not null && type.IsSubclassOf(interfaceType)) return type;

            throw new NotSupportedException($"{typeString} is not a sub-type of {interfaceType.Name}");
        }
    }
}

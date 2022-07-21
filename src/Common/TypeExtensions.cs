﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-FileCopyrightText: © 2019-2020 NVIDIA Corporation
// SPDX-License-Identifier: Apache License 2.0

using System.Reflection;
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

            return (T)processor;
        }

        public static Type GetType(this Type interfaceType, string typeString)
        {
            Guard.Against.Null(interfaceType, nameof(interfaceType));
            Guard.Against.NullOrWhiteSpace(typeString, nameof(typeString));

            var type = Type.GetType(
                      typeString,
                      (name) =>
                      {
                          var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(z => !string.IsNullOrWhiteSpace(z.FullName) && z.FullName.StartsWith(name.FullName));

                          if (assembly is null)
                          {
                              assembly = Assembly.Load($"{AppDomain.CurrentDomain.BaseDirectory}{name.FullName}.dll");
                          }

                          return assembly;
                      },
                      null,
                      true);

            if (type is not null &&
                (type.IsSubclassOf(interfaceType) ||
                    (type.BaseType is not null && type.BaseType.IsAssignableTo(interfaceType)))) return type;

            throw new NotSupportedException($"{typeString} is not a sub-type of {interfaceType.Name}");
        }
    }
}

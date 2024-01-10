/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Monai.Deploy.WorkflowManager.TaskManager.API.Extensions
{
    public static class TypeExtensions
    {
        public static T CreateInstance<T>(this Type type, IServiceProvider serviceProvider, params object[] parameters)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

            return (T)ActivatorUtilities.CreateInstance(serviceProvider, type, parameters);
        }

        public static T CreateInstance<T>(this Type interfaceType, IServiceProvider serviceProvider, string typeString, params object[] parameters)
        {
            ArgumentNullException.ThrowIfNull(interfaceType, nameof(interfaceType));
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(typeString, nameof(typeString));

            var type = interfaceType.GetType(typeString);
            var processor = ActivatorUtilities.CreateInstance(serviceProvider, type, parameters);

            return (T)processor;
        }

        public static Type GetType(this Type interfaceType, string typeString)
        {
            ArgumentNullException.ThrowIfNull(interfaceType, nameof(interfaceType));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(typeString, nameof(typeString));

            var type = Type.GetType(
                      typeString,
                      (name) =>
                      {
                          var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(z => !string.IsNullOrWhiteSpace(z.FullName) && z.FullName.StartsWith(name.FullName));

                          if (assembly is null)
                          {
                              assembly = Assembly.LoadFile($"{AppDomain.CurrentDomain.BaseDirectory}{name.FullName}.dll");
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

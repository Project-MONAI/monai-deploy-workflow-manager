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

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous
{
    public class MonaiServiceLocator : IMonaiServiceLocator
    {
        private readonly IList<Type> _types;
        private readonly IList<IMonaiService> _runningServices;
        private readonly IServiceProvider _serviceProvider;

        public MonaiServiceLocator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _types = LocateTypes();
            _runningServices = LocateServices();
        }

        public IEnumerable<IMonaiService> GetMonaiServices()
        {
            return _runningServices;
        }

        public Dictionary<string, ServiceStatus> GetServiceStatus()
        {
            return _runningServices.ToDictionary(k => k.ServiceName, v => v.Status);
        }

        private IMonaiService? GetService(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            return _serviceProvider.GetService(type) as IMonaiService;
        }

        private static List<Type> LocateTypes()
        {
            var serviceType = typeof(IMonaiService);
            var services = AppDomain.CurrentDomain.GetAssemblies()
                                .Where(a => !a.IsDynamic)
                                .SelectMany(p => p.GetTypes())
                                .Where(p =>
                                            serviceType.IsAssignableFrom(p) &&
                                            p != serviceType &&
                                            !p.IsAbstract &&
                                            p.FullName is not null && p.FullName.StartsWith("Monai", StringComparison.InvariantCulture));
            return services.Distinct().ToList();
        }

        private IList<IMonaiService> LocateServices()
        {
            var list = new List<IMonaiService>();
            foreach (var t in _types)
            {
                var service = GetService(t);
                if (service is not null)
                {
                    list.Add(service);
                }
            }
            return list;
        }
    }
}

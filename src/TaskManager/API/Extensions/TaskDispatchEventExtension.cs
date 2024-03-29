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

using Monai.Deploy.Messaging.Events;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.API.Extensions
{
    public static class TaskDispatchEventExtension
    {
        public static string? GetTaskPluginArgumentsParameter(this TaskDispatchEvent taskDispatchEvent, string key)
        {
            if (!taskDispatchEvent.TaskPluginArguments.TryGetValue(key, out var value))
            {
                return null;
            }

            return value;
        }

        public static T? GetTaskPluginArgumentsParameter<T>(this TaskDispatchEvent taskDispatchEvent, string key)
        {
            taskDispatchEvent.TaskPluginArguments.TryGetValue(key, out var value);
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}

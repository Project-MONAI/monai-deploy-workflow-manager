// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Common.Extensions
{
    public static class TaskDispatchEventExtension
    {
        public static string? GetTaskPluginArgumentsParameter(this TaskDispatchEvent taskDispatchEvent, string key)
        {
            taskDispatchEvent.TaskPluginArguments.TryGetValue(key, out string? value);
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value;
        }

        public static T? GetTaskPluginArgumentsParameter<T>(this TaskDispatchEvent taskDispatchEvent, string key)
        {
            taskDispatchEvent.TaskPluginArguments.TryGetValue(key, out string? value);
            if (string.IsNullOrWhiteSpace(value))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}

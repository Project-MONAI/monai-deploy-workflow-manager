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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Monai.Deploy.WorkflowManager.Common.Miscellaneous
{
    public static class HttpLoggingExtensions
    {
        public static IServiceCollection AddHttpLoggingForMonai(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            services.AddHttpLogging(options =>
            {
                options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPropertiesAndHeaders |
                                        Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
                if (configuration.GetValue<bool>("Kestrel:LogHttpRequestBody", false))
                {
                    options.LoggingFields |= Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestBody;
                }
                if (configuration.GetValue<bool>("Kestrel:LogHttpResponseBody", false))
                {
                    options.LoggingFields |= Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseBody;
                }
                if (configuration.GetValue<bool>("Kestrel:LogHttpRequestQuery", false))
                {
                    options.LoggingFields |= Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestQuery;
                }
            });

            return services;
        }
    }
}

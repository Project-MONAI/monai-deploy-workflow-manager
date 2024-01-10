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

using System.Net.Http.Formatting;
using System.Text;

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support
{
    public static class HttpRequestMessageExtensions
    {
        internal static HttpRequestMessage Clone(this HttpRequestMessage input)
        {
            var request = new HttpRequestMessage(input.Method, input.RequestUri)
            {
                Content = input.Content,
            };

            foreach (var prop in input.Options)
            {
                request.Options.TryAdd(prop.Key, prop.Value);
            }

            foreach (var header in input.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return request;
        }

        public static void AddJsonBody<T>(this HttpRequestMessage input, T body)
        {
            if (body == null || (body is string b && string.IsNullOrEmpty(b)))
            {
                input.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                return;
            }

            input.Content = new ObjectContent<T>(body, new JsonMediaTypeFormatter());
        }
    }
}

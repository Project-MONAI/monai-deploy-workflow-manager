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


using Ardalis.GuardClauses;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support
{
    [Binding]
    public class ApiHelper
    {
        public ApiHelper(HttpClient? httpClient)
        {
            Client = httpClient ?? throw new ArgumentException($"{nameof(httpClient)} can not be null");
        }

        public HttpResponseMessage? Response { get; private set; }

        public HttpRequestMessage? Request { get; set; }

        public HttpClient Client { get; }

        public void SetRequestVerb(string httpMethod)
        {
            Guard.Against.Null(Request);
            Request.Method = httpMethod.ToUpper() switch
            {
                "GET" => HttpMethod.Get,
                "PUT" => HttpMethod.Put,
                "PATCH" => HttpMethod.Patch,
                "POST" => HttpMethod.Post,
                "DELETE" => HttpMethod.Delete,
                _ => throw new Exception($"Unsupported request method: {httpMethod}. Please review your test scenario."),
            };
        }

        public async Task<HttpResponseMessage> GetResponseAsync()
        {
            Guard.Against.Null(Request);

            var request = Request.Clone();


            return Response = await Client.SendAsync(request);
        }

        public void SetUrl(Uri url) =>
            Request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{url}"),
            };
    }
}

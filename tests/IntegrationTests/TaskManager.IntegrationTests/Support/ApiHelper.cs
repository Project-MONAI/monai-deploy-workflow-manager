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

using System.Web;
using Monai.Deploy.WorkflowManager.Common.IntegrationTests.Support;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support
{
    [Binding]
    public class ApiHelper
    {
        public ApiHelper(HttpClient httpClient)
        {
            Client = httpClient;
        }

        public HttpResponseMessage? Response { get; private set; }

        public HttpRequestMessage? Request { get; set; }

        public HttpClient Client { get; }

        public void SetRequestVerb(string httpMethod)
        {
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
            var request = Request.Clone();

            return Response = await Client.SendAsync(request);
        }

        public void SetUrl(Uri url) =>
            Request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{url}"),
            };

        public void AddQueryParams(Dictionary<string, string> dict)
        {
            var builder = new UriBuilder(Request.RequestUri);
            var query = HttpUtility.ParseQueryString(builder.Query);
            foreach (var kv in dict)
            {
                query[kv.Key] = kv.Value;
            }
            SetUrl(new Uri(builder.ToString() + "?" + query.ToString()));
        }
    }
}
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

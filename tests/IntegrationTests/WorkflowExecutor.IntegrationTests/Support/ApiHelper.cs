
namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    [Binding]
    public class ApiHelper
    {
        public ApiHelper(HttpClient httpClient)
        {
            Client = httpClient;
        }

        public HttpResponseMessage Response { get; private set; }

        public HttpRequestMessage Request { get; set; }

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
    }
}

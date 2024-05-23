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

using System.Globalization;
using System.Text;
using Argo;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Logging;
using System.Net;
using Monai.Deploy.WorkflowManager.TaskManager.Argo.Exceptions;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    public class ArgoClient : BaseArgoClient, IArgoClient
    {
        public ArgoClient(HttpClient httpClient, ILoggerFactory logger) : base(httpClient, logger) { }

        public async Task<Workflow> Argo_CreateWorkflowAsync(string argoNamespace, WorkflowCreateRequest body, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(argoNamespace, nameof(argoNamespace));
            ArgumentNullException.ThrowIfNull(body, nameof(body));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(CultureInfo.InvariantCulture, $"{FormattedBaseUrl}/api/v1/workflows/{argoNamespace}");

            var method = "POST";
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body));
            return await SendRequest<Workflow>(content, urlBuilder, method, cancellationToken).ConfigureAwait(false);

        }

        public async Task<Workflow?> Argo_GetWorkflowAsync(string argoNamespace,
            string name,
            string? getOptionsResourceVersion,
            string? fields,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(argoNamespace, nameof(argoNamespace));
            ArgumentNullException.ThrowIfNull(name, nameof(name));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(CultureInfo.InvariantCulture, $"{FormattedBaseUrl}/api/v1/workflows/{argoNamespace}/{name}?");

            if (getOptionsResourceVersion != null)
            {
                urlBuilder.Append(Uri.EscapeDataString("getOptions.resourceVersion") + "=").Append(Uri.EscapeDataString(ConvertToString(getOptionsResourceVersion, CultureInfo.InvariantCulture))).Append('&');
            }
            if (fields != null)
            {
                urlBuilder.Append(Uri.EscapeDataString("fields") + "=").Append(Uri.EscapeDataString(ConvertToString(fields, CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder.Length--;

            return await GetRequest<Workflow>(urlBuilder).ConfigureAwait(false);

        }

        public async Task<Workflow> Argo_StopWorkflowAsync(string argoNamespace, string name, WorkflowStopRequest body)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(argoNamespace, nameof(argoNamespace));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentNullException.ThrowIfNull(body, nameof(body));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(CultureInfo.InvariantCulture, $"{FormattedBaseUrl}/api/v1/workflows/{argoNamespace}/{name}/stop");

            const string method = "PUT";
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body));
            try
            {
                return await SendRequest<Workflow>(content, urlBuilder, method, new CancellationToken()).ConfigureAwait(false);
            }
            catch (ApiException<Error> ex)
            {
                if (ex.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    throw new ArgoWorkflowNotFoundException(body.Name, ex);
                }
                throw;
            }
            catch (Exception)
            {
                throw;
            }


        }

        public async Task<Workflow> Argo_TerminateWorkflowAsync(string argoNamespace, string name, WorkflowTerminateRequest body)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(argoNamespace, nameof(argoNamespace));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentNullException.ThrowIfNull(body, nameof(body));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(CultureInfo.InvariantCulture, $"{FormattedBaseUrl}/api/v1/workflows/{argoNamespace}/{name}/terminate");

            const string method = "PUT";
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body));
            try
            {
                return await SendRequest<Workflow>(content, urlBuilder, method, new CancellationToken()).ConfigureAwait(false);
            }
            catch (ApiException<Error> ex)
            {
                if (ex.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    throw new ArgoWorkflowNotFoundException(body.Name, ex);
                }
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<WorkflowTemplate?> Argo_GetWorkflowTemplateAsync(string argoNamespace, string name, string? getOptionsResourceVersion)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(argoNamespace, nameof(argoNamespace));
            ArgumentNullException.ThrowIfNull(name, nameof(name));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(CultureInfo.InvariantCulture, $"{FormattedBaseUrl}/api/v1/workflow-templates/{argoNamespace}/{name}?");

            if (getOptionsResourceVersion != null)
            {
                urlBuilder.Append(Uri.EscapeDataString("getOptions.resourceVersion") + "=").Append(Uri.EscapeDataString(ConvertToString(getOptionsResourceVersion, CultureInfo.InvariantCulture))).Append('&');
            }
            urlBuilder.Length--;

            return await GetRequest<WorkflowTemplate?>(urlBuilder).ConfigureAwait(false);
        }

        public async Task<Version?> Argo_GetVersionAsync()
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(CultureInfo.InvariantCulture, $"{FormattedBaseUrl}/api/v1/version");

            return await GetRequest<Version>(urlBuilder).ConfigureAwait(false);
        }

        public async Task<string?> Argo_Get_WorkflowLogsAsync(string argoNamespace, string name, string? podName, string logOptionsContainer)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(argoNamespace, nameof(argoNamespace));
            ArgumentNullException.ThrowIfNull(name, nameof(name));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(CultureInfo.InvariantCulture, $"{FormattedBaseUrl}/api/v1/workflows/{argoNamespace}/{name}/log?");

            if (string.IsNullOrWhiteSpace(podName) is false)
            {
                urlBuilder.Append(Uri.EscapeDataString("podName") + "=").Append(Uri.EscapeDataString(ConvertToString(podName, CultureInfo.InvariantCulture))).Append('&');
            }
            if (logOptionsContainer != null)
            {
                urlBuilder.Append(Uri.EscapeDataString("logOptions.container") + "=").Append(Uri.EscapeDataString(ConvertToString(logOptionsContainer, CultureInfo.InvariantCulture))).Append('&');
            }

            urlBuilder.Length--;
            return await GetRequest<string>(urlBuilder, true).ConfigureAwait(false);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A successful response.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async Task<WorkflowTemplate> Argo_CreateWorkflowTemplateAsync(string argoNamespace, WorkflowTemplateCreateRequest body, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(argoNamespace, nameof(argoNamespace));
            ArgumentNullException.ThrowIfNull(body.Template, nameof(body.Template));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(CultureInfo.InvariantCulture, $"{FormattedBaseUrl}/api/v1/workflow-templates/{argoNamespace}");

            var method = "POST";
            var stringBody = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            var content = new StringContent(stringBody);

            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Debug($"Sending content to Argo :{stringBody.Replace(Environment.NewLine, "")}");
            return await SendRequest<WorkflowTemplate>(content, urlBuilder, method, cancellationToken).ConfigureAwait(false);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A successful response.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async Task<bool> Argo_DeleteWorkflowTemplateAsync(string argoNamespace, string templateName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(argoNamespace, nameof(argoNamespace));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(CultureInfo.InvariantCulture, $"{FormattedBaseUrl}/api/v1/workflow-templates/{argoNamespace}/{templateName}");

            var method = "DELETE";
            var response = await HttpClient.SendAsync(new HttpRequestMessage(new HttpMethod(method), urlBuilder.ToString()), cancellationToken).ConfigureAwait(false);
            return (int)response.StatusCode == 200;
        }

        public static string ConvertToString(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return "";
            }

            if (value is Enum)
            {
                var name = Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                    if (field != null)
                    {
                        if (System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute)) is System.Runtime.Serialization.EnumMemberAttribute attribute)
                        {
                            return attribute.Value ?? name;
                        }
                    }

                    var converted = Convert.ToString(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                    return converted ?? string.Empty;
                }
            }
            else if (value is bool boolean)
            {
                return Convert.ToString(boolean, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[] v)
            {
                return Convert.ToBase64String(v);
            }
            else if (value.GetType().IsArray)
            {
                var array = Enumerable.OfType<object>((Array)value);
                return string.Join(",", Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
            }

            var result = Convert.ToString(value, cultureInfo);
            return result ?? "";
        }
    }

    /// <summary>
    /// <see cref="BaseArgoClient"/> generic functions relating to argo requests
    /// </summary>
    public class BaseArgoClient
    {
        public string BaseUrl { get; set; } = "http://localhost:2746";

        protected string FormattedBaseUrl { get { return BaseUrl != null ? BaseUrl.TrimEnd('/') : ""; } }

        protected readonly HttpClient HttpClient;

        protected readonly ILogger Logger;
        public BaseArgoClient(HttpClient httpClient, ILoggerFactory loggerFactory)
        {
            HttpClient = httpClient;
            Logger = loggerFactory.CreateLogger("BaseArgoClient");
        }

        protected async Task<T> SendRequest<T>(StringContent stringContent, StringBuilder urlBuilder, string method, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage())
            {
                if (stringContent is not null)
                {
                    stringContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                    request.Content = stringContent;
                }
                request.Method = new HttpMethod(method);
                request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
                request.RequestUri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);

                HttpResponseMessage? response = null;
                var logStringContent = stringContent == null ? string.Empty : await stringContent.ReadAsStringAsync();
                Logger.CallingArgoHttpInfo(request.RequestUri.ToString(), method, logStringContent);
                response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);

                try
                {
                    var headers = Enumerable.ToDictionary(response.Headers, h => h.Key, h => h.Value);
                    if (response.Content != null && response.Content.Headers != null)
                    {
                        foreach (var item in response.Content.Headers)
                            headers[item.Key] = item.Value;
                    }

                    var status = (int)response.StatusCode;
                    if (status == 200)
                    {
                        var objectResponse = await ReadObjectResponseAsync<T>(response, headers).ConfigureAwait(false);
                        if (objectResponse.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);
                        }
                        return objectResponse.Object;
                    }
                    else
                    {
                        var objectResponse = await ReadObjectResponseAsync<Error>(response, headers).ConfigureAwait(false);
                        if (objectResponse.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);
                        }
                        throw new ApiException<Error>("An unexpected error response.", status, objectResponse.Text, headers, objectResponse.Object, null);
                    }
                }
                finally
                {
                    response.Dispose();
                }
            }
        }

        protected async Task<T?> GetRequest<T>(StringBuilder urlBuilder, bool isLogs = false)
        {

            using (var request = new HttpRequestMessage())
            {
                request.Method = new HttpMethod("GET");
                request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
                request.RequestUri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);

                var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                try
                {
                    var headers = Enumerable.ToDictionary(response.Headers, h => h.Key, h => h.Value);
                    if (response.Content != null && response.Content.Headers != null)
                    {
                        foreach (var item in response.Content.Headers)
                            headers[item.Key] = item.Value;
                    }

                    var status = (int)response.StatusCode;
                    if (status == 200)
                    {
                        ObjectResponseResult<T?> objectResponse;

                        objectResponse = await ReadObjectResponseAsync<T>(response, headers, isLogs).ConfigureAwait(false);

                        if (objectResponse.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);
                        }
                        return objectResponse.Object;
                    }
                    else
                    {
                        var objectResponse = await ReadObjectResponseAsync<Error>(response, headers, false).ConfigureAwait(false);
                        if (objectResponse.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);
                        }
                        throw new ApiException<Error>("An unexpected error response.", status, objectResponse.Text, headers, objectResponse.Object, null);
                    }
                }
                finally
                {
                    response.Dispose();
                }
            }
        }

        protected virtual async Task<ObjectResponseResult<T?>> ReadObjectResponseAsync<T>(HttpResponseMessage response, IReadOnlyDictionary<string, IEnumerable<string>> headers, bool isLogs = false)
        {
            if (response == null || response.Content == null || response.Content.GetType().Name == "EmptyContent")
            {
                return new ObjectResponseResult<T?>(default, string.Empty);
            }

            try
            {
                using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var streamReader = new StreamReader(responseStream);
                var inBody = await streamReader.ReadToEndAsync();

                T? typedBody;

                if (isLogs)
                {
                    typedBody = (T)(object)DecodeLogs(inBody);
                }
                else
                {
                    typedBody = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(inBody);
                }

                return new ObjectResponseResult<T?>(typedBody, string.Empty);

            }
            catch (Newtonsoft.Json.JsonException exception)
            {
                var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
            }
        }

        public static string DecodeLogs(string logInput)
        {
            var rows = logInput.Split(new String[] { "\n" }, StringSplitOptions.None);
            var jsonBody = $"[{string.Join(",", rows)}]";

            var typedBody = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<ArgoLogEntryResult>>(jsonBody);
            if (typedBody is null)
            {
                return "";
            }
            var outputLogs = string.Join("\n", typedBody.Select(b => b.Result.Content));
            return outputLogs;
        }

        protected virtual async Task<ObjectResponseResult<string>> ReadLogResponseAsync(HttpResponseMessage response, IReadOnlyDictionary<string, IEnumerable<string>> headers)
        {
            if (response == null || response.Content == null)
            {
                return new ObjectResponseResult<string>(string.Empty, string.Empty);
            }

            try
            {
                using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var streamReader = new StreamReader(responseStream);

                var inBody = await streamReader.ReadToEndAsync();
                var rows = inBody.Split(new String[] { "\n" }, StringSplitOptions.None);
                var jsonBody = $"[{string.Join(",", rows)}]";

                var typedBody = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<ArgoLogEntryResult>>(jsonBody);
                var outputLogs = string.Join("\n", typedBody?.Select(b => b.Result.Content) ?? Array.Empty<string>());
                return new ObjectResponseResult<string>(outputLogs, string.Empty);

            }
            catch (Newtonsoft.Json.JsonException exception)
            {
                var message = "Could not deserialize the response body stream as array of ArgoLogEntryResult.";
                throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
            }
        }

        protected readonly struct ObjectResponseResult<T>
        {
            public ObjectResponseResult(T responseObject, string responseText)
            {
                Object = responseObject;
                Text = responseText;
            }

            public T Object { get; }

            public string Text { get; }
        }

        class ArgoLogEntry
        {
            public string Content { get; set; } = "";

            public string PodName { get; set; } = "";
        }

        class ArgoLogEntryResult
        {
            public ArgoLogEntry Result { get; set; } = new ArgoLogEntry();
        }
    }

    public class Version
    {
        [Newtonsoft.Json.JsonProperty("buildDate", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string BuildDate { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("compiler", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Compiler { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("gitCommit", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string GitCommit { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("gitTag", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string GitTag { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("gitTreeState", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string GitTreeState { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("goVersion", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string GoVersion { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("platform", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Platform { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("version", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Version1 { get; set; } = "";

    }
}

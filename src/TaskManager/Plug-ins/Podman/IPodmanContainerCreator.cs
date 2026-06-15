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

using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monai.Deploy.WorkflowManager.TaskManager.Podman
{
    /// <summary>
    /// Creates containers via Podman's native libpod API.
    /// The Docker compat API does not support CDI device identifiers (e.g. "nvidia.com/gpu=all")
    /// in HostConfig.Devices — it treats PathOnHost as a literal file path.
    /// The libpod API's "devices" field with a "path" property correctly resolves CDI identifiers.
    /// </summary>
    public interface IPodmanContainerCreator
    {
        Task<PodmanCreateContainerResponse> CreateContainerAsync(Uri podmanEndpoint, PodmanCreateContainerRequest request, CancellationToken cancellationToken);
    }

    public class PodmanContainerCreator : IPodmanContainerCreator
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };

        public async Task<PodmanCreateContainerResponse> CreateContainerAsync(Uri podmanEndpoint, PodmanCreateContainerRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(podmanEndpoint, nameof(podmanEndpoint));
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            var socketPath = podmanEndpoint.LocalPath;

            var handler = new SocketsHttpHandler
            {
                ConnectCallback = async (context, ct) =>
                {
                    var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    var endpoint = new UnixDomainSocketEndPoint(socketPath);
                    await socket.ConnectAsync(endpoint, ct).ConfigureAwait(false);
                    return new NetworkStream(socket, ownsSocket: true);
                }
            };

            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var json = JsonSerializer.Serialize(request, s_jsonOptions);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // The path targets the libpod API at v4.0.0 — the minimum supported Podman version for CDI device support.
            using var response = await httpClient.PostAsync("/v4.0.0/libpod/containers/create", content, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ContainerMonitorException($"Podman libpod API returned {response.StatusCode}: {responseBody}");
            }

            var result = JsonSerializer.Deserialize<PodmanCreateContainerResponse>(responseBody, s_jsonOptions);
            return result ?? throw new ContainerMonitorException($"Failed to deserialize Podman create response: {responseBody}");
        }
    }

    public class PodmanCreateContainerRequest
    {
        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("entrypoint")]
        public IList<string>? Entrypoint { get; set; }

        [JsonPropertyName("command")]
        public IList<string>? Command { get; set; }

        [JsonPropertyName("env")]
        public IDictionary<string, string>? Env { get; set; }

        [JsonPropertyName("security_opt")]
        public IList<string>? SecurityOpt { get; set; }

        [JsonPropertyName("devices")]
        public IList<PodmanDevice>? Devices { get; set; }

        [JsonPropertyName("mounts")]
        public IList<PodmanMount>? Mounts { get; set; }

        [JsonPropertyName("user")]
        public string? User { get; set; }

        [JsonPropertyName("shm_size")]
        public long? ShmSize { get; set; }
    }

    public class PodmanDevice
    {
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;
    }

    public class PodmanMount
    {
        [JsonPropertyName("destination")]
        public string Destination { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "bind";

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonPropertyName("options")]
        public IList<string>? Options { get; set; }
    }

    public class PodmanCreateContainerResponse
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("Warnings")]
        public IList<string>? Warnings { get; set; }
    }
}

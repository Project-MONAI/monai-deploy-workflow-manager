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

namespace Monai.Deploy.WorkflowManager.TaskManager.Docker
{
    internal static class Keys
    {
        /// <summary>
        /// Key for the endpoint where the Docker server is running.
        /// </summary>
        public static readonly string BaseUrl = "server_url";

        /// <summary>
        /// Key for the image of the container to deploy the MAP.
        /// </summary>
        public static readonly string ContainerImage = "container_image";

        /// <summary>
        /// Key for the entrypoint to the container.
        /// </summary>
        public static readonly string EntryPoint = "entrypoint";

        /// <summary>
        /// Key for specifying the user to the container. Same as -u argument for docker run.
        /// </summary>
        public static readonly string User = "user";

        /// <summary>
        /// Key for the command to execute by the container.
        /// </summary>
        public static readonly string Command = "command";

        /// <summary>
        /// Key to indicate whether to always pull the image.
        /// </summary>
        public static readonly string AlwaysPull = "always_pull";

        /// <summary>
        /// Key for task timeout value.
        /// </summary>
        public static readonly string TaskTimeoutMinutes = "task_timeout_minutes";

        /// <summary>
        /// Key for priority classnames on task plugin arguments side
        /// </summary>
        public static readonly string TemporaryStorageContainerPath = "temp_storage_container_path";

        /// <summary>
        /// Prefix for envrionment variables.
        /// </summary>
        public static readonly string EnvironmentVariableKeyPrefix = "env_";

        /// <summary>
        /// Key to the intermediate volume map path.
        /// </summary>
        public static readonly string WorkingDirectory = "env_MONAI_WORKDIR";

        /// <summary>
        /// Required arguments to run the Docker workflow.
        /// </summary>
        public static readonly IReadOnlyList<string> RequiredParameters =
            new List<string> {
                BaseUrl,
                EntryPoint,
                Command,
                ContainerImage,
                TemporaryStorageContainerPath
            };
    }
}

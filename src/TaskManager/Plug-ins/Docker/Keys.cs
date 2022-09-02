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
        /// Key for the image of the container to deploy the MAP.
        /// </summary>
        public static readonly string Command = "command";

        /// <summary>
        /// Key for setting the message broker's endpoint.
        /// </summary>
        public static readonly string MessagingEnddpoint = "messaging_endpoint";

        /// <summary>
        /// Key for setting the user name to access the message broker.
        /// </summary>
        public static readonly string MessagingUsername = "messaging_username";

        /// <summary>
        /// Key for setting the password to access the message broker.
        /// </summary>
        public static readonly string MessagingPassword = "messaging_password";

        /// <summary>
        /// Key for setting the topic of the completion event.
        /// </summary>
        public static readonly string MessagingTopic = "messaging_topic";

        /// <summary>
        /// Key for setting the exchange of the message broker.
        /// </summary>
        public static readonly string MessagingExchange = "messaging_exchange";

        /// <summary>
        /// Key for setting the vhost of the message broker.
        /// </summary>
        public static readonly string MessagingVhost = "messaging_vhost";

        /// <summary>
        /// Key for priority classnames on task plugin arguments side
        /// </summary>
        public static readonly string TemporaryStorageContainerPath = "temp_storage_container_path";

        /// <summary>
        /// Key for priority classnames on task plugin arguments side
        /// </summary>
        public static readonly string TemporaryStorageHostPath = "temp_storage_host_path";

        /// <summary>
        /// Required arguments to run the Docker workflow.
        /// </summary>
        public static readonly IReadOnlyList<string> RequiredParameters =
            new List<string> {
                BaseUrl,
                Command,
                ContainerImage,
                MessagingEnddpoint,
                MessagingUsername,
                MessagingPassword,
                MessagingTopic,
                MessagingExchange,
                MessagingVhost,
                TemporaryStorageContainerPath,
                TemporaryStorageHostPath
            };
    }
}

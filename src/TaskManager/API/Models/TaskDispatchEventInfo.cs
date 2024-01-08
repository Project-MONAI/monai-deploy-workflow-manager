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
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.Migrations;
using Mongo.Migration.Documents;
using Mongo.Migration.Documents.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.API.Models
{
    [CollectionLocation("TaskDispatchEvents"), RuntimeVersion("1.0.0")]
    public class TaskDispatchEventInfo : IDocument
    {
        /// <summary>
        /// Gets or sets the ID of the object.
        /// </summary>
        [BsonId]
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets Db version.
        /// </summary>
        [JsonConverter(typeof(DocumentVersionConvert)), BsonSerializer(typeof(DocumentVersionConverBson))]
        public DocumentVersion Version { get; set; } = new DocumentVersion(1, 0, 0);

        /// <summary>
        /// Gets or sets the original task disatpch event.
        /// </summary>
        [JsonProperty(PropertyName = "event")]
        public TaskDispatchEvent Event { get; set; }

        /// <summary>
        /// Gets or sets the date time that the task started with the plug-in.
        /// </summary>
        [JsonProperty(PropertyName = "started")]
        public DateTime Started { get; set; }

        /// <summary>
        /// Gets or sets the storage user accounts created that need to be cleaned up later.
        /// </summary>
        [JsonProperty(PropertyName = "users")]
        public IList<string> UserAccounts { get; private set; }

        public TaskDispatchEventInfo(TaskDispatchEvent taskDispatchEvent)
        {
            Id = Guid.NewGuid();
            Event = taskDispatchEvent;
            Started = DateTime.UtcNow;
            UserAccounts = new List<string>();
        }

        public bool HasTimedOut(TimeSpan taskTimeout) => DateTime.UtcNow.Subtract(Started) >= taskTimeout;

        public void AddUserAccount(string username)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(username, nameof(username));
            UserAccounts.Add(username);
        }
    }
}

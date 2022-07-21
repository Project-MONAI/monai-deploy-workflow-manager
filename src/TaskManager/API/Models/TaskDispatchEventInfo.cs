// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Events;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.TaskManager.API.Models
{
    public class TaskDispatchEventInfo
    {
        /// <summary>
        /// Gets or sets the ID of the object.
        /// </summary>
        [BsonId]
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

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
            Guard.Against.NullOrWhiteSpace(username, nameof(username));
            UserAccounts.Add(username);
        }
    }
}

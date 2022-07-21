// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Runtime.Serialization;

namespace Monai.Deploy.WorkflowManager.TaskManager
{
    [Serializable]
    internal class TaskManagerException : Exception
    {
        public TaskManagerException()
        {
        }

        public TaskManagerException(string? message) : base(message)
        {
        }

        public TaskManagerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected TaskManagerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
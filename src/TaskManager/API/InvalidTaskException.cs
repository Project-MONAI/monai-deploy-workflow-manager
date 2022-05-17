// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Diagnostics;
using System.Runtime.Serialization;

namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    [Serializable, DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class InvalidTaskException : Exception
    {
        public InvalidTaskException()
        {
        }

        public InvalidTaskException(string message) : base(message)
        {
        }

        public InvalidTaskException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidTaskException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}

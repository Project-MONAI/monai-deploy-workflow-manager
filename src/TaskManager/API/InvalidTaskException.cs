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

using System.Diagnostics;
using System.Runtime.Serialization;

namespace Monai.Deploy.TaskManager.API
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

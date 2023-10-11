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

using System.Runtime.Serialization;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.Exceptions
{
    [Serializable]
    public class TemplateNotFoundException : Exception
    {
        public TemplateNotFoundException(string workflowTemplateName)
            : base($"WorkflowTmplate '{workflowTemplateName}' cannot be found.")
        {
        }

        public TemplateNotFoundException(string workflowTemplateName, string templateName)
            : base($"Template '{templateName}' cannot be found in the referenced WorkflowTmplate '{workflowTemplateName}'.")
        {
        }

        public TemplateNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected TemplateNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

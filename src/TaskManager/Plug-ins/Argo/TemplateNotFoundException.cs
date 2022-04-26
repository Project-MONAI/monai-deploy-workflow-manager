// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Runtime.Serialization;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
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

// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Runtime.Serialization;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    [Serializable]
    internal class ArtifactMappingNotFoundException : Exception
    {
        public ArtifactMappingNotFoundException()
        {
        }

        public ArtifactMappingNotFoundException(string? artifactName) : base($"Storage information cannot be found for artifact '{artifactName}'.")
        {
        }

        public ArtifactMappingNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ArtifactMappingNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Common
{
    public static class ArtifactExtensions
    {
        public static Dictionary<string, string> ToDictionary(this Artifact[] artifacts)
        {
            Guard.Against.NullOrEmpty(artifacts, nameof(artifacts));

            return artifacts.ToDictionary(a => a.Name, a => a.Value);
        }
    }
}

// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Argo;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    public interface IArgoProvier
    {
        ArgoWorkflowsAPIClient CreateClient();
    }
}

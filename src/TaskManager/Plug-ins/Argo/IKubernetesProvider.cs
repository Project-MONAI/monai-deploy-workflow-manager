// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using k8s;

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo
{
    public interface IKubernetesProvider
    {
        Kubernetes CreateClient();
    }
}

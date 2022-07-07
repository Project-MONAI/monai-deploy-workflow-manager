// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.TaskManager.Argo.StaticValues
{
    public static class ResourcesKeys
    {
        public static readonly ResourcesKey MemoryReservation = new() { TaskKey = "memory_reservation", ArgoKey = "requests.memory" };

        public static readonly ResourcesKey CpuReservation = new() { TaskKey = "cpu_reservation", ArgoKey = "requests.cpu" };

        public static readonly ResourcesKey GpuLimit = new() { TaskKey = "gpu_limit", ArgoKey = "nvidia.com/gpu" };

        public static readonly ResourcesKey MemoryLimit = new() { TaskKey = "memory_limit", ArgoKey = "limits.memory" };

        public static readonly ResourcesKey CpuLimit = new() { TaskKey = "cpu_limit", ArgoKey = "limits.cpu" };
    }
}

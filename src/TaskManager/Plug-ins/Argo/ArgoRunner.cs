// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.TaskManager.API;

namespace Argo
{
    public class ArgoRunner : ITaskRunner
    {
        public Task ExecuteTask() => throw new NotImplementedException();
        public Task<ExecutionStatus> GetStatus(string taskId) => throw new NotImplementedException();
    }
}

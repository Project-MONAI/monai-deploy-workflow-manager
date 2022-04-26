// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Services
{
    public interface IWorkflowExecuterService
    {
        Task<bool> ProcessPayload(WorkflowRequestEvent message);
    }
}

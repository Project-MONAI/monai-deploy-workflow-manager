// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Common
{
    public interface IConditionalParameterParser
    {
        WorkflowInstance? WorkflowInstance { get; }

        string ResolveParameters(string conditions, WorkflowInstance workflowInstance);
        bool TryParse(string conditions, WorkflowInstance workflowInstance);
    }
}
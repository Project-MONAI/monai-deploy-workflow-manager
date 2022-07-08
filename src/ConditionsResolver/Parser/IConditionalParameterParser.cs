// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Parser
{
    public interface IConditionalParameterParser
    {
        string ResolveParameters(string conditions, WorkflowInstance workflowInstance);

        string ResolveParameters(string conditions, string workflowInstanceId);

        bool TryParse(string conditions, WorkflowInstance workflowInstance);
    }
}

/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Parser
{
    public interface IConditionalParameterParser
    {
        /// <summary>
        /// Resolves parameters in query string.
        /// </summary>
        /// <param name="conditions">The query string Example: {{ context.executions.other_task.'Fred' }}</param>
        /// <param name="workflowInstance">workflow instance to resolve metadata parameter</param>
        /// <returns></returns>
        string ResolveParameters(string conditions, WorkflowInstance workflowInstance);

        /// <summary>
        /// Resolves parameters in query string.
        /// </summary>
        /// <param name="conditions">The query string Example: {{ context.executions.other_task.'Fred' }}</param>
        /// <param name="workflowInstanceId">workflow instance id to resolve metadata parameter</param>
        /// <returns></returns>
        string ResolveParameters(string conditions, string workflowInstanceId);

        /// <summary>
        /// Verifies if an array of strings of conditions evaluates to true.
        /// </summary>
        /// <param name="conditions">An array of strings of conditions.</param>
        /// <param name="workflowInstance">The workflow instance of the task.</param>
        /// <param name="resolvedConditional">outputs the resolved conditional.</param>
        bool TryParse(string[] conditions, WorkflowInstance workflowInstance, out string? resolvedConditional);

        /// <summary>
        /// Verifies if a string of conditions evaluates to true.
        /// </summary>
        /// <param name="conditions">A string of conditions.</param>
        /// <param name="workflowInstance">The workflow instance of the task.</param>
        /// <param name="resolvedConditional">outputs the resolved conditional.</param>
        bool TryParse(string conditions, WorkflowInstance workflowInstance, out string resolvedConditional);
    }
}

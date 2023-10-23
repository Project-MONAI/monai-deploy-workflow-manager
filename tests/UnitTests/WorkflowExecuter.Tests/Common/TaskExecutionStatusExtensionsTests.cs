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

using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Common;
using Xunit;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecuter.Tests.Common
{
    public class TaskExecutionStatusExtensions
    {
        [Theory]
        [InlineData(TaskExecutionStatus.Succeeded, TaskExecutionStatus.Failed, false)]
        [InlineData(TaskExecutionStatus.Succeeded, TaskExecutionStatus.Created, false)]
        [InlineData(TaskExecutionStatus.Succeeded, TaskExecutionStatus.Canceled, false)]
        [InlineData(TaskExecutionStatus.Accepted, TaskExecutionStatus.Created, false)]
        [InlineData(TaskExecutionStatus.Dispatched, TaskExecutionStatus.Created, false)]
        [InlineData(TaskExecutionStatus.Canceled, TaskExecutionStatus.Created, false)]
        [InlineData(TaskExecutionStatus.Canceled, TaskExecutionStatus.Dispatched, false)]
        [InlineData(TaskExecutionStatus.Created, TaskExecutionStatus.Dispatched, true)]
        [InlineData(TaskExecutionStatus.Dispatched, TaskExecutionStatus.Succeeded, true)]
        [InlineData(TaskExecutionStatus.Dispatched, TaskExecutionStatus.Failed, true)]
        [InlineData(TaskExecutionStatus.Dispatched, TaskExecutionStatus.Canceled, true)]
        [InlineData(TaskExecutionStatus.Accepted, TaskExecutionStatus.Succeeded, true)]
        [InlineData(TaskExecutionStatus.Accepted, TaskExecutionStatus.Failed, true)]
        [InlineData(TaskExecutionStatus.PartialFail, TaskExecutionStatus.Succeeded, true)]
        [InlineData(TaskExecutionStatus.PartialFail, TaskExecutionStatus.Failed, true)]
        public void IsTaskExecutionStatusUpdateValid_ReturnesExpected(TaskExecutionStatus oldStatus, TaskExecutionStatus newStatus, bool expected) => Assert.Equal(expected, newStatus.IsTaskExecutionStatusUpdateValid(oldStatus));
    }
}

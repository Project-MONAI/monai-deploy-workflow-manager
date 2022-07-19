// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TaskUpdateStepDefinitions
    {
        public TaskUpdateStepDefinitions(ObjectContainer objectContainer, ISpecFlowOutputHelper outputHelper)
        {
            _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
            DataHelper = objectContainer.Resolve<DataHelper>() ?? throw new ArgumentNullException(nameof(DataHelper));
            Assertions = new Assertions(_outputHelper);
        }

        private readonly ISpecFlowOutputHelper _outputHelper;
        public DataHelper DataHelper { get; }
        public Assertions Assertions { get; }

        [Then(@"A Task Update event with status (.*) is published with Task Dispatch details")]
        public void ATaskUpdateEventIsPublishedWithTaskDispatchDetails(string status)
        {
            var taskUpdateEvent = DataHelper.GetTaskUpdateEvent();

            switch (status.ToLower())
            {
                case "accepted":
                    Assertions.AssertTaskUpdateEventFromTaskDispatch(taskUpdateEvent, DataHelper.TaskDispatchEvent, TaskExecutionStatus.Accepted);
                    break;
                case "failed":
                    Assertions.AssertTaskUpdateEventFromTaskDispatch(taskUpdateEvent, DataHelper.TaskDispatchEvent, TaskExecutionStatus.Failed);
                    break;
                default:
                    throw new Exception($"Status {status} is not supported! Please check and try again!");
            }
        }

        [Then(@"A Task Update event with status (.*) is published with Task Callback details")]
        public void ATaskUpdateEventIsPublishedWithTaskCallbackDetails(string status)
        {
            var taskUpdateEvent = DataHelper.GetTaskUpdateEvent();

            switch (status.ToLower())
            {
                case "succeeded":
                    Assertions.AssertTaskUpdateEventFromTaskCallback(taskUpdateEvent, DataHelper.TaskCallbackEvent, TaskExecutionStatus.Succeeded);
                    break;
                case "failed":
                    Assertions.AssertTaskUpdateEventFromTaskCallback(taskUpdateEvent, DataHelper.TaskCallbackEvent, TaskExecutionStatus.Failed);
                    break;
                default:
                    throw new Exception($"Status {status} is not supported! Please check and try again!");
            }
        }

    }
}

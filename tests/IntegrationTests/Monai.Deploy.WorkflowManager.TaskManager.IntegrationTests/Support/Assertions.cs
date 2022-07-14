// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Events;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests
{
    public class Assertions
    {
        public Assertions(ISpecFlowOutputHelper output)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
        }

        private ISpecFlowOutputHelper Output { get; set; }

        public void AssertClinicalReviewEvent(ClinicalReviewRequestEvent clinicalReviewRequestEvent, TaskDispatchEvent taskDispatchEvent)
        {
            Output.WriteLine("Asserting details of ClinicalReviewRequestEvent with TaskDispatchEvent");
            clinicalReviewRequestEvent.ExecutionId.Should().Be(taskDispatchEvent.ExecutionId);
            clinicalReviewRequestEvent.CorrelationId.Should().Be(taskDispatchEvent.CorrelationId);
            clinicalReviewRequestEvent.TaskId.Should().Be(taskDispatchEvent.TaskId);
            clinicalReviewRequestEvent.PatientMetadata.PatientId.Should().Be(GetTaskPluginArguments(taskDispatchEvent, "patient_id"));
            clinicalReviewRequestEvent.PatientMetadata.PatientName.Should().Be(GetTaskPluginArguments(taskDispatchEvent, "patient_name"));
            clinicalReviewRequestEvent.PatientMetadata.PatientSex.Should().Be(GetTaskPluginArguments(taskDispatchEvent, "patient_sex"));
            clinicalReviewRequestEvent.PatientMetadata.PatientDob.Should().Be(GetTaskPluginArguments(taskDispatchEvent, "patient_dob"));
            clinicalReviewRequestEvent.WorkflowName.Should().Be(GetTaskPluginArguments(taskDispatchEvent, "workflow_name"));

            foreach (var file in clinicalReviewRequestEvent.Files)
            {
                var taskDispatchFile = taskDispatchEvent.Inputs.FirstOrDefault(x => x.Name.Equals(file.Name));

                file.Name.Should().Be(taskDispatchFile?.Name);
                file.Endpoint.Should().Be(taskDispatchFile?.Endpoint);
                file.Credentials.Should().BeEquivalentTo(taskDispatchFile?.Credentials);
                file.Bucket.Should().Be(taskDispatchFile?.Bucket);
                file.RelativeRootPath.Should().Be(taskDispatchFile?.RelativeRootPath);
            }
            Output.WriteLine("Details of ClinicalReviewRequestEvent matches TaskDispatchEvent");
        }

        public void AssertTaskUpdateEventFromTaskDispatch(TaskUpdateEvent taskUpdateEvent, TaskDispatchEvent taskDispatchEvent, TaskExecutionStatus status)
        {
            Output.WriteLine("Asserting details of TaskUpdateEvent with TaskDispatchEvent");
            taskUpdateEvent.ExecutionId.Should().Be(taskDispatchEvent.ExecutionId);
            // BUG taskUpdateEvent.CorrelationId.Should().Be(taskDispatchEvent.CorrelationId);
            taskUpdateEvent.Status.Should().Be(status);
            taskUpdateEvent.TaskId.Should().Be(taskDispatchEvent.TaskId);
            taskUpdateEvent.WorkflowInstanceId.Should().Be(taskDispatchEvent.WorkflowInstanceId);
            Output.WriteLine("Details of TaskUpdateEvent matches TaskDispatchEvent");
        }

        public void TaskUpdateEventFromTaskCallback(TaskUpdateEvent taskUpdateEvent, TaskCallbackEvent taskCallbackEvent)
        {
            // TODO
        }

        private string GetTaskPluginArguments(TaskDispatchEvent taskDispatchEvent, string key)
        {
            string? dictValue;

            taskDispatchEvent.TaskPluginArguments.TryGetValue(key, out dictValue);

            return dictValue;
        }
    }
}

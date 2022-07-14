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
            Output.WriteLine("Asserting details of clinical review request event match task dispatch event");
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
            Output.WriteLine("Details of clinical review request match task dispatch event");
        }

        private string GetTaskPluginArguments(TaskDispatchEvent taskDispatchEvent, string key)
        {
            string? dictValue;

            taskDispatchEvent.TaskPluginArguments.TryGetValue(key, out dictValue);

            return dictValue;
        }
    }
}

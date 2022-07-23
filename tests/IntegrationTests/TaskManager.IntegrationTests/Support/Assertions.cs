/*
 * Copyright 2021-2022 MONAI Consortium
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
                file.Credentials.Should().NotBeNull();
                file.Bucket.Should().Be(taskDispatchFile?.Bucket);
                file.RelativeRootPath.Should().Be(taskDispatchFile?.RelativeRootPath);
            }
            Output.WriteLine("Details of ClinicalReviewRequestEvent matches TaskDispatchEvent");
        }

        public void AssertTaskUpdateEventFromTaskCallback(TaskUpdateEvent taskUpdateEvent, TaskCallbackEvent taskCallbackEvent, TaskExecutionStatus status)
        {
            Output.WriteLine("Asserting details of TaskUpdateEvent with TaskCallbackEvent");
            taskUpdateEvent.ExecutionId.Should().Be(taskCallbackEvent.ExecutionId);
            taskUpdateEvent.CorrelationId.Should().Be(taskCallbackEvent.CorrelationId);
            taskUpdateEvent.Status.Should().Be(status);
            taskUpdateEvent.TaskId.Should().Be(taskCallbackEvent.TaskId);
            taskUpdateEvent.WorkflowInstanceId.Should().Be(taskCallbackEvent.WorkflowInstanceId);

            foreach (var dict in taskCallbackEvent.Metadata)
            {
                object updateMetadataValue;
                taskUpdateEvent.Metadata.TryGetValue(dict.Key, out updateMetadataValue);
                updateMetadataValue.Should().Be(dict.Value);
            }

            Output.WriteLine("Details of TaskUpdateEvent matches TaskCallbackEvent");
        }

        public void AssertTaskUpdateEventFromTaskDispatch(TaskUpdateEvent taskUpdateEvent, TaskDispatchEvent taskDispatchEvent, TaskExecutionStatus status)
        {
            Output.WriteLine("Asserting details of TaskUpdateEvent with TaskDispatchEvent");
            taskUpdateEvent.ExecutionId.Should().Be(taskDispatchEvent.ExecutionId);
            taskUpdateEvent.CorrelationId.Should().Be(taskDispatchEvent.CorrelationId); // - BUG 227 raised
            taskUpdateEvent.Status.Should().Be(status);
            taskUpdateEvent.TaskId.Should().Be(taskDispatchEvent.TaskId);
            taskUpdateEvent.WorkflowInstanceId.Should().Be(taskDispatchEvent.WorkflowInstanceId);
            Output.WriteLine("Details of TaskUpdateEvent matches TaskDispatchEvent");
        }

        private string GetTaskPluginArguments(TaskDispatchEvent taskDispatchEvent, string key)
        {
            string? dictValue;

            taskDispatchEvent.TaskPluginArguments.TryGetValue(key, out dictValue);

            return dictValue;
        }
    }
}

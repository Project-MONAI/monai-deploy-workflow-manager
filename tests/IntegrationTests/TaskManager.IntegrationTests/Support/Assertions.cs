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

using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Events;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support
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
            clinicalReviewRequestEvent.PatientMetadata!.PatientId.Should().Be(GetTaskPluginArguments(taskDispatchEvent, "patient_id"));
            clinicalReviewRequestEvent.PatientMetadata!.PatientName.Should().Be(GetTaskPluginArguments(taskDispatchEvent, "patient_name"));
            clinicalReviewRequestEvent.PatientMetadata!.PatientSex.Should().Be(GetTaskPluginArguments(taskDispatchEvent, "patient_sex"));
            clinicalReviewRequestEvent.PatientMetadata!.PatientDob.Should().Be(GetTaskPluginArguments(taskDispatchEvent, "patient_dob"));

            if (Boolean.TryParse(GetTaskPluginArguments(taskDispatchEvent, "notifications"), out bool result))
            {
                clinicalReviewRequestEvent.Notifications.Should().Be(result);
            }
            else
            {
                clinicalReviewRequestEvent.Notifications.Should().Be(true);
            }

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

        public void AssertEmailEvent(EmailRequestEvent emailRequestEvent, TaskDispatchEvent taskDispatchEvent)
        {
            Output.WriteLine("Asserting details of EmailRequestEvent with TaskDispatchEvent");
            emailRequestEvent.TaskId.Should().Be(taskDispatchEvent.TaskId);
            emailRequestEvent.WorkflowInstanceId.Should().Be(taskDispatchEvent.WorkflowInstanceId);
            emailRequestEvent.WorkflowName.Should().Be(GetTaskPluginArguments(taskDispatchEvent, "workflow_name", true));
            emailRequestEvent.Emails.Should().Be(GetTaskPluginArguments(taskDispatchEvent, "recipient_emails", true));
            emailRequestEvent.Roles.Should().Be(GetTaskPluginArguments(taskDispatchEvent, "recipient_roles", true));
            string.Join(',', emailRequestEvent.Metadata.Keys).Should().Be(GetTaskPluginArguments(taskDispatchEvent, "metadata_values"));
            Output.WriteLine("Details of EmailRequestEvent matches TaskDispatchEvent");
        }

        public void AssertTaskDispatchEventStoredInMongo(List<TaskDispatchEventInfo> storedTaskDispatchEvent, TaskDispatchEvent taskDispatchEvent)
        {
            Output.WriteLine("Asserting details of TaskDispatchEvent stored in Mongo");
            storedTaskDispatchEvent.Should().NotBeNullOrEmpty();
            storedTaskDispatchEvent.Count.Should().Be(1);

            // Remove AccessKey, AccessToken & SessionToken as they are modified by Task Manager
            storedTaskDispatchEvent.ForEach(e =>
            {
                e.Event.Inputs?.ForEach(i => i.Credentials = null);
                e.Event.Outputs?.ForEach(i => i.Credentials = null);
                if (e.Event.IntermediateStorage is not null)
                {
                    e.Event.IntermediateStorage.Credentials = null;
                }
            });
            taskDispatchEvent.Inputs.ForEach(i => i.Credentials = null);
            taskDispatchEvent.Outputs.ForEach(i => i.Credentials = null);
            if (taskDispatchEvent.IntermediateStorage is not null)
            {
                taskDispatchEvent.IntermediateStorage.Credentials = null;
            }

            storedTaskDispatchEvent[0].Event.Should().BeEquivalentTo(taskDispatchEvent);
            Output.WriteLine("Details of TaskDispatchEvent stored matches original TaskDispatchEvent");
        }

        internal void AssertTaskDispatchEventDeletedInMongo(List<TaskDispatchEventInfo> storedTaskDispatchEvent)
        {
            Output.WriteLine("Asserting details of TaskDispatchEvent deleted in Mongo");
            storedTaskDispatchEvent.Should().BeNullOrEmpty();
            Output.WriteLine("Details of TaskDispatchEvent deleted matches original TaskDispatchEvent");
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
                if (dict.Key == "reviewer_roles")
                {
                    continue;
                }

                taskUpdateEvent.Metadata.TryGetValue(dict.Key, out var updateMetadataValue);
                updateMetadataValue.Should().Be(dict.Value);
            }

            Output.WriteLine("Details of TaskUpdateEvent matches TaskCallbackEvent");
        }

        private string? GetTaskPluginArguments(TaskDispatchEvent taskDispatchEvent, string key, bool emptyIfNull = false)
        {
            taskDispatchEvent.TaskPluginArguments.TryGetValue(key, out var dictValue);

            return emptyIfNull ? dictValue ?? string.Empty : dictValue;
        }

        public void AssertTaskUpdateEventFromTaskDispatch(TaskUpdateEvent taskUpdateEvent, TaskDispatchEvent taskDispatchEvent, TaskExecutionStatus status)
        {
            Output.WriteLine("Asserting details of TaskUpdateEvent with TaskDispatchEvent");
            taskUpdateEvent.ExecutionId.Should().Be(taskDispatchEvent.ExecutionId);
            taskUpdateEvent.CorrelationId.Should().Be(taskDispatchEvent.CorrelationId);
            taskUpdateEvent.Status.Should().Be(status);
            taskUpdateEvent.TaskId.Should().Be(taskDispatchEvent.TaskId);
            taskUpdateEvent.WorkflowInstanceId.Should().Be(taskDispatchEvent.WorkflowInstanceId);
            Output.WriteLine("Details of TaskUpdateEvent matches TaskDispatchEvent");
        }
    }
}

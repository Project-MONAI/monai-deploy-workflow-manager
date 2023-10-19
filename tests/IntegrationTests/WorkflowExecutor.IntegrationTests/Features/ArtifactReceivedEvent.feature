# Copyright 2022 MONAI Consortium
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

@IntergrationTests
Feature: ArtifactReceivedEvent

Publishing a artifact received event is consumed by the Workflow Manager.

    @ArtifactReceivedEvent
    Scenario Outline: Publish a valid Artifact Received Event which creates an entry.
        Given I have a clinical workflow <clinicalWorkflow> I have a Workflow Instance <workflowInstance>
        When I publish a Artifact Received Event <artifactReceivedEvent>
        Then I can see 2 Artifact Received Items is created
        Examples:
          | clinicalWorkflow                               | workflowInstance                               | artifactReceivedEvent |
          | Workflow_Revision_For_Artifact_ReceivedEvent_1 | Workflow_Instance_For_Artifact_ReceivedEvent_1 | Test1                 |

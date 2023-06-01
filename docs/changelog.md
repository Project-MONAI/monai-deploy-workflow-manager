<!--
  ~ Copyright 2022 MONAI Consortium
  ~
  ~ Licensed under the Apache License, Version 2.0 (the "License");
  ~ you may not use this file except in compliance with the License.
  ~ You may obtain a copy of the License at
  ~
  ~ http://www.apache.org/licenses/LICENSE-2.0
  ~
  ~ Unless required by applicable law or agreed to in writing, software
  ~ distributed under the License is distributed on an "AS IS" BASIS,
  ~ WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  ~ See the License for the specific language governing permissions and
  ~ limitations under the License.
-->


# Changelog

Added a new Email plugin, this pulls together information and sends a Message (rabbitmq) to a queue for processing via an external application (for the actual sending)

Renamed the (Generated) Argo client to ArgoGeneratedClient, added a new ArgoClient using just the methods used by this codebase.

Enhanced the ArgoClient -> Argo_Get_WorkflowLogsAsync method to decode the json better and make the logs extracted from Argo more readable.

Added Mongo Migraions, to allow changes without breaking current stored entries

Added resource constraints to the generated ARGO templates, so these can operate within a Kubernetes cluster with a Resource Quota in the ARGO namespace.

Added ability for plugins (currently Argo) to have controllers, any dll marked with the new Monai.Deploy.WorkflowManager.Shared.PlugInAttribute will have any controllers added to TaskManager

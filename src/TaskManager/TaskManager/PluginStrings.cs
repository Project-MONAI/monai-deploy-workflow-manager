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

namespace Monai.Deploy.WorkflowManager.TaskManager
{
#pragma warning disable SA1600 // Elements should be documented
    public static class PluginStrings
    {
        // note not to be confused with the ApplicationId Monai.Deploy.WorkflowManager.TaskManager.Argo
        public const string Argo = "argo";

        public const string Docker = "docker";

        public static readonly IReadOnlyList<string> PlugsRequiresPermanentAccounts = new List<string>() { Argo, Docker };
    }
}
#pragma warning restore SA1600 // Elements should be documented

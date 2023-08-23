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

namespace Monai.Deploy.WorkflowManager.Common.ConditionsResolver.Constants
{
    public static class ParameterConstants
    {
        public const string TaskId = "task_id";
        public const string Status = "status";
        public const string ExecutionId = "execution_id";
        public const string OutputDirectory = "output_dir";
        public const string TaskType = "task_type";
        public const string PreviousTaskId = "previous_task_id";
        public const string ErrorMessage = "error_msg";
        public const string Result = "result";
        public const string ExecutionStats = "execution_stats";
        public const string StartTime = "start_time";
        public const string EndTime = "end_time";

        public const string Name = "name";
        public const string Description = "description";

        public const string PatientId = "id";
        public const string PatientName = "name";
        public const string PatientSex = "sex";
        public const string PatientDob = "dob";
        public const string PatientAge = "age";
        public const string PatientHospitalId = "hospital_id";
    }
}

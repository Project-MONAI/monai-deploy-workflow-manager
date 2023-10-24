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

using System.Globalization;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.WorkflowExecuter.Extensions
{
    public static class TaskExecutionExtension
    {
        /// <summary>
        /// Attaches patient metadata to task execution plugin arguments.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="patientDetails"></param>
        /// <param name="logger">Logging Method to log details.</param>
        public static void AttachPatientMetaData(this TaskExecution task, PatientDetails patientDetails, Action<string>? logger)
        {
            var attachedData = false;
            if (string.IsNullOrWhiteSpace(patientDetails.PatientId) is false)
            {
                attachedData = task.TaskPluginArguments.TryAdd(PatientKeys.PatientId, patientDetails.PatientId);
            }
            if (string.IsNullOrWhiteSpace(patientDetails.PatientAge) is false)
            {
                attachedData = task.TaskPluginArguments.TryAdd(PatientKeys.PatientAge, patientDetails.PatientAge);
            }
            if (string.IsNullOrWhiteSpace(patientDetails.PatientSex) is false)
            {
                attachedData = task.TaskPluginArguments.TryAdd(PatientKeys.PatientSex, patientDetails.PatientSex);
            }
            var patientDob = patientDetails.PatientDob;
            if (patientDob.HasValue)
            {
                attachedData = task.TaskPluginArguments.TryAdd(PatientKeys.PatientDob, patientDob.Value.ToString("o", CultureInfo.InvariantCulture));
            }
            if (string.IsNullOrWhiteSpace(patientDetails.PatientHospitalId) is false)
            {
                attachedData = task.TaskPluginArguments.TryAdd(PatientKeys.PatientHospitalId, patientDetails.PatientHospitalId);
            }
            if (string.IsNullOrWhiteSpace(patientDetails.PatientName) is false)
            {
                attachedData = task.TaskPluginArguments.TryAdd(PatientKeys.PatientName, patientDetails.PatientName);
            }
            if (attachedData && logger is not null)
            {
                logger(JsonConvert.SerializeObject(task.TaskPluginArguments));
            }
        }
    }
}

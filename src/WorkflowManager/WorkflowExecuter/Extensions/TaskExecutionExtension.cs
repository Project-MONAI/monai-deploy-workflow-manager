using System.Globalization;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.WorkfowExecuter.Extensions
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

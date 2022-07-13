namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview
{
    public class Keys
    {
        /// <summary>
        /// Key for the patient id.
        /// </summary>
        public static readonly string PatientId = "patient_id";

        /// <summary>
        /// Key for the patient name.
        /// </summary>
        public static readonly string PatientName = "patient_name";

        /// <summary>
        /// Key for the patient sex.
        /// </summary>
        public static readonly string PatientSex = "patient_sex";

        /// <summary>
        /// Key for the patient dob.
        /// </summary>
        public static readonly string PatientDob = "patient_dob";

        /// <summary>
        /// Key for the workflow name.
        /// </summary>
        public static readonly string WorkflowName = "workflow_name";

        /// <summary>
        /// Key for the reviewed task details.
        /// </summary>
        public static readonly string ReviewedTaskDetails = "reviewed_task_details";

        /// <summary>
        /// Key for the queue name to send the clinical review message.
        /// </summary>
        public static readonly string QueueName = "queue_name";

        /// <summary>
        /// Required arguments to run the Argo workflow.
        /// </summary>
        public static readonly IReadOnlyList<string> RequiredParameters =
            new List<string> {
                PatientId,
                PatientName,
                PatientSex,
                PatientDob,
                QueueName,
                WorkflowName,
                ReviewedTaskDetails
            };
    }
}

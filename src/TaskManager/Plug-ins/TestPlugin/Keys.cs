namespace Monai.Deploy.WorkflowManager.TaskManager.TestPlugin
{
    internal static class Keys
    {
        /// <summary>
        /// Key for the status to return.
        /// </summary>
        public static readonly string ExecuteTaskStatus = "executetaskstatus";

        /// <summary>
        /// Key for the status to return.
        /// </summary>
        public static readonly string GetStatusStatus = "getstatusstatus";

        /// <summary>
        /// Required arguments to run the Argo workflow.
        /// </summary>
        public static readonly IReadOnlyList<string> RequiredParameters =
            new List<string> {
                ExecuteTaskStatus
            };
    }
}

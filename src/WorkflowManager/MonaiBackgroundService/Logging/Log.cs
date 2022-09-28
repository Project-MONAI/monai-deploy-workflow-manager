namespace Monai.Deploy.WorkflowManager.MonaiBackgroundService.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Task {taskId} started at {startTime} and been running for {duration}, Timing out task. ExecutionId: {executionId}, CorrelationId: {correlationId}")]
        public static partial void TimingOutTask(this ILogger logger, string taskId, string startTime, string duration, string executionId, string correlationId);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "CancellationEvent triggered, Identity: {identity}, WorkflowInstanceId: {workflowInstanceId}")]
        public static partial void TimingOutTaskCancellationEvent(this ILogger logger, string identity, string workflowInstanceId);
    }

}

using Microsoft.Extensions.Logging;

namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Logging
{
    public static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Error sending message {eventType}.")]
        public static partial void ErrorSendingMessage(this ILogger logger, string eventType, Exception ex);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Sending {eventType}, Name={name} .")]
        public static partial void SendClinicalReviewRequestMessage(this ILogger logger, string eventType, string name);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "{eventType} sent.")]
        public static partial void SendClinicalReviewRequestMessageSent(this ILogger logger, string eventType);
    }
}

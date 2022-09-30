namespace Monai.Deploy.WorkflowManager.Common.Exceptions
{
    public class MonaiBadRequestException : Exception
    {
        public MonaiBadRequestException()
        {
        }

        public MonaiBadRequestException(string message)
            : base(message)
        {
        }

        public MonaiBadRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

namespace Monai.Deploy.WorkflowManager.Common.Exceptions
{
    public class MonaiNotFoundException : Exception
    {
        public MonaiNotFoundException()
        {
        }

        public MonaiNotFoundException(string message)
            : base(message)
        {
        }

        public MonaiNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

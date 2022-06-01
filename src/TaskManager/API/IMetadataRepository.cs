namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    public interface IMetadataRepository : IDisposable
    {
        /// <summary>
        /// Retrieves metadata for the current plugin.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="Task<Dictionary<string, object>>"/>metadata dictionary</returns>
        Task<Dictionary<string, object>> RetrieveMetadata(CancellationToken cancellationToken = default);
    }
}

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.API;
using Monai.Deploy.WorkflowManager.TaskManager.API;

namespace Monai.Deploy.WorkflowManager.TaskManager.AideClinicalReview.Repositories
{
    public sealed class AideClinicalReviewMetadataRepository : MetadataRepositoryBase
    {
        private readonly IServiceScope _scope;
        private readonly ILogger<AideClinicalReviewMetadataRepository> _logger;

        private readonly TaskDispatchEvent _taskDispatchEvent;

        public AideClinicalReviewMetadataRepository(
            IServiceScopeFactory serviceScopeFactory,
            IStorageService storageService,
            ILogger<AideClinicalReviewMetadataRepository> logger,
            TaskDispatchEvent taskDispatchEvent,
            TaskCallbackEvent taskCallbackEvent)
            : base(taskDispatchEvent, taskCallbackEvent)
        {
            Guard.Against.Null(serviceScopeFactory, nameof(serviceScopeFactory));

            _scope = serviceScopeFactory.CreateScope();

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taskDispatchEvent = taskDispatchEvent;

            Validate();
        }

        private void Validate()
        {
            Guard.Against.Null(DispatchEvent);
            Guard.Against.Null(CallbackEvent);

            Guard.Against.NullOrWhiteSpace(DispatchEvent.WorkflowInstanceId);
            Guard.Against.NullOrWhiteSpace(DispatchEvent.ExecutionId);
            Guard.Against.NullOrWhiteSpace(DispatchEvent.PayloadId);
        }

        public override Task<Dictionary<string, object>> RetrieveMetadata(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_taskDispatchEvent.Metadata);
        }
    }
}

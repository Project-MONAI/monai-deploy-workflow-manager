using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.WorkflowManager.Configuration;
using RabbitMQ.Client;

namespace Monai.Deploy.WorkflowManager.HealthChecks
{
    public static class HealthChecksExtensions
    {
        public static void AddMonaiHealthChecks(this IServiceCollection services, IOptions<WorkloadManagerDatabaseSettings> dbSettings, IConnectionFactory subscriberQueueFactory, IConnectionFactory publisherQueueFactory, HealthStatus failiureStatus) =>
            services.AddHealthChecks()
                        .AddMongoDb(
                            dbSettings.Value.ConnectionString,
                            HealthCheckSettings.DatabaseHealthCheckName,
                            failiureStatus,
                            HealthCheckSettings.DatabaseTags,
                            HealthCheckSettings.DatabaseHealthCheckTimeout)
                        .AddRabbitMQ(
                            _ => subscriberQueueFactory,
                            HealthCheckSettings.SubscriberQueueHealthCheckName,
                            failiureStatus,
                            HealthCheckSettings.SubscriberQueueTags,
                            HealthCheckSettings.SubscriberQueueHealthCheckTimeout)
                        .AddRabbitMQ(
                            _ => publisherQueueFactory,
                            HealthCheckSettings.PublisherQueueHealthCheckName,
                            failiureStatus,
                            HealthCheckSettings.PublisherQueueTags,
                            HealthCheckSettings.PublisherQueueHealthCheckTimeout);


        public static void GetRequiredServicesForHealthChecks(
            this IServiceCollection services,
            out IOptions<WorkloadManagerDatabaseSettings> dbSettings,
            out IConnectionFactory subscriberQueueFactory,
            out IConnectionFactory publisherQueueFactory)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8601 // Possible null reference assignment.
            // Should blow up in configuration is wrong.
            var sp = services.BuildServiceProvider();
            dbSettings = sp.GetService<IOptions<WorkloadManagerDatabaseSettings>>();
            var messageBrokerConfig = sp.GetService<IOptions<MessageBrokerServiceConfiguration>>();

            subscriberQueueFactory = RabbitHealthCheckFactory.Create(messageBrokerConfig.Value.SubscriberSettings);
            publisherQueueFactory = RabbitHealthCheckFactory.Create(messageBrokerConfig.Value.PublisherSettings);
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
    }
}

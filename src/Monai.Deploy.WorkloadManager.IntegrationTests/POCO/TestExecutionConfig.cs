namespace Monai.Deploy.WorkloadManager.IntegrationTests.POCO
{
    static class TestExecutionConfig
    {
        public static class RabbitConfig
        {
            public static string Host { get; set; }

            public static int Port { get; set; }

            public static string User { get; set; }

            public static string Password { get; set; }

            public static string ConsumerQueue { get; set; }

            public static string PublisherQueue { get; set; }
        }

        public static class MongoConfig
        {
            public static string Host { get; set; }

            public static int Port { get; set; }

            public static string User { get; set; }

            public static string Password { get; set; }
        }
    }
}

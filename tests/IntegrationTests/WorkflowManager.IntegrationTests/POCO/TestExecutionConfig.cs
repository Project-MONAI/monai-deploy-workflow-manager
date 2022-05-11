// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.IntegrationTests.POCO
{
    static class TestExecutionConfig
    {
        public static class RabbitConfig
        {
            public static string Host { get; set; }

            public static int Port { get; set; }

            public static string User { get; set; }

            public static string Password { get; set; }

            public static string Exchange { get; set; }

            public static string VirtualHost { get; set; }

            public static string WorkflowRequestQueue { get; set; }

            public static string TaskDispatchQueue { get; set; }

            public static string TaskCallbackQueue { get; set; }

            public static string WorkflowCompleteQueue { get; set; }
        }

        public static class MongoConfig
        {
            public static string ConnectionString { get; set; }

            public static int Port { get; set; }

            public static string User { get; set; }

            public static string Password { get; set; }

            public static string Database { get; set; }

            public static string WorkflowCollection { get; set; }

            public static string WorkflowInstanceCollection { get; set; }
        }
    }
}

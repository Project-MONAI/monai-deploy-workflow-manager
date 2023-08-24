/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.POCO
{
    internal static class TestExecutionConfig
    {
        public static class RabbitConfig
        {
            public static string? Host { get; set; }

            public static int WebPort { get; set; } = 15672;

            public static int Port { get; set; } = 5672;

            public static string? User { get; set; }

            public static string? Password { get; set; }

            public static string? Exchange { get; set; }

            public static string? VirtualHost { get; set; }

            public static string? WorkflowRequestQueue { get; set; }

            public static string? TaskDispatchQueue { get; set; }

            public static string? TaskCallbackQueue { get; set; }

            public static string? WorkflowCompleteQueue { get; set; }

            public static string? TaskUpdateQueue { get; set; }

            public static string? ClinicalReviewQueue { get; set; }

            public static string? EmailQueue { get; set; }

            public static object? TaskCancellationQueue { get; set; }
        }

        public static class ApiConfig
        {
            public static string? TaskManagerBaseUrl { get; set; }
        }

        public static class MongoConfig
        {
            public static string? ConnectionString { get; set; }

            public static int Port { get; set; }

            public static string? User { get; set; }

            public static string? Password { get; set; }

            public static string? Database { get; set; }

            public static string? TaskDispatchEventCollection { get; set; }

            public static string? ExecutionStatsCollection { get; set; }
        }

        public static class MinioConfig
        {
            public static string? Endpoint { get; set; }

            public static string? AccessKey { get; set; }

            public static string? AccessToken { get; set; }

            public static string? Bucket { get; set; }

            public static string? Region { get; set; }
        }
    }
}

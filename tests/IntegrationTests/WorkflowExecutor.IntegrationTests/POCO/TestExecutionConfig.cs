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

namespace Monai.Deploy.WorkflowManager.Common.IntegrationTests.POCO
{
    internal static class TestExecutionConfig
    {
        public static class RabbitConfig
        {
            public static string Host { get; set; } = string.Empty;

            public static int WebPort { get; set; } = 15672;

            public static int Port { get; set; } = 5672;

            public static string User { get; set; } = string.Empty;

            public static string Password { get; set; } = string.Empty;

            public static string Exchange { get; set; } = string.Empty;

            public static string VirtualHost { get; set; } = string.Empty;

            public static string WorkflowRequestQueue { get; set; } = string.Empty;

            public static string ArtifactsRequestQueue { get; set; } = string.Empty;

            public static string TaskDispatchQueue { get; set; } = string.Empty;

            public static string TaskCallbackQueue { get; set; } = string.Empty;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            public static string ExportCompleteQueue { get; set; }

            public static string ExportRequestQueue { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            public static string TaskUpdateQueue { get; set; } = string.Empty;
        }

        public static class MongoConfig
        {
            public static string ConnectionString { get; set; } = string.Empty;

            public static int Port { get; set; }

            public static string User { get; set; } = string.Empty;

            public static string Password { get; set; } = string.Empty;

            public static string Database { get; set; } = string.Empty;

            public static string WorkflowCollection { get; set; } = string.Empty;

            public static string WorkflowInstanceCollection { get; set; } = string.Empty;

            public static string PayloadCollection { get; set; } = string.Empty;

            public static string ArtifactsCollection { get; set; } = string.Empty;

            public static string ExecutionStatsCollection { get; set; } = string.Empty;
        }

        public static class MinioConfig
        {
            public static string Endpoint { get; set; } = string.Empty;

            public static string AccessKey { get; set; } = string.Empty;

            public static string AccessToken { get; set; } = string.Empty;

            public static string Bucket { get; set; } = string.Empty;

            public static string Region { get; set; } = string.Empty;
        }

        public static class ApiConfig
        {
            public static string BaseUrl { get; set; } = string.Empty;
        }
    }
}

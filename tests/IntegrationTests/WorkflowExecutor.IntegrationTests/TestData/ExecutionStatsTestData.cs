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

using Monai.Deploy.WorkflowManager.Common.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Common.WorkflowExecutor.IntegrationTests.TestData
{
    public class ExecutionStatTestData
    {
        public string? Name { get; set; }

        public ExecutionStats? ExecutionStats { get; set; }
    }

    public static class ExecutionStatsTestData
    {
        public static List<ExecutionStatTestData> TestData = new List<ExecutionStatTestData>()
        {
            new ExecutionStatTestData()
            {
                Name = "Execution_Stats_Task_1_Accepted",
                ExecutionStats = new ExecutionStats()
                {
                    Id = Guid.NewGuid(),
                    CorrelationId = "b9b7d802-eb19-45dc-91ec-850b2ac0665c",
                    ExecutionId = "6c9993a8-639d-4a68-b689-d59e26b187e8",
                    WorkflowInstanceId = "Workflow_1",
                    TaskId = "Task_1",
                    Status = "Accepted",
                    LastUpdatedUTC = DateTime.UtcNow,
                    StartedUTC = DateTime.UtcNow
                }
            },
            new ExecutionStatTestData()
            {
                Name = "Execution_Stats_Task_1_Succeeded_1",
                ExecutionStats = new ExecutionStats()
                {
                    Id = Guid.NewGuid(),
                    CorrelationId = "a332a1bc-b1fd-4034-9f3f-72da68fa089e",
                    ExecutionId = "9f545b20-abf6-4c70-b64e-52c68ece8e37",
                    WorkflowInstanceId = "Workflow_1",
                    TaskId = "Task_1",
                    Status = "Succeeded",
                    LastUpdatedUTC = DateTime.UtcNow.AddDays(-60),
                    StartedUTC = DateTime.UtcNow.AddDays(-60),
                    CompletedAtUTC = DateTime.UtcNow.AddDays(-59).AddHours(-23).AddSeconds(-30),
                    DurationSeconds = 30,
                    ExecutionTimeSeconds = 30,
                }
            },
            new ExecutionStatTestData()
            {
                Name = "Execution_Stats_Task_1_Succeeded_2",
                ExecutionStats = new ExecutionStats()
                {
                    Id = Guid.NewGuid(),
                    CorrelationId = "5d84094c-b0d0-4df6-8298-4201ff75340d",
                    ExecutionId = "83c734a4-a511-46ad-9384-7dbf54a2c696",
                    WorkflowInstanceId = "Workflow_1",
                    TaskId = "Task_1",
                    Status = "Succeeded",
                    LastUpdatedUTC = DateTime.UtcNow.AddDays(-60),
                    StartedUTC = DateTime.UtcNow.AddDays(-60),
                    CompletedAtUTC = DateTime.UtcNow.AddDays(-59).AddHours(-23).AddSeconds(-30),
                    DurationSeconds = 30,
                    ExecutionTimeSeconds = 30,
                }
            },
            new ExecutionStatTestData()
            {
                Name = "Execution_Stats_Task_1_Succeeded_3",
                ExecutionStats = new ExecutionStats()
                {
                    Id = Guid.NewGuid(),
                    CorrelationId = "5d84094c-b0d0-4df6-8298-4201ff75340d",
                    ExecutionId = "83c734a4-a511-46ad-9384-7dbf54a2c696",
                    WorkflowInstanceId = "Workflow_1",
                    TaskId = "Task_1",
                    Status = "Succeeded",
                    LastUpdatedUTC = DateTime.UtcNow.AddDays(-30),
                    StartedUTC = DateTime.UtcNow.AddDays(-30),
                    CompletedAtUTC = DateTime.UtcNow.AddDays(-29).AddHours(-23).AddSeconds(-30),
                    DurationSeconds = 30,
                    ExecutionTimeSeconds = 30,

                }
            },
            new ExecutionStatTestData()
            {
                Name = "Execution_Stats_Task_1_Failed",
                ExecutionStats = new ExecutionStats()
                {
                    Id = Guid.NewGuid(),
                    CorrelationId = "8fda262e-a6dd-46d1-ab8e-592b6442c607",
                    ExecutionId = "73127fd4-20cb-42c1-8138-01db8b0d35d6",
                    WorkflowInstanceId = "Workflow_1",
                    TaskId = "Task_1",
                    Status = "Failed",
                    LastUpdatedUTC = DateTime.UtcNow.AddDays(-60),
                    StartedUTC = DateTime.UtcNow.AddDays(-60),
                    CompletedAtUTC = DateTime.UtcNow.AddDays(-59).AddHours(-23).AddSeconds(-30),
                    DurationSeconds = 30,
                    ExecutionTimeSeconds = 30,
                }
            },
            new ExecutionStatTestData()
            {
                Name = "Execution_Stats_Task_2_Succeeded",
                ExecutionStats = new ExecutionStats()
                {
                    Id = Guid.NewGuid(),
                    CorrelationId = "8fda262e-a6dd-46d1-ab8e-592b6442c607",
                    ExecutionId = "73127fd4-20cb-42c1-8138-01db8b0d35d6",
                    WorkflowInstanceId = "Workflow_1",
                    TaskId = "Task_2",
                    Status = "Succeeded",
                    LastUpdatedUTC = DateTime.UtcNow.AddDays(-60),
                    StartedUTC = DateTime.UtcNow.AddDays(-60),
                    CompletedAtUTC = DateTime.UtcNow.AddDays(-59).AddHours(-23).AddSeconds(-30),
                    DurationSeconds = 30,
                    ExecutionTimeSeconds = 30,
                }
            },
        };
    }
}

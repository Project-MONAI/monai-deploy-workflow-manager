// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests
{
    public static class Helper
    {
        public static TaskDispatchTestData GetTaskDispatchByName(string name)
        {
            var taskDispatchTestData = TaskDispatchesTestData.TestData.FirstOrDefault(c => c.Name.Contains(name));

            if (taskDispatchTestData != null)
            {
                return taskDispatchTestData;
            }

            throw new Exception($"workflow {name} does not exist. Please check and try again!");
        }
    }
}

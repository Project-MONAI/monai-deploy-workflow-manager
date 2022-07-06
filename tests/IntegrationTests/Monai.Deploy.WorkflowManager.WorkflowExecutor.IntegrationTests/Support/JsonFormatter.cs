// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    static class JsonFormatter
    {
        public static string FormatJson(string json)
        {
            return JToken.Parse(json).ToString(Formatting.Indented);
        }
    }
}

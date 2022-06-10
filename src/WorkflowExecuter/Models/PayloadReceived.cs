// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.WorkfowExecuter.Models
{
    public class PayloadReceived
    {
        public Guid PayloadId { get; set; }

        public IEnumerable<string> Workflows { get; set; }

        public int FileCount { get; set; }

        public string CorrelationId { get; set; }

        public string Bucket { get; set; }

        public string CallingAeTitle { get; set; }

        public string CalledAeTitle { get; set; }

        public DateTime Timestamp { get; set; }

    }
}

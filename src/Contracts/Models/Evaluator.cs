// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class Evaluator
    {
        [ConfigurationKeyName("correlation_id")]
        public string CorrelationId { get; set; }

        [ConfigurationKeyName("input")]
        public Artifact Input { get; set; }

        [ConfigurationKeyName("executions")]
        public ExecutionContext Executions { get; set; }

        [ConfigurationKeyName("dicom")]
        public ExecutionContext Dicom { get; set; }
    }
}

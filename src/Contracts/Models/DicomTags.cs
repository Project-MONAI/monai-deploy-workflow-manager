// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class DicomTags
    {
        [ConfigurationKeyName("study_id")]
        public string StudyId { get; set; }

        [ConfigurationKeyName("tags")]
        public Dictionary<string, string> Tags { get; set; }

        [ConfigurationKeyName("series")]
        public List<string> Series { get; set; }
    }
}

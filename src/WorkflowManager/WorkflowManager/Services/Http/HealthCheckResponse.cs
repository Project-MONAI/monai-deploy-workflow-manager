/*
 * Copyright 2021-2022 MONAI Consortium
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

using System;
using System.Collections.Generic;
using Minio.DataModel;

namespace Monai.Deploy.WorkflowManager.Services.Http
{
    /// <summary>
    /// Health Check Response Model.
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets checks.
        /// </summary>
        public IEnumerable<HealthCheck> Checks { get; set; }

        /// <summary>
        /// Gets or sets Durations.
        /// </summary>
        public TimeSpan Duration { get; set; }
    }
}

﻿/*
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

using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading.Tasks;

namespace Monai.Deploy.WorkflowManager.HealthChecks
{
    /// <summary>
    /// Argo Health checks.
    /// </summary>
    public class ArgoHealth
    {
        /// <summary>
        /// Check method.
        /// </summary>
        /// <returns></returns>
        public static async Task<HealthCheckResult> Check()
        {
            return new HealthCheckResult(HealthStatus.Unhealthy, "desc", new Exception("boom"));
        }
    }
}

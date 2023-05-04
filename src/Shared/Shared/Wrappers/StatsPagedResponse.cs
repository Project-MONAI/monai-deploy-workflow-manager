﻿/*
 * Copyright 2023 MONAI Consortium
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
namespace Monai.Deploy.WorkflowManager.Shared.Wrappers
{
    public class StatsPagedResponse<T> : PagedResponse<T>
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public long TotalExecutions { get; set; }
        public long TotalFailures { get; set; }
        public long TotalInprogress { get; set; }
        public double AverageTotalExecutionSeconds { get; set; }
        public double AverageArgoExecutionSeconds { get; set; }

        public StatsPagedResponse(T data, int pageNumber, int pageSize) : base(data, pageNumber, pageSize)
        {

        }
        //public StatsPagedResponse(PagedResponse<T> paged) : base(paged.Data, paged.PageNumber, paged.PageSize)
        //{
        //    int re = 0;
        //}
    }
}

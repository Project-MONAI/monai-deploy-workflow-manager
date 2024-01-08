/*
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

using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Filter;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;

namespace Monai.Deploy.WorkflowManager.Common.ControllersShared
{
    /// <summary>
    /// Base Api Controller.
    /// </summary>
    [ApiController]
    public class PaginationApiControllerBase : ApiControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationApiControllerBase"/> class.
        /// </summary>
        /// <param name="options">Workflow manager options.</param>
        public PaginationApiControllerBase(IOptions<WorkflowManagerOptions> options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets Workflow Manager Options
        /// </summary>
        protected IOptions<WorkflowManagerOptions> Options { get; }

        /// <summary>
        /// CreateStatsPagedResponse
        /// </summary>
        /// <typeparam name="T">Generic.</typeparam>
        /// <param name="pagedData">IEnumerable of Generic Data.</param>
        /// <param name="validFilter">Pagination Filter.</param>
        /// <param name="totalRecords">Total number of records for given validation filter in dataset.</param>
        /// <param name="uriService">UriService.</param>
        /// <param name="route">Route being called.</param>
        /// <returns>StatsPagedResponse with data type of T.</returns>
        protected static StatsPagedResponse<IEnumerable<T>> CreateStatsPagedResponse<T>(
            IEnumerable<T> pagedData,
            PaginationFilter validFilter,
            long totalRecords,
            IUriService uriService,
            string route)
        {
            var response = new StatsPagedResponse<IEnumerable<T>>(pagedData, validFilter.PageNumber, validFilter.PageSize ?? 10);
            response.SetUp(validFilter, totalRecords, uriService, route);
            return response;
        }

        /// <summary>
        /// Creates a pagination paged response.
        /// </summary>
        /// <typeparam name="T">Data set type.</typeparam>
        /// <param name="pagedData">Data set.</param>
        /// <param name="validFilter">Filters.</param>
        /// <param name="totalRecords">Total records.</param>
        /// <param name="uriService">Uri service.</param>
        /// <param name="route">Route.</param>
        /// <returns>Returns <see cref="PagedResponse{T}"/>.</returns>
        protected PagedResponse<IEnumerable<T>> CreatePagedResponse<T>(IEnumerable<T> pagedData, PaginationFilter validFilter, long totalRecords, IUriService uriService, string route)
        {
            ArgumentNullException.ThrowIfNull(pagedData, nameof(pagedData));
            ArgumentNullException.ThrowIfNull(validFilter, nameof(validFilter));
            ArgumentNullException.ThrowIfNull(route, nameof(route));
            ArgumentNullException.ThrowIfNull(uriService, nameof(uriService));

            var pageSize = validFilter.PageSize ?? Options.Value.EndpointSettings.DefaultPageSize;
            var response = new PagedResponse<IEnumerable<T>>(pagedData, validFilter.PageNumber, pageSize);

            response.SetUp(validFilter, totalRecords, uriService, route);
            return response;
        }
    }
}

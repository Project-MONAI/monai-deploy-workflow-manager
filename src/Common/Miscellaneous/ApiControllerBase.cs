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

using System.Net;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;
using Monai.Deploy.WorkflowManager.Common.Configuration;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Filter;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;

namespace Monai.Deploy.WorkflowManager.Common.ControllersShared
{
    /// <summary>
    /// Base Api Controller.
    /// </summary>
    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
        public IOptions<WorkflowManagerOptions> Options { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiControllerBase"/> class.
        /// </summary>
        /// <param name="options">Workflow manager options.</param>
        public ApiControllerBase(IOptions<WorkflowManagerOptions> options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets internal Server Error 500.
        /// </summary>
        public static int InternalServerError => (int)HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets bad Request 400.
        /// </summary>
        public new static int BadRequest => (int)HttpStatusCode.BadRequest;

        /// <summary>
        /// Gets notFound 404.
        /// </summary>
        public new static int NotFound => (int)HttpStatusCode.NotFound;

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
        public PagedResponse<IEnumerable<T>> CreatePagedResponse<T>(IEnumerable<T> pagedData, PaginationFilter validFilter, long totalRecords, IUriService uriService, string route)
        {
            Guard.Against.Null(pagedData, nameof(pagedData));
            Guard.Against.Null(validFilter, nameof(validFilter));
            Guard.Against.Null(route, nameof(route));
            Guard.Against.Null(uriService, nameof(uriService));

            var pageSize = validFilter.PageSize ?? Options.Value.EndpointSettings.DefaultPageSize;
            var response = new PagedResponse<IEnumerable<T>>(pagedData, validFilter.PageNumber, pageSize);

            response.SetUp(validFilter, totalRecords, uriService, route);
            return response;
        }


        public StatsPagedResponse<IEnumerable<T>> CreateStatsPagedReponse<T>(IEnumerable<T> pagedData, PaginationFilter validFilter, long totalRecords, IUriService uriService, string route)
        {
            var response = new StatsPagedResponse<IEnumerable<T>>(pagedData, validFilter.PageNumber, validFilter.PageSize ?? 10);
            response.SetUp(validFilter, totalRecords, uriService, route);
            return response;
        }
    }
}

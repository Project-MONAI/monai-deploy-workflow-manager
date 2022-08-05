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

using System;
using System.Collections.Generic;
using System.Net;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Filter;
using Monai.Deploy.WorkflowManager.Services;
using Monai.Deploy.WorkflowManager.Wrappers;

namespace Monai.Deploy.WorkflowManager.Controllers
{
    /// <summary>
    /// Base Api Controller.
    /// </summary>
    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
        private readonly IOptions<WorkflowManagerOptions> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiControllerBase"/> class.
        /// </summary>
        /// <param name="options">Workflow manager options.</param>
        public ApiControllerBase(IOptions<WorkflowManagerOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets internal Server Error 500.
        /// </summary>
        public static int InternalServerError => (int)HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets bad Request 400.
        /// </summary>
        public static new int BadRequest => (int)HttpStatusCode.BadRequest;

        /// <summary>
        /// Gets notFound 404.
        /// </summary>
        public static new int NotFound => (int)HttpStatusCode.NotFound;

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
        public PagedResponse<List<T>> CreatePagedReponse<T>(List<T> pagedData, PaginationFilter validFilter, long totalRecords, IUriService uriService, string route)
        {
            Guard.Against.Null(pagedData);
            Guard.Against.Null(validFilter);
            Guard.Against.Null(route);
            Guard.Against.Null(uriService);

            var pageSize = validFilter.PageSize ?? _options.Value.EndpointSettings.DefaultPageSize;
            var respose = new PagedResponse<List<T>>(pagedData, validFilter.PageNumber, pageSize);
            var totalPages = (double)totalRecords / pageSize;
            var roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));

            respose.NextPage =
                validFilter.PageNumber >= 1 && validFilter.PageNumber < roundedTotalPages
                ? uriService.GetPageUriString(new PaginationFilter(validFilter.PageNumber + 1, pageSize), route)
                : null;

            respose.PreviousPage =
                validFilter.PageNumber - 1 >= 1 && validFilter.PageNumber <= roundedTotalPages
                ? uriService.GetPageUriString(new PaginationFilter(validFilter.PageNumber - 1, pageSize), route)
                : null;

            respose.FirstPage = uriService.GetPageUriString(new PaginationFilter(1, pageSize), route);
            respose.LastPage = uriService.GetPageUriString(new PaginationFilter(roundedTotalPages, pageSize), route);
            respose.TotalPages = roundedTotalPages;
            respose.TotalRecords = totalRecords;

            return respose;
        }
    }
}

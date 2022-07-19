// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using Microsoft.AspNetCore.WebUtilities;
using Monai.Deploy.WorkflowManager.Filter;

namespace Monai.Deploy.WorkflowManager.Services
{
    /// <summary>
    /// Uri Service.
    /// </summary>
    public class UriService : IUriService
    {
        private readonly Uri _baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="UriService"/> class.
        /// </summary>
        /// <param name="baseUri">Base Url.</param>
        public UriService(Uri baseUri)
        {
            _baseUri = baseUri;
        }

        /// <summary>
        /// Gets page uri.
        /// </summary>
        /// <param name="filter">Filters.</param>
        /// <param name="route">Route.</param>
        /// <returns>Uri.</returns>
        public string GetPageUriString(PaginationFilter filter, string route)
        {
            var endpointUri = new Uri(string.Concat(_baseUri, route));
            var modifiedUri = QueryHelpers.AddQueryString(endpointUri.ToString(), "pageNumber", filter.PageNumber.ToString());
            modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "pageSize", filter.PageSize.ToString());
            var uri = new Uri(modifiedUri);
            return uri.IsAbsoluteUri ? uri.PathAndQuery : uri.OriginalString;
        }
    }
}

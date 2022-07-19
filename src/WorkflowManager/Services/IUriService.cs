// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.WorkflowManager.Filter;

namespace Monai.Deploy.WorkflowManager.Services
{
    /// <summary>
    /// Uri Serivce.
    /// </summary>
    public interface IUriService
    {
        /// <summary>
        /// Gets Relative Uri path with filters as a string.
        /// </summary>
        /// <param name="filter">Filters.</param>
        /// <param name="route">Route.</param>
        /// <returns>Relative Uri string.</returns>
        public string GetPageUriString(PaginationFilter filter, string route);
    }
}

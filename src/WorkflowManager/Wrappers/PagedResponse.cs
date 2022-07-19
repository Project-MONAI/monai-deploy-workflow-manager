// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.Wrappers
{
    /// <summary>
    /// Paged Response for use with paginations.
    /// </summary>
    /// <typeparam name="T">Type of response.</typeparam>
    public class PagedResponse<T> : Response<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResponse{T}"/> class.
        /// </summary>
        /// <param name="data">Response Data.</param>
        /// <param name="pageNumber">Page number.</param>
        /// <param name="pageSize">Page size.</param>
        public PagedResponse(T data, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            Data = data;
            Message = null;
            Succeeded = true;
            Errors = null;
        }

        /// <summary>
        /// Gets or sets PageNumber.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets PageSize.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets FirstPage.
        /// </summary>
        public string FirstPage { get; set; }

        /// <summary>
        /// Gets or sets LastPage.
        /// </summary>
        public string LastPage { get; set; }

        /// <summary>
        /// Gets or sets TotalPages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets TotalRecords.
        /// </summary>
        public long TotalRecords { get; set; }

        /// <summary>
        /// Gets or sets NextPage.
        /// </summary>
        public string NextPage { get; set; }

        /// <summary>
        /// Gets or sets previousPage.
        /// </summary>
        public string PreviousPage { get; set; }
    }
}

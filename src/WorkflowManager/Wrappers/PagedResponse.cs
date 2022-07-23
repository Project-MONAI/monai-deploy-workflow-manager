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

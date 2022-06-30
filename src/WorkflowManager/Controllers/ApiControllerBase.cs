// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Monai.Deploy.WorkflowManager.Controllers
{
    /// <summary>
    /// Base Api Controller.
    /// </summary>
    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
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
    }
}

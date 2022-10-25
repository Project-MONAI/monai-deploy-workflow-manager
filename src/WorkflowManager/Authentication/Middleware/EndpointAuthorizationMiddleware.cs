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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Authentication.Extensions;

namespace Monai.Deploy.WorkflowManager.Authentication.Middleware
{
    /// <summary>
    /// EndpointAuthorizationMiddleware for checking endpoint configuration.
    /// </summary>
    public class EndpointAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EndpointAuthorizationMiddleware> _logger;

        public EndpointAuthorizationMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<EndpointAuthorizationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpcontext)
        {
            if (_configuration.BypassAuth(_logger))
            {
                await _next(httpcontext);
                return;
            }
            if (httpcontext.User is not null
                && httpcontext.User.Identity is not null
                && httpcontext.User.Identity.IsAuthenticated)
            {
                _configuration.GetAndValidateConfig(out var serverrealm, out var serverrealmkey, out var requiredUserClaims, out var requiredAdminClaims);

                if (httpcontext.GetRouteValue("controller") is string controller)
                {
                    var validEndpoints = httpcontext.GetValidEndpoints(requiredAdminClaims, requiredUserClaims);
                    var result = validEndpoints.Any(e => e.Equals(controller, StringComparison.InvariantCultureIgnoreCase)) || validEndpoints.Contains("all");

                    if (result is false)
                    {
                        httpcontext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                        await httpcontext.Response.CompleteAsync();

                        return;
                    }
                }
                await _next(httpcontext);
            }
            else
            {
                httpcontext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }
    }
}

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

using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http;

namespace Monai.Deploy.WorkflowManager.Authentication.Extensions
{
    public static class HttpContextExtension
    {
        /// <summary>
        /// Gets endpoints specified in config for roles in claims.
        /// </summary>
        /// <param name="httpcontext"></param>
        /// <param name="requiredClaims"></param>
        /// <returns></returns>
        public static List<string> GetValidEndpoints(this HttpContext httpcontext, Dictionary<string, string>[] requiredClaims)
        {
            Guard.Against.Null(requiredClaims);

            foreach (var claim in requiredClaims)
            {
                var claims = claim.Single(c => c.Key != AuthKeys.Endpoints);

                if (httpcontext.User.HasClaim(claims.Key, claims.Value))
                {
                    if (claim.TryGetValue(AuthKeys.Endpoints, out var claimEndpoints))
                    {
                        return claimEndpoints.Split(",").Select(s => s.Trim()).ToList();
                    }
                    else
                    {
                        return new List<string> { "all" };
                    }
                }
            }

            return new List<string>();
        }

        /// <summary>
        /// Wrapper for GetValidEndpoints but able to process admins and users passed in together.
        /// </summary>
        /// <param name="httpcontext"></param>
        /// <param name="requiredAdminClaims"></param>
        /// <param name="requiredUsersClaims"></param>
        /// <returns></returns>
        public static List<string> GetValidEndpoints(this HttpContext httpcontext, Dictionary<string, string>[] requiredAdminClaims, Dictionary<string, string>[] requiredUsersClaims)
        {
            var validEndpoints = httpcontext.GetValidEndpoints(requiredUsersClaims);
            validEndpoints.AddRange(httpcontext.GetValidEndpoints(requiredAdminClaims));

            return validEndpoints;
        }
    }
}

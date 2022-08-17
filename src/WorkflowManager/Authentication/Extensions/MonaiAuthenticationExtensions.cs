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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Monai.Deploy.WorkflowManager.Common.Extensions;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Monai.Deploy.WorkflowManager.Authentication.Extensions
{

    public static class MonaiAuthenticationExtensions
    {
        /// <summary>
        /// Adds MONAI OpenID Configuration to services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        public static IServiceCollection AddMonaiAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            configuration.GetAndValidateConfig(
                out var serverRealm, out var serverRealmKey, out var requiredUserClaims, out var requiredAdminClaims);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, "OpenId", options =>
            {
                options.Authority = serverRealm;
                options.Audience = serverRealm;
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(serverRealmKey)),
                    ValidIssuer = serverRealm,
                    ValidAudiences = new List<string>() { "account", "monai-app" },
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateAudience = true,
                };
            });

            services.AddAuthorization(options =>
            {
                if (requiredAdminClaims.IsNullOrEmpty() is false)
                {
                    AddPolicy(options, requiredAdminClaims, "Admin");
                }

                if (requiredUserClaims.IsNullOrEmpty() is false)
                {
                    AddPolicy(options, requiredUserClaims, "User");
                }
            });

            return services;
        }

        /// <summary>
        /// Takes dictionary of claims to add to policies.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="requiredClaims"></param>
        /// <param name="policyName"></param>
        private static void AddPolicy(AuthorizationOptions options, Dictionary<string, string>[] requiredClaims, string policyName)
        {
            foreach (var dict in requiredClaims)
            {
                var item = dict.Single(c => c.Key != "endpoints");
                options.AddPolicy(policyName, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(item.Key, item.Value));
            }
        }
    }
}

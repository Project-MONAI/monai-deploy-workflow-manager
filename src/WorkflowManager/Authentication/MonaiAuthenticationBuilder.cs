using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Monai.Deploy.WorkflowManager.Common.Extensions;

namespace Monai.Deploy.WorkflowManager.Authentication
{
    public class UserClaimsMiddleware
    {
        private readonly RequestDelegate _next;

        public UserClaimsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.User != null && httpContext.User.Identity.IsAuthenticated)
            {
                var claims = new List<Claim>
                {
                    new Claim("SomeClaim", "SomeValue")
                };

                var appIdentity = new ClaimsIdentity(claims);
                httpContext.User.AddIdentity(appIdentity);

                await _next(httpContext);
            }
        }
    }

    public static class UserClaimsMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserClaims(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserClaimsMiddleware>();
        }
    }

    // TODO Move GuardExtension
    public static class GuardExtension
    {
        public static bool MultipleAny<T>(this IGuardClause guard, string message, params T[][] array)
        {
            var result = true;
            if (typeof(T) == typeof(string))
            {
                result = array
                    .Where(a => a is not null)
                    .SelectMany(r => r)
                    .Where(i => i is not null && !string.IsNullOrEmpty(i.ToString()))
                    .Any();
                if (result is false)
                {
                    throw new ArgumentException(message ?? "Array is doesn't have any elements");
                }
                return result;
            }

            result = array
                .Where(a => a is not null)
                .SelectMany(r => r)
                .Any();
            if (result is false)
            {
                throw new ArgumentException(message ?? "Array is doesn't have any elements");
            }
            return result;
        }
    }

    public static class MonaiAuthenticationBuilder
    {
        /// <summary>
        /// Adds MONAI OpenID Configuration to services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="hostContext">HostBuilderContext used for referencing configuration</param>
        /// <param name="openIdKey"></param>
        public static IServiceCollection AddMonaiAuthentication(this IServiceCollection services, IConfiguration configuration, string openIdKey)
        {
            Guard.Against.NullOrWhiteSpace(openIdKey, nameof(openIdKey), "Open Id Key in config can not be null.");
            Guard.Against.Null(services, nameof(services));
            Guard.Against.Null(configuration, nameof(configuration));

            var authenticationSettings = configuration.GetSection("WorkflowManagerAuthentication");
            Guard.Against.Null(authenticationSettings, nameof(authenticationSettings), "Missing WorkflowManagerAuthentication section in config.");

            var authenticationSettingsSection = authenticationSettings.GetSection(openIdKey);
            Guard.Against.Null(authenticationSettings, nameof(authenticationSettings), "openIdKey section not found.");

            var serverRealm = authenticationSettingsSection["ServerRealm"];
            Guard.Against.NullOrWhiteSpace(serverRealm, nameof(serverRealm), "SeverRealm is a required authenticationSettings attribute.");

            var metaData = authenticationSettingsSection["Metadata"] ?? $"{serverRealm}/.well-known/openid-configuration";
            var tokenExchange = $"{serverRealm}/protocol/openid-connect/token";

            var requiredClaims = authenticationSettingsSection.GetSection("Claims");

            var requiredUsers = requiredClaims.GetSection("RequiredUsers").Get<string[]>();
            var requiredAdmins = requiredClaims.GetSection("RequiredAdmins").Get<string[]>();
            var requiredUserRoles = requiredClaims.GetSection("RequiredUserRoles").Get<string[]>();
            var requiredAdminRoles = requiredClaims.GetSection("RequiredAdminRoles").Get<string[]>();

            Guard.Against.MultipleAny(
                "Required config: at least one required RequiredUsers, RequiredAdmins, RequiredUserRoles, RequiredAdminRoles",
                requiredUsers,
                requiredAdmins,
                requiredUserRoles,
                requiredAdminRoles);

            services.AddAuthentication()
            .AddJwtBearer("OAuth", openIdKey, options =>
            {
                options.Authority = serverRealm;
                options.Audience = "account";
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = serverRealm,
                    ValidAudiences = new List<string>() { "account", "monai-app" },
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateAudience = true
                };
            });

            /*
             * Policy based authentication
            */
            services.AddAuthorization(options =>
            {
                //options.AddPolicy("test", p => p.RequireAssertion(a => a.))
                // Create policy with more than one claim
                options.AddPolicy("users", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            (c.Value == "user") || (c.Value == "admin")))); // Create policy with only one claim

                if (requiredAdminRoles.IsNullOrEmpty() is false)
                {
                    foreach (var item in requiredAdminRoles)
                    {
                        options.AddPolicy("admin", policy => policy.RequireClaim(ClaimTypes.Role, item));
                    }
                }
                if (requiredAdmins.IsNullOrEmpty() is false)
                {
                    foreach (var item in requiredAdmins)
                    {
                        options.AddPolicy("admin", policy => policy.RequireClaim(ClaimTypes.Email, item));
                        options.AddPolicy("admin", policy => policy.RequireUserName(item));
                    }
                }
                if (requiredUserRoles.IsNullOrEmpty() is false)
                {
                    foreach (var item in requiredUserRoles)
                    {
                        options.AddPolicy("user", policy => policy.RequireClaim(ClaimTypes.Role, item));
                    }
                }
                if (requiredUsers.IsNullOrEmpty() is false)
                {
                    foreach (var item in requiredUsers)
                    {
                        options.AddPolicy("user", policy => policy.RequireClaim(ClaimTypes.Email, item));
                        options.AddPolicy("user", policy => policy.RequireUserName(item));
                    }
                }
                options.AddPolicy("noaccess", policy => policy.RequireClaim(ClaimTypes.Role, "noaccess"));
            });
            /*
            * User based authentication
           */
            services.AddAuthorization(options =>
            {
                //options.AddPolicy("test", p => p.RequireAssertion(a => a.))
                // Create policy with more than one claim
                options.AddPolicy("users", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            (c.Value == "user") || (c.Value == "admin")))); // Create policy with only one claim

                options.AddPolicy("admins", policy => policy.RequireClaim(ClaimTypes.Role, "admin")); // Create a policy with a claim that doesn't exist or you are unauthorized to
                options.AddPolicy("noaccess", policy => policy.RequireClaim(ClaimTypes.Role, "noaccess"));
            });

            return services;
        }
    }
}

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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Monai.Deploy.WorkflowManager.Authentication.Extensions
{
    public static class ConfigurationExtension
    {
        public static bool BypassAuth(this IConfiguration configuration, ILogger logger)
        {
            Guard.Against.Null(configuration, nameof(configuration));
            Guard.Against.Null(logger, nameof(logger));

            var authenticationSettings = configuration.GetSection(AuthKeys.WorkflowManagerAuthentication);
            Guard.Against.Null(authenticationSettings, nameof(authenticationSettings), "Missing WorkflowManagerAuthentication section in config.");

            if (authenticationSettings[AuthKeys.OpenId] is null)
            {
                logger.LogInformation("Bypass Authentication");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets configuration values from the IConfiguration provider.
        /// under "WorkflowManagerAuthentication" key you are able to
        /// have any label of configuration for example could have
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serverRealm"></param>
        /// <param name="serverRealmKey"></param>
        /// <param name="requiredUserRoles"></param>
        /// <param name="requiredAdminRoles"></param>
        public static void GetAndValidateConfig(
            this IConfiguration configuration,
            out string serverRealm,
            out string serverRealmKey,
            out Dictionary<string, string>[] requiredUserRoles,
            out Dictionary<string, string>[] requiredAdminRoles)
        {
            Guard.Against.Null(configuration, nameof(configuration));

            var authenticationSettings = configuration.GetSection(AuthKeys.WorkflowManagerAuthentication);
            Guard.Against.Null(authenticationSettings, nameof(authenticationSettings), "Missing WorkflowManagerAuthentication section in config.");

            var authenticationSettingsSection = authenticationSettings.GetSection(AuthKeys.OpenId);
            serverRealm = authenticationSettingsSection[AuthKeys.ServerRealm];
            Guard.Against.NullOrWhiteSpace(serverRealm, nameof(serverRealm), "SeverRealm is a required authenticationSettings attribute.");
            serverRealmKey = authenticationSettingsSection[AuthKeys.ServerRealmKey];
            Guard.Against.NullOrWhiteSpace(serverRealm, nameof(serverRealmKey), "SeverRealmKey is a required authenticationSettings attribute.");

            var requiredClaims = authenticationSettingsSection.GetSection(AuthKeys.Claims);
            requiredUserRoles = requiredClaims.GetSection(AuthKeys.RequiredUserClaims).Get<Dictionary<string, string>[]>();
            requiredAdminRoles = requiredClaims.GetSection(AuthKeys.RequiredAdminClaims).Get<Dictionary<string, string>[]>();
        }
    }
}

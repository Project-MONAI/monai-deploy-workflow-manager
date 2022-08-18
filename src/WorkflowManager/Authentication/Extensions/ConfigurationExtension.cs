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
using Ardalis.GuardClauses;

namespace Monai.Deploy.WorkflowManager.Authentication.Extensions
{
    public static class ConfigurationExtension
    {
        public static bool BypassAuth(this IConfiguration configuration)
        {
            Guard.Against.Null(configuration, nameof(configuration));

            var authenticationSettings = configuration.GetSection("WorkflowManagerAuthentication");
            Guard.Against.Null(authenticationSettings, nameof(authenticationSettings), "Missing WorkflowManagerAuthentication section in config.");

            if (authenticationSettings["OpenId"] is null)
            {
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

            var authenticationSettings = configuration.GetSection("WorkflowManagerAuthentication");
            Guard.Against.Null(authenticationSettings, nameof(authenticationSettings), "Missing WorkflowManagerAuthentication section in config.");

            var authenticationSettingsSection = authenticationSettings.GetSection("OpenId");
            serverRealm = authenticationSettingsSection["ServerRealm"];
            Guard.Against.NullOrWhiteSpace(serverRealm, nameof(serverRealm), "SeverRealm is a required authenticationSettings attribute.");
            serverRealmKey = authenticationSettingsSection["ServerRealmKey"];
            Guard.Against.NullOrWhiteSpace(serverRealm, nameof(serverRealmKey), "SeverRealmKey is a required authenticationSettings attribute.");

            var requiredClaims = authenticationSettingsSection.GetSection("Claims");
            requiredUserRoles = requiredClaims.GetSection("RequiredUserClaims").Get<Dictionary<string, string>[]>();
            requiredAdminRoles = requiredClaims.GetSection("RequiredAdminClaims").Get<Dictionary<string, string>[]>();
        }
    }
}

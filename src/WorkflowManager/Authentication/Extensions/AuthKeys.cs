﻿/*
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

namespace Monai.Deploy.WorkflowManager.Authentication.Extensions
{
    public static class AuthKeys
    {
        public const string ServerRealm = "ServerRealm";
        public const string ServerRealmKey = "ServerRealmKey";
        public const string Claims = "Claims";
        public const string OpenId = "OpenId";
        public const string WorkflowManagerAuthentication = "WorkflowManagerAuthentication";
        public const string RequiredUserClaims = "RequiredUserClaims";
        public const string RequiredAdminClaims = "RequiredAdminClaims";
        public const string Testing = "testing";
        public const string Endpoints = "endpoints";
        public const string AdminPolicyName = "Admin";
        public const string UserPolicyName = "User";

    }
}

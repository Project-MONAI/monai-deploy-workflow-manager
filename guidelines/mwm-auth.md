<!--
  ~ Copyright 2022 MONAI Consortium
  ~
  ~ Licensed under the Apache License, Version 2.0 (the "License");
  ~ you may not use this file except in compliance with the License.
  ~ You may obtain a copy of the License at
  ~
  ~ http://www.apache.org/licenses/LICENSE-2.0
  ~
  ~ Unless required by applicable law or agreed to in writing, software
  ~ distributed under the License is distributed on an "AS IS" BASIS,
  ~ WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  ~ See the License for the specific language governing permissions and
  ~ limitations under the License.
-->

# Authentication notes

In the developement of the authentication we tested with Keycloak, in Keycloak it is required to flatten the roles for the authentication to be able to pick up the roles.

I recommend changing your main admin console theme too "keycloak" theme before following these instructions, this can be done by going to 

> select Master realm -> Realm Settings -> Themes -> Admin Console Theme -> "keycloak" -> refresh page & change back to your app realm

Now to create mapper to flatten the roles

> Clients -> Mappers -> Create

![| Option           | Value                 |
|------------------|-----------------------|
| Mapper Type      | User Client Role      |
| Token Claim Name | user_roles            |
| Client ID        | (Name of your client) |
|                  |                       |](static/keycloak-dev-example1.png)

Back on the client page navigate to roles tab and add a role (for example "monai-role-user")

Then in users -> roles mappings

select Client Roles, in the dropdown select your client from the list and should see in avaliable roles role your created above and move that into assigned roles. 

This is an example of MonaiDeployAuthentication configuration:

```json
  "MonaiDeployAuthentication": {
    "BypassAuthentication": false,
    "openId": {
      "realm": "http://localhost:8080/realms/monai-test-realm",
      "realmKey": "realmKey",
      "clientId": "monai-service",
      "audiences": [ "monai-deploy", "account" ],
      "claimMappings": {
        "userClaims": [
          {
            "claimType": "user_roles",
            "claimValues": [ "monai-role-user" ],
            "endpoints": [ "payloads", "workflows", "workflowinstances", "tasks" ]
          }
        ],
        "adminClaims": [
          {
            "claimType": "user_roles",
            "claimValues": [ "monai-role-admin" ],
            "endpoints": [ "all" ]
          }
        ]
      }
    }
  },
```
realmKey can be found in Clients -> Credentials -> Secret

and bare minimum for bypass is...

```json
  "MonaiDeployAuthentication": {
    "BypassAuthentication": false,
  }
```

- **realm**: link to you OpenId provider 

- **realmKey**: OpenId provider key

- **clientId**: name of you client in the client in openid provider

there are 2 types of claims

**userClaims**

**adminClaims**

example here we use: 

```json
"claimType": "user_roles",
"claimValues": [ "monai-role-user" ],
"endpoints": [ "payloads", "workflows", "workflowinstances", "tasks" ],
```
claimType **user_roles** maps back to setting above (JWT) Token Claim Name.

claimValues **monai-role-user** is role we have setup and expect to find for users.

**endspoints** is a list of endpoints that authorised to access comma seperated string.

expected values: (values can be upper lower or normal casing):
- **all**,
- **payload**,
- **workflows**,
- **workflowinstances**,
- **tasks**


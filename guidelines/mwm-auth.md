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

In the developement of the authentication we tested with Keycloak to note you need to flatten the roles for the authentication to be able to pick up the roles this can be done by going to 

Keycloak -> Admin dashboard -> your specific realm -> Clients -> Mappers -> Create

configure it how you see fit...
The value you put in the Token Claim Name will be what you use in the configuration so in testing of the development i used "user_roles" and "user_realm_roles"

This is an example of WorkflowManagerAuthentication configuration:

```
"WorkflowManagerAuthentication": {
    "OpenId": {
      "ServerRealm": "http://localhost:8080/realms/monai-test/",
      "ServerRealmKey": "19B34Q5xsRYf3oFki18ZtUuNybaujb72",
      "ClientId": "monai-app-test",
      "Claims": {
        "RequiredUserClaims": [
          {
            "user_roles": "monai-role-user",
            "endpoints": "payloads,workflows,workflowinstances,tasks"
          },
          {
            "user_realm_roles": "this_is_just_a_test",
            "endpoints": "all"
          }
        ],
        "RequiredAdminClaims": [
          { "user_roles": "monai-role-admin" },
          { "user_roles": "monai-role-user" }
        ]
      }
    }
  },
```

- **ServerRealm**: link to you OpenId provider 

- **ServerRealmKey**: OpenId provider key

- **ClientId**: name of you client in the client in openid provider

under **Claims** you have 2 sub objects

**RequiredUserClaims**

**RequiredAdminClaims**

this can take an array of objects which will be your users and endpoints they can access...

example here we use: 

```
"user_roles": "monai-role-user",
"endpoints": "payloads,workflows,workflowinstances,tasks"
```
**user_roles** maps back to setting above (JWT) Token Claim Name.

**monai-role-user** is role we have setup and expect to find for users.

**endspoints** is a list of endpoints that authorised to access comma seperated string.

expected values: (values can be upper lower or normal casing):
- **all**,
- **payload**,
- **workflows**,
- **workflowinstances**,
- **tasks**


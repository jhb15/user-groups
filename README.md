# user-groups
AberFitness User Groups Microservice

| Branch | Status |
|-|-|
| Development | [![Development Build Status](https://travis-ci.org/sem5640-2018/user-groups.svg?branch=development)](https://travis-ci.org/sem5640-2018/user-groups) |
| Release | [![Release Build Status](https://travis-ci.org/sem5640-2018/user-groups.svg?branch=master)](https://travis-ci.org/sem5640-2018/user-groups) |


# Maintained by
* Adam
* ???

# Environment Variables

## Required Keys (All Environments)

| Environment Variable | Default | Description |
|-|-|-|
| ASPNETCORE_ENVIRONMENT | Production | Runtime environment, should be 'Development', 'Staging', or 'Production'. |
| ConnectionStrings__UserGroupsContext | N/A | MariaDB connection string. |
| UserGroups__ClientId | N/A | Gatekeeper client ID. |
| UserGroups__ClientSecret | N/A | Gatekeeper client secret. |
| UserGroups__GatekeeperUrl | N/A | Gatekeeper URL. |
| UserGroups__ApiResourceName | N/A | Gatekeeper API resource name. |


## Required Keys (Production + Staging Environments)
In addition to the above keys, you will also require:

| Environment Variable | Default | Description |
|-|-|-|
| Kestrel__Certificates__Default__Path | N/A | Path to the PFX certificate to use for HTTPS. |
| Kestrel__Certificates__Default__Password | N/A | Password for the HTTPS certificate. |
| UserGroups__ReverseProxyHostname | http://nginx | The internal docker hostname of the reverse proxy being used. |
| UserGroups__PathBase | /user-groups | The pathbase (name of the directory) that the app is being served from. |

## Basic setup

1) Navigate to Crossroads.Service.Auth.IntegrationTest/
2) Install npm packages with ```$npm i```
3) Configure environment variables by creating a cypress.env.json file and add the following variables. (Do NOT check this file into GitHub)
```json
{
  "VAULT_ROLE_ID": "add id here",
  "VAULT_SECRET_ID": "add secret here",
  "VAULT_VAR_SOURCE": { "https://vault.url.example": ["vault variable 1", "vault variable 2"] }
}
```
The properties of VAULT_VAR_SOURCE are fully qualified Vault URLs and their values are an array of the variable names to retrieve from that Vault. The following Vault variables are needed:
```json
"OKTA_OAUTH_BASE_URL", "CRDS_GATEWAY_BASE_URL", "OKTA_TOKEN_AUTH", "BEN_KENOBI_PW"
```
Note: There are other ways to configure environment variables for Cypress. Check out the official documentation [here](https://docs.cypress.io/guides/guides/environment-variables.html#Setting).

## Run Tests
Once installed, run Cypress tests headless with

```$npx cypress run --config baseUrl=$BASE_URL```

or with display

```$npx cypress open --config baseUrl=$BASE_URL```

where $BASE_URL is the (fully qualified) auth service endpoint prefix to be tested (ex. https://api-int.crossroads.net/auth/).

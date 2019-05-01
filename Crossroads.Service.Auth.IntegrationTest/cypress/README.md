## Basic Setup
### Install & Configure Variables

1) Navigate to Crossroads.Service.Auth.IntegrationTest/
2) Install npm packages with ```$npm i```
3) Configure environment variables by creating a cypress.env.json file and add the following variables. (Do NOT check this file into GitHub)
```json
{
  "VAULT_ROLE_ID": "add id here",
  "VAULT_SECRET_ID": "add secret here",
  "CRDS_ENV": "environment prefix here"
}
```
Note: There are other ways to configure environment variables for Cypress. Check out the official documentation [here](https://docs.cypress.io/guides/guides/environment-variables.html#Setting).

### Run Tests

Once installed and configured, run Cypress tests headless with

```$npx cypress run --config baseUrl=$BASE_URL```

or with display

```$npx cypress open --config baseUrl=$BASE_URL```

where $BASE_URL is the (fully qualified) auth service endpoint prefix to be tested (ex. https://api-int.crossroads.net/auth/).


## Docker Setup

Docker must be installed and running on your machine first. You can find it [here](https://www.docker.com/products/docker-desktop).

### Configure Variables

1) Navigate to the top-level directory (where docker-compose-test.yml lives)
2) Configure environment variables by creating a .env file and add the following variables. (Do NOT check this file into GitHub)

```bash
VAULT_ROLE_ID=addIdHere
VAULT_SECRET_ID=addSecretHere
CRDS_ENV=addEnvironmentHere
```

### Run Tests

Once configured, the following commands will create Docker containers for the auth service and Cypress tests, and run the tests against that service.

```bash
$docker-compose -f docker-compose-test.yml build
$docker-compose -f docker-compose-test.yml up --abort-on-container-exit --exit-code-from cypress
```
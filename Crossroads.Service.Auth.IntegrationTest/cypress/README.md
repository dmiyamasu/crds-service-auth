## Basic setup

Navigate to Crossroads.Service.Auth.IntegrationTest/

Set up environment variable access. Environment variables are managed with Vault. (what do?...)

Install npm packages with ```$npm i```

Once installed, run Cypress tests headless with

```$npx cypress run --config baseUrl=$BASE_URL```

or with display

```$npx cypress open --config baseUrl=$BASE_URL```

replacing $BASE_URL with the auth service endpoint prefix to be tested.

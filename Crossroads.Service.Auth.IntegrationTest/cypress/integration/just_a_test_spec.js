const https = require('https');

describe('Tests Cypress logging', function () {
  it('Prints Hello World!', function () {
    cy.log('Hello World!');
  });
});

function assignVars(dataObject) {
  // const gateway = 'CRDS_GATEWAY_BASE_URL'; //cm
  // const oktaBase = "OKTA_OAUTH_BASE_URL"; //auth

  // const oktaAuthToken = "OKTA_TOKEN_AUTH"; //TODO autom
  // const testUserPW = "BEN_KENOBI_PW"; //TODO autom

  // if(dataObject[gateway] !== undefined){
  //   config.env[gateway] = dataObject[gateway];
  // }
  const reqEnvVar = ['CRDS_GATEWAY_BASE_URL', 'OKTA_OAUTH_BASE_URL', 'OKTA_TOKEN_AUTH', 'BEN_KENOBI_PW'];

  reqEnvVar.forEach(envVar => {
    const vaultVar = dataObject[envVar];
    if (vaultVar !== undefined) {
      //config.env[envVar] = vaultVar;
      cy.log(`vault variable ${vaultVar} assigned to ${envVar}`);//DEBUG
    }
  });
}

describe('Vault stuff', function () {
  it.only('checks if envvar is set?', function () {
    cy.log(`${Cypress.env('TEST_VAR')}`);
  });

  it('request', function () {
    cy.request({
      method: 'POST',
      url: 'https://vault.crossroads.net/v1/auth/approle/login',
      body: {
        role_id: `${Cypress.env('VAULT_ROLE')}`,
        secret_id: `${Cypress.env('VAULT_SECRET')}`
      }
    }).then(response => {
      const token = response.body.auth.client_token;
      const vaults = `${Cypress.env('VAULT_DIRS')}`.split(',');
      cy.log(vaults.length);
      vaults.forEach(vault => {
        cy.request({
          method: 'GET',
          url: `${Cypress.env('VAULT_ENDPOINT')}${vault}`,
          headers: { 'x-vault-token': token }
        }).then(response => {
          cy.log(`${Object.keys(response.body.data.data)}`);
          assignVars(response.body.data.data);
        });
      });

    });
  });

  it('http piecemeal', function () { //TODO try this back in plugins - see if affected by cross origin
    const loginOptions = {
      hostname: 'vault.crossroads.net/v1/auth/approle/login',
      method: 'POST',
      header: { 'Access-Control-Allow-Origin': 'https://vault.crossroads.net'}
      // body: {
      //   role_id: `${Cypress.env('VAULT_ROLE')}`,
      //   secret_id: `${Cypress.env('VAULT_SECRET')}`
      // }
    };
    cy.log(loginOptions);

    const config = {};//fake

    https.get(loginOptions, response => {
      let data = '';
      response.on('data', (chunk) => {
        data += chunk;
      });

      // The whole response has been received. Print out the result.
      response.on('end', () => {
        console.log(data);
        //console.log(JSON.parse(data).explanation);
      });
    }).on('error', (e) => {
      console.error(e);
    });
  })

  it('http request', function () {
    const loginOptions = {
      hostname: 'vault.crossroads.net/v1/auth/approle/login',
      method: 'POST',
      body: {
        role_id: `${Cypress.env('VAULT_ROLE')}`,
        secret_id: `${Cypress.env('VAULT_SECRET')}`
      }
    };
    const config = {};//fake

    https.request(loginOptions, (response) => {
      const token = response.body.auth.client_token;
      const vaults = `${Cypress.env('VAULT_DIRS')}`.split(',');

      let data = '';
      response.on('data', (d) => {
        data += d;
      });

      response.on('end', () => {
        test = JSON.parse(data).explanation;
      });
      // vaults.forEach(vault => {
      //   https.get({ hostname: `${Cypress.env('VAULT_ENDPOINT')}${vault}`, headers: { 'x-vault-token': token } }, (vaultResponse) => {
      //     assignVars(vaultResponse.body.data.data, config);
      //     expect(vaultResponse).to.be('cats');
      //   });
      // });
      // return "dogs";
    })
  });

describe('vars loaded?', function () {
  it('tests if envvars loaded', function () {
    //VAult
    cy.log(`CRDS_GATEWAY_BASE_URL = ${Cypress.env('CRDS_GATEWAY_BASE_URL')}`);
    cy.log(`OKTA_OAUTH_BASE_URL = ${Cypress.env('OKTA_OAUTH_BASE_URL')}`);
    cy.log(`OKTA_TOKEN_AUTH = ${Cypress.env('OKTA_TOKEN_AUTH')}`);
    cy.log(`BEN_KENOBI_PW = ${Cypress.env('BEN_KENOBI_PW')}`);

    //passed in
    cy.log(`VAULT_ROLE = ${Cypress.env('VAULT_ROLE')}`);
    cy.log(`VAULT_SECRET = ${Cypress.env('VAULT_SECRET')}`);
    cy.log(`VAULT_ENDPOINT = ${Cypress.env('VAULT_ENDPOINT')}`);
    cy.log(`VAULT_DIRS = ${Cypress.env('VAULT_DIRS')}`);
  })
})
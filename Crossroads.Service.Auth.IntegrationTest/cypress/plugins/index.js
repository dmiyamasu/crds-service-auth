// ***********************************************************
// This example plugins/index.js can be used to load plugins
//
// You can change the location of this file or turn off loading
// the plugins file with the 'pluginsFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/plugins-guide
// ***********************************************************

// This function is called when a project is opened or re-opened (e.g. due to
// the project's config changing)

module.exports = (on, config) => {
  // `on` is used to hook into various events Cypress emits
  // `config` is the resolved Cypress config

};

//Retrieves environment variables from Vault
function assignVars(dataObject, config) {
  const reqEnvVar = ['CRDS_GATEWAY_BASE_URL', 'OKTA_OAUTH_BASE_URL', 'OKTA_TOKEN_AUTH', 'BEN_KENOBI_PW'];

  reqEnvVar.forEach(envVar => {
    const vaultVar = dataObject[envVar];
    if (vaultVar !== undefined) {
      config.env[envVar] = vaultVar;
    }
  });
}
//this is working
function getClientToken(config) {
  return new Promise(function (resolve, reject) {
    var options = {
      'method': 'POST',
      'hostname': 'vault.crossroads.net',
      'path': '/v1/auth/approle/login',
      'headers': {
        'cache-control': 'no-cache'
      }
    };

    var req = https.request(options, function (res) {
      var chunks = [];

      res.on('data', function (chunk) {
        chunks.push(chunk);
      });

      res.on('end', function () {
        var body = Buffer.concat(chunks);
        const clientToken = JSON.parse(body).auth.client_token;
        resolve(clientToken);
      });
    }).on('error', (e) => {
      reject(Error(`Something went wrong retreiving the client token from Okta:\n${e}`));
    });

    req.write(`{\n"role_id": "${config.env.VAULT_ROLE}",\n"secret_id": "${config.env.VAULT_SECRET}"\n}`);
    req.end();
  });
}
//TODO fix
function getVariablesFromVault(hostname, path, clientToken) {
  //given path and all tokens to assign
  //const vaultVars = config.env.VAULT_VAR_PATHS;
  return new Promise(function (resolve, reject) {
    var options = {
      'hostname': hostname,
      'path': path,
      'headers': {
        'cache-control': 'no-cache',
        'x-vault-token': clientToken,
      }
    };

    var req = https.get(options, function (res) {
      var chunks = [];

      res.on('data', function (chunk) {
        chunks.push(chunk);
      });

      res.on('end', function () {
        var body = Buffer.concat(chunks);
        const variables = JSON.parse(body).data.data;
        console.log(JSON.parse(body));
        resolve(variables);
      });
    }).on('error', (e) => {
      reject(Error(`Something went wrong requesting a secret from Okta:\n${e}`));
    });

    req.end();
  });
}

const https = require('https');
module.exports = (on, config) => {
  getClientToken(config).then((response) => {
    const token = response;
    //TODO need to break down the VAULT_VAR_PATH components - get all variables for each path, then assign
    //  the needed envvar based on the response
    //TODO Regex the VAULT_ENDPOINT and current VAULT_VAR_PATH and pass to getVariablesFromVault
    const hostname = 'vault.crossroads.net';
    const path = '...';

    getVariablesFromVault(hostname, path, token).then((response) => {
      console.log(`variable response ${Object.keys(response)}`);

    }, (error) => {
      throw error;
    });

    //return response;
  }, (error) => {
    throw error;
  });
};
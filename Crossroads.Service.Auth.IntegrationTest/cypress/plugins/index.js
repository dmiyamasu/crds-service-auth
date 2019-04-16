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

//TODO working as is, but try to find a way to store the VaultToken for use later
const https = require('https');
// function getVaultToken(config) {
//   return new Promise(function (resolve, reject) {
//     var options = {
//       'method': 'POST',
//       'hostname': 'vault.crossroads.net',
//       'path': '/v1/auth/approle/login',
//       'headers': {
//         'cache-control': 'no-cache'
//       }
//     };

//     var req = https.request(options, function (res) {
//       var chunks = [];

//       res.on('data', function (chunk) {
//         chunks.push(chunk);
//       });

//       res.on('end', function () {
//         var body = Buffer.concat(chunks);
//         const clientToken = JSON.parse(body).auth.client_token;
//         resolve(clientToken);
//       });
//     }).on('error', (e) => {
//       reject(Error(`Something went wrong retreiving the client token from Okta:\n${e}`));
//     });

//     req.write(`{\n"role_id": "${config.env.VAULT_ROLE}",\n"secret_id": "${config.env.VAULT_SECRET}"\n}`);
//     req.end();
//   });
// }

// function getVaultSecrets(vaultUrl, vaultToken) {
//   const pathComponents = vaultUrl.match(RegExp('https?://([\\w|.]+)(/.*)'));
//   const hostname = pathComponents[1];
//   const path = pathComponents[2];

//   return new Promise(function (resolve, reject) {
//     var options = {
//       'hostname': hostname,
//       'path': path,
//       'headers': {
//         'cache-control': 'no-cache',
//         'x-vault-token': vaultToken,
//       }
//     };

//     var req = https.get(options, function (res) {
//       var chunks = [];

//       res.on('data', function (chunk) {
//         chunks.push(chunk);
//       });

//       res.on('end', function () {
//         var body = Buffer.concat(chunks);
//         const variables = JSON.parse(body).data.data;
//         resolve(variables);
//       });
//     }).on('error', (e) => {
//       reject(Error(`Something went wrong requesting a secret from Okta:\n${e}`));
//     });

//     req.end();
//   });
// }

// //Returns modified config file or error message
// function addVariablesToConfig(config, vaultToken, vaultUrl, varsToStore) {
//   return new Promise(function (resolve, reject) {
//     getVaultSecrets(vaultUrl, vaultToken).then(vaultVariables => {
//       varsToStore.forEach(varName => {
//         config.env[varName] = vaultVariables[varName];
//       });
//       resolve(config);
//     }, error => {
//       reject(error);
//     });
//   });
// }

//for each entry in VAULT_VAR_PATHS, addVariablesToConfig
function addAllVariablesToConfig(config) {
  const vvm = new VaultVariables(config.env.VAULT_ROLE, config.env.VAULT_SECRET);

  return vvm.getVaultToken().then(vaultToken => {
    return vvm.addVariablesToConfigBulk(vaultToken, config.env.VAULT_VAR_PATHS).then(() => {
      return vvm.config;
    })
    // const vaultPathList = Object.keys(config.env.VAULT_VAR_PATHS);
    // return Promise.all(vaultPathList.map(path => {
    //   const varsToStore = config.env.VAULT_VAR_PATHS[path];
    //   return vvm.addVariablesToConfig(vaultToken, path, varsToStore);
    // })).then(() => {
    //   console.log(`after Promise.all, VV config is ${Object.keys(vvm.config)}`);
    //   return vvm.config;
    // }, error => {
    //   throw error;
    // });
  }, error => {
    throw error;
  });
}

module.exports = (on, config) => {
  return addAllVariablesToConfig(config).then(newConfig => {
    config.env = newConfig;
    // console.log(`In the end, new config env is ${Object.keys(config.env)}`);
    return config;
  });
};

class VaultVariables {
  constructor (vaultRole, vaultSecret) {
    this._vault_components = {vault_token: undefined};//TODO add role and secret?

    this._vault_role = vaultRole;
    this._vault_secret = vaultSecret;
    this._config_object = {};
  }

  getVaultToken() {
    const vaultRole = this._vault_role; //TODO can these be "this"?
    const vaultSecret = this._vault_secret;
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

      req.write(`{\n"role_id": "${vaultRole}",\n"secret_id": "${vaultSecret}"\n}`);
      req.end();
    });
  }

  //If vault token has been retreived, will use existing
  // _storeVaultToken() {
  //   const vaultComponents = this._vault_components;
  //   const getVaultToken = this.getVaultToken;
  //   return new Promise(function (resolve, reject) {
  //     if (vaultComponents.vault_token === undefined) {
  //       getVaultToken().then(token => {
  //         vaultComponents.vault_token = token;
  //         resolve();
  //       }, error => {
  //         reject(error);
  //       });
  //     }
  //     else {
  //       resolve();
  //     }
  //   });
  // }

  _storeVaultToken(vaultToken){
    this._vault_token = vaultToken;
  }

  getVaultSecrets(vaultUrl, vaultToken) {
    const pathComponents = vaultUrl.match(RegExp('https?://([\\w|.]+)(/.*)'));
    const hostname = pathComponents[1];
    const path = pathComponents[2];

    return new Promise(function (resolve, reject) {
      var options = {
        'hostname': hostname,
        'path': path,
        'headers': {
          'cache-control': 'no-cache',
          'x-vault-token': vaultToken,
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
          resolve(variables);
        });
      }).on('error', (e) => {
        reject(Error(`Something went wrong requesting a secret from Okta:\n${e}`));
      });

      req.end();
    });
  }

  addVariablesToConfigWORKING(vaultToken, vaultUrl, varsToStore) {
    const configObj = this._config_object;
    const getVaultSecrets = this.getVaultSecrets;
    return new Promise(function (resolve, reject) {
      getVaultSecrets(vaultUrl, vaultToken).then(vaultVariables => {
        varsToStore.forEach(varName => {
          configObj[varName] = vaultVariables[varName];
        });
        resolve();
      }, error => {
        reject(error);
      });
    });
  }

  //returns the config - do we want/need this?
  addVariablesToConfig(vaultToken, vaultUrl, varsToStore) {
    const configObj = this._config_object;
    const getVaultToken = this._storeVaultToken;
    const getVaultSecrets = this.getVaultSecrets;
    const vaultComponents = this._vault_components;

    return new Promise(function (resolve, reject) {
      getVaultSecrets(vaultUrl, vaultToken).then(vaultVariables => {
        varsToStore.forEach(varName => {
          configObj[varName] = vaultVariables[varName];
        });
        resolve();
      }, error => {
        reject(error);
      });
    });
  }

  /**
  * Given an object where properties are vault urls and their values are an array of secrets
  * to retreive from that vault.
  * @example
  * { "https://vault.crossroads.net/env/staging/testUsers": ["TEST_USER_1_ID", "TEST_USER_1_PW"] }
  **/
  addVariablesToConfigBulk(vaultToken, pathWithVariables) {
    const vaultPathList = Object.keys(pathWithVariables);

    const promiseArray = vaultPathList.map((path) => {
      const varsToStore = pathWithVariables[path];
      return this.addVariablesToConfig(vaultToken, path, varsToStore);
    });

    return Promise.all(promiseArray);
    // .then(() => {
    //   return vvm.config;
    // }, error => {
    //   throw error;
    // });
  }

  get config() {
    return this._config_object;
  }
}
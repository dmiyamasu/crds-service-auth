module.exports = (on, config) => {
  const vaultsAndVarNames = {
    'Crossroads.Service.Auth': ['OKTA_OAUTH_BASE_URL'],
    'common': ['CRDS_GATEWAY_BASE_URL'],
    'Crossroads.Automation': ['OKTA_TOKEN_AUTH', 'BEN_KENOBI_PW']
  };

  const vaultVariables = new VaultVariables(config.env.VAULT_ROLE_ID, config.env.VAULT_SECRET_ID);
  return vaultVariables.getVariablesFromManyVaults(config.env.CRDS_ENV, vaultsAndVarNames).then(vaultConfig => {
    config.env = vaultConfig;
    return config;
  });
};

const https = require('https');
class VaultVariables {
  constructor (vaultRole, vaultSecret) {
    this._vault_role = vaultRole;
    this._vault_secret = vaultSecret;
    this._variable_config = {};
  }

  /**
   * @returns {Object} the class's config object
   */
  get config() {
    return this._variable_config;
  }

  /**
   * Fetches and stores one, many or all variables from Vault in the class's config object.
   * @param {String} vaultEnvironment - the Vault's environment
   * @param {String} vaultName - Vault name
   * @param {String | Object[]} vaultVariableNames - a single variable name OR an array. The array can be empty (to store all variables from vault) or have variable names (to store only those variables).
   * @returns {Promise} the updated config object OR an error
   * @example
   * getVariablesFromOneVault("staging", "testVariables", "SECRET_VARIABLE"); //Stores one variable
   * getVariablesFromOneVault("staging", "testUsers", ["TEST_USER_1_ID", "TEST_USER_1_PW"]); //Stores both variables
   * getVariablesFromOneVault("staging", "testConstants": []); //Stores all variables
   */
  getVariablesFromOneVault(vaultEnvironment, vaultName, vaultVariableNames) {
    if (!Array.isArray(vaultVariableNames)) {
      vaultVariableNames = [vaultVariableNames];
    }

    return this._getVaultToken().then(vaultToken => {
      return this._fetchAndAddVariablesToConfig(vaultToken, vaultEnvironment, vaultName, vaultVariableNames);
    });
  }

  /**
   * Fetches and stores one, many, or all variables from each Vault in the class's config object.
   * @param {String} vaultEnvironment - the Vault's environment
   * @param {Object} vaultAndVariableNames - an object where properties are Vault names with an array assigned them. The array can be empty (to store all variables from vault) or have variable names (to store only those variables).
   * @returns {Promise} the updated config object OR an error
   * @example
   * getVariablesFromManyVaults (
   * 'staging',
   * { "testUsers": ["TEST_USER_1_ID", "TEST_USER_1_PW"], //Stores both variables
   *   "testVariables": ["SECRET_VARIABLE"] }, //Stores one variable
   *   "testConstants": [] }); //Stores all variables
   */
  getVariablesFromManyVaults(vaultEnvironment, vaultAndVariableNames) {
    const vaultNames = Object.keys(vaultAndVariableNames);
    const config = this._variable_config;

    return this._getVaultToken().then(vaultToken => {
      const storeVarsPromiseArray = vaultNames.map(vault => {
        const variableNames = vaultAndVariableNames[vault];
        return this._fetchAndAddVariablesToConfig(vaultToken, vaultEnvironment, vault, variableNames);
      });

      return Promise.all(storeVarsPromiseArray).then(() => {
        return config;
      });
    });
  }

  /**
   * Fetches the x-vault-token needed to query Vault for secrets.
   * @returns {Promise} the token's value OR an error
   */
  _getVaultToken() {
    const vaultRole = this._vault_role;
    const vaultSecret = this._vault_secret;

    return new Promise(function (resolve, reject) {
      var options = {
        'method': 'POST',
        'hostname': 'vault.crossroads.net',
        'path': '/v1/auth/approle/login',
        'headers': {
          'cache-control': 'no-cache'
        },
        'timeout': 5000
      };

      var req = https.request(options, function (res) {
        var chunks = [];

        res.on('data', function (chunk) {
          chunks.push(chunk);
        });

        res.on('end', function () {
          var body = Buffer.concat(chunks);

          if (res.statusCode === 200) {
            const vaultToken = JSON.parse(body).auth.client_token;
            resolve(vaultToken);
          }
          else {
            reject(Error(`Requesting the Vault token failed with status code ${res.statusCode}. Response was:\n${body}`));
          }
        });
      }).on('error', (e) => {
        reject(Error(`Something went wrong retreiving the Vault token:\n${e}`));
      });

      req.write(`{\n"role_id": "${vaultRole}",\n"secret_id": "${vaultSecret}"\n}`);
      req.end();
    });
  }

  /**
   * Helper function that fetches the desired variables (or all variables) from Vault and stores them in the class's config object.
   * @param {String} vaultToken - x-vault-token
   * @param {String} vaultEnvironment - the Vault's environment
   * @param {String} vaultName - Vault name
   * @param {Object[]} vaultVariableNames - array of variables to retrieve from Vault OR and empty array
   * @returns {Promise} the updated config object OR an error
   */
  _fetchAndAddVariablesToConfig(vaultToken, vaultEnvironment, vaultName, vaultVariableNames) {
    const config = this._variable_config;
    const getVaultSecrets = this._getVaultSecrets;

    if (!Array.isArray(vaultVariableNames)) {
      throw Error(`Expected an array, but given ${vaultVariableNames} which is a ${typeof (vaultVariableNames)}.`);
    }

    return new Promise(function (resolve, reject) {
      getVaultSecrets(vaultToken, vaultEnvironment, vaultName).then(vaultVariables => {
        if (vaultVariableNames.length == 0) {
          Object.assign(config, vaultVariables);
        }
        else {
          vaultVariableNames.forEach(varName => {
            config[varName] = vaultVariables[varName];

            if (vaultVariables[varName] === undefined) {
              reject(Error(`${varName} could not be found in ${vaultName}`));
            }
          });
        }

        resolve(config);
      }, error => {
        reject(error);
      });
    });
  }

  /**
   * Fetches the secrets stored at the given Vault name.
   * @param {String} vaultToken - x-vault-token
   * @param {String} vaultEnvironment - the Vault's environment
   * @param {String} vaultName - Vault name
   * @returns {Promise} an object with the secret's keys and their values OR an error
   */
  _getVaultSecrets(vaultToken, vaultEnvironment, vaultName) {
    return new Promise(function (resolve, reject) {
      var options = {
        'hostname': 'vault.crossroads.net',
        'path': `/v1/kv/data/${vaultEnvironment}/${vaultName}`,
        'headers': {
          'cache-control': 'no-cache',
          'x-vault-token': vaultToken,
        },
        'timeout': 5000
      };

      var req = https.get(options, function (res) {
        var chunks = [];

        res.on('data', function (chunk) {
          chunks.push(chunk);
        });

        res.on('end', function () {
          var body = Buffer.concat(chunks);

          if (res.statusCode === 200) {
            const variables = JSON.parse(body).data.data;
            resolve(variables);
          }
          else {
            reject(Error(`Requesting a Vault secret failed with status code ${res.statusCode}. Response was:\n${body}`));
          }
        });
      }).on('error', (e) => {
        reject(new Error(`Something went wrong requesting a Vault secret:\n${e}`));
      });

      req.end();
    });
  }
}
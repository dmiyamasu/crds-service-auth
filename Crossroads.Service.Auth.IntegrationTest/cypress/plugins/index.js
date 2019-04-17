module.exports = (on, config) => {
  let vaultUrls = config.env.VAULT_VAR_SOURCE;
  if(typeof vaultUrls === 'string') {
    vaultUrls = JSON.parse(vaultUrls);
  }

  const vaultVariables = new VaultVariables(config.env.VAULT_ROLE_ID, config.env.VAULT_SECRET_ID);
  return vaultVariables.getVariablesFromManyVaults(vaultUrls).then(vaultConfig => {
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
   * Fetches and stores each variable from Vault in the class's config object.
   * @param {String} vaultURL - fully qualified Vault url
   * @param {String | Object[]} vaultVariableNames - a single variable name OR an array of variable names to retrieve from Vault
   * @returns {Promise} the updated config object OR an error
   * @example
   * getVariablesFromOneVault("https://vault.crossroads.net/env/staging/testVariables", "SECRET_VARIABLE");
   * getVariablesFromOneVault("https://vault.crossroads.net/env/staging/testUsers", ["TEST_USER_1_ID", "TEST_USER_1_PW"]);
   */
  getVariablesFromOneVault(vaultURL, vaultVariableNames) {
    if (!Array.isArray(vaultVariableNames)) {
      vaultVariableNames = [vaultVariableNames];
    }

    return this._getVaultToken().then(vaultToken => {
      return this._fetchAndAddVariablesToConfig(vaultToken, vaultURL, vaultVariableNames);
    });
  }

  /**
   * Fetches and stores each variable from each Vault url in the class's config object.
   * @param {Object} vaultURLsAndVariables - an object where properties are Vault URLs with an array of Vault variable names assigned them.
   * @returns {Promise} the updated config object OR an error
   * @example
   * getVariablesFromManyVaults (
   * { "https://vault.crossroads.net/env/staging/testUsers": ["TEST_USER_1_ID", "TEST_USER_1_PW"],
   *   "https://vault.crossroads.net/env/staging/testVariables": ["SECRET_VARIABLE"] });
   */
  getVariablesFromManyVaults(vaultURLsAndVariables) {
    const vaultURLs = Object.keys(vaultURLsAndVariables);
    const config = this._variable_config;

    return this._getVaultToken().then(vaultToken => {
      const storeVarsPromiseArray = vaultURLs.map(url => {
        const variableNames = vaultURLsAndVariables[url];
        return this._fetchAndAddVariablesToConfig(vaultToken, url, variableNames);
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
   * Helper function that fetches the desired variables from Vault and stores them in the class's config object.
   * @param {String} vaultToken - x-vault-token
   * @param {String} vaultUrl - fully qualified Vault url
   * @param {Object[]} vaultVariableNames - array of variables to retrieve from Vault
   * @returns {Promise} the updated config object OR an error
   */
  _fetchAndAddVariablesToConfig(vaultToken, vaultUrl, vaultVariableNames) {
    const config = this._variable_config;
    const getVaultSecrets = this._getVaultSecrets;

    if (!Array.isArray(vaultVariableNames)) {
      throw Error(`Expected an array, but given ${vaultVariableNames} which is a ${typeof(vaultVariableNames)}.`);
    }

    return new Promise(function (resolve, reject) {
      getVaultSecrets(vaultToken, vaultUrl).then(vaultVariables => {
        vaultVariableNames.forEach(varName => {
          config[varName] = vaultVariables[varName];

          if (vaultVariables[varName] === undefined) {
            reject(Error(`${varName} could not be found in ${vaultUrl}`));
          }
        });

        resolve(config);
      }, error => {
        reject(error);
      });
    });
  }

  /**
   * Fetches the secrets stored at the given Vault url.
   * @param {String} vaultToken - x-vault-token
   * @param {String} vaultUrl - fully qualified Vault url
   * @returns {Promise} an object with the secret's keys and their values OR an error
   */
  _getVaultSecrets(vaultToken, vaultUrl) {
    const urlPattern = 'https?://([\\w|.]+)(/.*)';
    let hostname;
    let path;

    try {
      const pathComponents = vaultUrl.match(RegExp(urlPattern));
      hostname = pathComponents[1];
      path = pathComponents[2];
    } catch (e) {
      throw Error(`Vault url "${vaultUrl}" did not match pattern "${urlPattern}".\n${e}`);
    }

    return new Promise(function (resolve, reject) {
      var options = {
        'hostname': hostname,
        'path': path,
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
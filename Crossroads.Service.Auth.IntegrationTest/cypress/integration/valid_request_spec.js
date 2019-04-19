/**
 * Tests will run for each user scenario in the list below.
 */
const userScenario = [
  {
    description: 'user with all record ids and one role',
    fixtureFile: 'testUser_allRecords_1Role.json',
    userPassword: `${Cypress.env('BEN_KENOBI_PW')}`
  }
];

userScenario.forEach(scenario => {
  describe(`Tests response for current MP tokens for a ${scenario.description}`, function () {
    let mpResponseBody;
    before(function () {
      cy.fixture(scenario.fixtureFile).then(testUser => {
        cy.request({
          method: 'POST',
          url: `${Cypress.env('CRDS_GATEWAY_BASE_URL')}/api/login`,
          body: {
            username: testUser.MpUserInfo.Email,
            password: scenario.userPassword
          }
        }).then(response => {
          const mpUserToken = response.body.userToken;

          cy.request({
            method: 'GET',
            url: '/api/authorize',
            headers: { Authorization: mpUserToken }
          }).then(response => {
            expect(response.status).to.eq(200);
            mpResponseBody = response.body;
          });
        });
      });
    });

    it('Request authenticated by MP', function () {
      expect(mpResponseBody).to.have.property('Authentication').and.have.property('Provider', 'mp');
    });

    it('Request contains MpRoles', function () {
      verifyMpRoles(mpResponseBody, scenario.fixtureFile);
    });

    it('Request does not contain OktaRoles', function () {
      expect(mpResponseBody).to.have.property('Authorization').and.have.property('OktaRoles', null);
    });

    it('Request contains Mp User Info', function () {
      verifyMPUserInfo(mpResponseBody, scenario.fixtureFile);
    });
  });

  describe(`Tests response for current Okta tokens for a ${scenario.description}`, function () {
    let oktaResponseBody;
    before(function () {
      cy.fixture(scenario.fixtureFile).then(testUser => {
        cy.request({
          method: 'POST',
          url: `${Cypress.env('OKTA_OAUTH_BASE_URL')}/v1/token`,
          headers: { authorization: `${Cypress.env('OKTA_TOKEN_AUTH')}` },
          form: true,
          body: {
            grant_type: 'password',
            username: testUser.MpUserInfo.Email,
            password: scenario.userPassword,
            scope: 'openid'
          }
        }).then(response => {
          const oktaUserToken = response.body.access_token;

          cy.request({
            method: 'GET',
            url: '/api/authorize',
            headers: { Authorization: oktaUserToken }
          }).then(response => {
            expect(response.status).to.eq(200);
            oktaResponseBody = response.body;
          });
        });
      });
    });

    it('Request authenticated by Okta', function () {
      expect(oktaResponseBody).to.have.property('Authentication').and.have.property('Provider', 'okta');
    });

    it('Request contains MpRoles', function () {
      verifyMpRoles(oktaResponseBody, scenario.fixtureFile);
    });

    it('Request contains OktaRoles', function () {
      verifyOktaRoles(oktaResponseBody, scenario.fixtureFile);
    });

    it('Request contains Mp User Info', function () {
      verifyMPUserInfo(oktaResponseBody, scenario.fixtureFile);
    });
  });
});

/*
* Verify properties of responses
*/
function verifyMpRoles(responseBody, userFixture) {
  expect(responseBody).to.have.property('Authorization').and.have.property('MpRoles');
  cy.fixture(userFixture).then(testUser => {
    _verifyObjectPropertiesMatch(testUser.MpRoles, responseBody.Authorization.MpRoles, 'MpRoles in response');
  });
}

function verifyOktaRoles(responseBody, userFixture) {
  expect(responseBody).to.have.property('Authorization').and.have.property('OktaRoles');
  cy.fixture(userFixture).then(testUser => {
    _verifyObjectPropertiesMatch(testUser.OktaRoles, responseBody.Authorization.OktaRoles, 'OktaRoles in response');
  });
}

function verifyMPUserInfo(responseBody, userFixture) {
  expect(responseBody).to.have.property('UserInfo').and.have.property('Mp');
  cy.fixture(userFixture).then(testUser => {
    _verifyObjectPropertiesMatch(testUser.MpUserInfo, responseBody.UserInfo.Mp, 'UserInfo.Mp in the response');
  });
}

function _verifyObjectPropertiesMatch(expected, actual, actualObjectDescription = 'Actual object') {
  const expectedKeys = Object.keys(expected);
  const actualKeys = Object.keys(actual);

  expectedKeys.forEach(key => {
    assert.propertyVal(actual, key, expected[key], `${actualObjectDescription} should have the expected property and value`);
  });
  assert.equal(actualKeys.length, expectedKeys.length, `${actualObjectDescription} should have the expected number of properties`);
}
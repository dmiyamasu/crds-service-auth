/**
 * Tests will run for each user scenario in the list below.
 */
const userScenario = [{
  description: 'user with all record ids and one role',
  fixtureFile: 'testUser_allRecords_1Role.json',
  userPassword: `${Cypress.env('BEN_KENOBI_PW')}`
}];

userScenario.forEach(scenario => {
  describe(`Tests response for current MP tokens for a ${scenario.description}`, function () {
    let mpResponseBody;
    before(function () {
      cy.fixture(scenario.fixtureFile).then(testUser => {
        cy.request({
          method: 'POST',
          url: `${Cypress.env('CRDS_GATEWAY_BASE_URL')}/api/login`,
          body: {
            username: testUser.email,
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
            username: testUser.email,
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

    it('Request contains Mp User Info', function () {
      verifyMPUserInfo(oktaResponseBody, scenario.fixtureFile);
    });
  });
});

/*
* Responses for MP and Okta tokens should match
*/
function verifyMpRoles(responseBody, userFixture) {
  expect(responseBody).to.have.property('Authorization').and.have.property('MpRoles');

  const responseMpRoles = responseBody.Authorization.MpRoles;
  cy.fixture(userFixture).then(testUser => {
    //Response should match expected roles exactly
    const expectedIds = Object.keys(testUser.mpRoles);
    const responseIds = Object.keys(responseMpRoles);
    assert.equal(responseIds.length, expectedIds.length, 'Response should have the expected number of roles');
    expectedIds.forEach(expId => {
      assert.include(responseIds, expId, 'Response should have the expected role id');
      assert.propertyVal(responseMpRoles, expId, testUser.mpRoles[expId], 'Response should have the expected role name');
    });
  });
}

function verifyMPUserInfo(responseBody, userFixture) {
  expect(responseBody).to.have.property('UserInfo').and.have.property('Mp');

  cy.fixture(userFixture).then(testUser => {
    const userInfo = responseBody.UserInfo.Mp;
    expect(userInfo).to.have.property('ContactId', testUser.contactId);
    expect(userInfo).to.have.property('UserId', testUser.userId);
    expect(userInfo).to.have.property('ParticipantId', testUser.participantId);
    expect(userInfo).to.have.property('HouseholdId', testUser.householdId);
    expect(userInfo).to.have.property('Email', testUser.email);
    expect(userInfo).to.have.property('DonorId', testUser.donorId);
    expect(userInfo).to.have.property('CanImpersonate', testUser.canImpersonate);
  });
}
/*
* Responses for MP and Okta tokens should match
*/
function verifyMpRoles(responseBody) {
  expect(responseBody).to.have.property('Authorization').and.have.property('MpRoles');

  const mpRoles = responseBody.Authorization.MpRoles;
  cy.fixture('authUser.json').then(authUser => {
    authUser.mpRoles.forEach(role => {
      expect(mpRoles).to.have.property(role.id, role.name);
    });
  });
}

function verifyUserInfo(responseBody) {
  expect(responseBody).to.have.property('UserInfo').and.have.property('Mp');

  cy.fixture('authUser.json').then(authUser => {
    const userInfo = responseBody.UserInfo.Mp;
    expect(userInfo).to.have.property('ContactId', authUser.contactId);
    expect(userInfo).to.have.property('UserId', authUser.userId);
    expect(userInfo).to.have.property('ParticipantId', authUser.participantId);
    expect(userInfo).to.have.property('HouseholdId', authUser.householdId);
    expect(userInfo).to.have.property('Email', authUser.email);
    expect(userInfo).to.have.property('DonorId', authUser.donorId);
    expect(userInfo).to.have.property('CanImpersonate', authUser.canImpersonate);
  });
}

describe('Tests response for current MP tokens', function () {
  let mpResponseBody;
  before(function () {
    cy.fixture('authUser.json').then(authUser => {
      cy.request({
        method: 'POST',
        url: `${Cypress.env('CRDS_GATEWAY_BASE_URL')}/api/login`,
        body: {
          username: authUser.email,
          password: `${Cypress.env('BEN_KENOBI_PW')}`
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
    verifyMpRoles(mpResponseBody);
  });

  it('Request contains Mp User Info', function () {
    verifyUserInfo(mpResponseBody);
  });
});

describe('Tests response for current Okta tokens', function () {
  let oktaResponseBody;
  before(function () {
    cy.fixture('authUser.json').then(authUser => {
      cy.request({
        method: 'POST',
        url: `${Cypress.env('OKTA_OAUTH_BASE_URL')}/v1/token`,
        headers: { authorization: `${Cypress.env('OKTA_TOKEN_AUTH')}` },
        form: true,
        body: {
          grant_type: 'password',
          username: authUser.email,
          password: `${Cypress.env('BEN_KENOBI_PW')}`,
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
    verifyMpRoles(oktaResponseBody);
  });

  it('Request contains Mp User Info', function () {
    verifyUserInfo(oktaResponseBody);
  });
});
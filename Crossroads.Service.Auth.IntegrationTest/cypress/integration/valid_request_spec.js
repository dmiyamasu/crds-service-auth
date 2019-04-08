//TODO need to do this based on env - can have access to vault?
describe('Tests response for current MP tokens', function () {
  let authResponse;
  before(function () {
    cy.request({
      method: 'POST',
      url: `${Cypress.env('MP_LOGIN_ENDPOINT')}/gateway/api/login`,
      body: { username: `${Cypress.env('AUTH_USER_EMAIL')}`, password: `${Cypress.env('AUTH_USER_PW')}` },
      log: false
    }).then(response => {
      return response.body.userToken;

      //cy.setCookie('userId', response.body.userId.toString(), { domain: domain, log: false });
    }).then(sessionId => {
      cy.request({
        method: 'GET',
        url: `${Cypress.config().baseUrl}/api/authorize`,
        headers: { Authorization: sessionId }
      }).then(response => {
        expect(response.status).to.eq(200);
        // //Want the call to 'fail' if it takes too long to respond? We can do this!
        expect(response.duration).to.be.lte(1000);

        authResponse = response;//.body;
      });
    });
  });

  it('Request authenticated by MP', function () {
    cy.wrap({ authResponse }).its('authResponse.body').should('have.property', 'Authentication').and('have.property', 'Provider', 'mp');
  });

  it('Request contains MpRoles', function () {

    cy.wrap({ authResponse }).its('authResponse.body').should('have.property', 'Authorization').should('have.property', 'MpRoles').then(mpRoles => {
      expect(mpRoles).to.have.property('39', 'All Platform Users');
      //expect(mpRoles).to.have.property('116', 'Manage Events Tools - CRDS');
    });
  });

  it('Request contains Mp User Info', function () {
    cy.wrap({ authResponse }).its('authResponse.body').should('have.property', 'UserInfo').should('have.property', 'Mp').then(userInfo => {
      expect(userInfo).to.have.property('ContactId', 7772248);
      expect(userInfo).to.have.property('UserId', 4488274);
      expect(userInfo).to.have.property('ParticipantId', 7654359);
      expect(userInfo).to.have.property('HouseholdId', 5819396);
      expect(userInfo).to.have.property('Email', `${Cypress.env('AUTH_USER_EMAIL')}`);
      expect(userInfo).to.have.property('DonorId', 7745938);
      expect(userInfo).to.have.property('CanImpersonate', false);
    });
  });
});

//response 200 for valid me
// {
//   "Authentication": {
//       "Provider": "mp"
//   },
//   "Authorization": {
//       "MpRoles": {
//           "39": "All Platform Users",
//           "84": "User Administrator - CRDS",
//           "85": "Security Administrator - CRDS",
//           "101": "Finance Data Admin - CRDS",
//           "106": "Finance Management - CRDS",
//           "107": "System Administrator - CRDS",
//           "109": "Hidden Fields - CRDS",
//           "111": "Finance Tools - CRDS",
//           "112": "Kids Club Tools - CRDS",
//           "116": "Manage Events Tools - CRDS",
//           "1002": "Attendance Create/View",
//           "1003": "Attendance Create/View/Edit",
//           "1006": "Batch Manager Administrators",
//           "1013": "Product Coordinator - CRD",
//           "1014": "Facilities App",
//           "1016": "FRED - CRDS",
//           "1021": "FRED Report Tool - CRDS"
//       },
//       "OktaRoles": null
//   },
//   "UserInfo": {
//       "Mp": {
//           "ContactId": 7697116,
//           "UserId": 4451263,
//           "ParticipantId": 7586715,
//           "HouseholdId": 5774197,
//           "Email": "shenney@callibrity.com",
//           "DonorId": 7736119,
//           "CanImpersonate": true
//       }
//   }
// }
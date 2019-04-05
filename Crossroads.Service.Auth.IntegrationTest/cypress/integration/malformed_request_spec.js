describe('Tests malformed requests', function () {
  const malformedResponse = 'Unable to decode token, it was malformed, empty, or null';
  const malformedTokens = ['123456', '', null, undefined];

  malformedTokens.forEach(token => {
    it(`Request cannot be decoded given token '${token}'`, function () {
      cy.request({
        method: 'GET',
        url: `${Cypress.config().baseUrl}/api/authorize`,
        headers: { Authorization: token },
        failOnStatusCode: false
      }).then(response => {
        expect(response.body).to.contain(malformedResponse);
        expect(response.status).to.eq(400);
        //Want the call to 'fail' if it takes too long to respond? We can do this!
        expect(response.duration).to.be.lte(1000);
      });
    });
  });

  it('Request cannot be decoded when missing headers', function () {
    cy.request({
      method: 'GET',
      url: `${Cypress.config().baseUrl}/api/authorize`,
      failOnStatusCode: false
    }).then(response => {
      expect(response.body).to.contain(malformedResponse);
      expect(response.status).to.eq(400);
      //Want the call to 'fail' if it takes too long to respond? We can do this!
      expect(response.duration).to.be.lte(1000);
    });
  });

  it('Request cannot be decoded when headers have misspelled keys', function () {
    const expiredMPToken = 'eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSIsImtpZCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSJ9.eyJpc3MiOiJGb3JtcyIsImF1ZCI6IkZvcm1zL3Jlc291cmNlcyIsImV4cCI6MTU0MTAxMDEzNSwibmJmIjoxNTQxMDA4MzM1LCJjbGllbnRfaWQiOiJDUkRTLkNvbW1vbiIsInNjb3BlIjpbIm9wZW5pZCIsIm9mZmxpbmVfYWNjZXNzIiwiaHR0cDovL3d3dy50aGlua21pbmlzdHJ5LmNvbS9kYXRhcGxhdGZvcm0vc2NvcGVzL2FsbCJdLCJzdWIiOiJiZDg4ZTk3Mi00ZDEwLTRmNzAtOGM3Zi04ZTUzZTg5YTAwNDgiLCJhdXRoX3RpbWUiOjE1Mzk5NTE4NDEsImlkcCI6Imlkc3J2IiwibmFtZSI6ImRjb3VydHNAY2FsbGlicml0eS5jb20iLCJhbXIiOlsicGFzc3dvcmQiXX0.yx4INlKhWf63zrt0qJtWzfqsHDGQ-LBXo7Outzy9nLCWyw3vHbtRTv4RzjMMg9GkRhdD0woMXhkfp4j9WKw_fJLlZU_A_T9P0NAvXJWMhsZnQwZzTn-xb6_WSqp7QzGwj8JAMXgd1-fvuwA6bflHwrGtEQYr3SMpq8nhHq9KTBCHOMY4-8tKZ1Me0mgCdemeMeYyjlDkuUFJHxWy1L7l4ZhFc1GFxZK5bbzRlqmMcpLSuMw2fb8UVBsRgcWfV3-xLDTqlTfrKLyBKBX5FUplxZqQic-c9d6PpgN_axR5Nss504nNgtPx32eBch-S-_RupM9hdY62ml85UxPgOSL1LA';

    cy.request({
      method: 'GET',
      url: `${Cypress.config().baseUrl}/api/authorize`,
      headers: { authorisation: expiredMPToken },
      failOnStatusCode: false
    }).then(response => {
      expect(response.body).to.contain(malformedResponse);
      expect(response.status).to.eq(400);
      //Want the call to 'fail' if it takes too long to respond? We can do this!
      expect(response.duration).to.be.lte(1000);
    });
  });
});
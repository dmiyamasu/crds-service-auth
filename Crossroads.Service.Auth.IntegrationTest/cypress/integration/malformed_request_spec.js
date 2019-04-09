describe('Tests malformed requests', function () {
  const malformedResponse = 'Unable to decode token, it was malformed, empty, or null';
  const malformedTokens = ['123456', '', null, undefined];

  malformedTokens.forEach(token => {
    it(`Request cannot be decoded given token '${token}'`, function () {
      cy.request({
        method: 'GET',
        url: '/api/authorize',
        headers: { Authorization: token },
        failOnStatusCode: false
      }).then(response => {
        expect(response.body).to.contain(malformedResponse);
        expect(response.status).to.eq(400);
      });
    });
  });

  it('Request cannot be decoded when missing headers', function () {
    cy.request({
      method: 'GET',
      url: '/api/authorize',
      failOnStatusCode: false
    }).then(response => {
      expect(response.body).to.contain(malformedResponse);
      expect(response.status).to.eq(400);
    });
  });

  it('Request cannot be decoded when headers have misspelled keys', function () {
    const expiredOktaToken = 'eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSIsImtpZCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSJ9.eyJpc3MiOiJGb3JtcyIsImF1ZCI6IkZvcm1zL3Jlc291cmNlcyIsImV4cCI6MTU1NDc0OTMwNiwibmJmIjoxNTU0NzQ3NTA2LCJjbGllbnRfaWQiOiJDUkRTLkNvbW1vbiIsInNjb3BlIjpbImh0dHA6Ly93d3cudGhpbmttaW5pc3RyeS5jb20vZGF0YXBsYXRmb3JtL3Njb3Blcy9hbGwiLCJvZmZsaW5lX2FjY2VzcyIsIm9wZW5pZCJdLCJzdWIiOiI4YTI5MDA5Mi0zNDU5LTRmNWEtOTc3ZS01Y2M3YjAyZTczNDIiLCJhdXRoX3RpbWUiOjE1NTQ3NDc1MDYsImlkcCI6Imlkc3J2IiwibmFtZSI6Im1wY3JkcythdXRvKzJAZ21haWwuY29tIiwiYW1yIjpbInBhc3N3b3JkIl19.PJuvVth43CIkH1UEgM9n2QXqW91dXwJxn19W_QKQqMbUGvj7d5MVL3aBRfD7O_sZzW3mbZM3LkNBN1caVVVvizi88mqwwf2ywGHWqS-fhDxBoTyhMJ0emm5ZfWdpDHMTh5r0bG3jYys0voY1sNVu1F3QY9_BXFyorHdwS178juHjtNe6hhRNoVJG6Mg82NMr_piaqCCvNIiehlMlpf4B458JDUanbp63PiCHZhWK3Y4LIPpg1MQ7xadI14KMnEgIMoy4nLapK6-dv52Oe-9Qz-sejbQeaUAHRhUDJjkFwZ0xE8jyLGwzXp35uWROXjt8HnbsTobvwKoat7rDVBGdIg';

    cy.request({
      method: 'GET',
      url: '/api/authorize',
      headers: { authorisation: expiredOktaToken },
      failOnStatusCode: false
    }).then(response => {
      expect(response.body).to.contain(malformedResponse);
      expect(response.status).to.eq(400);
    });
  });
});
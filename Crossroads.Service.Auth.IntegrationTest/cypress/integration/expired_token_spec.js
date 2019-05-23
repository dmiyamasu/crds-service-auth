describe('Tests expired token response', function () {
  const expiredResponse = 'Lifetime validation failed. The token is expired.';

  it('Expired MP token', function () {
    const expiredMPToken = 'eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSIsImtpZCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSJ9.eyJpc3MiOiJGb3JtcyIsImF1ZCI6IkZvcm1zL3Jlc291cmNlcyIsImV4cCI6MTU1NTYxNjc0NSwibmJmIjoxNTU1NjE0OTQ1LCJjbGllbnRfaWQiOiJDUkRTLkNvbW1vbiIsInNjb3BlIjpbImh0dHA6Ly93d3cudGhpbmttaW5pc3RyeS5jb20vZGF0YXBsYXRmb3JtL3Njb3Blcy9hbGwiLCJvZmZsaW5lX2FjY2VzcyIsIm9wZW5pZCJdLCJzdWIiOiI4YTI5MDA5Mi0zNDU5LTRmNWEtOTc3ZS01Y2M3YjAyZTczNDIiLCJhdXRoX3RpbWUiOjE1NTU2MTQ5NDUsImlkcCI6Imlkc3J2IiwibmFtZSI6Im1wY3JkcythdXRvKzJAZ21haWwuY29tIiwiYW1yIjpbInBhc3N3b3JkIl19.LlS8PW7LgjOGokVXNVT11XiBbFyHBZQfJyEg1pzTtRVf0NQVqZPNGrHyefEwOhKa8QhhzKhDJ5D7XFeVIHkV-tvX5mEBn5d0_AeWB1igMVB3SNNQHkOEIi4N10fue_tLY6MJpKNvgYNnIkpYGi2Tb5OA3PhSi3FlTFm22rinpwVfyNCcO_tOh6TeK11yq2KJudqVWqmthjLW1bbXd58y57VTdmu65pvwvHpjFMbsbVCTXxn9rYi_OdYjk79cg_Jy0b4ema9dW-Sh9-LP4exu0kGSISfrkYuaLMOVLdb3MIiwTr_yKpNkH6w-bWcSFqruzf7r_kdcPkwwuUzXtAWIKA';

    cy.request({
      method: 'GET',
      url: '/api/authorize',
      headers: { Authorization: expiredMPToken },
      failOnStatusCode: false
    }).then(response => {
      expect(response.body).to.contain(expiredResponse);
      expect(response.status).to.eq(403);
    });
  });

  //TODO Skipping this test until there is a reliable way to get valid expired Okta tokens.
  //  Expired tokens become invalid tokens when Okta changes its signing key and there is not an easy way to get a valid expired token programmatically.
  //  This test should be updated when the auth service has a way to provide expired tokens on request.
  it.skip('Expired Okta token', function () {
    const expiredOktaToken = 'eyJraWQiOiJ3eXlrOXRtS0VzSzdGNHlBSW5oX1gteGNJTlF6LXltc0VCbHNMUUExeWpFIiwiYWxnIjoiUlMyNTYifQ.eyJ2ZXIiOjEsImp0aSI6IkFULmhvWWlrZmdDRHZwa29hSEh3aUlRa2U0c3RlcWVldmYzS2VGWVRrSXVOZFEiLCJpc3MiOiJodHRwczovL2Nyb3Nzcm9hZHMub2t0YXByZXZpZXcuY29tL29hdXRoMi9kZWZhdWx0IiwiYXVkIjoiYXBpOi8vZGVmYXVsdCIsImlhdCI6MTU1NTYxNTM0OSwiZXhwIjoxNTU1NjE3MTQ5LCJjaWQiOiIwb2FrNzZncjltaUpJRklDSjBoNyIsInVpZCI6IjAwdWkzOG0xd21yeTZVeUNDMGg3Iiwic2NwIjpbIm9wZW5pZCJdLCJzdWIiOiJtcGNyZHMrYXV0bysyQGdtYWlsLmNvbSIsIm1wQ29udGFjdElkIjoiNzc3MjI0OCJ9.pwc00xXR1v616oAaQ6JudMjWpf3jpgrsTuJu1ztHksH_6mMUrFyf4jdJlBE0fKWI7D2mFLi_iFlbyynJeNlXBkIMvAFnWICOMB4k4bsk8tq_M07ZOZ42wRXbJbtqAWkmVDUDAK332LpphafMqEKc-7p9qK2sG43qWye7I2l6Ft9hYALjQoLzPATevDdOMLr_i2NX-nz6dDKINqTTqZHj15XA6NXiwb_BSn9aQjdJLg6DaH1lz6NyBCJKnWRKqG_sBbP6Qss3JbMDaOpT0KwcQ1VzpLLKAaZpj3oQNRFwWVkxj45_Tvn5lzFsJQGEP4i-3u3JawK-2A9nswukqXGRsQ';

    cy.request({
      method: 'GET',
      url: '/api/authorize',
      headers: { Authorization: expiredOktaToken },
      failOnStatusCode: false
    }).then(response => {
      expect(response.body).to.contain(expiredResponse);
      expect(response.status).to.eq(403);
    });
  });
});
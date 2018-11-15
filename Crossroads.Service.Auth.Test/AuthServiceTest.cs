using System;
using Xunit;
using Moq;
using Crossroads.Service.Auth.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Service.Auth.Services;
using System.Threading.Tasks;
using Crossroads.Service.Auth.Models;
using static Crossroads.Service.Auth.Services.JwtService;
using Microsoft.IdentityModel.Tokens;
using Crossroads.Service.Auth.Exceptions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Crossroads.Service.Auth.Tests
{
    public class AuthServiceTest
    {
        private Mock<IOIDConfigurationService> _configService;
        private Mock<IUserService> _userService;
        private Mock<IApiUserRepository> _apiUserRepository;
        private Mock<JwtService> _jwtService;

        private IAuthService _fixture;

        public AuthServiceTest()
        {
            _configService = new Mock<IOIDConfigurationService>();
            _userService = new Mock<IUserService>();
            _apiUserRepository = new Mock<IApiUserRepository>();
            _jwtService = new Mock<JwtService> { CallBase = true };

            _fixture = new AuthService(_configService.Object, _userService.Object, _apiUserRepository.Object, _jwtService.Object);
        }

        [Fact]
        public async Task GetAuthorization_ThrowsException_TokenMalformedException_TokenMalformed()
        {
            string malformedToken = "eyJ0eXAiOiJKV1Qng1dCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSIsImtpZCI6Ijkyc3c1bmhtbjBQS3N0T0k1YS1nVVZlUC1NWSJ9.eyJpc3MiOiJGb3JtcyIsImF1ZCI6IkZvcm1zL3Jlc291cmNlcyIsImV4cCI6MTU0MjIwNTI5MCwibmJmIjoxNTQyMjAzNDkwLCJjbGllbnRfaWQiOiJDUkRTLkNvbW1vbiIsInNjb3BlIjpbIm9wZW5pZCIsIm9mZmxpbmVfYWNjZXNzIiwiaHR0cDovL3d3dy50aGlua21pbmlzdHJ5LmNvbS9kYXRhcGxhdGZvcm0vc2NvcGVzL2FsbCJdLCJzdWIiOiJiZDg4ZTk3Mi00ZDEwLTRmNzAtOGM3Zi04ZTUzZTg5YTAwNDgiLCJhdXRoX3RpbWUiOjE1Mzk5NTE4NDEsImlkcCI6Imlkc3J2IiwibmFtZSI6ImRjb3VydHNAY2FsbGlicml0eS5jb20iLCJhbXIiOlsicGFzc3dvcmQiXX0.qxK20cNWbiw8hU9FLRVoiI-ivJKri6SORst1Z1l2oTHoeE6iiJe2s18vWLsWrNHLkFdGlTfOd-W-TrWl3SVO4vy03lJcz9tEq6Fkg61eoA7MyY2_R2kF514QROvsxRr_IDITiDPD_ZRKovjjCYz_BzOiHyWq6Nrx5Kqw1sEwrD8gBCFzfTJW600XCesA1GODGUr-zOOexsNVwqBTTm-zGCh_IyCUBjwZsCE1iEpUKu2JKnt79kXP_BwfV0uucXvaZWu7-YTCsIFsQhg_BReI9yK1w8o-qX2EUknWsE42o7_ETy-ku8_NN3Zqde5CC_UrIEbpnFeOyf8xJfp-AmXdeA";

            SetupForTokenMalformedException(malformedToken);

            await Assert.ThrowsAsync<TokenMalformedException>(async () => await _fixture.GetAuthorization(malformedToken));
        }

        [Fact]
        public async Task GetAuthorization_ThrowsException_TokenMalformedException_TokenEmptyString()
        {
            string emptyToken = "";

            SetupForTokenMalformedException(emptyToken);

            await Assert.ThrowsAsync<TokenMalformedException>(async () => await _fixture.GetAuthorization(emptyToken));
        }

        [Fact]
        public async Task GetAuthorization_ThrowsException_TokenMalformedException_TokenNull()
        {
            string nullToken = null;

            SetupForTokenMalformedException(nullToken);

            await Assert.ThrowsAsync<TokenMalformedException>(async () => await _fixture.GetAuthorization(nullToken));
        }

        private void SetupForTokenMalformedException(string token)
        {
            _apiUserRepository.Setup(r => r.GetDefaultApiClientToken()).Returns("");

            _userService.Setup(r => r.GetUserInfo(token, new CrossroadsDecodedToken(), "")).Returns(new UserInfoDTO());
            _userService.Setup(r => r.GetAuthorizations(new CrossroadsDecodedToken(), "", 1234)).Returns(new AuthorizationDTO());

            string mpOpenIdConfig = @"{'scopes_supported': ['openid', 'offline_access', 'http://www.thinkministry.com/dataplatform/scopes/all'], 'userinfo_endpoint': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/userinfo', 'revocation_endpoint': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/revocation', 'jwks_uri': 'https://adminint.crossroads.net/ministryplatformapi/oauth/.well-known/jwks', 'introspection_endpoint': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/introspect', 'frontchannel_logout_session_supported': true, 'frontchannel_logout_supported': true, 'code_challenge_methods_supported': ['plain', 'S256'], 'check_session_iframe': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/checksession', 'grant_types_supported': ['authorization_code', 'client_credentials', 'password', 'refresh_token', 'implicit'], 'token_endpoint': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/token', 'id_token_signing_alg_values_supported': ['RS256'], 'response_modes_supported': ['form_post', 'query', 'fragment'], 'subject_types_supported': ['public'], 'token_endpoint_auth_methods_supported': ['client_secret_post', 'client_secret_basic'], 'response_types_supported': ['code', 'token', 'id_token', 'id_token token', 'code id_token', 'code token', 'code id_token token'], 'claims_supported': [], 'end_session_endpoint': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/endsession', 'authorization_endpoint': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/authorize', 'issuer': 'Forms'}";

            OpenIdConnectConfiguration configuration = new OpenIdConnectConfiguration(mpOpenIdConfig);

            _configService.Setup(r => r.GetMpConfigAsync()).Returns(Task.FromResult(new OpenIdConnectConfiguration(mpOpenIdConfig)));
        }
    }
}

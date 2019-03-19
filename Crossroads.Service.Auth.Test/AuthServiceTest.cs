using Xunit;
using Moq;
using Crossroads.Service.Auth.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Service.Auth.Services;
using System.Threading.Tasks;
using Crossroads.Service.Auth.Exceptions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Crossroads.Web.Auth.Models;

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
            string malformedToken = "a;sldkfjasl;dfjka;lsdfjka;sldfjkas;ldfjasdf";

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

            _userService.Setup(r => r.GetUserInfo(token, new CrossroadsDecodedToken(), "")).Returns(new UserInfo());
            _userService.Setup(r => r.GetAuthorizations(new CrossroadsDecodedToken(), "", 1234)).Returns(new Authorization());

            string mpOpenIdConfig = @"{'scopes_supported': ['openid', 'offline_access', 'http://www.thinkministry.com/dataplatform/scopes/all'], 'userinfo_endpoint': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/userinfo', 'revocation_endpoint': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/revocation', 'jwks_uri': 'https://adminint.crossroads.net/ministryplatformapi/oauth/.well-known/jwks', 'introspection_endpoint': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/introspect', 'frontchannel_logout_session_supported': true, 'frontchannel_logout_supported': true, 'code_challenge_methods_supported': ['plain', 'S256'], 'check_session_iframe': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/checksession', 'grant_types_supported': ['authorization_code', 'client_credentials', 'password', 'refresh_token', 'implicit'], 'token_endpoint': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/token', 'id_token_signing_alg_values_supported': ['RS256'], 'response_modes_supported': ['form_post', 'query', 'fragment'], 'subject_types_supported': ['public'], 'token_endpoint_auth_methods_supported': ['client_secret_post', 'client_secret_basic'], 'response_types_supported': ['code', 'token', 'id_token', 'id_token token', 'code id_token', 'code token', 'code id_token token'], 'claims_supported': [], 'end_session_endpoint': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/endsession', 'authorization_endpoint': 'https://adminint.crossroads.net/ministryplatformapi/oauth/connect/authorize', 'issuer': 'Forms'}";

            OpenIdConnectConfiguration configuration = new OpenIdConnectConfiguration(mpOpenIdConfig);

            _configService.Setup(r => r.GetMpConfigAsync()).Returns(Task.FromResult(new OpenIdConnectConfiguration(mpOpenIdConfig)));
        }
    }
}

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using AstroGathering.Objects;
using System;
using System.Threading.Tasks;

namespace AstroGathering.Services
{
    public class GoogleAuthService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;

        public GoogleAuthService(string clientId, string clientSecret, string redirectUri)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
        }

        public string GetAuthorizationUrl()
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Scopes = new[] { 
                    "https://www.googleapis.com/auth/userinfo.email",
                    "https://www.googleapis.com/auth/userinfo.profile"
                }
            });

            return flow.CreateAuthorizationCodeRequest(_redirectUri).Build().ToString();
        }

        public async Task<User> ProcessAuthorizationCodeAsync(string code)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret
            });

            var token = await flow.ExchangeCodeForTokenAsync(
                "user",
                code,
                _redirectUri,
                CancellationToken.None);

            var userInfo = await GetUserInfoAsync(token.AccessToken);
            return new User
            {
                GoogleId = userInfo.Id,
                Email = userInfo.Email,
                Name = userInfo.Name,
                AccessToken = token.AccessToken,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };
        }

        private async Task<Google.Apis.Auth.GoogleJsonWebSignature.Payload> GetUserInfoAsync(string accessToken)
        {
            var validPayload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(accessToken);
            return validPayload;
        }
    }
}
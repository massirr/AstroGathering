using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AstroGathering.Objects;

namespace AstroGathering.Services
{
    /// <summary>
    /// OAuth 2.0 service for desktop applications using PKCE (Proof Key for Code Exchange)
    /// Follows Google's recommended practices for desktop app authentication
    /// </summary>
    public class DesktopOAuthService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUrl;
        private string _codeVerifier = "";

        public DesktopOAuthService(string clientId, string clientSecret, string redirectUri)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUrl = redirectUri;
        }

        public string GetAuthorizationUrl()
        {
            // Generate PKCE parameters for better security
            _codeVerifier = GenerateCodeVerifier();
            //Console.WriteLine(_codeVerifier);
            var codeChallenge = GenerateCodeChallenge(_codeVerifier);
            //Console.WriteLine(codeChallenge);

            var parameters = new Dictionary<string, string>
            {
                {"client_id", _clientId},
                {"redirect_uri", _redirectUrl},
                {"response_type", "code"},
                {"scope", "openid profile email"},
                {"code_challenge", codeChallenge},
                {"code_challenge_method", "S256"},
                {"access_type", "offline"},
                {"prompt", "consent"}
            };

            var queryString = string.Join("&", 
                parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            return $"https://accounts.google.com/o/oauth2/v2/auth?{queryString}";
        }

        public async Task<User> ProcessAuthorizationCodeAsync(string code)
        {
            using var httpClient = new HttpClient();
            
            var tokenRequest = new Dictionary<string, string>
            {
                {"client_id", _clientId},
                {"client_secret", _clientSecret},
                {"code", code},
                {"grant_type", "authorization_code"},
                {"redirect_uri", _redirectUrl},
                {"code_verifier", _codeVerifier}
            };

            var tokenContent = new FormUrlEncodedContent(tokenRequest);
            var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenContent);
            
            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                throw new Exception($"Token exchange failed: {errorContent}");
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(tokenJson);
            
            if (tokenData == null)
                throw new Exception("Failed to deserialize token response");

            var accessToken = tokenData["access_token"].GetString();
            var refreshToken = tokenData.ContainsKey("refresh_token") ? tokenData["refresh_token"].GetString() : null;

            if (string.IsNullOrEmpty(accessToken))
                throw new Exception("Access token is null or empty");

            // Get user info
            var userInfo = await GetUserInfoAsync(accessToken);

            return new User
            {
                GoogleId = userInfo.Sub,
                Email = userInfo.Email,
                Name = userInfo.Name,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };
        }

        private async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json);
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return userInfo ?? throw new InvalidOperationException($"Failed to deserialize user info. Raw JSON: {json}");
        }

        private static string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Convert.ToBase64String(challengeBytes)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
        }
    }

    public class GoogleUserInfo
    {
        public string Sub { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
    }
}

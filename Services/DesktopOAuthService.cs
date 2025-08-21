using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AstroGathering.Objects;
using AstroGathering.Database;

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

        // Keep the old method name for backward compatibility
        public async Task<User> ProcessAuthorizationCodeAsync(string code)
        {
            return await GetUserAsync(code);
        }

        public async Task<User> GetUserAsync(string code)
        {
            // Exchange authorization code for tokens
            var tokenResponse = await ExchangeCodeForTokensAsync(code);
            
            if (string.IsNullOrEmpty(tokenResponse.AccessToken))
                throw new Exception("Access token is null or empty");

            // Get user info from Google
            var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken);
            var googleId = !string.IsNullOrEmpty(userInfo.Sub) ? userInfo.Sub : userInfo.Id;
            
            // Try to load existing user from database
            var database = new DatabaseOut();
            var existingUser = database.GetUserByEmail(userInfo.Email);
            
            // Simple name splitting for OAuth data
            var names = userInfo.Name?.Split(' ') ?? new string[0];
            
            if (existingUser != null)
            {
                // User exists - update with fresh OAuth data and return
                existingUser.AccessToken = tokenResponse.AccessToken;
                existingUser.RefreshToken = tokenResponse.RefreshToken;
                existingUser.LastLogin = DateTime.UtcNow;
                existingUser.ProfilePictureUrl = userInfo.Picture; // Update profile picture
                
                // Update first/last name if not set
                if (string.IsNullOrEmpty(existingUser.FirstName) && names.Length > 0)
                    existingUser.FirstName = names[0];
                if (string.IsNullOrEmpty(existingUser.LastName) && names.Length > 1)
                    existingUser.LastName = string.Join(" ", names.Skip(1));
                
                // Update last login in database
                var databaseIn = new DatabaseIn();
                databaseIn.UpdateUserLastLogin(existingUser.UserId, DateTime.UtcNow);
                
                return existingUser;
            }
            else
            {
                // New user - create with OAuth data
                var newUser = new User
                {
                    GoogleId = googleId,
                    Email = userInfo.Email,
                    FirstName = names.Length > 0 ? names[0] : null,
                    LastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : null,
                    ProfilePictureUrl = userInfo.Picture,
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    IsAdmin = false // New users start as non-admin
                };
                
                // Insert into database and get the user ID
                var databaseIn = new DatabaseIn();
                newUser.UserId = databaseIn.InsertUser(newUser);
                
                return newUser;
            }
        }

        private async Task<TokenResponse> ExchangeCodeForTokensAsync(string code)
        {
            using var httpClient = new HttpClient();
            
            var parameters = new Dictionary<string, string>
            {
                {"code", code},
                {"client_id", _clientId},
                {"client_secret", _clientSecret},
                {"redirect_uri", _redirectUrl},
                {"grant_type", "authorization_code"},
                {"code_verifier", _codeVerifier}
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            return tokenResponse ?? throw new Exception("Token response is null");
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

    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }

    public class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Sub { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
    }
}

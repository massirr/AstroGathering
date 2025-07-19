using System;
using DotNetEnv;

namespace AstroGathering.Services
{
    public class ConfigurationService
    {
        public ConfigurationService()
        {
            // Load .env file
            Env.Load();
        }

        public string GoogleClientId => Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? throw new InvalidOperationException("GOOGLE_CLIENT_ID environment variable is not set");
        public string GoogleClientSecret => Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? throw new InvalidOperationException("GOOGLE_CLIENT_SECRET environment variable is not set");
        public string GoogleRedirectUri => Environment.GetEnvironmentVariable("GOOGLE_REDIRECT_URI") ?? throw new InvalidOperationException("GOOGLE_REDIRECT_URI environment variable is not set");
    }
}

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
        public string GoogleRedirectUrl => Environment.GetEnvironmentVariable("GOOGLE_REDIRECT_URL") ?? throw new InvalidOperationException("GOOGLE_REDIRECT_URL environment variable is not set");
        public string AstronomyApiKey => Environment.GetEnvironmentVariable("ASTRONOMY_API_KEY") ?? throw new InvalidOperationException("ASTRONOMY_API_KEY environment variable is not set");
    }
}

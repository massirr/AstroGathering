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
        
        // NASA API Configuration
        public static string NasaApiKey => Environment.GetEnvironmentVariable("NASA_API_KEY") ?? throw new InvalidOperationException("NASA_API_KEY environment variable is not set");
        
        // Astronomy API Configuration (backup)
        public static string AstronomyApplicationId => Environment.GetEnvironmentVariable("ASTRONOMY_APPLICATION_ID") ?? throw new InvalidOperationException("ASTRONOMY_APPLICATION_ID environment variable is not set");
        public static string AstronomyApplicationSecret => Environment.GetEnvironmentVariable("ASTRONOMY_APPLICATION_SECRET") ?? throw new InvalidOperationException("ASTRONOMY_APPLICATION_SECRET environment variable is not set");
    }
}

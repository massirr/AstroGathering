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

        public string GoogleClientId => Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"); // reads the value of an environment variable named "GOOGLE_CLIENT_ID"
        public string GoogleClientSecret => Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
        public string GoogleRedirectUri => Environment.GetEnvironmentVariable("GOOGLE_REDIRECT_URI");
    }
}

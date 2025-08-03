using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using AstroGathering.Objects;

namespace AstroGathering.Services
{
    public class AuthCallbackService
    {
        private readonly DesktopOAuthService _authService;
        private IWebHost? _webHost;
        private TaskCompletionSource<User>? _authCompletionSource;

        public AuthCallbackService(DesktopOAuthService authService)
        {
            _authService = authService;
        }
        public async Task<User> StartCallbackServer()
        {
            // Create a TaskCompletionSource to handle the asynchronous authentication flow
            _authCompletionSource = new TaskCompletionSource<User>();

            // Configure and build a temporary web server using Kestrel
            _webHost = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://127.0.0.1:3000")
                .Configure(app =>
                {
                    // Configure the request handling pipeline
                    app.Run(async context =>
                    {
                        // Only handle requests to the /callback endpoint
                        if (context.Request.Path == "/callback")
                        {
                            // Extract the authorization code from the query parameters
                            var code = context.Request.Query["code"].ToString();
                            try
                            {
                                // Process the authorization code and get the authenticated user
                                var user = await _authService.ProcessAuthorizationCodeAsync(code);
                                Console.WriteLine("User Object Received:");
                                Console.WriteLine($"Name: {user.Name}");
                                Console.WriteLine($"Email: {user.Email}");
                                Console.WriteLine($"Google ID: {user.GoogleId}");
                                Console.WriteLine($"Created At: {user.CreatedAt}");
                                Console.WriteLine($"Last Login: {user.LastLogin}");
                                await context.Response.WriteAsync("<html><body><h1>Authentication successful!</h1><p>You can close this window now.</p><script>window.close();</script></body></html>");
                                _authCompletionSource.SetResult(user);
                            }
                            catch (Exception ex)
                            {
                                _authCompletionSource.SetException(ex);
                                await context.Response.WriteAsync($"<html><body><h1>Authentication failed!</h1><p>{ex.Message}</p></body></html>");
                            }
                        }
                    });
                })
                .Build();

            await _webHost.StartAsync();
            return await _authCompletionSource.Task;
        }

        public async Task StopCallbackServer()
        {
            if (_webHost != null)
            {
                await _webHost.StopAsync();
                _webHost.Dispose();
            }
        }
    }
}

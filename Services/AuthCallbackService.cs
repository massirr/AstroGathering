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
        private readonly GoogleAuthService _authService;
        private IWebHost _webHost;
        private TaskCompletionSource<User> _authCompletionSource;

        public AuthCallbackService(GoogleAuthService authService)
        {
            _authService = authService;
        }

        public async Task<User> StartCallbackServer()
        {
            _authCompletionSource = new TaskCompletionSource<User>();

            _webHost = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.ListenLocalhost(8080); // This ensures we only listen on localhost
                })
                .Configure(app =>
                {
                    app.Run(async context =>
                    {
                        if (context.Request.Path == "/callback")
                        {
                            var code = context.Request.Query["code"].ToString();
                            try
                            {
                                var user = await _authService.ProcessAuthorizationCodeAsync(code);
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

using Avalonia.Controls;
using AstroGathering.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Interactivity;
using System;

namespace AstroGathering.Pages
{
    public partial class LoginPage : UserControl
    {
        private readonly DesktopOAuthService _authService;
        private readonly AuthCallbackService _callbackService;

        public LoginPage()
        {
            InitializeComponent();
            var config = new ConfigurationService();
            _authService = new DesktopOAuthService(
                config.GoogleClientId,
                config.GoogleClientSecret,
                config.GoogleRedirectUrl
            );
            _callbackService = new AuthCallbackService(_authService);
            
            // Connect the button click event
            GoogleLoginButton.Click += GoogleLoginButton_Click;
        }

        private async void GoogleLoginButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (StatusMessage != null)
                    StatusMessage.Text = "Initializing authentication...";
                GoogleLoginButton.IsEnabled = false;

                Console.WriteLine("Starting callback server..."); 
                var authTask = _callbackService.StartCallbackServer(); // gets the user that's returned from the auth flow
                
                Console.WriteLine("Getting auth URL...");
                var authUrl = _authService.GetAuthorizationUrl();
                Console.WriteLine($"Auth URL: {authUrl}");
                
                OpenBrowser(authUrl);
                
                var user = await authTask;
                await _callbackService.StopCallbackServer();
                
                if (StatusMessage != null)
                    StatusMessage.Text = "Authentication successful!";
                
                // Open the main application window with persistent navigation
                var appWindow = new MainApplicationWindow(user);
                appWindow.Show();
                
                // Close the current login window
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel is Window loginWindow)
                {
                    loginWindow.Close();
                }
            }
            catch (System.Exception ex)
            {
                if (StatusMessage != null)
                    StatusMessage.Text = "Authentication failed: " + ex.Message;
                await _callbackService.StopCallbackServer();
            }
            finally
            {
                GoogleLoginButton.IsEnabled = true;
            }
        }

        private void OpenBrowser(string url)
        {
            try
            {
                Console.WriteLine($"Attempting to open URL: {url}");

                // Trying the default browser first, then Safari as fallback
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Try default browser first
                    var defaultPsi = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/open",
                        Arguments = $"\"{url}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    
                    var defaultProcess = Process.Start(defaultPsi);
                    defaultProcess?.WaitForExit(2000); // Wait max 2 seconds
                    
                    if (defaultProcess?.ExitCode != 0)
                    {
                        // Fallback to Safari if default browser fails
                        var safariPsi = new ProcessStartInfo
                        {
                            FileName = "/usr/bin/open",
                            Arguments = $"-a Safari \"{url}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        Process.Start(safariPsi);
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                
                Console.WriteLine("Browser launch initiated. Please complete authentication in the browser.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening browser: {ex.Message}");
                // As a fallback, print the URL for manual copying
                Console.WriteLine($"Please copy and paste this URL into your browser manually: {url}");
                Console.WriteLine("After authentication, the callback will be handled automatically.");
            }
        }
    }
}

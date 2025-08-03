using Avalonia.Controls;
using AstroGathering.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Interactivity;
using System;
using AstroGathering.Pages;

namespace AstroGathering
{
    public partial class MainWindow : Window
    {
        private readonly DesktopOAuthService _authService;
        private readonly AuthCallbackService _callbackService;

        // Removed manual field declarations for GoogleLoginButton and StatusMessage

        public MainWindow()
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
                
                var homePage = new HomePage(user);
                var homeWindow = new Window
                {
                    Title = "AstroGathering - Home",
                    Content = homePage,
                    Width = 800,
                    Height = 450
                };
                homeWindow.Show();
                this.Close();
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

                // On macOS, try the open command with the -a flag to specify Safari
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/open",
                        Arguments = $"-a Safari \"{url}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    
                    var process = Process.Start(psi);
                    string? error = process?.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error output: {error}");
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening browser: {ex.Message}");
                // As a fallback, print the URL for manual copying
                Console.WriteLine($"Please copy and paste this URL into your browser if it doesn't open automatically: {url}");
            }
        }
    }
}
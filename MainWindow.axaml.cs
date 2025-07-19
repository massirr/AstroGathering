using Avalonia.Controls;
using AstroGathering.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Interactivity;

namespace AstroGathering
{
    public partial class MainWindow : Window
    {
        private readonly GoogleAuthService _authService;
        private readonly AuthCallbackService _callbackService;

        private Button GoogleLoginButton;
        private TextBlock StatusMessage;

        public MainWindow()
        {
            InitializeComponent();
            var config = new ConfigurationService();
            _authService = new GoogleAuthService(
                config.GoogleClientId,
                config.GoogleClientSecret,
                config.GoogleRedirectUri
            );
            _callbackService = new AuthCallbackService(_authService);
            
            // Connect the button click event
            GoogleLoginButton.Click += GoogleLoginButton_Click;
        }

        private async void GoogleLoginButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                StatusMessage.Text = "Initializing authentication...";
                GoogleLoginButton.IsEnabled = false;

                var authTask = _callbackService.StartCallbackServer();
                var authUrl = _authService.GetAuthorizationUrl();
                OpenBrowser(authUrl);
                
                var user = await authTask;
                await _callbackService.StopCallbackServer();
                
                StatusMessage.Text = "Authentication successful!";
                
                // TODO: Replace with your actual Home page navigation
                // this.Content = new Home();
            }
            catch (System.Exception ex)
            {
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
    }
}
using Avalonia.Controls;
using Avalonia.Interactivity;
using AstroGathering.Objects;

namespace AstroGathering.Pages
{
    public partial class SettingsPageContent : UserControl
    {
        private User? _user;

        public SettingsPageContent()
        {
            InitializeComponent();
        }

        public SettingsPageContent(User user) : this()
        {
            _user = user;
            LoadUserData();
            
            // Connect the logout button event
            if (LogoutButton != null)
                LogoutButton.Click += OnLogoutClick;
        }

        private void LoadUserData()
        {
            if (_user == null) return;

            // Display user information
            if (UserNameText != null)
                UserNameText.Text = !string.IsNullOrEmpty(_user.Name) ? _user.Name : "No name provided";
                
            if (UserEmailText != null)
                UserEmailText.Text = _user.Email ?? "No email provided";
                
            if (UserIdText != null)
                UserIdText.Text = _user.GoogleId ?? "No ID provided";
                
            if (CreatedAtText != null)
                CreatedAtText.Text = _user.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
                
            if (LastLoginText != null)
                LastLoginText.Text = _user.LastLogin?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Never";
        }

        private void OnLogoutClick(object? sender, RoutedEventArgs e)
        {
            // Find the main application window and close it, then show login
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is Window parentWindow)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                parentWindow.Close();
            }
        }
    }
}

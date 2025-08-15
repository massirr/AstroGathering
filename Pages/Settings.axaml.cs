using Avalonia.Controls;
using Avalonia.Interactivity;
using AstroGathering.Objects;

namespace AstroGathering.Pages
{
    public partial class SettingsPage : UserControl
    {
        private User? _user;

        // Parameterless constructor for XAML designer support
        public SettingsPage()
        {
            InitializeComponent();
        }

        public SettingsPage(User user) : this()
        {
            _user = user;
            LoadUserData();
            
            // Connect the logout button event
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

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            // TODO: Implement logout logic
            // For now, we'll close current window and show main login
            if (Parent is Window window)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                window.Close();
            }
            // find parent window
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

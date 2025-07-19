using Avalonia.Controls;
using Avalonia.Interactivity;
using AstroGathering.Objects;

namespace AstroGathering.Pages
{
    public partial class HomePage : UserControl
    {
        private User _user;

        public HomePage(User user)
        {
            InitializeComponent();
            _user = user;
            if (UserNameText != null)
                UserNameText.Text = $"Welcome, {user.Name}!";
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            // TODO: Implement logout logic
            if (Parent is Window window)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                window.Close();
            }
        }
    }
}

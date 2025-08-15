using Avalonia.Controls;
using Avalonia.Interactivity;
using AstroGathering.Objects;
using AstroGathering.Pages;

namespace AstroGathering
{
    public partial class MainApplicationWindow : Window
    {
        private User _user;
        private string _currentPage = "Home";

        public MainApplicationWindow(User user)
        {
            InitializeComponent();
            _user = user;
            
            InitializeNavigation();
            LoadHomePage(); // Start with home page
        }

        private void InitializeNavigation()
        {
            // Show admin tab if user is admin
            if (_user?.IsAdmin == true && AdminTab != null)
            {
                AdminTab.IsVisible = true;
            }
            
            // Connect navigation events
            if (HomeTab != null) HomeTab.Click += OnHomeClick;
            if (UploadTab != null) UploadTab.Click += OnUploadClick;
            if (GalleryTab != null) GalleryTab.Click += OnGalleryClick;
            if (SettingsTab != null) SettingsTab.Click += OnSettingsClick;
            if (AdminTab != null) AdminTab.Click += OnAdminClick;
            if (HelpTab != null) HelpTab.Click += OnHelpClick;
        }

        private void UpdateTabHighlight(string activePage)
        {
            _currentPage = activePage;
            
            // Reset all tabs to inactive style
            ResetTabStyle(HomeTab);
            ResetTabStyle(UploadTab);
            ResetTabStyle(GalleryTab);
            ResetTabStyle(SettingsTab);
            ResetTabStyle(AdminTab);
            ResetTabStyle(HelpTab);
            
            // Highlight the active tab
            Button? activeTab = activePage switch
            {
                "Home" => HomeTab,
                "Upload" => UploadTab,
                "Gallery" => GalleryTab,
                "Settings" => SettingsTab,
                "Admin" => AdminTab,
                "Help" => HelpTab,
                _ => HomeTab
            };
            
            if (activeTab != null)
            {
                activeTab.Background = Avalonia.Media.Brush.Parse("#4c3a8a");
                activeTab.Foreground = Avalonia.Media.Brushes.White;
            }
        }

        private void ResetTabStyle(Button? tab)
        {
            if (tab != null)
            {
                tab.Background = Avalonia.Media.Brushes.Transparent;
                tab.Foreground = Avalonia.Media.Brush.Parse("#b8a7d9");
            }
        }

        // Navigation event handlers
        private void OnHomeClick(object? sender, RoutedEventArgs e)
        {
            LoadHomePage();
        }

        private void OnUploadClick(object? sender, RoutedEventArgs e)
        {
            LoadUploadPage();
        }

        private void OnGalleryClick(object? sender, RoutedEventArgs e)
        {
            LoadGalleryPage();
        }

        private void OnSettingsClick(object? sender, RoutedEventArgs e)
        {
            LoadSettingsPage();
        }

        private void OnAdminClick(object? sender, RoutedEventArgs e)
        {
            LoadAdminPage();
        }

        private void OnHelpClick(object? sender, RoutedEventArgs e)
        {
            LoadHelpPage();
        }

        // Page loading methods
        private void LoadHomePage()
        {
            var homePage = new HomePageContent(_user);
            if (PageContent != null)
            {
                PageContent.Content = homePage;
            }
            UpdateTabHighlight("Home");
        }

        private void LoadSettingsPage()
        {
            var settingsPage = new SettingsPageContent(_user);
            if (PageContent != null)
            {
                PageContent.Content = settingsPage;
            }
            UpdateTabHighlight("Settings");
        }

        private void LoadUploadPage()
        {
            var uploadPage = new ComingSoonPageContent("Upload", "📸 Upload your amazing astrophotography!");
            if (PageContent != null)
            {
                PageContent.Content = uploadPage;
            }
            UpdateTabHighlight("Upload");
        }

        private void LoadGalleryPage()
        {
            var galleryPage = new ComingSoonPageContent("Gallery", "🖼️ Browse stunning celestial images!");
            if (PageContent != null)
            {
                PageContent.Content = galleryPage;
            }
            UpdateTabHighlight("Gallery");
        }

        private void LoadAdminPage()
        {
            var adminPage = new ComingSoonPageContent("Admin", "⚡ Manage users and astronomical data!");
            if (PageContent != null)
            {
                PageContent.Content = adminPage;
            }
            UpdateTabHighlight("Admin");
        }

        private void LoadHelpPage()
        {
            var helpPage = new ComingSoonPageContent("Help", "❓ Get help with AstroGathering features!");
            if (PageContent != null)
            {
                PageContent.Content = helpPage;
            }
            UpdateTabHighlight("Help");
        }
    }
}

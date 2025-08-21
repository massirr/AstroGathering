using Avalonia.Controls;
using Avalonia.Interactivity;
using AstroGathering.Objects;

namespace AstroGathering.Pages
{
    public partial class HelpPage : UserControl
    {
        private readonly User _user;

        public HelpPage(User user)
        {
            InitializeComponent();
            _user = user;
            
            // Set up navigation event handlers
            SetupNavigation();
        }

        private void SetupNavigation()
        {
            // Wire up button click events for navigation
            if (HelpSupportBtn != null) HelpSupportBtn.Click += OnHelpSupportClick;
            if (DataSourcesBtn != null) DataSourcesBtn.Click += OnDataSourcesClick;
            if (ContactSupportBtn != null) ContactSupportBtn.Click += OnContactSupportClick;
            if (AboutBtn != null) AboutBtn.Click += OnAboutClick;
            if (PrivacyBtn != null) PrivacyBtn.Click += OnPrivacyClick;
        }

        private void OnHelpSupportClick(object? sender, RoutedEventArgs e)
        {
            ShowSection("HelpSupport");
            UpdateButtonHighlight(HelpSupportBtn);
        }

        private void OnDataSourcesClick(object? sender, RoutedEventArgs e)
        {
            ShowSection("DataSources");
            UpdateButtonHighlight(DataSourcesBtn);
        }

        private void OnContactSupportClick(object? sender, RoutedEventArgs e)
        {
            ShowSection("ContactSupport");
            UpdateButtonHighlight(ContactSupportBtn);
        }

        private void OnAboutClick(object? sender, RoutedEventArgs e)
        {
            ShowSection("About");
            UpdateButtonHighlight(AboutBtn);
        }

        private void OnPrivacyClick(object? sender, RoutedEventArgs e)
        {
            ShowSection("Privacy");
            UpdateButtonHighlight(PrivacyBtn);
        }

        private void ShowSection(string sectionName)
        {
            // Hide all sections first
            if (HelpSupportContent != null) HelpSupportContent.IsVisible = false;
            if (DataSourcesContent != null) DataSourcesContent.IsVisible = false;
            if (ContactSupportContent != null) ContactSupportContent.IsVisible = false;
            if (AboutContent != null) AboutContent.IsVisible = false;
            if (PrivacyContent != null) PrivacyContent.IsVisible = false;

            // Show the selected section
            switch (sectionName)
            {
                case "HelpSupport":
                    if (HelpSupportContent != null) HelpSupportContent.IsVisible = true;
                    break;
                case "DataSources":
                    if (DataSourcesContent != null) DataSourcesContent.IsVisible = true;
                    break;
                case "ContactSupport":
                    if (ContactSupportContent != null) ContactSupportContent.IsVisible = true;
                    break;
                case "About":
                    if (AboutContent != null) AboutContent.IsVisible = true;
                    break;
                case "Privacy":
                    if (PrivacyContent != null) PrivacyContent.IsVisible = true;
                    break;
            }
        }

        private void UpdateButtonHighlight(Button? activeButton)
        {
            // Reset all buttons to default style
            var defaultColor = "#3d2d5a";
            var activeColor = "#4c3a8a";

            if (HelpSupportBtn != null) HelpSupportBtn.Background = Avalonia.Media.Brush.Parse(defaultColor);
            if (DataSourcesBtn != null) DataSourcesBtn.Background = Avalonia.Media.Brush.Parse(defaultColor);
            if (ContactSupportBtn != null) ContactSupportBtn.Background = Avalonia.Media.Brush.Parse(defaultColor);
            if (AboutBtn != null) AboutBtn.Background = Avalonia.Media.Brush.Parse(defaultColor);
            if (PrivacyBtn != null) PrivacyBtn.Background = Avalonia.Media.Brush.Parse(defaultColor);

            // Highlight the active button
            if (activeButton != null)
            {
                activeButton.Background = Avalonia.Media.Brush.Parse(activeColor);
            }
        }
    }
}

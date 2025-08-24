using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using AstroGathering.Objects;
using AstroGathering.Database;
using System;
using System.Threading.Tasks;

namespace AstroGathering.Pages
{
    public partial class HelpPage : UserControl
    {
        private readonly User _user;
        private readonly DatabaseOut _databaseOut;

        public HelpPage(User user)
        {
            InitializeComponent();
            _user = user;
            _databaseOut = new DatabaseOut();
            
            // Set up navigation event handlers
            SetupNavigation();
            
            // Load default section content on initialization
            LoadDefaultContent();
        }

        private async void LoadDefaultContent()
        {
            // Load Help & Support content by default
            if (HelpSupportContent != null)
            {
                await LoadDynamicContent(HelpSupportContent, "help_support");
            }
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

        private async void OnHelpSupportClick(object? sender, RoutedEventArgs e)
        {
            await ShowSection("HelpSupport", "help_support");
            UpdateButtonHighlight(HelpSupportBtn);
        }

        private async void OnDataSourcesClick(object? sender, RoutedEventArgs e)
        {
            await ShowSection("DataSources", "data_sources");
            UpdateButtonHighlight(DataSourcesBtn);
        }

        private async void OnContactSupportClick(object? sender, RoutedEventArgs e)
        {
            await ShowSection("ContactSupport", "contact_support");
            UpdateButtonHighlight(ContactSupportBtn);
        }

        private async void OnAboutClick(object? sender, RoutedEventArgs e)
        {
            await ShowSection("About", "about");
            UpdateButtonHighlight(AboutBtn);
        }

        private async void OnPrivacyClick(object? sender, RoutedEventArgs e)
        {
            await ShowSection("Privacy", "privacy");
            UpdateButtonHighlight(PrivacyBtn);
        }

        private async Task ShowSection(string sectionName, string dbSection)
        {
            // Hide all sections first
            if (HelpSupportContent != null) HelpSupportContent.IsVisible = false;
            if (DataSourcesContent != null) DataSourcesContent.IsVisible = false;
            if (ContactSupportContent != null) ContactSupportContent.IsVisible = false;
            if (AboutContent != null) AboutContent.IsVisible = false;
            if (PrivacyContent != null) PrivacyContent.IsVisible = false;

            // Show the selected section and load dynamic content
            switch (sectionName)
            {
                case "HelpSupport":
                    if (HelpSupportContent != null) 
                    {
                        HelpSupportContent.IsVisible = true;
                        await LoadDynamicContent(HelpSupportContent, dbSection);
                    }
                    break;
                case "DataSources":
                    if (DataSourcesContent != null) 
                    {
                        DataSourcesContent.IsVisible = true;
                        await LoadDynamicContent(DataSourcesContent, dbSection);
                    }
                    break;
                case "ContactSupport":
                    if (ContactSupportContent != null) 
                    {
                        ContactSupportContent.IsVisible = true;
                        await LoadDynamicContent(ContactSupportContent, dbSection);
                    }
                    break;
                case "About":
                    if (AboutContent != null) 
                    {
                        AboutContent.IsVisible = true;
                        await LoadDynamicContent(AboutContent, dbSection);
                    }
                    break;
                case "Privacy":
                    if (PrivacyContent != null) 
                    {
                        PrivacyContent.IsVisible = true;
                        await LoadDynamicContent(PrivacyContent, dbSection);
                    }
                    break;
            }
        }

        private async Task LoadDynamicContent(StackPanel section, string dbSection)
        {
            try
            {
                var helpContent = await _databaseOut.GetHelpContentBySectionAsync(dbSection);
                
                // Clear existing dynamic content (keep title and description - first 2 children)
                while (section.Children.Count > 2)
                {
                    section.Children.RemoveAt(2);
                }

                // Add content from database
                foreach (var content in helpContent)
                {
                    var border = new Border
                    {
                        Background = Brush.Parse("#2d1b69"),
                        CornerRadius = new Avalonia.CornerRadius(15),
                        Padding = new Avalonia.Thickness(25),
                        Margin = new Avalonia.Thickness(0, 0, 0, 20)
                    };

                    var stackPanel = new StackPanel { Spacing = 15 };
                    
                    var titleBlock = new TextBlock
                    {
                        Text = content.Title,
                        FontSize = 20,
                        FontWeight = FontWeight.Bold,
                        Foreground = Brushes.White
                    };

                    var contentBlock = new TextBlock
                    {
                        Text = content.Content,
                        FontSize = 14,
                        Foreground = Brush.Parse("#E0E0E0"),
                        TextWrapping = TextWrapping.Wrap
                    };

                    stackPanel.Children.Add(titleBlock);
                    stackPanel.Children.Add(contentBlock);
                    border.Child = stackPanel;
                    section.Children.Add(border);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dynamic content for section {dbSection}: {ex.Message}");
            }
        }

        private void UpdateButtonHighlight(Button? activeButton)
        {
            // Reset all buttons to default style
            var defaultColor = "#3d2d5a";
            var activeColor = "#4c3a8a";

            if (HelpSupportBtn != null) HelpSupportBtn.Background = Brush.Parse(defaultColor);
            if (DataSourcesBtn != null) DataSourcesBtn.Background = Brush.Parse(defaultColor);
            if (ContactSupportBtn != null) ContactSupportBtn.Background = Brush.Parse(defaultColor);
            if (AboutBtn != null) AboutBtn.Background = Brush.Parse(defaultColor);
            if (PrivacyBtn != null) PrivacyBtn.Background = Brush.Parse(defaultColor);

            // Highlight the active button
            if (activeButton != null)
            {
                activeButton.Background = Brush.Parse(activeColor);
            }
        }
    }
}

using Avalonia.Controls;
using Avalonia.Interactivity;
using AstroGathering.Objects;
using System;

namespace AstroGathering.Pages
{
    public partial class HomePage : UserControl
    {
        private User? _user;
        private DateTime _currentMonth = DateTime.Now;

        // Parameterless constructor for XAML designer support
        public HomePage()
        {
            InitializeComponent();
            InitializeEvents();
            LoadCalendarData();
        }

        public HomePage(User user) : this()
        {
            _user = user;
            
            // Show admin tab if user is admin
            if (_user?.IsAdmin == true && AdminTab != null)
            {
                AdminTab.IsVisible = true;
            }
        }

        private void InitializeEvents()
        {
            // Navigation button events
            if (SettingsTab != null)
                SettingsTab.Click += OnSettingsClick;
            if (UploadTab != null)
                UploadTab.Click += OnUploadClick;
            if (GalleryTab != null)
                GalleryTab.Click += OnGalleryClick;
            if (AdminTab != null)
                AdminTab.Click += OnAdminClick;
            if (HelpTab != null)
                HelpTab.Click += OnHelpClick;
                
            // Calendar navigation events
            if (PrevMonthButton != null)
                PrevMonthButton.Click += OnPrevMonthClick;
            if (NextMonthButton != null)
                NextMonthButton.Click += OnNextMonthClick;
        }

        private void LoadCalendarData()
        {
            // Update current month display
            if (CurrentMonthText != null)
            {
                CurrentMonthText.Text = _currentMonth.ToString("MMMM yyyy");
            }
            
            // TODO: Load astronomical events for the current month
            // This will be connected to astronomical APIs later
            LoadTodaysEvents();
            LoadMonthlySummary();
        }

        private void LoadTodaysEvents()
        {
            if (TodaysEventsText != null)
            {
                // Mock data for now - will be replaced with API data
                var today = DateTime.Now;
                if (today.Day == 15) // Mock: 15th has an event
                {
                    TodaysEventsText.Text = "🌕 Full Moon tonight!\nBest viewing after 9 PM";
                }
                else
                {
                    TodaysEventsText.Text = "No events today. Perfect for general stargazing!";
                }
            }
        }

        private void LoadMonthlySummary()
        {
            // Mock data for now - will be replaced with API data
            if (TotalEventsText != null) TotalEventsText.Text = "3";
            if (MeteorShowersText != null) MeteorShowersText.Text = "1";
            if (MoonPhasesText != null) MoonPhasesText.Text = "1";
            if (PlanetaryEventsText != null) PlanetaryEventsText.Text = "1";
        }

        // Navigation event handlers
        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            if (_user != null && Parent is Window window)
            {
                var settingsWindow = new Window
                {
                    Title = "AstroGathering - Settings",
                    Content = new SettingsPage(_user),
                    Width = 800,
                    Height = 600,
                    WindowState = Avalonia.Controls.WindowState.Maximized
                };
                settingsWindow.Show();
                window.Close();
            }
        }

        private void OnUploadClick(object sender, RoutedEventArgs e)
        {
            // TODO: Implement upload page
            ShowComingSoon("Upload");
        }

        private void OnGalleryClick(object sender, RoutedEventArgs e)
        {
            // TODO: Implement gallery page
            ShowComingSoon("Gallery");
        }

        private void OnAdminClick(object sender, RoutedEventArgs e)
        {
            // TODO: Implement admin page
            ShowComingSoon("Admin");
        }

        private void OnHelpClick(object sender, RoutedEventArgs e)
        {
            // TODO: Implement help page
            ShowComingSoon("Help");
        }

        private void ShowComingSoon(string pageName)
        {
            // For now, show a simple message
            if (TodaysEventsText != null)
            {
                TodaysEventsText.Text = $"🚀 {pageName} page coming soon!\nStay tuned for updates";
            }
        }

        // Calendar navigation
        private void OnPrevMonthClick(object sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            LoadCalendarData();
        }

        private void OnNextMonthClick(object sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(1);
            LoadCalendarData();
        }
    }
}

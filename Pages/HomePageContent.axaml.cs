using Avalonia.Controls;
using Avalonia.Interactivity;
using AstroGathering.Objects;
using System;

namespace AstroGathering.Pages
{
    public partial class HomePageContent : UserControl
    {
        private User? _user;
        private DateTime _currentMonth = DateTime.Now;

        public HomePageContent()
        {
            InitializeComponent();
            InitializeEvents();
            LoadCalendarData();
        }

        public HomePageContent(User user) : this()
        {
            _user = user;
        }

        private void InitializeEvents()
        {
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

        // Calendar navigation
        private void OnPrevMonthClick(object? sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            LoadCalendarData();
        }

        private void OnNextMonthClick(object? sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(1);
            LoadCalendarData();
        }
    }
}

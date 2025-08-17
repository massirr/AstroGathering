using Avalonia.Controls;
using Avalonia.Interactivity;
using AstroGathering.Objects;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

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
        }

        private void InitializeEvents()
        {
            // Calendar navigation events only (no navigation tabs since they're in MainApplicationWindow)
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
            
            // Load calendar grid with event indicators
            PopulateCalendarGrid();
            
            // Load today's events and monthly summary
            LoadTodaysEvents();
            LoadMonthlySummary();
        }

        private void PopulateCalendarGrid()
        {
            if (CalendarGrid == null) return;

            // Clear existing calendar day controls (keep headers)
            var toRemove = CalendarGrid.Children
                .Where(child => Grid.GetRow(child) > 0)
                .ToList();
            
            foreach (var child in toRemove)
            {
                CalendarGrid.Children.Remove(child);
            }

            // Get the first day of the month and calculate calendar layout
            var firstDay = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);
            var firstDayOfWeek = (int)firstDay.DayOfWeek; // 0 = Sunday
            
            // Add calendar day buttons (no events for now)
            for (int day = 1; day <= daysInMonth; day++)
            {
                var dayButton = CreateDayButton(day, 0); // 0 events
                
                // Calculate position in grid
                var totalDays = firstDayOfWeek + day - 1;
                var row = (totalDays / 7) + 1; // +1 because row 0 is headers
                var col = totalDays % 7;
                
                Grid.SetRow(dayButton, row);
                Grid.SetColumn(dayButton, col);
                CalendarGrid.Children.Add(dayButton);
            }
        }

        private Button CreateDayButton(int day, int eventCount)
        {
            var button = new Button
            {
                Content = CreateDayContent(day, eventCount),
                Background = GetDayBackground(day, eventCount),
                BorderBrush = GetDayBorder(day),
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = new Avalonia.CornerRadius(8),
                Margin = new Avalonia.Thickness(2),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
                FontSize = 12,
                Padding = new Avalonia.Thickness(4)
            };

            // Add click handler for day selection
            button.Click += (sender, e) => OnDaySelected(day);

            return button;
        }

        private object CreateDayContent(int day, int eventCount)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            // Day number
            stackPanel.Children.Add(new TextBlock
            {
                Text = day.ToString(),
                Foreground = GetDayTextColor(day),
                FontWeight = IsToday(day) ? Avalonia.Media.FontWeight.Bold : Avalonia.Media.FontWeight.Normal,
                FontSize = 12,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });

            // Event indicator
            if (eventCount > 0)
            {
                var indicator = GetEventIndicator(eventCount);
                stackPanel.Children.Add(new TextBlock
                {
                    Text = indicator,
                    FontSize = 8,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, -2, 0, 0)
                });
            }

            return stackPanel;
        }

        private Avalonia.Media.IBrush GetDayBackground(int day, int eventCount)
        {
            if (IsToday(day))
                return Avalonia.Media.Brush.Parse("#4c3a8a"); // Today - purple highlight
            
            if (eventCount > 0)
                return Avalonia.Media.Brush.Parse("#2d1b69"); // Has events - darker purple
            
            return Avalonia.Media.Brush.Parse("#1a1235"); // No events - dark background
        }

        private Avalonia.Media.IBrush GetDayBorder(int day)
        {
            if (IsToday(day))
                return Avalonia.Media.Brush.Parse("#ffffff"); // Today - white border
            
            return Avalonia.Media.Brush.Parse("#444444"); // Normal - gray border
        }

        private Avalonia.Media.IBrush GetDayTextColor(int day)
        {
            if (IsToday(day))
                return Avalonia.Media.Brush.Parse("#ffffff"); // Today - white text
            
            return Avalonia.Media.Brush.Parse("#b8a7d9"); // Normal - light purple text
        }

        private string GetEventIndicator(int eventCount)
        {
            return eventCount switch
            {
                1 => "•", // Single dot for 1 event
                2 => "••", // Two dots for 2 events
                3 => "•••", // Three dots for 3 events
                _ => "⭐" // Star for 4+ events
            };
        }

        private bool IsToday(int day)
        {
            return _currentMonth.Year == DateTime.Today.Year && 
                   _currentMonth.Month == DateTime.Today.Month && 
                   day == DateTime.Today.Day;
        }

        private void OnDaySelected(int day)
        {
            try
            {
                var selectedDate = new DateTime(_currentMonth.Year, _currentMonth.Month, day);
                
                if (TodaysEventsText != null)
                {
                    TodaysEventsText.Text = $"Selected {selectedDate:MMM dd}. Calendar functionality preserved - no API dependencies.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error selecting day {day}: {ex.Message}");
            }
        }

        private void LoadTodaysEvents()
        {
            if (TodaysEventsText != null)
            {
                try
                {
                    TodaysEventsText.Text = "Calendar ready! Click any day to select it. API dependencies removed.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading today's events: {ex.Message}");
                    TodaysEventsText.Text = "Calendar ready!";
                }
            }
        }

        private void LoadMonthlySummary()
        {
            try
            {
                // Static placeholder data since API is removed
                if (TotalEventsText != null) TotalEventsText.Text = "0";
                if (MeteorShowersText != null) MeteorShowersText.Text = "0";
                if (MoonPhasesText != null) MoonPhasesText.Text = "0";
                if (PlanetaryEventsText != null) PlanetaryEventsText.Text = "0";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading monthly summary: {ex.Message}");
                // Fallback values
                if (TotalEventsText != null) TotalEventsText.Text = "0";
                if (MeteorShowersText != null) MeteorShowersText.Text = "0";
                if (MoonPhasesText != null) MoonPhasesText.Text = "0";
                if (PlanetaryEventsText != null) PlanetaryEventsText.Text = "0";
            }
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

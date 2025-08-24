using Avalonia.Controls;
using Avalonia.Interactivity;
using AstroGathering.Objects;
using AstroGathering.Services;
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
        private int _selectedDay = DateTime.Today.Day; // Track which day is selected
        private readonly NasaApiService _nasaService;
        private Dictionary<DateTime, List<AstronomicalEvent>> _monthlyEvents = new();

        // Parameterless constructor for XAML designer support
        public HomePage()
        {
            InitializeComponent();
            _nasaService = new NasaApiService();
            InitializeEvents();
            _ = LoadCalendarData(); // Fire and forget for constructor
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

        private async Task LoadCalendarData()
        {
            // Update current month display
            if (CurrentMonthText != null)
            {
                CurrentMonthText.Text = _currentMonth.ToString("MMMM yyyy");
            }
            
            // Load events for the month
            await LoadEventsForMonth();
            
            // Load calendar grid with event indicators
            PopulateCalendarGrid();
            
            // Load today's events and monthly summary
            await LoadTodaysEvents();
            LoadMonthlySummary();
        }

        private async Task LoadEventsForMonth()
        {
            try
            {
                Console.WriteLine($"Loading events for {_currentMonth:MMMM yyyy}");
                
                // Use the new monthly batch API call
                _monthlyEvents = await _nasaService.GetMonthlyEventsAsync(_currentMonth);
                
                Console.WriteLine($"Loaded {_monthlyEvents.Count} days with events");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading events for month: {ex.Message}");
                _monthlyEvents.Clear();
            }
        }

        private void PopulateCalendarGrid()
        {
            if (CalendarGrid == null) return;

            Console.WriteLine($"PopulateCalendarGrid: Selected day {_selectedDay}, Total events for month: {_monthlyEvents.Count} dates");

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
            
            // Add calendar day buttons with event indicators
            for (int day = 1; day <= daysInMonth; day++)
            {
                var currentDate = new DateTime(_currentMonth.Year, _currentMonth.Month, day);
                var eventCount = _monthlyEvents.ContainsKey(currentDate) ? _monthlyEvents[currentDate].Count : 0;
                
                var dayButton = CreateDayButton(day, eventCount);
                
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
            if (day == _selectedDay && _currentMonth.Month == DateTime.Today.Month && _currentMonth.Year == DateTime.Today.Year)
                return Avalonia.Media.Brush.Parse("#ffffff"); // Selected day - white border
            
            if (IsToday(day))
                return Avalonia.Media.Brush.Parse("#ffaa00"); // Today - orange border
            
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
                Console.WriteLine($"Day {day} selected");
                
                // Update selected day
                _selectedDay = day;
                
                // Refresh the calendar to update visual selection
                PopulateCalendarGrid();
                
                var selectedDate = new DateTime(_currentMonth.Year, _currentMonth.Month, day);
                
                // Update today's events display with selected day's events
                if (_monthlyEvents.ContainsKey(selectedDate))
                {
                    var events = _monthlyEvents[selectedDate];
                    var eventText = string.Join("\n\n", events.Select(e => 
                    {
                        // Apply same formatting as LoadTodaysEvents for consistency
                        var eventName = e.EventName;
                        var description = e.Description;
                        
                        if (eventName.Contains("Moon Phase"))
                        {
                            return $"• {eventName}\n  Optimal for deep sky observation";
                        }
                        else if (eventName.Contains("Asteroid"))
                        {
                            return $"• {eventName}\n  {description}";
                        }
                        else
                        {
                            if (description.Length > 60)
                            {
                                var truncated = description.Substring(0, 57) + "...";
                                var lastSpace = truncated.LastIndexOf(' ');
                                if (lastSpace > 40)
                                {
                                    truncated = description.Substring(0, lastSpace) + "...";
                                }
                                description = truncated;
                            }
                            return $"• {eventName}\n  {description}";
                        }
                    }));
                    
                    if (TodaysEventsText != null)
                    {
                        TodaysEventsText.Text = $"Events for {selectedDate:MMM dd}:\n\n{eventText}";
                    }
                }
                else
                {
                    if (TodaysEventsText != null)
                    {
                        TodaysEventsText.Text = $"No events for {selectedDate:MMM dd}.";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error selecting day {day}: {ex.Message}");
            }
        }

        private async Task LoadTodaysEvents()
        {
            if (TodaysEventsText != null)
            {
                try
                {
                    var today = DateTime.Today;
                    if (_monthlyEvents.ContainsKey(today) && _currentMonth.Month == today.Month && _currentMonth.Year == today.Year)
                    {
                        var events = _monthlyEvents[today];
                        var eventText = string.Join("\n\n", events.Select(e => 
                        {
                            // Preserve moon phase emoji and improve formatting
                            var eventName = e.EventName;
                            var description = e.Description;
                            
                            // Format based on event type for better readability
                            if (eventName.Contains("Moon Phase"))
                            {
                                // Keep emoji for moon phases, add practical info
                                return $"• {eventName}\n  Optimal for deep sky observation";
                            }
                            else if (eventName.Contains("Asteroid"))
                            {
                                // Keep asteroid info concise
                                return $"• {eventName}\n  {description}";
                            }
                            else
                            {
                                // For other events, truncate and format description
                                if (description.Length > 60)
                                {
                                    // Find a good break point
                                    var truncated = description.Substring(0, 57) + "...";
                                    // Try to break at word boundary
                                    var lastSpace = truncated.LastIndexOf(' ');
                                    if (lastSpace > 40)
                                    {
                                        truncated = description.Substring(0, lastSpace) + "...";
                                    }
                                    description = truncated;
                                }
                                
                                // Add line break for better readability
                                return $"• {eventName}\n  {description}";
                            }
                        }));
                        TodaysEventsText.Text = $"Today's Events:\n\n{eventText}";
                    }
                    else
                    {
                        TodaysEventsText.Text = "No astronomical events for today.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading today's events: {ex.Message}");
                    TodaysEventsText.Text = "Error loading events";
                }
            }
        }

        private void LoadMonthlySummary()
        {
            try
            {
                var totalEvents = _monthlyEvents.Values.SelectMany(events => events).Count();
                var meteorShowers = _monthlyEvents.Values.SelectMany(events => events).Count(e => e.Type == "Near Earth Object");
                var moonPhases = _monthlyEvents.Values.SelectMany(events => events).Count(e => e.Type == "Moon Phase");
                var planetaryEvents = _monthlyEvents.Values.SelectMany(events => events).Count(e => e.Type == "Astronomy Feature");

                if (TotalEventsText != null) TotalEventsText.Text = totalEvents.ToString();
                if (MeteorShowersText != null) MeteorShowersText.Text = meteorShowers.ToString();
                if (MoonPhasesText != null) MoonPhasesText.Text = moonPhases.ToString();
                if (PlanetaryEventsText != null) PlanetaryEventsText.Text = planetaryEvents.ToString();
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
        private async void OnPrevMonthClick(object? sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            _selectedDay = 1; // Reset selection to first day when changing months
            await LoadCalendarData();
        }

        private async void OnNextMonthClick(object? sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(1);
            _selectedDay = 1; // Reset selection to first day when changing months
            await LoadCalendarData();
        }
    }
}

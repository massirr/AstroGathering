using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Input;
using AstroGathering.Services;
using AstroGathering.Objects;
using AstroGathering.Database;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Avalonia.Threading;

namespace AstroGathering.Pages
{
    public partial class UploadPage : UserControl
    {
        private readonly PhotoUploadService _uploadService;
        private readonly DatabaseIn _databaseIn;
        private readonly GeocodingService _geocodingService;
        private readonly User _user;
        private string? _selectedImagePath;

        public UploadPage(User user)
        {
            InitializeComponent();
            _user = user;
            _uploadService = new PhotoUploadService();
            _databaseIn = new DatabaseIn();
            _geocodingService = new GeocodingService();
            
            // Set current date and time
            if (DatePicker != null)
            {
                DatePicker.SelectedDate = DateTime.Now;
                // Set date format to show full month name
                DatePicker.DayFormat = "dd";
                DatePicker.MonthFormat = "MMMM";
                DatePicker.YearFormat = "yyyy";
            }
            if (TimePicker != null)
            {
                TimePicker.SelectedTime = DateTime.Now.TimeOfDay;
                // Set 12-hour time format
                TimePicker.ClockIdentifier = "12HourClock";
            }
        }

        private async void ChoosePhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Choose Photo",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Image Files")
                        {
                            Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.webp" }
                        }
                    }
                });

                if (files.Any())
                {
                    _selectedImagePath = files[0].Path.LocalPath;
                    if (SelectedFileText != null)
                        SelectedFileText.Text = Path.GetFileName(_selectedImagePath);
                    if (UploadButton != null)
                        UploadButton.IsEnabled = true;
                        
                    // Auto-fill name if empty
                    if (NameBox != null && string.IsNullOrEmpty(NameBox.Text))
                    {
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_selectedImagePath);
                        NameBox.Text = fileNameWithoutExtension;
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error", $"Failed to select file: {ex.Message}");
            }
        }

        private async void UseCurrentLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // For now, set demo coordinates (Antwerp, Belgium)
                // In a real app, you'd use geolocation services
                if (LatitudeBox != null)
                    LatitudeBox.Text = "51.2993";
                if (LongitudeBox != null)
                    LongitudeBox.Text = "4.4785";
                if (LocationSearchBox != null)
                    LocationSearchBox.Text = "Antwerp, Belgium";
                    
                await ShowMessageAsync("Location Set", "Demo coordinates set to Antwerp, Belgium");
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error", $"Failed to set location: {ex.Message}");
            }
        }

        private async void SearchLocation_Click(object sender, RoutedEventArgs e)
        {
            await PerformLocationSearch();
        }

        private async void LocationSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await PerformLocationSearch();
            }
        }

        private async Task PerformLocationSearch()
        {
            if (LocationSearchBox == null || string.IsNullOrWhiteSpace(LocationSearchBox.Text))
            {
                await ShowMessageAsync("Error", "Please enter a location to search for.");
                return;
            }

            try
            {
                // Disable search button and show loading state
                if (SearchLocationButton != null)
                {
                    SearchLocationButton.IsEnabled = false;
                    SearchLocationButton.Content = "🔄 Searching...";
                }

                var location = LocationSearchBox.Text.Trim();
                var (latitude, longitude, error) = await _geocodingService.GetCoordinatesAsync(location);

                if (error != null)
                {
                    await ShowMessageAsync("Location Search Error", error);
                    return;
                }

                if (latitude.HasValue && longitude.HasValue)
                {
                    if (LatitudeBox != null)
                        LatitudeBox.Text = latitude.Value.ToString("F6");
                    if (LongitudeBox != null)
                        LongitudeBox.Text = longitude.Value.ToString("F6");

                    await ShowMessageAsync("Location Found", $"Coordinates found for: {location}");
                }
                else
                {
                    await ShowMessageAsync("Location Not Found", "Could not find coordinates for the specified location. Please try a different search term.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Search Error", $"An error occurred while searching: {ex.Message}");
            }
            finally
            {
                // Re-enable search button
                if (SearchLocationButton != null)
                {
                    SearchLocationButton.IsEnabled = true;
                    SearchLocationButton.Content = "🔍 Search";
                }
            }
        }

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedImagePath))
            {
                await ShowMessageAsync("Error", "Please select a photo first.");
                return;
            }

            try
            {
                // Disable button and show loading state
                if (UploadButton != null)
                {
                    UploadButton.IsEnabled = false;
                    UploadButton.Content = "Uploading...";
                }

                // Validate inputs
                if (string.IsNullOrWhiteSpace(NameBox?.Text))
                {
                    await ShowMessageAsync("Error", "Please enter a name for your photo.");
                    return;
                }

                // Test Azure connection first
                var connectionTest = await _uploadService.TestConnectionAsync();
                if (!connectionTest)
                {
                    await ShowMessageAsync("Error", "Failed to connect to Azure Storage. Please check your configuration.");
                    return;
                }

                // Upload to Azure
                using var fileStream = File.OpenRead(_selectedImagePath);
                var imageUrl = await _uploadService.UploadPhotoAsync(fileStream, Path.GetFileName(_selectedImagePath));

                // Ensure user exists in database before inserting photo
                if (_user.UserId <= 0)
                {
                    _user.UserId = _databaseIn.InsertUser(_user);
                    if (_user.UserId <= 0)
                    {
                        await ShowMessageAsync("Error", "Failed to create user in database.");
                        return;
                    }
                }

                // Parse coordinates if available
                double? latitude = null;
                double? longitude = null;
                if (!string.IsNullOrEmpty(LatitudeBox?.Text) && double.TryParse(LatitudeBox.Text, out double lat))
                {
                    latitude = lat;
                }
                if (!string.IsNullOrEmpty(LongitudeBox?.Text) && double.TryParse(LongitudeBox.Text, out double lng))
                {
                    longitude = lng;
                }

                // Create photo object for upload (with clean description)
                var eventName = NameBox?.Text?.Trim() ?? "Untitled Observation";
                var photo = new Photo
                {
                    ImageUrl = imageUrl,
                    EventName = eventName, // Store event name directly
                    Location = LocationSearchBox?.Text?.Trim(), // Store the search location text
                    Latitude = latitude,
                    Longitude = longitude,
                    Description = DescriptionBox?.Text?.Trim() ?? "", // Clean description only
                    DateTaken = (DatePicker?.SelectedDate?.Date ?? DateTime.Now.Date) + (TimePicker?.SelectedTime ?? DateTime.Now.TimeOfDay),
                    UserId = _user.UserId
                };

                // Save to database
                var photoId = await _databaseIn.InsertPhotoAsync(photo);

                // Handle tags separately
                if (!string.IsNullOrEmpty(TagsBox?.Text))
                {
                    var tags = TagsBox.Text.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t));
                    foreach (var tag in tags)
                    {
                        var tagId = _databaseIn.InsertTag(tag);
                        if (tagId > 0 && photoId > 0)
                        {
                            _databaseIn.AddPhotoTag(photoId, tagId);
                        }
                    }
                }

                if (photoId > 0)
                {
                    await ShowMessageAsync("Success", $"Photo uploaded successfully! '{eventName}' saved to gallery.");
                    ClearForm();
                }
                else
                {
                    await ShowMessageAsync("Error", "Photo uploaded to cloud but failed to save to database. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error", $"Upload failed: {ex.Message}");
            }
            finally
            {
                // Re-enable button
                if (UploadButton != null)
                {
                    UploadButton.IsEnabled = !string.IsNullOrEmpty(_selectedImagePath);
                    UploadButton.Content = "Share with Community";
                }
            }
        }

        private void ClearForm()
        {
            if (NameBox != null) NameBox.Text = "";
            if (DescriptionBox != null) DescriptionBox.Text = "";
            if (LocationSearchBox != null) LocationSearchBox.Text = "";
            if (LatitudeBox != null) LatitudeBox.Text = "";
            if (LongitudeBox != null) LongitudeBox.Text = "";
            if (TagsBox != null) TagsBox.Text = "";
            if (SelectedFileText != null) SelectedFileText.Text = "No file selected";
            if (DatePicker != null) 
            {
                DatePicker.SelectedDate = DateTime.Now;
                // Ensure date format is maintained after clearing
                DatePicker.DayFormat = "dd";
                DatePicker.MonthFormat = "MMMM";
                DatePicker.YearFormat = "yyyy";
            }
            if (TimePicker != null) 
            {
                TimePicker.SelectedTime = DateTime.Now.TimeOfDay;
                // Ensure time format is maintained after clearing
                TimePicker.ClockIdentifier = "12HourClock";
            }
            
            _selectedImagePath = null;
            
            if (UploadButton != null)
                UploadButton.IsEnabled = false;
        }

        private async Task ShowMessageAsync(string title, string message)
        {
            try
            {
                var messageWindow = new Window
                {
                    Title = title,
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CanResize = false,
                    Background = Avalonia.Media.Brushes.DarkSlateGray,
                    Content = new StackPanel
                    {
                        Margin = new Avalonia.Thickness(20),
                        Spacing = 15,
                        Children =
                        {
                            new TextBlock 
                            { 
                                Text = message, 
                                Foreground = Avalonia.Media.Brushes.White,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                FontSize = 14
                            },
                            new Button 
                            { 
                                Content = "OK", 
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                Padding = new Avalonia.Thickness(20, 10),
                                Background = Avalonia.Media.Brushes.DodgerBlue,
                                Foreground = Avalonia.Media.Brushes.White
                            }
                        }
                    }
                };

                // Add click handler to close button
                var okButton = (Button)((StackPanel)messageWindow.Content).Children[1];
                okButton.Click += (s, e) => messageWindow.Close();

                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel is Window parentWindow)
                {
                    await messageWindow.ShowDialog(parentWindow);
                }
                else
                {
                    messageWindow.Show();
                }
            }
            catch (Exception ex)
            {
                // Fallback: just log to console if we can't show the dialog
                Console.WriteLine($"{title}: {message} (Error showing dialog: {ex.Message})");
            }
        }

        protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _geocodingService?.Dispose();
        }
    }
}

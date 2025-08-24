using Avalonia.Controls;
using Avalonia.Interactivity;
using AstroGathering.Objects;
using AstroGathering.Database;
using System;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Linq;

namespace AstroGathering.Pages
{
    public partial class SettingsPage : UserControl, INotifyPropertyChanged
    {
        private User? _user;
        private DatabaseOut _database;

        public new event PropertyChangedEventHandler? PropertyChanged;

        // Parameterless constructor for XAML designer support
        public SettingsPage()
        {
            InitializeComponent();
            _database = new DatabaseOut();
            DataContext = this;
        }

        public SettingsPage(User user) : this()
        {
            _user = user;
            
            // Debug: Check user admin status
            Console.WriteLine($"Settings Page - User: {_user.Email}, IsAdmin: {_user.IsAdmin}");
            
            LoadUserData();
            LoadProfileImage();
            LoadStatistics();
            
            // Set admin panel visibility based on user role
            if (AdminPanel != null)
            {
                AdminPanel.IsVisible = _user.IsAdmin;
                Console.WriteLine($"Admin Panel found and visibility set to: {_user.IsAdmin}");
                Console.WriteLine($"AdminPanel.IsVisible after setting: {AdminPanel.IsVisible}");
            }
            else
            {
                Console.WriteLine("❌ AdminPanel control is NULL - not found in XAML");
            }
            
            // Connect event handlers
            if (LogoutButton != null)
                LogoutButton.Click += OnLogoutClick;
            
            // Load admin data if user is admin
            if (_user.IsAdmin)
            {
                LoadAdminData();
            }
        }

        private void LoadUserData()
        {
            if (_user == null) return;

            // Display user information
            if (UserNameText != null)
            {
                string displayName = "";
                if (!string.IsNullOrEmpty(_user.FirstName) || !string.IsNullOrEmpty(_user.LastName))
                {
                    displayName = $"{_user.FirstName} {_user.LastName}".Trim();
                }
                else if (!string.IsNullOrEmpty(_user.Name))
                {
                    displayName = _user.Name;
                }
                else
                {
                    displayName = "No name provided";
                }
                UserNameText.Text = displayName;
            }
                
            if (UserEmailText != null)
                UserEmailText.Text = _user.Email ?? "No email provided";
                
            if (UserIdText != null)
                UserIdText.Text = _user.GoogleId ?? "No ID provided";
                
            if (CreatedAtText != null)
                CreatedAtText.Text = _user.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
                
            if (LastLoginText != null)
                LastLoginText.Text = _user.LastLogin?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Never";
            
            if (AdminBadge != null)
                AdminBadge.Text = _user.IsAdmin ? "ADMIN" : "";
                
            if (AdminBadge != null)
                AdminBadge.IsVisible = _user.IsAdmin;
        }

        private void LoadProfileImage()
        {
            // Profile image functionality removed as it's not being used in the UI
            // The XAML shows ProfileInitials with static "ID" text instead
        }

        private void LoadStatistics()
        {
            if (_user == null) return;

            try
            {
                // Load user-specific stats
                var userPhotosCount = _database.GetUserPhotosCount(_user.UserId);
                var userLikesCount = _database.GetUserLikesCount(_user.UserId);
                
                // Update UI text blocks
                if (PhotosUploadedText != null)
                    PhotosUploadedText.Text = userPhotosCount.ToString();
                    
                if (LikesReceivedText != null)
                    LikesReceivedText.Text = userLikesCount.ToString();

                // Load admin stats if user is admin
                if (_user.IsAdmin)
                {
                    var totalUsers = _database.GetTotalUsersCount();
                    var totalPhotos = _database.GetTotalPhotosCount();
                    var pendingReports = _database.GetPendingReportsCount();
                    
                    // Update admin UI text blocks
                    if (TotalUsersText != null)
                        TotalUsersText.Text = totalUsers.ToString();
                        
                    if (TotalPhotosText != null)
                        TotalPhotosText.Text = totalPhotos.ToString();
                        
                    if (PendingReportsText != null)
                        PendingReportsText.Text = pendingReports.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load statistics: {ex.Message}");
            }
        }


        private void OnLogoutClick(object? sender, RoutedEventArgs e)
        {
            // Find parent window and return to login
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is Window parentWindow)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                parentWindow.Close();
            }
        }

        private async Task ShowMessageDialog(string title, string message)
        {
            // For now, just print to console - can be enhanced later with a proper dialog
            Console.WriteLine($"{title}: {message}");
            
            // Optional: Show a simple notification in the UI if needed
            await Task.Delay(10); // Just to make it async
        }

        /// <summary>
        /// Load admin data and populate the UI tables
        /// </summary>
        private void LoadAdminData()
        {
            if (_user == null || !_user.IsAdmin) return;

            try
            {
                // Get all data from database
                var users = _database.GetAllUsers();
                var photos = _database.GetAllPhotos();
                var reports = _database.GetAllReports();

                // Populate Users Panel
                PopulateUsersPanel(users);
                
                // Populate Photos Panel
                PopulatePhotosPanel(photos, users);
                
                // Populate Reports Panel
                PopulateReportsPanel(reports, users, photos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load admin data: {ex.Message}");
            }
        }

        /// <summary>
        /// Populate the users panel with user information
        /// </summary>
        private void PopulateUsersPanel(System.Collections.Generic.List<User> users)
        {
            if (UsersPanel == null) return;

            UsersPanel.Children.Clear();

            foreach (var user in users.OrderByDescending(u => u.CreatedAt))
            {
                var userPhotosCount = _database.GetUserPhotosCount(user.UserId);
                var userLikesCount = _database.GetUserLikesCount(user.UserId);
                
                // Create user row
                var userBorder = new Border
                {
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0x2d, 0x1b, 0x69)),
                    CornerRadius = new Avalonia.CornerRadius(8),
                    Padding = new Avalonia.Thickness(15),
                    Margin = new Avalonia.Thickness(0, 0, 0, 5)
                };

                var mainGrid = new Grid();
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                
                var userStack = new StackPanel { Spacing = 5 };
                
                // User email and admin status
                var emailText = new TextBlock
                {
                    Text = $"{user.Email} {(user.IsAdmin ? "admin" : "")}",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    FontSize = 14
                };
                
                // User details
                var detailsText = new TextBlock
                {
                    Text = $"Joined: {user.CreatedAt:dd/MM/yyyy} | Photos: {userPhotosCount} | Likes: {userLikesCount} | Last Login: {user.LastLogin?.ToString("dd/MM/yyyy HH:mm") ?? "Never"}",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0xb8, 0xa7, 0xd9)),
                    FontSize = 12
                };
                
                userStack.Children.Add(emailText);
                userStack.Children.Add(detailsText);
                
                // Add delete button (only if not deleting self and not the only admin)
                var deleteButton = new Button
                {
                    Content = "Delete",
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0xFF, 0x6B, 0x6B)),
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                    CornerRadius = new Avalonia.CornerRadius(4),
                    Padding = new Avalonia.Thickness(8, 4),
                    FontSize = 10,
                    Margin = new Avalonia.Thickness(0, 5, 0, 0)
                };
                deleteButton.Click += (s, e) => DeleteUserFromSettings(user);
                
                Grid.SetColumn(userStack, 0);
                Grid.SetColumn(deleteButton, 1);
                
                mainGrid.Children.Add(userStack);
                if (user.UserId != _user?.UserId) // Don't allow self-deletion
                {
                    mainGrid.Children.Add(deleteButton);
                }
                
                userBorder.Child = mainGrid;
                
                UsersPanel.Children.Add(userBorder);
            }
            
            // Add "No users" message if empty
            if (!users.Any())
            {
                var noUsersText = new TextBlock
                {
                    Text = "No users found",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0xb8, 0xa7, 0xd9)),
                    FontSize = 12,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };
                UsersPanel.Children.Add(noUsersText);
            }
        }

        /// <summary>
        /// Populate the photos panel with photo information
        /// </summary>
        private void PopulatePhotosPanel(System.Collections.Generic.List<Photo> photos, System.Collections.Generic.List<User> users)
        {
            if (PhotosPanel == null) return;

            PhotosPanel.Children.Clear();

            foreach (var photo in photos.OrderByDescending(p => p.TimeUploaded))
            {
                var owner = users.FirstOrDefault(u => u.UserId == photo.UserId);
                
                // Create photo row
                var photoBorder = new Border
                {
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0x2d, 0x1b, 0x69)),
                    CornerRadius = new Avalonia.CornerRadius(8),
                    Padding = new Avalonia.Thickness(15),
                    Margin = new Avalonia.Thickness(0, 0, 0, 5)
                };

                var mainPhotoGrid = new Grid();
                mainPhotoGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                mainPhotoGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                
                var photoStack = new StackPanel { Spacing = 5 };
                
                // Photo ID and owner
                var photoHeaderText = new TextBlock
                {
                    Text = $"Photo #{photo.PhotoId} by {owner?.Email ?? "Unknown User"}",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    FontSize = 14
                };
                
                // Photo details
                var photoDetailsText = new TextBlock
                {
                    Text = $"Uploaded: {photo.TimeUploaded:dd/MM/yyyy HH:mm}",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0xb8, 0xa7, 0xd9)),
                    FontSize = 12
                };
                
                // Photo URL (truncated)
                var photoUrlText = new TextBlock
                {
                    Text = $"URL: {(photo.ImageUrl.Length > 50 ? photo.ImageUrl.Substring(0, 50) + "..." : photo.ImageUrl)}",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0xb8, 0xa7, 0xd9)),
                    FontSize = 10
                };
                
                photoStack.Children.Add(photoHeaderText);
                photoStack.Children.Add(photoDetailsText);
                photoStack.Children.Add(photoUrlText);
                
                // Add delete button
                var deletePhotoButton = new Button
                {
                    Content = "Delete Photo",
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0xFF, 0x6B, 0x6B)),
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                    CornerRadius = new Avalonia.CornerRadius(4),
                    Padding = new Avalonia.Thickness(8, 4),
                    FontSize = 10,
                    Margin = new Avalonia.Thickness(0, 5, 0, 0)
                };
                deletePhotoButton.Click += (s, e) => DeletePhotoFromSettings(photo);
                
                Grid.SetColumn(photoStack, 0);
                Grid.SetColumn(deletePhotoButton, 1);
                
                mainPhotoGrid.Children.Add(photoStack);
                mainPhotoGrid.Children.Add(deletePhotoButton);
                
                photoBorder.Child = mainPhotoGrid;
                
                PhotosPanel.Children.Add(photoBorder);
            }
            
            // Add "No photos" message if empty
            if (!photos.Any())
            {
                var noPhotosText = new TextBlock
                {
                    Text = "No photos found",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0xb8, 0xa7, 0xd9)),
                    FontSize = 12,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };
                PhotosPanel.Children.Add(noPhotosText);
            }
        }

        /// <summary>
        /// Populate the reports panel with report information
        /// </summary>
        private void PopulateReportsPanel(System.Collections.Generic.List<Report> reports, System.Collections.Generic.List<User> users, System.Collections.Generic.List<Photo> photos)
        {
            if (ReportsPanel == null) return;

            ReportsPanel.Children.Clear();

            var pendingReports = reports.Where(r => r.ReportStatus == "Pending")
                                      .OrderByDescending(r => r.DateReported)
                                      .ToList();

            if (!pendingReports.Any())
            {
                var noReportsText = new TextBlock
                {
                    Text = "No pending reports - All clear!",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0x90, 0xEE, 0x90)),
                    FontSize = 14,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    FontWeight = Avalonia.Media.FontWeight.Bold
                };
                ReportsPanel.Children.Add(noReportsText);
                return;
            }

            foreach (var report in pendingReports)
            {
                var reporter = users.FirstOrDefault(u => u.UserId == report.UserId);
                var reportedPhoto = photos.FirstOrDefault(p => p.PhotoId == report.PhotoId);
                
                // Create report row
                var reportBorder = new Border
                {
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0x2d, 0x1b, 0x69)),
                    CornerRadius = new Avalonia.CornerRadius(8),
                    Padding = new Avalonia.Thickness(15),
                    Margin = new Avalonia.Thickness(0, 0, 0, 5)
                };

                var mainReportGrid = new Grid();
                mainReportGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                mainReportGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                
                var reportStack = new StackPanel { Spacing = 5 };
                
                // Report header
                var reportHeaderText = new TextBlock
                {
                    Text = $"Report #{report.ReportId} - {report.ReportStatus}",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0xFF, 0xA5, 0x00)),
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    FontSize = 14
                };
                
                // Report details
                var reportDetailsText = new TextBlock
                {
                    Text = $"Reason: {report.Reason}",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                    FontSize = 12
                };
                
                // Reporter and date
                var reportInfoText = new TextBlock
                {
                    Text = $"Reporter: {reporter?.Email ?? "Unknown"} | Date: {report.DateReported:dd/MM/yyyy HH:mm} | Photo ID: {report.PhotoId}",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0xb8, 0xa7, 0xd9)),
                    FontSize = 11
                };
                
                reportStack.Children.Add(reportHeaderText);
                reportStack.Children.Add(reportDetailsText);
                reportStack.Children.Add(reportInfoText);
                
                // Add action buttons
                var buttonStack = new StackPanel { Spacing = 5 };
                
                // Resolve button
                var resolveButton = new Button
                {
                    Content = "Resolve",
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0x4C, 0xAF, 0x50)),
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                    CornerRadius = new Avalonia.CornerRadius(4),
                    Padding = new Avalonia.Thickness(8, 4),
                    FontSize = 10
                };
                resolveButton.Click += (s, e) => ResolveReportFromSettings(report);
                
                // Delete button
                var deleteReportButton = new Button
                {
                    Content = "Delete",
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0xFF, 0x6B, 0x6B)),
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                    CornerRadius = new Avalonia.CornerRadius(4),
                    Padding = new Avalonia.Thickness(8, 4),
                    FontSize = 10
                };
                deleteReportButton.Click += (s, e) => DeleteReportFromSettings(report);
                
                buttonStack.Children.Add(resolveButton);
                buttonStack.Children.Add(deleteReportButton);
                
                Grid.SetColumn(reportStack, 0);
                Grid.SetColumn(buttonStack, 1);
                
                mainReportGrid.Children.Add(reportStack);
                mainReportGrid.Children.Add(buttonStack);
                
                reportBorder.Child = mainReportGrid;
                
                ReportsPanel.Children.Add(reportBorder);
            }
        }

        /// <summary>
        /// Delete a user from the settings admin panel
        /// </summary>
        private async void DeleteUserFromSettings(User user)
        {
            if (_user == null || !_user.IsAdmin) return;
            if (user.UserId == _user.UserId)
            {
                await ShowMessageDialog("Error", "You cannot delete your own account!");
                return;
            }

            try
            {
                Console.WriteLine($"Admin {_user.Email} is attempting to delete user: {user.Email}");
                
                bool success = _database.DeleteUser(user.UserId);
                if (success)
                {
                    await ShowMessageDialog("Success", $"User {user.Email} and all associated data (photos, reports, photo_tags) have been deleted successfully.");
                    
                    // Reload admin data to reflect changes
                    LoadAdminData();
                    LoadStatistics();
                }
                else
                {
                    await ShowMessageDialog("Error", "Failed to delete user. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Error", $"Error deleting user: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a photo from the settings admin panel
        /// </summary>
        private async void DeletePhotoFromSettings(Photo photo)
        {
            if (_user == null || !_user.IsAdmin) return;

            try
            {
                Console.WriteLine($"Admin {_user.Email} is attempting to delete photo ID: {photo.PhotoId}");
                
                bool success = _database.DeletePhoto(photo.PhotoId);
                if (success)
                {
                    await ShowMessageDialog("Success", $"Photo #{photo.PhotoId} and all associated data (photo_tags, likes, reports) have been deleted successfully.");
                    
                    // Reload admin data to reflect changes
                    LoadAdminData();
                    LoadStatistics();
                }
                else
                {
                    await ShowMessageDialog("Error", "Failed to delete photo. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Error", $"Error deleting photo: {ex.Message}");
            }
        }

        /// <summary>
        /// Resolve a report from the settings admin panel
        /// </summary>
        private async void ResolveReportFromSettings(Report report)
        {
            if (_user == null || !_user.IsAdmin) return;

            try
            {
                Console.WriteLine($"Admin {_user.Email} is resolving report ID: {report.ReportId}");
                
                bool success = _database.UpdateReportStatus(report.ReportId, "Resolved");
                if (success)
                {
                    await ShowMessageDialog("Success", $"Report #{report.ReportId} has been resolved.");
                    
                    // Reload admin data to reflect changes
                    LoadAdminData();
                    LoadStatistics();
                }
                else
                {
                    await ShowMessageDialog("Error", "Failed to resolve report. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Error", $"Error resolving report: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a report from the settings admin panel
        /// </summary>
        private async void DeleteReportFromSettings(Report report)
        {
            if (_user == null || !_user.IsAdmin) return;

            try
            {
                Console.WriteLine($"Admin {_user.Email} is attempting to delete report ID: {report.ReportId}");
                
                bool success = _database.DeleteReport(report.ReportId);
                if (success)
                {
                    await ShowMessageDialog("Success", $"Report #{report.ReportId} has been deleted successfully.");
                    
                    // Reload admin data to reflect changes
                    LoadAdminData();
                    LoadStatistics();
                }
                else
                {
                    await ShowMessageDialog("Error", "Failed to delete report. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Error", $"Error deleting report: {ex.Message}");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

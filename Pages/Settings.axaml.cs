using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using AstroGathering.Objects;
using AstroGathering.Database;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace AstroGathering.Pages
{
    public partial class SettingsPage : UserControl, INotifyPropertyChanged
    {
        private User? _user;
        private DatabaseOut _database;
        private Bitmap? _profileImage;

        // Admin Statistics Properties
        private int _totalUsers;
        private int _totalPhotos;
        private int _pendingReports;
        private int _userPhotosCount;
        private int _userLikesCount;

        // Collections for admin
        private ObservableCollection<User> _allUsers = new();

        public new event PropertyChangedEventHandler? PropertyChanged;

        // Properties for data binding
        public Bitmap? ProfileImage
        {
            get => _profileImage;
            set
            {
                _profileImage = value;
                OnPropertyChanged(nameof(ProfileImage));
            }
        }

        public int TotalUsers
        {
            get => _totalUsers;
            set
            {
                _totalUsers = value;
                OnPropertyChanged(nameof(TotalUsers));
            }
        }

        public int TotalPhotos
        {
            get => _totalPhotos;
            set
            {
                _totalPhotos = value;
                OnPropertyChanged(nameof(TotalPhotos));
            }
        }

        public int PendingReports
        {
            get => _pendingReports;
            set
            {
                _pendingReports = value;
                OnPropertyChanged(nameof(PendingReports));
            }
        }

        public int UserPhotosCount
        {
            get => _userPhotosCount;
            set
            {
                _userPhotosCount = value;
                OnPropertyChanged(nameof(UserPhotosCount));
            }
        }

        public int UserLikesCount
        {
            get => _userLikesCount;
            set
            {
                _userLikesCount = value;
                OnPropertyChanged(nameof(UserLikesCount));
            }
        }

        public ObservableCollection<User> AllUsers
        {
            get => _allUsers;
            set
            {
                _allUsers = value;
                OnPropertyChanged(nameof(AllUsers));
            }
        }

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
            
            if (ManageUsersBtn != null)
                ManageUsersBtn.Click += OnViewAllUsersClick;
            
            if (MakeAdminBtn != null)
                MakeAdminBtn.Click += OnMakeAdminClick;
            
            if (RemoveAdminBtn != null)
                RemoveAdminBtn.Click += OnRemoveAdminClick;
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

        private async void LoadProfileImage()
        {
            if (_user?.ProfilePictureUrl == null) return;

            try
            {
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(_user.ProfilePictureUrl);
                using var stream = new MemoryStream(imageBytes);
                ProfileImage = new Bitmap(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load profile image: {ex.Message}");
                // Profile image will remain null, which is fine
            }
        }

        private async void LoadStatistics()
        {
            if (_user == null) return;

            try
            {
                // Load user-specific stats
                UserPhotosCount = _database.GetUserPhotosCount(_user.UserId);
                UserLikesCount = _database.GetUserLikesCount(_user.UserId);
                
                // Update UI text blocks
                if (PhotosUploadedText != null)
                    PhotosUploadedText.Text = UserPhotosCount.ToString();
                    
                if (LikesReceivedText != null)
                    LikesReceivedText.Text = UserLikesCount.ToString();

                // Load admin stats if user is admin
                if (_user.IsAdmin)
                {
                    TotalUsers = _database.GetTotalUsersCount();
                    TotalPhotos = _database.GetTotalPhotosCount();
                    PendingReports = _database.GetPendingReportsCount();
                    
                    // Update admin UI text blocks
                    if (TotalUsersText != null)
                        TotalUsersText.Text = TotalUsers.ToString();
                        
                    if (TotalPhotosText != null)
                        TotalPhotosText.Text = TotalPhotos.ToString();
                        
                    if (PendingReportsText != null)
                        PendingReportsText.Text = PendingReports.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load statistics: {ex.Message}");
            }
        }

        private async void OnViewAllUsersClick(object sender, RoutedEventArgs e)
        {
            if (_user == null || !_user.IsAdmin) return;

            try
            {
                var users = _database.GetAllUsers();
                AllUsers.Clear();
                foreach (var user in users)
                {
                    AllUsers.Add(user);
                }

                // Show users in a dialog or update the UI
                var userList = string.Join("\n", users.Select(u => 
                    $"• {u.Email} - {(u.IsAdmin ? "Admin" : "User")} - Photos: {_database.GetUserPhotosCount(u.UserId)}"));
                
                await ShowMessageDialog("All Users", $"Total Users: {users.Count}\n\n{userList}");
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Error", $"Failed to load users: {ex.Message}");
            }
        }

        private async void OnMakeAdminClick(object sender, RoutedEventArgs e)
        {
            if (_user == null || !_user.IsAdmin) return;

            try
            {
                // Get email from input text box
                string email = UserEmailInput?.Text?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(email))
                {
                    await ShowMessageDialog("Error", "Please enter an email address.");
                    return;
                }

                var targetUser = _database.GetUserByEmail(email);
                if (targetUser == null)
                {
                    await ShowMessageDialog("Error", "User not found with that email address.");
                    return;
                }

                if (targetUser.IsAdmin)
                {
                    await ShowMessageDialog("Info", "User is already an administrator.");
                    return;
                }

                bool success = _database.UpdateUserAdminStatus(email, true);
                if (success)
                {
                    await ShowMessageDialog("Success", $"User {email} has been made an administrator.");
                    // Clear input and refresh stats
                    if (UserEmailInput != null)
                        UserEmailInput.Text = "";
                    LoadStatistics();
                }
                else
                {
                    await ShowMessageDialog("Error", "Failed to update user admin status.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Error", $"Failed to make user admin: {ex.Message}");
            }
        }

        private async void OnRemoveAdminClick(object sender, RoutedEventArgs e)
        {
            if (_user == null || !_user.IsAdmin) return;

            try
            {
                // Get email from input text box
                string email = UserEmailInput?.Text?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(email))
                {
                    await ShowMessageDialog("Error", "Please enter an email address.");
                    return;
                }

                if (email.Equals(_user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    await ShowMessageDialog("Error", "You cannot remove your own admin privileges.");
                    return;
                }

                var targetUser = _database.GetUserByEmail(email);
                if (targetUser == null)
                {
                    await ShowMessageDialog("Error", "User not found with that email address.");
                    return;
                }

                if (!targetUser.IsAdmin)
                {
                    await ShowMessageDialog("Info", "User is not an administrator.");
                    return;
                }

                bool success = _database.UpdateUserAdminStatus(email, false);
                if (success)
                {
                    await ShowMessageDialog("Success", $"Admin rights removed from {email}.");
                    // Clear input and refresh stats
                    if (UserEmailInput != null)
                        UserEmailInput.Text = "";
                    LoadStatistics();
                }
                else
                {
                    await ShowMessageDialog("Error", "Failed to update user admin status.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Error", $"Failed to remove admin rights: {ex.Message}");
            }
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

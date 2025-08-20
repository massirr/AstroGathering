using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AstroGathering.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AstroGathering.Pages
{
    public partial class GalleryPage : UserControl
    {
        private readonly DatabaseOut _databaseOut;
        private List<GalleryPhoto> _photos = new();

        public GalleryPage()
        {
            InitializeComponent();
            _databaseOut = new DatabaseOut();
            _ = LoadPhotosAsync(); // Fire and forget for constructor
        }

        private async Task LoadPhotosAsync()
        {
            try
            {
                // Show loading state
                LoadingPanel!.IsVisible = true;
                NoPhotosPanel!.IsVisible = false;
                PhotoGrid!.IsVisible = false;

                // Load data from database
                var databaseOut = new DatabaseOut();
                var events = await Task.Run(() => databaseOut.GetAllAstronomicalEventsWithImages());
                
                // Convert to gallery photos
                var galleryPhotos = events.Select(evt => new GalleryPhoto
                {
                    Id = evt.Id,
                    Name = evt.Name ?? "Astronomical Event",
                    CleanName = CleanPhotoName(evt.Name ?? "Astronomical Event"),
                    Description = evt.Description ?? "No description available",
                    ImageUrl = evt.ImageUrl ?? "",
                    HdImageUrl = evt.HdImageUrl ?? "",
                    Source = evt.Source ?? "NASA APOD",
                    Date = evt.Date,
                    FormattedDate = evt.Date.ToString("MMM dd, yyyy"),
                    Type = evt.Type ?? "Astronomical",
                    TypeTag = GetTypeTag(evt.Type ?? "Astronomical"),
                    LikeCount = GetRandomLikeCount()
                }).Where(photo => !string.IsNullOrEmpty(photo.ImageUrl)).ToList();

                // Update UI on main thread
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PhotoGrid!.ItemsSource = galleryPhotos;
                    
                    // Update photo count
                    if (PhotoCountText != null)
                        PhotoCountText.Text = $"{galleryPhotos.Count} photos found";
                    
                    // Show appropriate panel
                    LoadingPanel!.IsVisible = false;
                    if (galleryPhotos.Any())
                    {
                        PhotoGrid!.IsVisible = true;
                    }
                    else
                    {
                        NoPhotosPanel!.IsVisible = true;
                    }
                });
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LoadingPanel!.IsVisible = false;
                    NoPhotosPanel!.IsVisible = true;
                    if (PhotoCountText != null)
                        PhotoCountText.Text = $"Error loading photos: {ex.Message}";
                });
            }
        }

        private void UpdatePhotoCount()
        {
            if (PhotoCountText != null)
            {
                var count = _photos.Count;
                PhotoCountText.Text = count == 1 ? "1 photo found" : $"{count} photos found";
            }
        }

        private void UpdateGalleryDisplay()
        {
            if (_photos.Any())
            {
                ShowPhotosState();
                if (PhotoGrid != null)
                {
                    PhotoGrid.ItemsSource = _photos;
                }
            }
            else
            {
                ShowNoPhotosState();
            }
        }

        private void ShowPhotosState()
        {
            if (LoadingPanel != null) LoadingPanel.IsVisible = false;
            if (NoPhotosPanel != null) NoPhotosPanel.IsVisible = false;
            if (PhotoGrid != null) PhotoGrid.IsVisible = true;
        }

        private void ShowNoPhotosState()
        {
            if (LoadingPanel != null) LoadingPanel.IsVisible = false;
            if (PhotoGrid != null) PhotoGrid.IsVisible = false;
            if (NoPhotosPanel != null) NoPhotosPanel.IsVisible = true;
        }

        private string CleanPhotoName(string name)
        {
            // Remove emoji and clean up the name for display
            return name.Replace("🌌 ", "").Trim();
        }

        private string GetTypeTag(string type)
        {
            return type switch
            {
                "Astronomy Feature" => "#apod",
                "Near Earth Object" => "#neo",
                "Moon Phase" => "#moon",
                _ => "#space"
            };
        }

        private int GetRandomLikeCount()
        {
            // Simulate like counts between 50-500 for demo purposes
            var random = new Random();
            return random.Next(50, 501);
        }
    }

    // Data model for gallery photos
    public class GalleryPhoto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CleanName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string HdImageUrl { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string FormattedDate { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TypeTag { get; set; } = string.Empty;
        public int LikeCount { get; set; }
    }
}

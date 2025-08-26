using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Media.Imaging;
using AstroGathering.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Threading;
using System.ComponentModel;

namespace AstroGathering.Pages
{
    public partial class GalleryPage : UserControl
    {
        // Add static cache at class level to persist between gallery visits
        private static readonly Dictionary<string, Bitmap> _imageCache = new();
        
        // Public static methods to access cache from GalleryPhoto
        public static bool TryGetCachedImage(string url, out Bitmap? bitmap)
        {
            return _imageCache.TryGetValue(url, out bitmap);
        }
        
        public static void CacheImage(string url, Bitmap bitmap)
        {
            _imageCache[url] = bitmap;
        }
        
        // Optional: Clear cache for memory management
        public static void ClearImageCache()
        {
            Console.WriteLine($"🗑️ Clearing image cache ({_imageCache.Count} images)");
            foreach (var bitmap in _imageCache.Values)
            {
                bitmap?.Dispose();
            }
            _imageCache.Clear();
        }
        
        // Optional: Get cache info for debugging
        public static int GetCacheSize()
        {
            return _imageCache.Count;
        }
        
        public GalleryPage()
        {
            InitializeComponent();
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
                
                // Load astronomical events and user photos in parallel
                var eventsTask = Task.Run(() => databaseOut.GetAllAstronomicalEventsWithImages());
                var photosTask = databaseOut.GetAllPhotosAsync();
                
                await Task.WhenAll(eventsTask, photosTask);
                
                var events = eventsTask.Result;
                var photos = photosTask.Result;
                
                // Convert astronomical events to gallery photos
                var eventPhotos = events.Select(evt => new GalleryPhoto
                {
                    Id = evt.EventId, // Updated to use EventId
                    Name = evt.EventName ?? "Astronomical Event", // Updated to use EventName
                    CleanName = CleanPhotoName(evt.EventName ?? "Astronomical Event"),
                    Description = evt.Description ?? "No description available",
                    ImageUrl = evt.ImageUrl ?? "",
                    HdImageUrl = evt.HdImageUrl ?? "",
                    Source = evt.Source ?? "NASA APOD",
                    Date = evt.EventDate, // Updated to use EventDate
                    FormattedDate = evt.EventDate.ToString("MMM dd, yyyy"),
                    Type = "NASA APOD",
                    TypeTag = GetTypeTag(evt.Type ?? "Astronomical"),
                    LikeCount = GetRandomLikeCount(),
                    IsUserPhoto = false,
                    Location = FormatLocation(null, evt.Latitude, evt.Longitude),
                    HasLocation = evt.Latitude.HasValue && evt.Longitude.HasValue
                }).Where(photo => !string.IsNullOrEmpty(photo.ImageUrl));

                var eventPhotosList = eventPhotos.ToList();

                // Debug: Log first few image URLs
                Console.WriteLine($"Sample image URLs from events:");
                foreach (var photo in eventPhotosList.Take(3))
                {
                    Console.WriteLine($"  - {photo.Name}: {photo.ImageUrl}");
                }

                // Convert user photos to gallery photos
                var userGalleryPhotos = photos.Select(photo => new GalleryPhoto
                {
                    Id = photo.PhotoId,
                    Name = photo.EventName ?? "User Photo", // Use event name stored directly in photo
                    CleanName = CleanPhotoName(photo.EventName ?? "User Photo"),
                    Description = photo.Description ?? "", // Use photo description
                    ImageUrl = photo.ImageUrl,
                    HdImageUrl = photo.ImageUrl, // Use same image for HD
                    Source = "Community Upload",
                    Date = photo.DateTaken ?? photo.TimeUploaded, // Use DateTaken if available, otherwise TimeUploaded
                    FormattedDate = (photo.DateTaken ?? photo.TimeUploaded).ToString("MMM dd, yyyy"),
                    Type = "User Upload",
                    TypeTag = "#community", // Temporary value, will be updated below
                    LikeCount = GetRandomLikeCount(),
                    IsUserPhoto = true,
                    Location = FormatLocation(photo.Location, photo.Latitude, photo.Longitude),
                    HasLocation = !string.IsNullOrEmpty(photo.Location) || (photo.Latitude.HasValue && photo.Longitude.HasValue),
                    Tags = ""  // We'll load tags separately later if needed
                }).ToList();

                // Now update tags asynchronously for each photo
                foreach (var galleryPhoto in userGalleryPhotos)
                {
                    var originalPhoto = photos.FirstOrDefault(p => p.PhotoId == galleryPhoto.Id);
                    if (originalPhoto != null)
                    {
                        galleryPhoto.TypeTag = await GetUserPhotoTagAsync(originalPhoto);
                    }
                }

                // Debug: Log user photos
                Console.WriteLine($"Sample user photo URLs:");
                foreach (var photo in userGalleryPhotos.Take(2))
                {
                    Console.WriteLine($"  - {photo.Name}: {photo.ImageUrl}");
                }

                // Combine all photos and sort by date (newest first)
                var allPhotos = eventPhotosList.Concat(userGalleryPhotos)
                    .OrderByDescending(p => p.Date)
                    .ToList();

                Console.WriteLine($"Total photos to display: {allPhotos.Count}");

                // Update UI on main thread first to show the cards
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PhotoGrid!.ItemsSource = allPhotos;
                    
                    // Update photo count
                    if (PhotoCountText != null)
                        PhotoCountText.Text = $"{allPhotos.Count} photos found ({photos.Count} community uploads, {events.Count} NASA APOD)";
                    
                    // Show appropriate panel
                    LoadingPanel!.IsVisible = false;
                    if (allPhotos.Any())
                    {
                        PhotoGrid!.IsVisible = true;
                        Console.WriteLine("Gallery photos loaded and grid made visible");
                    }
                    else
                    {
                        NoPhotosPanel!.IsVisible = true;
                        Console.WriteLine("No photos found, showing no photos panel");
                    }
                });

                // Load images asynchronously after UI is shown
                Console.WriteLine("Starting async image loading...");
                _ = Task.Run(async () =>
                {
                    var semaphore = new SemaphoreSlim(3, 3); // Limit to 3 concurrent downloads
                    var tasks = allPhotos.Select(async photo =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await photo.LoadImageAsync();
                            // Trigger UI update on main thread
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                // Force UI refresh for this item
                                PhotoGrid?.InvalidateVisual();
                            });
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });
                    
                    await Task.WhenAll(tasks);
                    Console.WriteLine("All images loaded!");
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


        private string CleanPhotoName(string name)
        {
            // Remove emoji and clean up the name for display
            return name.Replace("🌌 ", "").Trim();
        }

        private string ExtractPhotoName(string description)
        {
            // Extract the first line as the photo name, or use first sentence if no line breaks
            if (string.IsNullOrEmpty(description)) return "Untitled Photo";
            
            var lines = description.Split('\n');
            var name = lines[0].Trim();
            
            // If the name is too long, truncate it
            if (name.Length > 50)
            {
                name = name.Substring(0, 47) + "...";
            }
            
            return string.IsNullOrEmpty(name) ? "Untitled Photo" : name;
        }

        private string ExtractTagsFromDescription(string description)
        {
            // Look for "Tags: " in the description and extract the tags
            if (string.IsNullOrEmpty(description)) return "";
            
            var tagStart = description.IndexOf("Tags: ");
            if (tagStart == -1) return "";
            
            var tagString = description.Substring(tagStart + 6);
            var tagEnd = tagString.IndexOf('\n');
            if (tagEnd != -1)
            {
                tagString = tagString.Substring(0, tagEnd);
            }
            
            return tagString.Trim();
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

        private async Task<string> GetUserPhotoTagAsync(Objects.Photo photo)
        {
            try
            {
                // First, try to get tags from the database
                var dbInstance = new DatabaseOut();
                var databaseTags = await dbInstance.GetPhotoTagsAsync(photo.PhotoId);
                
                if (databaseTags.Count > 0)
                {
                    // Return the first tag from the database
                    return databaseTags[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting database tags for photo {photo.PhotoId}: {ex.Message}");
            }
            
            // Fallback to text analysis if no database tags found
            var description = photo.Description?.ToLowerInvariant() ?? "";
            var eventName = photo.EventName?.ToLowerInvariant() ?? "";
            
            // Priority 1: Check for specific astronomical events
            if (eventName.Contains("meteor") || description.Contains("meteor"))
                return "#meteor";
            if (eventName.Contains("moon") || description.Contains("moon"))
                return "#moon";
            if (eventName.Contains("planet") || description.Contains("planet"))
                return "#planet";
            if (eventName.Contains("star") || description.Contains("star"))
                return "#stars";
            if (eventName.Contains("galaxy") || description.Contains("galaxy"))
                return "#galaxy";
            if (eventName.Contains("nebula") || description.Contains("nebula"))
                return "#nebula";
            if (eventName.Contains("comet") || description.Contains("comet"))
                return "#comet";
            if (eventName.Contains("eclipse") || description.Contains("eclipse"))
                return "#eclipse";
            if (eventName.Contains("conjunction") || description.Contains("conjunction"))
                return "#conjunction";
            
            // Priority 2: Check for general space terms
            if (eventName.Contains("space") || description.Contains("space"))
                return "#space";
            if (eventName.Contains("astro") || description.Contains("astro"))
                return "#astronomy";
            
            // Default fallback
            return "#community";
        }

        private int GetRandomLikeCount()
        {
            // Simulate like counts between 50-500 for demo purposes
            var random = new Random();
            return random.Next(50, 501);
        }

        private string FormatLocation(string? locationText, double? latitude, double? longitude)
        {
            // Prioritize the location text if available
            if (!string.IsNullOrEmpty(locationText))
            {
                return locationText;
            }
            
            // If no location text but coordinates are available, format them
            if (latitude.HasValue && longitude.HasValue)
            {
                return $"{latitude.Value:F2}°, {longitude.Value:F2}°";
            }
            
            return "Unknown location";
        }
    }

    // Data model for gallery photos
    public class GalleryPhoto : INotifyPropertyChanged
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
        public bool IsUserPhoto { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool HasLocation { get; set; } = false;
        public string Tags { get; set; } = string.Empty;
        
        // Add bitmap property for loaded images
        private Bitmap? _loadedImage;
        public Bitmap? LoadedImage 
        { 
            get => _loadedImage;
            set 
            {
                _loadedImage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadedImage)));
            }
        }
        
        public bool IsImageLoaded { get; set; } = false;
        public string LoadingStatus { get; set; } = "Loading...";
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public async Task LoadImageAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(ImageUrl))
                {
                    LoadingStatus = "No image URL";
                    return;
                }

                // Check cache first
                if (GalleryPage.TryGetCachedImage(ImageUrl, out var cachedBitmap))
                {
                    LoadedImage = cachedBitmap;
                    IsImageLoaded = true;
                    LoadingStatus = "Loaded from cache";
                    Console.WriteLine($"Loaded from cache: {CleanName}");
                    return;
                }

                // Only download if not cached
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "AstroGathering/1.0");
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                Console.WriteLine($".. Downloading image: {CleanName}");
                var imageData = await httpClient.GetByteArrayAsync(ImageUrl);
                using var stream = new MemoryStream(imageData);
                
                var bitmap = new Bitmap(stream);
                
                // Cache the bitmap for future use
                GalleryPage.CacheImage(ImageUrl, bitmap);
                
                LoadedImage = bitmap;
                IsImageLoaded = true;
                LoadingStatus = "Loaded";
                
                Console.WriteLine($">> Successfully loaded and cached image: {CleanName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to load image {CleanName}: {ex.Message}");
                LoadingStatus = "Failed to load";
                IsImageLoaded = false;
            }
        }
    }
}

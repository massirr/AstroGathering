-- Sample help content for AstroGathering
-- Run this script to populate your help_content table with initial data

-- Help & Support content
INSERT INTO help_content (section, title, content, display_order) VALUES
('help_support', 'Getting Started', 'Welcome to AstroGathering! Sign in with your Google account to access all features and start exploring the cosmos with our community.', 1),
('help_support', 'Uploading Photos', 'Share your astrophotography with the community. Navigate to the Upload page, select your image, add a description and location, then click upload. Your photos will be stored securely in Azure cloud storage.', 2),
('help_support', 'Viewing Astronomical Events', 'Use the Home calendar to explore upcoming astronomical events. Click on any date to see detailed information about celestial happenings, complete with NASA imagery and descriptions.', 3),
('help_support', 'Gallery Features', 'Browse stunning astrophotography from our community and NASA''s Astronomy Picture of the Day. Use filters to find specific types of images or events.', 4);

-- Contact & Support content
INSERT INTO help_content (section, title, content, display_order) VALUES
('contact_support', 'Technical Support', 'Experiencing technical issues? Please describe your problem in detail using the contact form. Include information about your browser, operating system, and the specific error you encountered.', 1),
('contact_support', 'Feature Requests', 'Have an idea for improving AstroGathering? We''d love to hear your suggestions! Use the contact form to share your ideas for new features or improvements.', 2),
('contact_support', 'Community Guidelines', 'Help us maintain a welcoming community. Report inappropriate content, be respectful in comments, and only upload original or properly credited astrophotography.', 3),
('contact_support', 'Account Issues', 'Having trouble with your account? Contact us for help with login issues, profile updates, or account deletion requests.', 4);

-- About content
INSERT INTO help_content (section, title, content, display_order) VALUES
('about', 'Our Mission', 'AstroGathering brings together astronomy enthusiasts to share their passion for the cosmos. We provide a platform to discover celestial events, share astrophotography, and connect with fellow stargazers around the world.', 1),
('about', 'Features', 'Explore upcoming astronomical events with our interactive calendar, upload and browse stunning astrophotography, view NASA''s daily astronomy pictures, and connect with a community of space enthusiasts.', 2),
('about', 'Technology', 'Built with Avalonia UI for cross-platform compatibility, powered by NASA APIs for astronomical data, and secured with Google OAuth authentication. Your photos are safely stored in Microsoft Azure cloud storage.', 3),
('about', 'Developer', 'AstroGathering is developed by Irakoze Darlo at Thomas More InspirationLab as part of ongoing research and development in astronomical applications and community platforms.', 4);

-- Privacy content
INSERT INTO help_content (section, title, content, display_order) VALUES
('privacy', 'Data Collection', 'We only collect necessary information via Google OAuth for authentication: your email, name, and profile picture. We do not share this information with third parties.', 1),
('privacy', 'Photo Storage', 'Your uploaded photos are stored securely in Microsoft Azure cloud storage with enterprise-grade security. Only you and the community can view photos you choose to share.', 2),
('privacy', 'Account Control', 'You have full control over your data. You can delete your account and all associated data at any time through the Settings page or by contacting support.', 3),
('privacy', 'Cookies and Tracking', 'We use minimal tracking for essential functionality only. No third-party advertising or unnecessary tracking cookies are used in AstroGathering.', 4),
('privacy', 'Data Security', 'All data transmission is encrypted, user authentication is handled securely through Google OAuth, and we follow best practices for data protection and privacy.', 5);

-- Data Sources content
INSERT INTO help_content (section, title, content, display_order) VALUES
('data_sources', 'NASA APOD API', 'NASA''s Astronomy Picture of the Day provides daily stunning images and explanations of our universe. Source: api.nasa.gov/planetary/apod | Service: NasaApiService.cs', 1),
('data_sources', 'Azure Blob Storage', 'Your uploaded photos are securely stored in Microsoft Azure cloud storage for fast, reliable access. Service: PhotoUploadService.cs', 2),
('data_sources', 'Google OAuth Authentication', 'Secure authentication using Google''s OAuth 2.0 service for seamless sign-in and user management. Services: DesktopOAuthService.cs, AuthCallbackService.cs', 3),
('data_sources', 'Geocoding Service', 'Location services to convert addresses to coordinates and provide geographical context for astronomical events. Service: GeocodingService.cs', 4),
('data_sources', 'Configuration Management', 'Secure configuration management for API keys, database connections, and application settings. Service: ConfigurationService.cs', 5);

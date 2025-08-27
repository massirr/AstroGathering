
# AstroGathering

AstroGathering is a modern, cross-platform desktop application built with Avalonia, designed to help manage and share astronomical gatherings, events, and photos. The app leverages Google OAuth for secure authentication and integrates with external APIs for enhanced features.

## Features
- User authentication with Google OAuth 2.0
- Cross-platform desktop application (Windows, macOS, Linux)
- Modern, responsive UI built with Avalonia
- Photo gallery and upload functionality
- Tagging and categorization of astronomical events and photos
- Integration with NASA and geocoding APIs
- Social features: likes, reports, and user profiles

## Getting Started
1. Clone the repository
2. Set up your environment variables in a `.env` file (see `.env.example` for required keys)
3. Build and run the application:

```bash
dotnet build
dotnet run
```

## Requirements
- .NET 8.0 not later as avalonia will not work
- Valid Google OAuth credentials (see Google Cloud Console)

## What's Next: (probably)
- Real-time notifications with Google
- Advanced photo categorization with search filters
- Enhanced social features
- Deployment (cross-platform installers, auto-update)

## Key Takeaways
- Proper architecture planning pays off
- External API integrations add real value and exposure
- Modern C# enables clean, maintainable code
- Layered design supports future growth

## Deployment
Deployment plans include packaging the app for Windows, macOS, and Linux using Avalonia's publishing tools. Future updates may include auto-update functionality and distribution via app stores or direct downloads.

---
*A Thomas More project for InspirationLab*

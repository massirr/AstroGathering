# AstroGathering - Google OAuth Implementation Guide

## Overview
This document details the complete process of implementing Google OAuth 2.0 authentication in an Avalonia desktop application, including all the challenges encountered and solutions implemented.

## Project Structure
```
AstroGathering/
├── Services/
│   ├── DesktopOAuthService.cs     # Main OAuth service with PKCE
│   ├── AuthCallbackService.cs     # Handles OAuth callback
│   └── ConfigurationService.cs    # Environment variable management
├── Objects/
│   └── user.cs                    # User data model
├── Pages/
│   └── Home.axaml(.cs)           # Post-authentication page
├── MainWindow.axaml(.cs)          # Main application window
├── .env                          # Environment variables (Desktop OAuth credentials)
└── README.md                     # This documentation
```

## Google Cloud Console Setup

### Step 1: Create OAuth 2.0 Client ID
1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Navigate to **APIs & Services → Credentials**
3. Click **"Create Credentials" → "OAuth 2.0 Client ID"**
4. **IMPORTANT**: Select **"Desktop application"** (NOT Web application)
5. Give it a descriptive name (e.g., "AstroGathering Desktop App")
6. Click **"Create"**
7. Copy the **Client ID** and **Client Secret**

### Step 2: Configure Environment Variables
Create/update `.env` file in project root:
```env
GOOGLE_CLIENT_ID=your_desktop_client_id_here
GOOGLE_CLIENT_SECRET=your_desktop_client_secret_here
GOOGLE_REDIRECT_URI=http://127.0.0.1:8080/callback
```

## Implementation Details

### OAuth Flow Architecture
Our implementation uses the **Authorization Code Flow with PKCE** (Proof Key for Code Exchange), which is the recommended approach for desktop applications.

#### Flow Steps:
1. **Generate PKCE parameters** (code_verifier & code_challenge)
2. **Open browser** to Google's authorization URL
3. **User authenticates** with Google
4. **Google redirects** to local callback server
5. **Exchange authorization code** for tokens using PKCE
6. **Fetch user information** using access token

### Key Components

#### DesktopOAuthService.cs
- **Purpose**: Main OAuth service implementing PKCE flow
- **Key Features**:
  - PKCE code generation (SHA256 challenge)
  - Authorization URL construction
  - Token exchange with Google
  - User info retrieval
- **Security**: Uses PKCE to prevent authorization code interception

#### AuthCallbackService.cs
- **Purpose**: Local HTTP server to handle OAuth callback
- **Listens on**: `http://127.0.0.1:8080/callback`
- **Response**: Success/failure HTML page with auto-close script

#### ConfigurationService.cs
- **Purpose**: Manages environment variables
- **Features**: Loads `.env` file and provides type-safe access to OAuth credentials

## Common Issues & Solutions

### Issue 1: "OAuth 2 parameters can only have a single value: access_type"
**Problem**: Duplicate parameters in OAuth URL
**Solution**: Implemented proper parameter checking to avoid duplicates

### Issue 2: "You can't sign in to this app because it doesn't comply with Google's OAuth 2.0 policy"
**Problem**: Using Web application OAuth client for desktop app
**Solution**: Created Desktop application OAuth client in Google Cloud Console

### Issue 3: `localhost` vs `127.0.0.1`
**Problem**: Some systems don't resolve localhost properly
**Solution**: Use `127.0.0.1:8080` instead of `localhost:8080`

### Issue 4: Google Auth Library Limitations
**Problem**: Google.Apis.Auth library designed for web applications
**Solution**: Implemented custom OAuth service with direct HTTP calls to Google's APIs

## Technical Decisions

### Why PKCE?
- **Security**: Prevents authorization code interception attacks
- **Best Practice**: Recommended by OAuth 2.1 specification
- **Desktop Apps**: Essential for public clients (desktop apps)

### Why Custom Implementation?
- **Flexibility**: Full control over OAuth flow
- **Desktop Optimized**: Designed specifically for desktop applications
- **No Dependencies**: Reduced external package dependencies
- **Standards Compliant**: Follows OAuth 2.0/2.1 specifications

### Why 127.0.0.1 over localhost?
- **Reliability**: Avoids DNS resolution issues
- **Google Compatibility**: Works consistently with Google's OAuth
- **Cross-Platform**: Works on all operating systems

## Environment Setup

### Required NuGet Packages
```xml
<PackageReference Include="Avalonia" Version="11.3.2" />
<PackageReference Include="Avalonia.Desktop" Version="11.3.2" />
<PackageReference Include="DotNetEnv" Version="3.1.1" />
<PackageReference Include="Microsoft.AspNetCore.Server.Kestrel" Version="2.3.0" />
```

### Development Environment
- **.NET**: 8.0
- **UI Framework**: Avalonia 11.3.2
- **Platform**: Cross-platform (Windows, macOS, Linux)

## Testing & Verification

### How to Test
1. Ensure `.env` file has correct Desktop OAuth credentials
2. Run the application
3. Click "Login with Google" button
4. Browser should open to Google's authorization page
5. After authentication, app should display user information

### Expected Behavior
- Browser opens automatically
- Google login page appears
- After authentication, browser shows "Authentication successful!"
- Desktop app displays user's name and email
- Tokens are properly stored for session

## Security Considerations

### What's Secure
- ✅ PKCE implementation prevents code interception
- ✅ Local callback server only accepts localhost connections
- ✅ Client secret properly protected in environment variables
- ✅ Tokens handled securely in memory

### Important Notes
- 🔒 Never commit `.env` file to version control
- 🔒 Client secret should be treated as confidential
- 🔒 Consider token refresh implementation for long-running sessions

## Troubleshooting

### Build Issues
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Authentication Issues
1. **Check .env file**: Ensure correct Desktop OAuth credentials
2. **Verify OAuth client type**: Must be "Desktop application" in Google Cloud Console
3. **Check redirect URI**: Should be `http://127.0.0.1:8080/callback`
4. **Port conflicts**: Ensure port 8080 is available

### Runtime Issues
```bash
# Check if callback server is running
netstat -an | grep 8080

# View application logs
dotnet run --verbosity normal
```

## Future Enhancements

### Potential Improvements
- **Token Refresh**: Implement automatic token refresh
- **Persistent Storage**: Save refresh tokens securely
- **Multi-Account**: Support multiple Google accounts
- **Error Recovery**: Better error handling and retry logic

### Code Quality
- ✅ Zero build warnings
- ✅ Proper null handling
- ✅ Async/await best practices
- ✅ Clean separation of concerns

## Development History

### Evolution of Implementation
1. **Initial Attempt**: Used Google.Apis.Auth library (designed for web apps)
2. **Problem Discovery**: Library not suitable for desktop OAuth flows
3. **Custom Implementation**: Built OAuth service from scratch
4. **PKCE Addition**: Added security enhancement
5. **Final Refinement**: Clean code, proper error handling

### Lessons Learned
- Desktop OAuth requires different approach than web OAuth
- PKCE is essential for desktop application security
- Direct API calls provide more flexibility than libraries
- Environment configuration is crucial for deployment

## References

- [OAuth 2.0 for Native Apps (RFC 8252)](https://tools.ietf.org/html/rfc8252)
- [OAuth 2.0 Security Best Practices](https://tools.ietf.org/html/draft-ietf-oauth-security-topics)
- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [PKCE Specification (RFC 7636)](https://tools.ietf.org/html/rfc7636)

---
*Last Updated: July 19, 2025*
*Author: Development Team*
*Project: AstroGathering Desktop Application*

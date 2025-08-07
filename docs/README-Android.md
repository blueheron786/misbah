# Misbah Android App

Misbah is a note-taking application built with Blazor WebAssembly and wrapped for Android using Capacitor.

## Project Structure

- `Misbah.Core/` - Core business logic and services
- `Misbah.BlazorDesktop/` - Original desktop Blazor application
- `Misbah.Web/` - Blazor WebAssembly version for mobile
- `Misbah.Core.Tests/` - Unit tests
- `android/` - Generated Android project (Capacitor)
- `dist/` - Built web assets for mobile app
- `node_modules/` - Node.js dependencies for Capacitor

## Prerequisites for Android Development

1. **Node.js** (v16 or later) - Already installed ✅
2. **Android Studio** - Already installed ✅
3. **Java Development Kit (JDK)** - Already installed ✅
4. **.NET 8 SDK** - Already installed ✅

## Building and Running the Android App

### Step 1: Build the Web App
```bash
cd Misbah.Web
dotnet publish -c Release -o ../dist
```

### Step 2: Sync Capacitor
```bash
# From the root directory (d:\code\misbah)
npx capacitor sync android
```

### Step 3: Open in Android Studio
```bash
npx capacitor open android
```

### Step 4: Build and Run
1. Android Studio will open with the project
2. Connect an Android device or start an emulator
3. Click the "Run" button or press Shift+F10
4. The app will be installed and launched on your device/emulator

## Development Workflow

### Making Changes to the App
1. Edit the Blazor components in `Misbah.Web/`
2. Rebuild the web app: `cd Misbah.Web && dotnet publish -c Release -o ../dist`
3. Sync with Capacitor: `npx capacitor sync android`
4. Refresh/rebuild in Android Studio

### Useful Commands
```bash
# Install dependencies
npm install

# Build web app
npm run build-web

# Sync Capacitor
npx capacitor sync

# Open Android Studio
npx capacitor open android

# Run on connected device (alternative to Android Studio)
npx capacitor run android
```

## Troubleshooting

### Common Issues

1. **Build errors in Android Studio**
   - Make sure all Android SDK components are installed
   - Check that JAVA_HOME is set correctly
   - Try cleaning the project: Build → Clean Project

2. **Web assets not updating**
   - Run `npx capacitor sync android` after rebuilding the web app
   - Clear app data on the device/emulator

3. **Device not recognized**
   - Enable Developer Options and USB Debugging on your Android device
   - Install device drivers if needed

4. **Duplicate resources error (Task :app:mergeDebugAssets FAILED)**
   - This was fixed by disabling compression in the Misbah.Web project
   - The project now builds without `.gz` files to avoid duplicates
   - If you see this error, run the build script which includes cleanup steps

### Fixed Issues

✅ **Duplicate Resources**: Resolved by disabling Blazor compression for mobile builds
- Problem: Android build failed with 93+ duplicate resource errors
- Solution: Added `<BlazorEnableCompression>false</BlazorEnableCompression>` to project file
- Result: No more `.gz` files and successful Android builds

### Capacitor Configuration

The app configuration is in `capacitor.config.json`. Key settings:
- `appId`: `com.misbah.app` - Android package identifier
- `appName`: `misbah` - Display name
- `webDir`: `dist\wwwroot` - Directory containing built web assets

## Features Adapted for Mobile

### File System Access
- The original desktop app used native file dialogs
- Mobile version uses a default "Notes" directory
- Future enhancement: Implement mobile-friendly file picking

### UI Considerations
- Touch-friendly interface
- Responsive design for various screen sizes
- Mobile-optimized navigation

## Next Steps

1. **Test on Physical Device**: Deploy to a real Android device for testing
2. **Optimize for Mobile**: Adjust UI components for better mobile experience
3. **Add Mobile Features**: 
   - File system access using Capacitor Filesystem API
   - Share functionality
   - Push notifications
4. **Performance**: Optimize bundle size and loading times
5. **Distribution**: Prepare for Google Play Store (signing, metadata, etc.)

## Technology Stack

- **Frontend**: Blazor WebAssembly
- **Backend**: .NET 8
- **Mobile Wrapper**: Capacitor
- **Platform**: Android
- **Build Tools**: Android Studio, Gradle

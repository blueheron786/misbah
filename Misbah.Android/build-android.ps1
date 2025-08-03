# Build the Blazor WebAssembly app in Release mode
Write-Host "Building Misbah.Android in Release mode..." -ForegroundColor Green
dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Clean and copy files using Capacitor sync
Write-Host "Syncing files with Capacitor..." -ForegroundColor Green
npx cap sync android
if ($LASTEXITCODE -ne 0) {
    Write-Host "Capacitor sync failed!" -ForegroundColor Red
    exit 1
}

# Build Android APK
Write-Host "Building Android APK..." -ForegroundColor Green
cd android
./gradlew assembleDebug
if ($LASTEXITCODE -ne 0) {
    Write-Host "Android build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "APK location: android/app/build/outputs/apk/debug/app-debug.apk" -ForegroundColor Yellow

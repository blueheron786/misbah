@echo off
echo Building Misbah.Android in Release mode...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo Build failed!
    exit /b 1
)

echo Syncing files with Capacitor...
npx cap sync android
if %errorlevel% neq 0 (
    echo Capacitor sync failed!
    exit /b 1
)

echo Building Android APK...
cd android
call gradlew.bat assembleDebug
if %errorlevel% neq 0 (
    echo Android build failed!
    exit /b 1
)

echo Build completed successfully!
echo APK location: android\app\build\outputs\apk\debug\app-debug.apk

@echo off
echo Building Misbah Android App...
echo.

echo [1/4] Cleaning previous build...
if exist "android\app\src\main\assets\public" rmdir /s /q "android\app\src\main\assets\public"
if exist "dist" rmdir /s /q "dist"

echo [2/4] Building Blazor WebAssembly app...
cd Misbah.Web
dotnet publish -c Release -o ..\dist
if %errorlevel% neq 0 (
    echo Error: Failed to build Blazor app
    pause
    exit /b 1
)

echo.
echo [3/4] Syncing with Capacitor...
cd ..
npx capacitor sync android
if %errorlevel% neq 0 (
    echo Error: Failed to sync Capacitor
    pause
    exit /b 1
)

echo.
echo [4/4] Opening Android Studio...
npx capacitor open android

echo.
echo Build complete! Android Studio should be opening...
echo You can now build and run the app in Android Studio.
pause

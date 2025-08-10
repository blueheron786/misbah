# Build Misbah Android App
Write-Host "Building Misbah Android App..." -ForegroundColor Green
Write-Host ""

Write-Host "[1/4] Cleaning previous build..." -ForegroundColor Yellow
Remove-Item -Path "android\app\src\main\assets\public" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "dist" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "[2/4] Building Blazor WebAssembly app..." -ForegroundColor Yellow

# Build Blazor app from Misbah.BlazorDesktop
Set-Location "Misbah.BlazorDesktop"
dotnet publish -c Release -o ..\dist
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to build Blazor app" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Copy published wwwroot to android assets/public for Capacitor
Set-Location ".."
$publicAssets = "android/app/src/main/assets/public"
if (Test-Path $publicAssets) { Remove-Item -Path $publicAssets -Recurse -Force -ErrorAction SilentlyContinue }
New-Item -ItemType Directory -Path $publicAssets -Force | Out-Null
Copy-Item -Path "dist/wwwroot/*" -Destination $publicAssets -Recurse -Force

Write-Host ""
Write-Host "[3/4] Syncing with Capacitor..." -ForegroundColor Yellow
Set-Location ".."
npx capacitor sync android
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to sync Capacitor" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "[4/4] Opening Android Studio..." -ForegroundColor Yellow
npx capacitor open android

Write-Host ""
Write-Host "Build complete! Android Studio should be opening..." -ForegroundColor Green
Write-Host "You can now build and run the app in Android Studio." -ForegroundColor Green
Read-Host "Press Enter to exit"

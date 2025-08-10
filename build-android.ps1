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
# Copy all files and folders, including those at the root of wwwroot
Get-ChildItem -Path "dist/wwwroot" | ForEach-Object {
    $dest = Join-Path $publicAssets $_.Name
    if ($_.PSIsContainer) {
        Copy-Item $_.FullName -Destination $dest -Recurse -Force
    } else {
        Copy-Item $_.FullName -Destination $publicAssets -Force
    }
}


Write-Host ""
Write-Host "[3/4] Copying assets with Capacitor..." -ForegroundColor Yellow
Set-Location "."
npx cap copy android
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to copy assets with Capacitor" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "[4/5] Syncing with Capacitor..." -ForegroundColor Yellow
npx cap sync android
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

# Top-level build script for Misbah
Write-Host ">>> Cleaning all output..." -ForegroundColor Yellow
Remove-Item -Path "Misbah.BlazorDesktop/dist" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Misbah.BlazorDesktop/bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Misbah.BlazorDesktop/obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Misbah.Android/bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Misbah.Android/obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Misbah.Core/bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Misbah.Core/obj" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ">>> Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to restore NuGet packages" -ForegroundColor Red
    exit 1
}

Write-Host ">>> Building all projects..." -ForegroundColor Yellow
dotnet build Misbah.sln -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to build solution" -ForegroundColor Red
    exit 1
}

Write-Host ">>> Building Android app..." -ForegroundColor Yellow
./build-android.ps1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Android build failed" -ForegroundColor Red
    exit 1
}

Write-Host ">>> Build complete!" -ForegroundColor Green

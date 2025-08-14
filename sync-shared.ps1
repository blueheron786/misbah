# Sync shared files to both Web and BlazorDesktop projects
# Run this script whenever you modify shared files

Write-Host "Syncing shared files..." -ForegroundColor Green

# Copy CSS files
Copy-Item "Shared\wwwroot\css\toast.css" "Misbah.Web\wwwroot\css\" -Force
Copy-Item "Shared\wwwroot\css\toast.css" "Misbah.BlazorDesktop\wwwroot\css\" -Force

# Copy JS files  
Copy-Item "Shared\wwwroot\js\api.js" "Misbah.Web\wwwroot\js\" -Force
Copy-Item "Shared\wwwroot\js\api.js" "Misbah.BlazorDesktop\wwwroot\js\" -Force

Copy-Item "Shared\wwwroot\js\app.js" "Misbah.Web\wwwroot\js\" -Force
Copy-Item "Shared\wwwroot\js\app.js" "Misbah.BlazorDesktop\wwwroot\js\" -Force

Write-Host "✅ Shared files synced to both projects" -ForegroundColor Green
Write-Host "   - toast.css" -ForegroundColor Yellow
Write-Host "   - api.js" -ForegroundColor Yellow  
Write-Host "   - app.js" -ForegroundColor Yellow

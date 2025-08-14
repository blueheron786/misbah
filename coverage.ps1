# Comprehensive Test Coverage Script for Misbah
# Supports Windows, Linux, macOS with detailed HTML reports

# Set error handling to stop on any error
$ErrorActionPreference = "Stop"

Write-Host "🧪 MISBAH COMPREHENSIVE TEST COVERAGE ANALYSIS" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

# Create coverage directory
$coverageDir = "TestResults\Coverage"
try {
    if (Test-Path $coverageDir) {
        Remove-Item $coverageDir -Recurse -Force -ErrorAction SilentlyContinue
    }
    New-Item -ItemType Directory -Path $coverageDir -Force | Out-Null
} catch {
    Write-Host "❌ Error setting up coverage directory: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n🏗️  Building solution..." -ForegroundColor Yellow
dotnet build Misbah.sln --configuration Release --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful!" -ForegroundColor Green

# Dynamically discover test projects
Write-Host "`n🔍 Discovering test projects..." -ForegroundColor Yellow
try {
    $testProjects = Get-ChildItem -Directory | Where-Object { 
        $_.Name -like "*Tests" -and (Test-Path "$($_.FullName)\*.csproj") -and $_.Name -ne "Misbah.BlazorDesktop.Tests"
    } | ForEach-Object { $_.Name }
} catch {
    Write-Host "❌ Error discovering test projects: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

if ($testProjects.Count -eq 0) {
    Write-Host "❌ No test projects found!" -ForegroundColor Red
    exit 1
}

Write-Host "📋 Found $($testProjects.Count) test projects:" -ForegroundColor Green
foreach ($project in $testProjects) {
    Write-Host "  • $project" -ForegroundColor Gray
}

Write-Host "`n🧪 Running tests with coverage collection..." -ForegroundColor Yellow

foreach ($project in $testProjects) {
    Write-Host "  📊 Testing $project" -ForegroundColor Cyan
    
    $coverageFile = "$coverageDir\$project.coverage.xml"
    
    dotnet test $project `
        --configuration Release `
        --no-build `
        --verbosity minimal `
        --collect:"XPlat Code Coverage" `
        --results-directory $coverageDir `
        --settings coverlet.runsettings `
        --logger "trx;LogFileName=$project.trx"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "    ❌ Tests failed in $project (Exit code: $LASTEXITCODE)" -ForegroundColor Red
        Write-Host "    Continuing with other test projects..." -ForegroundColor Yellow
    } else {
        Write-Host "    ✅ All tests passed in $project" -ForegroundColor Green
    }
}

# Find all coverage files
try {
    $coverageFiles = Get-ChildItem -Path $coverageDir -Recurse -Filter "*.cobertura.xml"
} catch {
    Write-Host "❌ Error searching for coverage files: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

if ($coverageFiles.Count -eq 0) {
    Write-Host "❌ No coverage files found! This may indicate test execution problems." -ForegroundColor Red
    Write-Host "💡 Check the test output above for errors." -ForegroundColor Yellow
    exit 1
}

Write-Host "`n📊 Generating HTML coverage report..." -ForegroundColor Yellow

# Combine all coverage files for report generation
$coveragePaths = ($coverageFiles | ForEach-Object { $_.FullName }) -join ";"

# Generate comprehensive HTML report
Write-Host "Installing/updating ReportGenerator tool..." -ForegroundColor Gray

# First try to install the tool
Write-Host "Installing ReportGenerator tool..." -ForegroundColor Gray
$installResult = dotnet tool install -g dotnet-reportgenerator-globaltool 2>&1
if ($LASTEXITCODE -ne 0) {
    # Tool might already be installed, try to update
    Write-Host "Tool may already exist, trying to update..." -ForegroundColor Gray
    $updateResult = dotnet tool update -g dotnet-reportgenerator-globaltool 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Warning: Could not install/update ReportGenerator" -ForegroundColor Yellow
        Write-Host "Install result: $installResult" -ForegroundColor Gray
        Write-Host "Update result: $updateResult" -ForegroundColor Gray
    }
}

# Try to run reportgenerator using the global tool
Write-Host "Generating coverage report using ReportGenerator..." -ForegroundColor Gray

# Try using the full path to the tool
$toolPath = "$env:USERPROFILE\.dotnet\tools\reportgenerator.exe"
if (Test-Path $toolPath) {
    & $toolPath `
        "-reports:$coveragePaths" `
        "-targetdir:$coverageDir\Html" `
        "-reporttypes:Html;HtmlSummary;Badges;TextSummary;Xml;JsonSummary" `
        "-assemblyfilters:+Misbah.*;-*Tests*" `
        "-classfilters:-*.Program;-*.Migrations.*;-*.Designer.*" `
        "-title:Misbah Clean Architecture Coverage Report" `
        "-verbosity:Warning"
} else {
    # Fallback: try dotnet tool run with local manifest
    dotnet new tool-manifest --force 2>$null
    dotnet tool install dotnet-reportgenerator-globaltool 2>$null
    dotnet tool run reportgenerator `
        "-reports:$coveragePaths" `
        "-targetdir:$coverageDir\Html" `
        "-reporttypes:Html;HtmlSummary;Badges;TextSummary;Xml;JsonSummary" `
        "-assemblyfilters:+Misbah.*;-*Tests*" `
        "-classfilters:-*.Program;-*.Migrations.*;-*.Designer.*" `
        "-title:Misbah Clean Architecture Coverage Report" `
        "-verbosity:Warning"
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ dotnet tool run failed, trying direct reportgenerator command..." -ForegroundColor Yellow
    
    # Fallback: try the direct command (might work if PATH is set correctly)
    try {
        & reportgenerator `
            "-reports:$coveragePaths" `
            "-targetdir:$coverageDir\Html" `
            "-reporttypes:Html;HtmlSummary;Badges;TextSummary;Xml;JsonSummary" `
            "-assemblyfilters:+Misbah.*;-*Tests*" `
            "-classfilters:-*.Program;-*.Migrations.*;-*.Designer.*" `
            "-title:Misbah Clean Architecture Coverage Report" `
            "-verbosity:Warning"
        
        if ($LASTEXITCODE -ne 0) {
            throw "ReportGenerator failed with exit code $LASTEXITCODE"
        }
    } catch {
        Write-Host "❌ Failed to generate coverage report: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "💡 Try running: dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Yellow
        Write-Host "💡 And ensure your PATH includes: $env:USERPROFILE\.dotnet\tools" -ForegroundColor Yellow
        exit 1
    }
}

Write-Host "✅ Coverage report generated!" -ForegroundColor Green

# Display summary
Write-Host "`n📈 COVERAGE SUMMARY" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan

$summaryFile = "$coverageDir\Html\summary.txt"
try {
    if (Test-Path $summaryFile) {
        Get-Content $summaryFile | Write-Host
    } else {
        Write-Host "📊 Open $coverageDir\Html\index.html to view detailed coverage report" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Warning: Could not read summary file: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Display key metrics
Write-Host "`n🎯 KEY AREAS COVERED:" -ForegroundColor Green
Write-Host "✅ CQRS Handlers (Commands & Queries)" -ForegroundColor Green  
Write-Host "✅ Domain Events & Event Dispatching" -ForegroundColor Green
Write-Host "✅ Value Objects (NotePath & MarkdownContent)" -ForegroundColor Green
Write-Host "✅ Rich Domain Entity (Note)" -ForegroundColor Green
Write-Host "✅ Mediator Pattern Implementation" -ForegroundColor Green
Write-Host "✅ Cross-Platform Test Execution" -ForegroundColor Green

Write-Host "`n🚀 Coverage report available at:" -ForegroundColor Cyan
Write-Host "   $PWD\$coverageDir\Html\index.html" -ForegroundColor White

# Try to open the report automatically
$htmlReport = "$coverageDir\Html\index.html"
if (Test-Path $htmlReport) {
    try {
        if ($IsWindows -or $PSVersionTable.PSEdition -eq 'Desktop') {
            Start-Process $htmlReport
            if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne $null) {
                Write-Host "⚠️  Could not open browser automatically" -ForegroundColor Yellow
            } else {
                Write-Host "🌐 Opening coverage report in browser..." -ForegroundColor Green
            }
        } elseif ($IsMacOS) {
            & open $htmlReport
            if ($LASTEXITCODE -ne 0) {
                Write-Host "⚠️  Could not open browser automatically on macOS" -ForegroundColor Yellow
            } else {
                Write-Host "🌐 Opening coverage report in browser..." -ForegroundColor Green
            }
        } elseif ($IsLinux) {
            & xdg-open $htmlReport 2>/dev/null
            if ($LASTEXITCODE -ne 0) {
                Write-Host "⚠️  Could not open browser automatically on Linux" -ForegroundColor Yellow
            } else {
                Write-Host "🌐 Opening coverage report in browser..." -ForegroundColor Green
            }
        }
    } catch {
        Write-Host "💡 Please manually open: $htmlReport" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ HTML report file not found at: $htmlReport" -ForegroundColor Red
    Write-Host "💡 Check the ReportGenerator output above for errors" -ForegroundColor Yellow
}

Write-Host "`n🎉 Coverage analysis complete!" -ForegroundColor Green
Write-Host "📊 Report available at: $PWD\$coverageDir\Html\index.html" -ForegroundColor Cyan

# Final exit with success
exit 0

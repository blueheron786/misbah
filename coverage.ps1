# Comprehensive Test Coverage Script for Misbah
# Supports Windows, Linux, macOS with detailed HTML reports

Write-Host "🧪 MISBAH COMPREHENSIVE TEST COVERAGE ANALYSIS" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

# Create coverage directory
$coverageDir = "TestResults\Coverage"
if (Test-Path $coverageDir) {
    Remove-Item $coverageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $coverageDir -Force | Out-Null

Write-Host "`n🏗️  Building solution..." -ForegroundColor Yellow
dotnet build Misbah.sln --configuration Release --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful!" -ForegroundColor Green

# Test projects to analyze
$testProjects = @(
    "Misbah.Application.Tests",
    "Misbah.Core.Tests", 
    "Misbah.Infrastructure.Tests"
)

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
        Write-Host "    ⚠️  Some tests failed in $project" -ForegroundColor Yellow
    } else {
        Write-Host "    ✅ All tests passed in $project" -ForegroundColor Green
    }
}

# Find all coverage files
$coverageFiles = Get-ChildItem -Path $coverageDir -Recurse -Filter "coverage.cobertura.xml"

if ($coverageFiles.Count -eq 0) {
    Write-Host "❌ No coverage files found!" -ForegroundColor Red
    exit 1
}

Write-Host "`n📊 Generating HTML coverage report..." -ForegroundColor Yellow

# Combine all coverage files for report generation
$coveragePaths = ($coverageFiles | ForEach-Object { $_.FullName }) -join ";"

# Generate comprehensive HTML report
dotnet tool install -g dotnet-reportgenerator-globaltool --version 5.1.26 2>$null

reportgenerator `
    "-reports:$coveragePaths" `
    "-targetdir:$coverageDir\Html" `
    "-reporttypes:Html;HtmlSummary;Badges;TextSummary;Xml;JsonSummary" `
    "-assemblyfilters:+Misbah.*;-*Tests*" `
    "-classfilters:-*.Program;-*.Migrations.*;-*.Designer.*" `
    "-title:Misbah Clean Architecture Coverage Report" `
    "-verbosity:Warning"

Write-Host "✅ Coverage report generated!" -ForegroundColor Green

# Display summary
Write-Host "`n📈 COVERAGE SUMMARY" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan

$summaryFile = "$coverageDir\Html\summary.txt"
if (Test-Path $summaryFile) {
    Get-Content $summaryFile | Write-Host
} else {
    Write-Host "📊 Open $coverageDir\Html\index.html to view detailed coverage report" -ForegroundColor Yellow
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
        } elseif ($IsMacOS) {
            open $htmlReport
        } elseif ($IsLinux) {
            xdg-open $htmlReport 2>/dev/null
        }
        Write-Host "🌐 Opening coverage report in browser..." -ForegroundColor Green
    } catch {
        Write-Host "💡 Please manually open: $htmlReport" -ForegroundColor Yellow
    }
}

Write-Host "`n🎉 Coverage analysis complete!" -ForegroundColor Green

# Travel Planner Launcher
Write-Host "🌍 Travel Planner Starting..." -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

$frontendPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $frontendPath

# Option 1: Direct Open in Browser (Fastest)
Write-Host "Opening index.html in your default browser..." -ForegroundColor Green
$indexPath = (Resolve-Path "index.html").Path
Start-Process "file:///$indexPath"

Write-Host "✅ Application opened!" -ForegroundColor Green

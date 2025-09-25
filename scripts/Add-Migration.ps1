param(
  [Parameter(Mandatory = $true)][string]$Name,
  [string]$Context = "AppDbContext"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$dal  = Join-Path $root "src\EducationPortal.DataAccess\EducationPortal.DataAccess.csproj"
$ui   = Join-Path $root "src\EducationPortal.Presentation\EducationPortal.Presentation.csproj"

Write-Host "Restoring local tools..." -ForegroundColor Cyan
dotnet tool restore | Out-Null

Write-Host "Adding migration '$Name' for context '$Context'..." -ForegroundColor Cyan
dotnet ef migrations add $Name -c $Context -p $dal -s $ui
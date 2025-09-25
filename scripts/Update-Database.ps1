param(
  [string]$Context = "AppDbContext",
  [string]$Migration,          
  [switch]$Verbose
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$dal  = Join-Path $root "src\EducationPortal.DataAccess\EducationPortal.DataAccess.csproj"
$ui   = Join-Path $root "src\EducationPortal.Presentation\EducationPortal.Presentation.csproj"

dotnet tool restore | Out-Null

$cmd = @("ef","database","update","-p",$dal,"-s",$ui,"-c",$Context)
if ($Migration) { $cmd += $Migration }
if ($Verbose)   { $cmd += "--verbose" }

Write-Host "Updating database..." -ForegroundColor Cyan
dotnet @cmd
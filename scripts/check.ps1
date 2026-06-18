#!/usr/bin/env pwsh
# One-command feedback sensor: build + tests + coverage + lint + typecheck.
# Console stays quiet (summary only); full output goes to .logs/. Exit != 0 on any failure.
#
#   pwsh scripts/check.ps1            # backend + frontend
#   pwsh scripts/check.ps1 -Backend  # backend only
#   pwsh scripts/check.ps1 -Frontend # frontend only
[CmdletBinding()]
param(
  [switch]$Backend,
  [switch]$Frontend,
  [int]$Coverage = 85
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$logs = Join-Path $root '.logs'
New-Item -ItemType Directory -Force -Path $logs | Out-Null
if (-not $Backend -and -not $Frontend) { $Backend = $true; $Frontend = $true }

$failed = @()
function Step([string]$name, [string]$log, [scriptblock]$cmd) {
  Write-Host -NoNewline ("  - {0,-32}" -f $name)
  try { & $cmd *>> $log; $ok = ($LASTEXITCODE -eq 0 -or $null -eq $LASTEXITCODE) }
  catch { $ok = $false; $_ | Out-File -Append $log }
  if ($ok) { Write-Host 'PASS' -ForegroundColor Green }
  else { Write-Host 'FAIL' -ForegroundColor Red; $script:failed += $name; Write-Host "      -> see $log" }
}

if ($Backend) {
  $sln = Join-Path $root 'backend/TwitterClone.slnx'
  if (-not (Test-Path $sln)) { $sln = Join-Path $root 'backend/TwitterClone.sln' }
  Write-Host "`nBackend (.NET 10)" -ForegroundColor Cyan
  if (-not (Test-Path $sln)) {
    Write-Host '  - skipped: backend not scaffolded yet (backend/TwitterClone.sln missing)' -ForegroundColor DarkYellow
  } else {
    Step 'build (warnings as errors)' "$logs/backend-build.log" { dotnet build $sln -warnaserror --nologo }
    Step 'format (lint)'              "$logs/backend-format.log" { dotnet format $sln --verify-no-changes }
    Step 'tests (all passing)'        "$logs/backend-test.log"   { dotnet test $sln --nologo }
    # Coverage gate — skipped on Windows when Smart App Control blocks DLL instrumentation.
    # On Linux/CI this is the enforcement gate; run `pwsh scripts/coverage.ps1` for local details.
    $runningOnLinux = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux) -or
                      [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)
    if ($runningOnLinux) {
      Step "coverage >= $Coverage%"   "$logs/backend-coverage.log" {
        dotnet test $sln --nologo /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura `
          /p:Threshold=$Coverage /p:ThresholdType=line /p:ThresholdStat=total
      }
    } else {
      Write-Host -NoNewline ("  - {0,-32}" -f "coverage >= $Coverage%")
      Write-Host 'SKIP (Windows SAC — run on Linux/CI to enforce)' -ForegroundColor DarkYellow
    }
  }
}

if ($Frontend) {
  $pkg = Join-Path $root 'frontend/package.json'
  Write-Host "`nFrontend (Vue 3)" -ForegroundColor Cyan
  if (-not (Test-Path $pkg)) {
    Write-Host '  - skipped: frontend not scaffolded yet (frontend/package.json missing)' -ForegroundColor DarkYellow
  } else {
    Push-Location (Join-Path $root 'frontend')
    try {
      if (-not (Test-Path 'node_modules')) { Step 'npm install' "$logs/frontend-install.log" { npm install } }
      Step 'lint'        "$logs/frontend-lint.log"   { npm run lint }
      Step 'type-check'  "$logs/frontend-tsc.log"    { npm run type-check }
      Step 'unit tests'  "$logs/frontend-test.log"   { npm run test:unit }
    } finally { Pop-Location }
  }
}

Write-Host ''
if ($failed.Count -gt 0) {
  Write-Host ("CHECK FAILED ({0}): {1}" -f $failed.Count, ($failed -join ', ')) -ForegroundColor Red
  exit 1
}
Write-Host 'CHECK PASSED' -ForegroundColor Green
exit 0

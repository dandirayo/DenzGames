param(
    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$artifactRoot = Join-Path $repoRoot 'artifacts'
$publishRoot = Join-Path $artifactRoot "launcher-$Runtime"
$zipPath = Join-Path $artifactRoot "DenzGames-Launcher-$Runtime.zip"

New-Item -ItemType Directory -Path $artifactRoot -Force | Out-Null
dotnet publish (Join-Path $repoRoot 'launcher\DenzGames.Launcher\DenzGames.Launcher.csproj') `
    --configuration Release `
    --runtime $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    --output $publishRoot

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath
}
Compress-Archive -Path (Join-Path $publishRoot '*') -DestinationPath $zipPath
$hash = Get-FileHash -LiteralPath $zipPath -Algorithm SHA256
Set-Content -LiteralPath "$zipPath.sha256" -Value ($hash.Hash.ToLowerInvariant() + '  ' + (Split-Path -Leaf $zipPath))
Write-Host "Created $zipPath"
Write-Host "SHA256 $($hash.Hash.ToLowerInvariant())"



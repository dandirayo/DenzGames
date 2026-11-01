$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$manifestPath = Join-Path $repoRoot 'manifest\games.json'
$games = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json

foreach ($game in $games) {
    $sourceRoot = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $game.sourcePath))
    if (-not (Test-Path -LiteralPath $sourceRoot -PathType Container)) {
        throw "Source folder missing for $($game.name): $sourceRoot"
    }

    if ($game.executable) {
        $executable = Join-Path $sourceRoot $game.executable
        if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) {
            throw "Executable missing for $($game.name): $executable"
        }

        $header = [System.IO.File]::ReadAllBytes($executable)[0..1]
        if ($header[0] -ne 0x4D -or $header[1] -ne 0x5A) {
            throw "Executable is not a Windows PE file for $($game.name): $executable"
        }
    }
    elseif ($game.archive -and $game.archiveExecutable) {
        $archive = Join-Path $sourceRoot $game.archive
        if (-not (Test-Path -LiteralPath $archive -PathType Leaf)) {
            throw "Archive missing for $($game.name): $archive"
        }

        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [System.IO.Compression.ZipFile]::OpenRead($archive)
        try {
            $entry = $zip.Entries | Where-Object FullName -EQ $game.archiveExecutable | Select-Object -First 1
            if (-not $entry) {
                throw "Archive executable missing for $($game.name): $($game.archiveExecutable)"
            }
        }
        finally {
            $zip.Dispose()
        }
    }
    else {
        throw "No local executable or archive configured for $($game.name)."
    }

    Write-Host "OK  $($game.name)"
}

dotnet build (Join-Path $repoRoot 'DenzGames.sln') --configuration Release --nologo
if ($LASTEXITCODE -ne 0) {
    throw 'Launcher build failed.'
}

git -C $repoRoot lfs fsck
if ($LASTEXITCODE -ne 0) {
    throw 'Git LFS integrity check failed.'
}

Write-Host "Verified $($games.Count) games, launcher build, and Git LFS objects."

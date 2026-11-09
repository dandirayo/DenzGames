@echo off
setlocal
cd /d "%~dp0"

:: 1. Cek rilis mandiri di folder artifacts (paling cepat & stabil)
if exist "%~dp0artifacts\launcher-win-x64\DenzGames.Launcher.exe" (
    start "" /d "%~dp0" "%~dp0artifacts\launcher-win-x64\DenzGames.Launcher.exe"
    exit /b 0
)

:: 2. Cek build Release di folder bin
if exist "%~dp0launcher\DenzGames.Launcher\bin\Release\net8.0-windows\win-x64\DenzGames.Launcher.exe" (
    start "" /d "%~dp0" "%~dp0launcher\DenzGames.Launcher\bin\Release\net8.0-windows\win-x64\DenzGames.Launcher.exe"
    exit /b 0
)

:: 3. Cek build Debug di folder bin
if exist "%~dp0launcher\DenzGames.Launcher\bin\Debug\net8.0-windows\DenzGames.Launcher.exe" (
    start "" /d "%~dp0" "%~dp0launcher\DenzGames.Launcher\bin\Debug\net8.0-windows\DenzGames.Launcher.exe"
    exit /b 0
)

:: 4. Fallback jika executable belum di-build
echo Menyiapkan dan menjalankan DenzGames Launcher...
dotnet run --project "%~dp0launcher\DenzGames.Launcher\DenzGames.Launcher.csproj"
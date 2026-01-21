@echo off
setlocal enabledelayedexpansion

echo.
echo ========================================
echo  Ludusavi Playnite Extension Builder
echo ========================================
echo.

REM Clean previous builds
echo [1/5] Cleaning previous builds...
if exist "dist\LudusaviPlaynite.pext" del /F /Q "dist\LudusaviPlaynite.pext" >nul 2>&1
if exist "dist\LudusaviPlaynite.zip" del /F /Q "dist\LudusaviPlaynite.zip" >nul 2>&1
echo     Done.

REM Build the project
echo.
echo [2/5] Building project (Release configuration)...
dotnet build src\LudusaviPlaynite.csproj -c Release >build.log 2>&1

REM Check if build was successful - look for success message and no errors
findstr /C:"Compilazione completat" build.log >nul
if %errorlevel% neq 0 (
    echo.
    echo ========================================
    echo  ERROR: Build failed!
    echo ========================================
    echo.
    type build.log
    pause
    exit /b 1
)

REM Check if there are actual errors (not just warnings)
findstr /C:"error CS" build.log >nul
if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo  ERROR: Build failed with errors!
    echo ========================================
    echo.
    type build.log
    pause
    exit /b 1
)

echo     Done.

REM Create dist directory if it doesn't exist
echo.
echo [3/5] Preparing output directory...
if not exist "dist" mkdir "dist"
echo     Done.

REM Create the package
echo.
echo [4/5] Creating package...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$files = @(" ^
    "'src\bin\Release\net462\ByteSize.dll'," ^
    "'src\bin\Release\net462\extension.yaml'," ^
    "'src\bin\Release\net462\icon.png'," ^
    "'src\bin\Release\net462\IndexRange.dll'," ^
    "'src\bin\Release\net462\Linguini.Bundle.dll'," ^
    "'src\bin\Release\net462\Linguini.Shared.dll'," ^
    "'src\bin\Release\net462\Linguini.Syntax.dll'," ^
    "'src\bin\Release\net462\LudusaviPlaynite.dll'," ^
    "'src\bin\Release\net462\Newtonsoft.Json.dll'," ^
    "'src\bin\Release\net462\System.ValueTuple.dll'," ^
    "'src\bin\Release\net462\YamlDotNet.dll'" ^
    ");" ^
    "Compress-Archive -Path $files -DestinationPath 'dist\LudusaviPlaynite.zip' -Force;" ^
    "Rename-Item -Path 'dist\LudusaviPlaynite.zip' -NewName 'LudusaviPlaynite.pext' -Force"

if %errorlevel% neq 0 (
    echo.
    echo ========================================
    echo  ERROR: Failed to create package!
    echo ========================================
    pause
    exit /b 1
)
echo     Done.

REM Get package info
echo.
echo [5/5] Getting package information...

REM Read version from extension.yaml
for /f "tokens=2 delims=: " %%a in ('findstr /C:"Version:" extension.yaml') do set VERSION=%%a

REM Get file size
for %%A in ("dist\LudusaviPlaynite.pext") do set SIZE=%%~zA
set /a SIZE_KB=!SIZE!/1024

echo     Done.

REM Clean up log file
del /F /Q build.log >nul 2>&1

REM Success message
echo.
echo ========================================
echo  SUCCESS! Package created!
echo ========================================
echo.
echo   File: dist\LudusaviPlaynite.pext
echo   Version: %VERSION%
echo   Size: !SIZE_KB! KB
echo.
echo   You can now install this .pext file in Playnite
echo   by double-clicking it or dragging it into Playnite.
echo.
echo ========================================
echo.

REM Ask if user wants to open the dist folder
set /p OPEN="Open dist folder? (Y/N): "
if /i "%OPEN%"=="Y" (
    explorer dist
)

endlocal

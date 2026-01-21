@echo off
REM Quick build script for Ludusavi Playnite Extension
echo Building...

REM Clean
if exist "dist\LudusaviPlaynite.pext" del /F /Q "dist\LudusaviPlaynite.pext"
if exist "dist\LudusaviPlaynite.zip" del /F /Q "dist\LudusaviPlaynite.zip"

REM Build
dotnet build src\LudusaviPlaynite.csproj -c Release --nologo -v quiet
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

REM Create dist directory
if not exist "dist" mkdir "dist"

REM Package
powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path @('src\bin\Release\net462\ByteSize.dll','src\bin\Release\net462\extension.yaml','src\bin\Release\net462\icon.png','src\bin\Release\net462\IndexRange.dll','src\bin\Release\net462\Linguini.Bundle.dll','src\bin\Release\net462\Linguini.Shared.dll','src\bin\Release\net462\Linguini.Syntax.dll','src\bin\Release\net462\LudusaviPlaynite.dll','src\bin\Release\net462\Newtonsoft.Json.dll','src\bin\Release\net462\System.ValueTuple.dll','src\bin\Release\net462\YamlDotNet.dll') -DestinationPath 'dist\LudusaviPlaynite.zip' -Force; Rename-Item -Path 'dist\LudusaviPlaynite.zip' -NewName 'LudusaviPlaynite.pext' -Force"

echo Done! Package: dist\LudusaviPlaynite.pext
pause

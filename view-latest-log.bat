@echo off
REM Mostra il contenuto del log più recente di LudusaviPlaynite

set LOG_PATH=%AppData%\Playnite\ExtensionsData\72e2de43-d859-44d8-914e-4277741c8208\logs

if exist "%LOG_PATH%" (
    echo ========================================
    echo LOG DI LUDUSAVI PLAYNITE
    echo ========================================
    echo.
    
    REM Trova il file più recente
    for /f "delims=" %%a in ('dir /b /od "%LOG_PATH%\LudusaviPlaynite_*.log" 2^>nul') do set "LATEST=%%a"
    
    if defined LATEST (
        echo File: %LATEST%
        echo Percorso: %LOG_PATH%\%LATEST%
        echo.
        echo ========================================
        echo CONTENUTO:
        echo ========================================
        type "%LOG_PATH%\%LATEST%"
    ) else (
        echo Nessun file di log trovato in: %LOG_PATH%
    )
) else (
    echo ERRORE: La cartella log non esiste ancora.
    echo Avvia Playnite almeno una volta per creare la cartella.
    echo Percorso atteso: %LOG_PATH%
)

echo.
echo ========================================
pause

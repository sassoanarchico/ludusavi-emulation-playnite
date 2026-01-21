@echo off
REM Apre la cartella dei log di LudusaviPlaynite in Esplora File

set LOG_PATH=%AppData%\Playnite\ExtensionsData\72e2de43-d859-44d8-914e-4277741c8208\logs

if exist "%LOG_PATH%" (
    echo Apertura cartella log: %LOG_PATH%
    start "" explorer "%LOG_PATH%"
) else (
    echo ERRORE: La cartella log non esiste ancora.
    echo Avvia Playnite almeno una volta per creare la cartella.
    echo Percorso atteso: %LOG_PATH%
    pause
)

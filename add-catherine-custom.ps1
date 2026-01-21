# Script per aggiungere Catherine PS3 come Custom Game in Ludusavi
# Questo risolve il problema dell'exit code 1 per i giochi RPCS3

$ErrorActionPreference = "Stop"

$configPath = "$env:APPDATA\ludusavi\config.yaml"
$catherineSavePath = "C:\Users\paffe\OneDrive\Documenten\rpcs3-v0.0.38-18540-f946054a_win64_msvc\dev_hdd0\home\00000001\savedata\BLES01459-00-FIXED-"
$gameName = "Catherine (Sony PlayStation 3)"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AGGIUNGI CATHERINE PS3 A LUDUSAVI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verifica che il percorso dei salvataggi esista
Write-Host "1. Verifica salvataggi Catherine..." -NoNewline
if (-not (Test-Path $catherineSavePath)) {
    Write-Host " ERRORE!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Salvataggi non trovati:" -ForegroundColor Red
    Write-Host "  $catherineSavePath" -ForegroundColor Yellow
    Read-Host "Premi INVIO per uscire"
    exit 1
}
$files = Get-ChildItem $catherineSavePath
Write-Host " OK! ($($files.Count) file trovati)" -ForegroundColor Green

# Mostra i file trovati
Write-Host ""
Write-Host "   File di salvataggio:" -ForegroundColor Gray
foreach ($file in $files) {
    $size = "{0:N0} KB" -f ($file.Length / 1KB)
    Write-Host "   - $($file.Name) ($size)" -ForegroundColor White
}
Write-Host ""

# Verifica che il file config di Ludusavi esista
Write-Host "2. Verifica configurazione Ludusavi..." -NoNewline
if (-not (Test-Path $configPath)) {
    Write-Host " ERRORE!" -ForegroundColor Red
    Write-Host ""
    Write-Host "File config non trovato:" -ForegroundColor Red
    Write-Host "  $configPath" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Assicurati di aver avviato Ludusavi almeno una volta." -ForegroundColor Yellow
    Read-Host "Premi INVIO per uscire"
    exit 1
}
Write-Host " OK!" -ForegroundColor Green

# Backup del file originale
Write-Host "3. Creazione backup config..." -NoNewline
$backupPath = "$configPath.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
try {
    Copy-Item $configPath $backupPath -ErrorAction Stop
    Write-Host " OK!" -ForegroundColor Green
    Write-Host "   Backup: $(Split-Path $backupPath -Leaf)" -ForegroundColor Gray
} catch {
    Write-Host " ERRORE!" -ForegroundColor Red
    Write-Host "  $_" -ForegroundColor Red
    Read-Host "Premi INVIO per uscire"
    exit 1
}

# Leggi il file config
Write-Host "4. Lettura configurazione..." -NoNewline
try {
    $config = Get-Content $configPath -Raw -ErrorAction Stop
    Write-Host " OK!" -ForegroundColor Green
} catch {
    Write-Host " ERRORE!" -ForegroundColor Red
    Write-Host "  $_" -ForegroundColor Red
    Read-Host "Premi INVIO per uscire"
    exit 1
}

# Controlla se Catherine è già configurata
Write-Host "5. Verifica se già configurato..." -NoNewline
if ($config -like "*$gameName*" -or $config -like "*BLES01459*") {
    Write-Host " GIÀ PRESENTE!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host "Catherine PS3 è già configurato!" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Se ancora non funziona, verifica:" -ForegroundColor Cyan
    Write-Host "  1. Il nome in Playnite corrisponde esattamente a:" -ForegroundColor White
    Write-Host "     '$gameName'" -ForegroundColor Green
    Write-Host "  2. Riavvia Playnite dopo le modifiche" -ForegroundColor White
    Write-Host "  3. Prova un backup manuale da Ludusavi GUI" -ForegroundColor White
    Write-Host ""
    Read-Host "Premi INVIO per uscire"
    exit 0
}
Write-Host " NON PRESENTE" -ForegroundColor Yellow

# Prepara l'entry YAML per Catherine
# Nota: Il percorso deve usare doppie backslash per YAML
$yamlSavePath = $catherineSavePath -replace '\\', '\\\\'

$customGameEntry = @"

  - name: "$gameName"
    integration: override
    alias: "Catherine"
    files:
      - "$yamlSavePath"
    registry: []
    installDir: []
    winePrefix: []
"@

# Trova la sezione customGames e aggiungi l'entry
Write-Host "6. Aggiunta Catherine alla configurazione..." -NoNewline

# Cerca la sezione customGames
if ($config -match "customGames:") {
    # Aggiungi dopo la prima voce di customGames
    # Trova la fine della prima voce vuota o l'inizio della sezione
    
    # Metodo: aggiungi dopo "customGames:\n"
    $newConfig = $config -replace "(customGames:)", "`$1$customGameEntry"
    
    try {
        Set-Content -Path $configPath -Value $newConfig -NoNewline -ErrorAction Stop
        Write-Host " OK!" -ForegroundColor Green
    } catch {
        Write-Host " ERRORE!" -ForegroundColor Red
        Write-Host "  $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "Ripristino backup..." -ForegroundColor Yellow
        Copy-Item $backupPath $configPath -Force
        Read-Host "Premi INVIO per uscire"
        exit 1
    }
} else {
    Write-Host " ERRORE!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Sezione 'customGames' non trovata nel file config!" -ForegroundColor Red
    Write-Host "Devi aggiungerla manualmente." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Apri: $configPath" -ForegroundColor Cyan
    Write-Host "E aggiungi alla fine:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "customGames:" -ForegroundColor White
    Write-Host $customGameEntry -ForegroundColor White
    Write-Host ""
    Read-Host "Premi INVIO per aprire il file in Notepad"
    notepad $configPath
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  CONFIGURAZIONE COMPLETATA!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Catherine PS3 è stato aggiunto come custom game." -ForegroundColor Green
Write-Host ""
Write-Host "Dettagli:" -ForegroundColor Cyan
Write-Host "  Nome: $gameName" -ForegroundColor White
Write-Host "  Path: $catherineSavePath" -ForegroundColor White
Write-Host "  File: GAME.DAT, ICON0.PNG, PARAM.SFO" -ForegroundColor White
Write-Host ""
Write-Host "PROSSIMI PASSI:" -ForegroundColor Cyan
Write-Host "  1. Chiudi Ludusavi se aperto" -ForegroundColor White
Write-Host "  2. Riavvia Playnite" -ForegroundColor White
Write-Host "  3. Seleziona Catherine in Playnite" -ForegroundColor White
Write-Host "  4. Extensions -> Ludusavi -> Back Up Last Game" -ForegroundColor White
Write-Host "  5. Controlla il log con: .\monitor-log.ps1" -ForegroundColor White
Write-Host ""
Write-Host "Il log ora dovrebbe mostrare:" -ForegroundColor Cyan
Write-Host '  DEBUG| Ludusavi exited with 0 (Success: data found/saved)' -ForegroundColor Green
Write-Host ""

# Mostra un'anteprima della configurazione aggiunta
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Configurazione aggiunta:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host $customGameEntry -ForegroundColor White
Write-Host ""

Read-Host "Premi INVIO per uscire"

# Script per aggiungere automaticamente il percorso RPCS3 a Ludusavi
# Configurazione automatica RPCS3 OneDrive per Ludusavi

$ErrorActionPreference = "Stop"

$configPath = "$env:APPDATA\ludusavi\config.yaml"
$rpcs3Path = "C:\Users\paffe\OneDrive\Documenten\rpcs3-v0.0.38-18540-f946054a_win64_msvc\dev_hdd0\home"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CONFIGURAZIONE RPCS3 PER LUDUSAVI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verifica che il percorso RPCS3 esista
Write-Host "Verifica percorso RPCS3..." -NoNewline
if (-not (Test-Path $rpcs3Path)) {
    Write-Host " ERRORE!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Il percorso RPCS3 non esiste:" -ForegroundColor Red
    Write-Host "  $rpcs3Path" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Verifica che:" -ForegroundColor Yellow
    Write-Host "  1. RPCS3 sia installato" -ForegroundColor White
    Write-Host "  2. Il percorso nel script sia corretto" -ForegroundColor White
    Write-Host ""
    Read-Host "Premi INVIO per uscire"
    exit 1
}
Write-Host " OK!" -ForegroundColor Green

# Verifica che esistano salvataggi
Write-Host "Verifica salvataggi..." -NoNewline
$savedataPath = Join-Path $rpcs3Path "00000001\savedata"
if (-not (Test-Path $savedataPath)) {
    Write-Host " ATTENZIONE!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "La cartella savedata non esiste o è vuota:" -ForegroundColor Yellow
    Write-Host "  $savedataPath" -ForegroundColor White
    Write-Host ""
    Write-Host "Continuo comunque..." -ForegroundColor Yellow
    Write-Host ""
} else {
    $saveCount = (Get-ChildItem $savedataPath -Directory).Count
    Write-Host " OK! ($saveCount giochi trovati)" -ForegroundColor Green
}

# Verifica che il file config di Ludusavi esista
Write-Host "Verifica configurazione Ludusavi..." -NoNewline
if (-not (Test-Path $configPath)) {
    Write-Host " ERRORE!" -ForegroundColor Red
    Write-Host ""
    Write-Host "File di configurazione Ludusavi non trovato:" -ForegroundColor Red
    Write-Host "  $configPath" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Assicurati di:" -ForegroundColor Yellow
    Write-Host "  1. Aver installato Ludusavi" -ForegroundColor White
    Write-Host "  2. Averlo avviato almeno una volta" -ForegroundColor White
    Write-Host ""
    Read-Host "Premi INVIO per uscire"
    exit 1
}
Write-Host " OK!" -ForegroundColor Green

# Backup del file originale
Write-Host "Creazione backup configurazione..." -NoNewline
$backupPath = "$configPath.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
try {
    Copy-Item $configPath $backupPath -ErrorAction Stop
    Write-Host " OK!" -ForegroundColor Green
    Write-Host "  Backup: $(Split-Path $backupPath -Leaf)" -ForegroundColor Gray
} catch {
    Write-Host " ERRORE!" -ForegroundColor Red
    Write-Host "  $_" -ForegroundColor Red
    Read-Host "Premi INVIO per uscire"
    exit 1
}

# Leggi il file config
Write-Host "Lettura configurazione..." -NoNewline
try {
    $config = Get-Content $configPath -Raw -ErrorAction Stop
    Write-Host " OK!" -ForegroundColor Green
} catch {
    Write-Host " ERRORE!" -ForegroundColor Red
    Write-Host "  $_" -ForegroundColor Red
    Read-Host "Premi INVIO per uscire"
    exit 1
}

# Controlla se il percorso è già presente
Write-Host "Verifica se già configurato..." -NoNewline
if ($config -like "*$rpcs3Path*") {
    Write-Host " SÌ!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host "Il percorso RPCS3 è già configurato!" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Non è necessaria alcuna modifica." -ForegroundColor Green
    Write-Host ""
    Read-Host "Premi INVIO per uscire"
    exit 0
}
Write-Host " NO" -ForegroundColor Yellow

Write-Host ""
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "  MODIFICA RICHIESTA" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "Il percorso RPCS3 deve essere aggiunto manualmente." -ForegroundColor Yellow
Write-Host ""
Write-Host "Apri il file di configurazione:" -ForegroundColor Cyan
Write-Host "  $configPath" -ForegroundColor White
Write-Host ""
Write-Host "Trova la sezione 'roots:' e aggiungi:" -ForegroundColor Cyan
Write-Host ""
Write-Host "  roots:" -ForegroundColor White
Write-Host "    - path: $rpcs3Path" -ForegroundColor Green
Write-Host "      store: other" -ForegroundColor Green
Write-Host ""
Write-Host "ATTENZIONE: Mantieni l'indentazione YAML corretta!" -ForegroundColor Yellow
Write-Host ""
Write-Host "Vuoi aprire il file ora in Notepad? (S/N): " -ForegroundColor Cyan -NoNewline
$risposta = Read-Host

if ($risposta -eq "S" -or $risposta -eq "s" -or $risposta -eq "") {
    Write-Host ""
    Write-Host "Apertura in Notepad..." -ForegroundColor Cyan
    Start-Process notepad $configPath
    Write-Host ""
    Write-Host "Dopo aver modificato il file:" -ForegroundColor Cyan
    Write-Host "  1. Salva e chiudi Notepad" -ForegroundColor White
    Write-Host "  2. Riavvia Playnite" -ForegroundColor White
    Write-Host "  3. Prova a backuppare Catherine" -ForegroundColor White
    Write-Host "  4. Controlla il log con: .\monitor-log.ps1" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "OK, modifica il file manualmente quando vuoi." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Percorso completo da aggiungere:" -ForegroundColor Cyan
Write-Host $rpcs3Path -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Mostra i giochi trovati
if (Test-Path $savedataPath) {
    Write-Host "Giochi PS3 trovati in RPCS3:" -ForegroundColor Cyan
    $games = Get-ChildItem $savedataPath -Directory | Sort-Object Name
    $count = 0
    foreach ($game in $games) {
        $count++
        Write-Host "  $count. $($game.Name)" -ForegroundColor White
        if ($count -ge 10) {
            Write-Host "  ... e altri $($games.Count - $count) giochi" -ForegroundColor Gray
            break
        }
    }
    Write-Host ""
    Write-Host "Totale: $($games.Count) giochi" -ForegroundColor Green
    Write-Host ""
}

Read-Host "Premi INVIO per uscire"

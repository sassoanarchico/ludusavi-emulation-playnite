# Script per pulire le voci corrotte di Catherine dalla configurazione Ludusavi

$configPath = "$env:APPDATA\ludusavi\config.yaml"

Write-Host "=== PULIZIA CONFIGURAZIONE LUDUSAVI ===" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $configPath)) {
    Write-Host "File config non trovato!" -ForegroundColor Red
    exit 1
}

# Backup
$backupPath = "$configPath.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Copy-Item $configPath $backupPath
Write-Host "Backup creato: $(Split-Path $backupPath -Leaf)" -ForegroundColor Green

# Leggi il contenuto
$content = Get-Content $configPath -Raw

# Rimuovi le voci vuote e corrotte
# Rimuovi entries con name vuoto
$content = $content -replace '  - name: ""\s+integration: override\s+alias: ""\s+files: \[\]\s+registry: \[\]\s+installDir: \[\]\s+winePrefix: \[\]\s*', ''

# Rimuovi le doppie backslash in eccesso
$content = $content -replace '\\\\\\\\', '\\'
$content = $content -replace '\\\\\\\\', '\\'

# Salva
Set-Content -Path $configPath -Value $content -NoNewline

Write-Host "Configurazione pulita!" -ForegroundColor Green
Write-Host ""
Write-Host "Riavvia Playnite e riprova il backup di Catherine." -ForegroundColor Yellow

Read-Host "Premi INVIO per uscire"

# fix-catherine-config.ps1
# Aggiunge Catherine alla configurazione Ludusavi in modo sicuro

$configPath = "$env:APPDATA\ludusavi\config.yaml"
$backupPath = "$configPath.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# Backup
Copy-Item $configPath $backupPath
Write-Host "Backup: $backupPath" -ForegroundColor Green

# Leggi config
$lines = Get-Content $configPath

# Cerca la riga customGames e inserisci Catherine dopo
$newLines = @()
$found = $false

foreach ($line in $lines) {
    $newLines += $line
    
    if ($line -match "^customGames:" -and -not $found) {
        $found = $true
        # Aggiungi Catherine
        $newLines += '  - name: "Catherine (Sony PlayStation 3)"'
        $newLines += '    integration: override'
        $newLines += '    alias: ""'
        $newLines += '    files:'
        $newLines += '      - "C:\Users\paffe\OneDrive\Documenten\rpcs3-v0.0.38-18540-f946054a_win64_msvc\dev_hdd0\home\00000001\savedata\BLES01459-00-FIXED-"'
        $newLines += '    registry: []'
        $newLines += '    installDir: []'
        $newLines += '    winePrefix: []'
    }
}

# Rimuovi voci vuote
$content = $newLines -join "`n"
$content = $content -replace '\n  - name: ""\n    integration: override\n    alias: ""\n    files: \[\]\n    registry: \[\]\n    installDir: \[\]\n    winePrefix: \[\]', ''

# Salva
Set-Content $configPath -Value $content -NoNewline

Write-Host "Catherine aggiunta alla configurazione!" -ForegroundColor Green
Write-Host ""
Write-Host "Verifica:" -ForegroundColor Cyan
Get-Content $configPath | Select-String "Catherine" -Context 0,8

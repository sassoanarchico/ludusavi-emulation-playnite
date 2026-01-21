# fix-ludusavi-config-final.ps1
# Corregge la configurazione Ludusavi rimuovendo voci vuote e aggiungendo Catherine

$configPath = "$env:APPDATA\ludusavi\config.yaml"

Write-Host "=== FIX CONFIGURAZIONE LUDUSAVI ===" -ForegroundColor Cyan
Write-Host ""

# Backup
$backupPath = "$configPath.fix-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Copy-Item $configPath $backupPath
Write-Host "Backup creato: $(Split-Path $backupPath -Leaf)" -ForegroundColor Green

# Leggi il file riga per riga
$lines = Get-Content $configPath
$newLines = New-Object System.Collections.ArrayList
$skipUntilNextEntry = $false
$inCustomGames = $false
$addedCatherine = $false

for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    
    # Rileva sezione customGames
    if ($line -match "^customGames:") {
        $inCustomGames = $true
        [void]$newLines.Add($line)
        
        # Aggiungi Catherine subito dopo
        [void]$newLines.Add('  - name: "Catherine (Sony PlayStation 3)"')
        [void]$newLines.Add('    integration: override')
        [void]$newLines.Add('    alias: ""')
        [void]$newLines.Add('    files:')
        [void]$newLines.Add('      - "C:/Users/paffe/OneDrive/Documenten/rpcs3-v0.0.38-18540-f946054a_win64_msvc/dev_hdd0/home/00000001/savedata/BLES01459-00-FIXED-"')
        [void]$newLines.Add('    registry: []')
        [void]$newLines.Add('    installDir: []')
        [void]$newLines.Add('    winePrefix: []')
        $addedCatherine = $true
        continue
    }
    
    # Salta voci con name vuoto
    if ($inCustomGames -and $line -match '^\s+- name: ""') {
        # Salta questa voce e le prossime 7 righe
        $skipCount = 0
        while ($skipCount -lt 7 -and $i -lt $lines.Count - 1) {
            $i++
            $skipCount++
        }
        Write-Host "Rimossa voce vuota" -ForegroundColor Yellow
        continue
    }
    
    # Salta voci Catherine duplicate
    if ($addedCatherine -and $line -match 'Catherine \(Sony PlayStation 3\)') {
        # Salta questa voce e le prossime 8 righe
        $skipCount = 0
        while ($skipCount -lt 8 -and $i -lt $lines.Count - 1) {
            $nextLine = $lines[$i + 1]
            if ($nextLine -match '^\s+- name:' -or $nextLine -notmatch '^\s') {
                break
            }
            $i++
            $skipCount++
        }
        Write-Host "Saltata voce Catherine duplicata" -ForegroundColor Yellow
        continue
    }
    
    [void]$newLines.Add($line)
}

# Scrivi il nuovo file
$newLines -join "`n" | Set-Content $configPath -NoNewline

Write-Host ""
Write-Host "Configurazione corretta!" -ForegroundColor Green
Write-Host ""

# Verifica
Write-Host "=== VERIFICA ===" -ForegroundColor Cyan
$check = Get-Content $configPath | Select-String "Catherine" -Context 0,8
$check

# Test Ludusavi
Write-Host ""
Write-Host "=== TEST LUDUSAVI ===" -ForegroundColor Cyan
$ludusaviPath = "C:\Users\paffe\OneDrive\Documenten\ludusavi-v0.30.0-win64\ludusavi.exe"
if (Test-Path $ludusaviPath) {
    & $ludusaviPath backup --preview -- "Catherine (Sony PlayStation 3)" 2>&1
}

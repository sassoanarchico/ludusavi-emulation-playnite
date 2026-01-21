# Monitor in tempo reale del log di LudusaviPlaynite
# Mostra le nuove righe man mano che vengono aggiunte (come 'tail -f' in Linux)

$logPath = "$env:AppData\Playnite\ExtensionsData\72e2de43-d859-44d8-914e-4277741c8208\logs"
$today = Get-Date -Format "yyyy-MM-dd"
$logFile = Join-Path $logPath "LudusaviPlaynite_$today.log"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "MONITOR LOG LUDUSAVI PLAYNITE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (Test-Path $logFile) {
    Write-Host "Monitoraggio: $logFile" -ForegroundColor Green
    Write-Host "Premi CTRL+C per interrompere" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    
    # Mostra le ultime 20 righe
    Get-Content $logFile -Tail 20
    
    Write-Host ""
    Write-Host "--- NUOVE VOCI ---" -ForegroundColor Yellow
    Write-Host ""
    
    # Monitora nuove righe
    Get-Content $logFile -Wait -Tail 0 | ForEach-Object {
        $line = $_
        
        # Colora in base al livello
        if ($line -match "\|ERROR\|") {
            Write-Host $line -ForegroundColor Red
        }
        elseif ($line -match "\|WARN\|") {
            Write-Host $line -ForegroundColor Yellow
        }
        elseif ($line -match "\|INFO\|") {
            Write-Host $line -ForegroundColor Green
        }
        elseif ($line -match "\|DEBUG\|") {
            Write-Host $line -ForegroundColor Gray
        }
        elseif ($line -match "\|TRACE\|") {
            Write-Host $line -ForegroundColor DarkGray
        }
        else {
            Write-Host $line
        }
    }
}
else {
    Write-Host "ERRORE: File di log non trovato" -ForegroundColor Red
    Write-Host "Percorso atteso: $logFile" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Assicurati che:" -ForegroundColor Yellow
    Write-Host "1. Playnite sia stato avviato almeno una volta oggi" -ForegroundColor Yellow
    Write-Host "2. L'estensione LudusaviPlaynite sia installata" -ForegroundColor Yellow
    Write-Host ""
    
    # Mostra i file disponibili
    if (Test-Path $logPath) {
        Write-Host "File di log disponibili:" -ForegroundColor Cyan
        Get-ChildItem $logPath -Filter "LudusaviPlaynite_*.log" | ForEach-Object {
            Write-Host "  - $($_.Name)" -ForegroundColor White
        }
    }
}

Write-Host ""
Read-Host "Premi INVIO per uscire"

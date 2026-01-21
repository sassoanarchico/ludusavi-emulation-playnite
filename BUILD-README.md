# ??? Build Scripts per Ludusavi Playnite Extension

## ?? Script Disponibili

### 1?? **build-pext.bat** (Consigliato)
Script completo con output dettagliato, controllo errori e informazioni sul pacchetto.

**Caratteristiche:**
- ? Pulizia automatica dei file precedenti
- ? Build con log dettagliato
- ? Controllo errori di compilazione
- ? Creazione pacchetto .pext
- ? Informazioni su versione e dimensione
- ? Opzione per aprire la cartella dist

**Come usare:**
```bash
build-pext.bat
```

---

### 2?? **build-quick.bat**
Script veloce senza fronzoli, ideale per build rapide durante lo sviluppo.

**Caratteristiche:**
- ? Build veloce
- ?? Output minimale
- ? Controllo errori base

**Come usare:**
```bash
build-quick.bat
```

---

## ?? Risultato

Entrambi gli script creano il file:
```
dist\LudusaviPlaynite.pext
```

---

## ?? Come Installare

1. Esegui uno degli script `.bat`
2. Attendi la compilazione
3. Trova il file `.pext` nella cartella `dist\`
4. **Fai doppio click** sul file `.pext` per installarlo in Playnite
   - Oppure trascinalo nella finestra di Playnite
   - Oppure da Playnite: Menu ? Componenti aggiuntivi ? Installa...

---

## ?? Requisiti

- **Windows** (qualsiasi versione recente)
- **.NET SDK** installato (per `dotnet` command)
- **PowerShell** (già presente in Windows)

---

## ?? Risoluzione Problemi

### "dotnet non è riconosciuto..."
Installa .NET SDK da: https://dotnet.microsoft.com/download

### "Build failed!"
1. Apri il file `build.log` per vedere l'errore
2. Verifica che tutti i file sorgente siano presenti
3. Prova a pulire manualmente: `dotnet clean src\LudusaviPlaynite.csproj`

### Il pacchetto non si installa in Playnite
1. Verifica che il file `.pext` esista in `dist\`
2. Controlla che Playnite non sia in esecuzione
3. Prova a riavviare Playnite

---

## ?? Versioni

Per cambiare la versione, modifica il file `extension.yaml`:
```yaml
Version: 0.19.1
```

Poi ri-esegui lo script di build.

---

## ?? Workflow Consigliato

Durante lo sviluppo:
```bash
# Build rapida per testare
build-quick.bat
```

Per il rilascio finale:
```bash
# Build completa con verifiche
build-pext.bat
```

---

**Buon sviluppo! ??**

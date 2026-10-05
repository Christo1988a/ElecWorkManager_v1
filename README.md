# ElecWork Manager

Gestionale operativo per commesse elettriche, pensato per organizzare cantieri, squadre e scadenze normative in un'unica applicazione desktop.

Il progetto nasce dalla mia esperienza diretta come caposquadra in ambito elettrico/fotovoltaico: l'obiettivo era digitalizzare processi che gestivo manualmente (assegnazione squadre, verifica scadenze di certificazioni e DPI, monitoraggio interventi) in un'applicazione realistica, costruita da zero.

## Funzionalità

- **Cantieri**: anagrafica commesse con stato (pianificato, in lavorazione, sospeso, completato), cliente, indirizzo e squadra assegnata.
- **Squadre & operatori**: composizione squadre, caposquadra, livello di esperienza di ciascun operatore.
- **Interventi**: pianificazione con priorità e stato, rilevamento automatico degli interventi in ritardo rispetto alla data programmata.
- **Certificazioni & DPI**: scadenzario con calcolo automatico dello stato (regolare / in scadenza / scaduta) su una soglia di preavviso configurabile.
- **Mezzi e materiali**: censimento mezzi/attrezzature, gestione magazzino con movimenti e allarme sotto-scorta.
- **Dashboard**: KPI operativi in tempo reale (cantieri attivi, squadre disponibili, interventi del giorno, interventi in ritardo, scadenze critiche, materiali sotto scorta).
- **Dati demo**: al primo avvio, se il database è vuoto, l'app popola cantieri, squadre, interventi e scadenze di esempio per una prova immediata.

## Architettura

Applicazione desktop WinForms in C# / .NET 8, organizzata in livelli separati:

```
ElecWorkManager.Domain          → entità ed enum di dominio (Cantiere, Squadra, Intervento, Certificazione, ...)
ElecWorkManager.Application     → logica di business (calcolo scadenze, KPI dashboard, contratti dei servizi)
ElecWorkManager.Infrastructure  → persistenza dati
ElecWorkManager.WinForms        → interfaccia utente
```

Persistenza su **SQLite**, con identificazione dei record tramite ID interno (non tramite nome/descrizione), per garantire operazioni di modifica ed eliminazione affidabili anche in presenza di record omonimi.

## Avvio del progetto

1. Aprire `ElecWorkManager.sln` con Visual Studio 2022 (o successivo).
2. Impostare `ElecWorkManager.WinForms` come progetto di avvio.
3. Eseguire (target: .NET 8 / WinForms, Windows).

## Possibili sviluppi futuri

- Reportistica esportabile (PDF/Excel) su interventi e scadenze.
- Autenticazione multi-utente con ruoli differenziati (caposquadra / amministrazione).
- Notifiche automatiche in prossimità delle scadenze.

## Autore

Marco Cristofari — in transizione da un percorso professionale in ambito elettrico/fotovoltaico allo sviluppo software.  
- **LinkedIn**: [Marco Cristofari](https://www.linkedin.com/in/marco-cristofari-15001b422/)

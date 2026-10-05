using System.Globalization;
using ElecWorkManager.Application.Services;
using ElecWorkManager.Domain.Entities;
using ElecWorkManager.Domain.Enums;
using ElecWorkManager.Infrastructure;
using ElecWorkManager.Infrastructure.Repositories;
using ElecWorkManager.Infrastructure.Sqlite;

if (args.Contains("--verify"))
{
    VerifyNucleo();
    return;
}

var service = new SqliteGestionaleService(new Database());
DemoDataSeeder.EnsureSeeded(service);
var dashboard = new DashboardService();
var scadenze = new ScadenzeService();

while (true)
{
    Console.Clear();
    var c = service.Cantieri();
    var s = service.Squadre();
    var i = service.Interventi();
    var d = service.Certificazioni();
    var k = dashboard.CreaKpi(c, s, i, d);
    Console.WriteLine("╭──────────────────────────────────────────────────────╮");
    Console.WriteLine("│                 ELECWORK MANAGER                     │");
    Console.WriteLine("│                 Gestionale v5                        │");
    Console.WriteLine("╰──────────────────────────────────────────────────────╯");
    Console.WriteLine($"  Cantieri attivi: {k.CantieriAttivi}   Interventi oggi: {k.InterventiOggi}   Ritardi: {k.InterventiInRitardo}   Scadenze: {k.ScadenzeCritiche}");
    Console.WriteLine("  [1] Cantieri  [2] Interventi  [3] Scadenze  [4] Squadre");
    Console.WriteLine("  [5] Nuovo cantiere  [6] Nuovo intervento  [0] Esci");
    Console.Write("  > ");
    switch (Console.ReadLine())
    {
        case "1":
            foreach (var x in c) Console.WriteLine($"{x.Id,3} | {x.Codice,-12} | {x.Nome,-30} | {x.Stato}");
            Console.ReadLine();
            break;
        case "2":
            foreach (var x in i) Console.WriteLine($"{x.Id,3} | {x.DataProgrammata:dd/MM/yyyy} | {x.StatoVisuale,-12} | {x.Priorita,-8} | {x.Descrizione}");
            Console.ReadLine();
            break;
        case "3":
            foreach (var x in scadenze.Critiche(d)) Console.WriteLine($"{x.Titolo,-35} | {x.DataScadenza:dd/MM/yyyy} | {x.GetStato()}");
            Console.ReadLine();
            break;
        case "4":
            foreach (var x in s) Console.WriteLine($"{x.Id,3} | {x.Nome,-20} | {x.Caposquadra,-20} | {x.Operatori.Count} operatori");
            Console.ReadLine();
            break;
        case "5":
            service.SalvaCantiere(new Cantiere { Codice = P("Codice"), Nome = P("Nome"), Cliente = P("Cliente"), Indirizzo = P("Indirizzo"), Stato = StatoCantiere.Pianificato });
            Console.WriteLine("Cantiere salvato.");
            Console.ReadLine();
            break;
        case "6":
            service.SalvaIntervento(new Intervento
            {
                CantiereId = int.Parse(P("ID cantiere"), CultureInfo.InvariantCulture),
                Descrizione = P("Descrizione"),
                DataProgrammata = DateTime.ParseExact(P("Data (dd/MM/yyyy)"), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                TecnicoResponsabile = P("Tecnico"),
                Stato = StatoIntervento.Programmato
            });
            Console.WriteLine("Intervento salvato.");
            Console.ReadLine();
            break;
        case "0":
            return;
    }
}

static string P(string label)
{
    Console.Write($"  {label}: ");
    return Console.ReadLine() ?? "";
}

static void VerifyNucleo()
{
    var path = Path.Combine(Path.GetTempPath(), $"elecwork-verify-{Guid.NewGuid():N}.db");
    try
    {
        var db = new Database(path);
        var svc = new SqliteGestionaleService(db);
        DemoDataSeeder.EnsureSeeded(svc);

        var cantieri = svc.Cantieri();
        var squadre = svc.Squadre();
        var interventi = svc.Interventi();
        var certs = svc.Certificazioni();
        if (cantieri.Count == 0) throw new Exception("Nessun cantiere seed.");
        if (interventi.Any(x => x.CantiereId == 0 || cantieri.All(c => c.Id != x.CantiereId)))
            throw new Exception("Interventi non collegati dopo insert.");
        if (certs.Any(x => x.CantiereId == 0 || cantieri.All(c => c.Id != x.CantiereId)))
            throw new Exception("Certificazioni non collegate dopo insert.");
        if (squadre.Any(s => s.Operatori.Count == 0))
            throw new Exception("Operatori non persistiti.");
        if (squadre.SelectMany(s => s.Operatori).Any(o => (int)o.Livello == 0 || o.Eta == 0))
            throw new Exception("Eta/Livello operaio non persistiti correttamente.");
        if (interventi.Any(x => (int)x.Stato == 3))
            throw new Exception("Stato InRitardo ancora persistito.");

        var mezzi = svc.Mezzi();
        var dpi = svc.DpiList();
        var materiali = svc.Materiali();
        if (mezzi.Count == 0 || dpi.Count == 0 || materiali.Count == 0)
            throw new Exception("Mezzi/DPI/Materiali seed mancanti.");
        var cavo = materiali.FirstOrDefault(m => m.Nome.Contains("Cavo", StringComparison.OrdinalIgnoreCase));
        if (cavo == null || cavo.Giacenza != 80)
            throw new Exception($"Giacenza materiale calcolata male (attesa 80, trovata {cavo?.Giacenza}).");

        var squadraConDpi = squadre.First(s => dpi.Any(d => d.SquadraId == s.Id));
        svc.EliminaSquadra(squadraConDpi.Id);
        if (svc.DpiList().Any(d => d.SquadraId == squadraConDpi.Id) || svc.Operatori().Any(o => o.SquadraId == squadraConDpi.Id))
            throw new Exception("FK CASCADE su Operatori/Dpi non attiva dopo eliminazione squadra.");

        var target = cantieri[0];
        var before = interventi.Count(x => x.CantiereId == target.Id);
        if (before == 0) throw new Exception("Cantiere seed senza interventi.");
        svc.EliminaCantiere(target.Id);
        if (svc.Interventi().Any(x => x.CantiereId == target.Id) || svc.Certificazioni().Any(x => x.CantiereId == target.Id))
            throw new Exception("FK CASCADE non attiva.");

        Console.WriteLine("OK nucleo: ID after insert, operatori, FK cascade, ritardo non persistito.");
    }
    finally
    {
        try { File.Delete(path); } catch { /* ignore */ }
    }
}

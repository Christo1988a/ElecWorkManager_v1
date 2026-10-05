using ElecWorkManager.Application.Services;
using ElecWorkManager.Domain.Entities;
using ElecWorkManager.Domain.Enums;

namespace ElecWorkManager.Infrastructure;

public static class DemoDataSeeder
{
    public static void EnsureSeeded(IGestionaleService service)
    {
        if (!service.Cantieri().Any())
            SeedAll(service);
        else
            RepairMissingChildren(service);
    }

    private static void SeedAll(IGestionaleService service)
    {
        var oggi = DateTime.Today;
        var squadra1 = NewSquadra("Squadra Alfa", "Luca Bianchi");
        var squadra2 = NewSquadra("Squadra Beta", "Davide Conti");
        service.SalvaSquadra(squadra1);
        service.SalvaSquadra(squadra2);

        SeedOperatoriAlfa(service, squadra1.Id);
        SeedOperatoriBeta(service, squadra2.Id);

        var c1 = NewCantiere("EW-2026-001", "Residenza Modena Nord", "EdilNova S.r.l.", "Modena (MO)", StatoCantiere.InLavorazione, squadra1.Id);
        var c2 = NewCantiere("EW-2026-002", "Capannone Industrial Park", "TecnoLogistica S.p.A.", "Carpi (MO)", StatoCantiere.InLavorazione, squadra2.Id);
        var c3 = NewCantiere("EW-2026-003", "Uffici Centro Direzionale", "NovaServizi S.p.A.", "Reggio Emilia (RE)", StatoCantiere.Pianificato, squadra1.Id);
        service.SalvaCantiere(c1);
        service.SalvaCantiere(c2);
        service.SalvaCantiere(c3);

        SeedInterventi(service, oggi, c1.Id, c2.Id, c3.Id);
        SeedCertificazioni(service, oggi, c1.Id, c2.Id, c3.Id);
        SeedMezziEDpi(service, oggi, squadra1.Id, squadra2.Id);
        SeedMagazzino(service, oggi, c1.Id, c2.Id);
    }

    private static void RepairMissingChildren(IGestionaleService service)
    {
        var oggi = DateTime.Today;
        var squadre = service.Squadre().ToList();
        var operatori = service.Operatori().ToList();
        foreach (var squadra in squadre.Where(s => operatori.All(o => o.SquadraId != s.Id)))
        {
            if (squadra.Nome.Contains("Alfa", StringComparison.OrdinalIgnoreCase))
                SeedOperatoriAlfa(service, squadra.Id);
            else if (squadra.Nome.Contains("Beta", StringComparison.OrdinalIgnoreCase))
                SeedOperatoriBeta(service, squadra.Id);
            else if (!string.IsNullOrWhiteSpace(squadra.Caposquadra))
                service.SalvaOperatore(new Operatore { SquadraId = squadra.Id, Nome = squadra.Caposquadra, Ruolo = "Caposquadra", Eta = 40, Livello = LivelloEsperienza.Esperto });
        }

        var cantieri = service.Cantieri().OrderBy(x => x.Id).ToList();
        if (cantieri.Count == 0) return;

        var c1 = cantieri.FirstOrDefault(x => x.Codice == "EW-2026-001") ?? cantieri[0];
        var c2 = cantieri.FirstOrDefault(x => x.Codice == "EW-2026-002") ?? cantieri.ElementAtOrDefault(1) ?? c1;
        var c3 = cantieri.FirstOrDefault(x => x.Codice == "EW-2026-003") ?? cantieri.ElementAtOrDefault(2) ?? c1;

        if (!service.Interventi().Any())
            SeedInterventi(service, oggi, c1.Id, c2.Id, c3.Id);
        if (!service.Certificazioni().Any())
            SeedCertificazioni(service, oggi, c1.Id, c2.Id, c3.Id);

        squadre = service.Squadre().ToList();
        var squadra1 = squadre.FirstOrDefault(s => s.Nome.Contains("Alfa", StringComparison.OrdinalIgnoreCase)) ?? squadre.ElementAtOrDefault(0);
        var squadra2 = squadre.FirstOrDefault(s => s.Nome.Contains("Beta", StringComparison.OrdinalIgnoreCase)) ?? squadre.ElementAtOrDefault(1) ?? squadra1;
        if (squadra1 != null && squadra2 != null && !service.Mezzi().Any() && !service.DpiList().Any())
            SeedMezziEDpi(service, oggi, squadra1.Id, squadra2.Id);
        if (!service.Materiali().Any())
            SeedMagazzino(service, oggi, c1.Id, c2.Id);
    }

    private static Squadra NewSquadra(string nome, string caposquadra) => new() { Nome = nome, Caposquadra = caposquadra };

    private static void SeedOperatoriAlfa(IGestionaleService service, int squadraId)
    {
        service.SalvaOperatore(new Operatore { SquadraId = squadraId, Nome = "Luca Bianchi", Ruolo = "Caposquadra", Eta = 42, Livello = LivelloEsperienza.Esperto });
        service.SalvaOperatore(new Operatore { SquadraId = squadraId, Nome = "Marco Rinaldi", Ruolo = "Elettricista", Eta = 29, Livello = LivelloEsperienza.Specializzato });
    }

    private static void SeedOperatoriBeta(IGestionaleService service, int squadraId)
    {
        service.SalvaOperatore(new Operatore { SquadraId = squadraId, Nome = "Davide Conti", Ruolo = "Caposquadra", Eta = 38, Livello = LivelloEsperienza.Esperto });
        service.SalvaOperatore(new Operatore { SquadraId = squadraId, Nome = "Andrea Neri", Ruolo = "Elettricista", Eta = 24, Livello = LivelloEsperienza.Qualificato });
    }

    private static Cantiere NewCantiere(string codice, string nome, string cliente, string indirizzo, StatoCantiere stato, int squadraId) =>
        new()
        {
            Codice = codice,
            Nome = nome,
            Cliente = cliente,
            Indirizzo = indirizzo,
            Stato = stato,
            SquadraId = squadraId
        };

    private static void SeedInterventi(IGestionaleService service, DateTime oggi, int c1, int c2, int c3)
    {
        service.SalvaIntervento(new Intervento
        {
            CantiereId = c1,
            Descrizione = "Posa quadri elettrici piano terra",
            DataProgrammata = oggi.AddDays(-1),
            Stato = StatoIntervento.Completato,
            Priorita = PrioritaIntervento.Alta,
            TecnicoResponsabile = "Luca Bianchi",
            DataCompletamento = oggi.AddDays(-1)
        });
        service.SalvaIntervento(new Intervento
        {
            CantiereId = c1,
            Descrizione = "Cablaggio illuminazione piano primo",
            DataProgrammata = oggi.AddDays(2),
            Stato = StatoIntervento.Programmato,
            Priorita = PrioritaIntervento.Media,
            TecnicoResponsabile = "Marco Rinaldi"
        });
        service.SalvaIntervento(new Intervento
        {
            CantiereId = c2,
            Descrizione = "Verifica linee e collaudo quadro generale",
            DataProgrammata = oggi.AddDays(-3),
            Stato = StatoIntervento.InCorso,
            Priorita = PrioritaIntervento.Critica,
            TecnicoResponsabile = "Davide Conti"
        });
        service.SalvaIntervento(new Intervento
        {
            CantiereId = c2,
            Descrizione = "Installazione prese industriali",
            DataProgrammata = oggi.AddDays(5),
            Stato = StatoIntervento.InCorso,
            Priorita = PrioritaIntervento.Alta,
            TecnicoResponsabile = "Andrea Neri"
        });
        service.SalvaIntervento(new Intervento
        {
            CantiereId = c3,
            Descrizione = "Sopralluogo tecnico e rilievo impianti",
            DataProgrammata = oggi.AddDays(7),
            Stato = StatoIntervento.Programmato,
            Priorita = PrioritaIntervento.Bassa,
            TecnicoResponsabile = "Luca Bianchi"
        });
    }

    private static void SeedCertificazioni(IGestionaleService service, DateTime oggi, int c1, int c2, int c3)
    {
        service.SalvaCertificazione(new Certificazione
        {
            CantiereId = c1,
            Titolo = "DURC impresa",
            Tipo = TipoCertificazione.DocumentazioneCantiere,
            Intestatario = "EdilNova S.r.l.",
            DataScadenza = oggi.AddDays(12)
        });
        service.SalvaCertificazione(new Certificazione
        {
            CantiereId = c2,
            Titolo = "Abilitazione PES/PAV",
            Tipo = TipoCertificazione.Abilitazione,
            Intestatario = "Davide Conti",
            DataScadenza = oggi.AddDays(45)
        });
        service.SalvaCertificazione(new Certificazione
        {
            CantiereId = c3,
            Titolo = "Formazione sicurezza",
            Tipo = TipoCertificazione.Formazione,
            Intestatario = "Luca Bianchi",
            DataScadenza = oggi.AddDays(-4)
        });
    }

    private static void SeedMezziEDpi(IGestionaleService service, DateTime oggi, int squadra1Id, int squadra2Id)
    {
        service.SalvaMezzo(new Mezzo { Nome = "Fiat Ducato - AB123CD", Tipo = TipoMezzo.Automezzo, SquadraId = squadra1Id });
        service.SalvaMezzo(new Mezzo { Nome = "Iveco Daily - EF456GH", Tipo = TipoMezzo.Automezzo, SquadraId = squadra2Id });
        service.SalvaMezzo(new Mezzo { Nome = "Trapano tassellatore Bosch", Tipo = TipoMezzo.Attrezzatura, SquadraId = squadra1Id });
        service.SalvaMezzo(new Mezzo { Nome = "Tester multifunzione impianti", Tipo = TipoMezzo.Attrezzatura, SquadraId = squadra2Id });

        service.SalvaDpi(new Dpi { Nome = "Casco protettivo", SquadraId = squadra1Id, DataScadenza = oggi.AddDays(20) });
        service.SalvaDpi(new Dpi { Nome = "Guanti isolanti Cat. 0", SquadraId = squadra1Id, DataScadenza = oggi.AddDays(-5) });
        service.SalvaDpi(new Dpi { Nome = "Imbracatura anticaduta", SquadraId = squadra2Id, DataScadenza = oggi.AddDays(90) });
    }

    private static void SeedMagazzino(IGestionaleService service, DateTime oggi, int c1, int c2)
    {
        var cavo = new Materiale { Nome = "Cavo FG16OR16 3x2.5mm²", UnitaMisura = "metri", ScortaMinima = 100 };
        var interruttori = new Materiale { Nome = "Interruttore magnetotermico 16A", UnitaMisura = "pz", ScortaMinima = 10 };
        var canaline = new Materiale { Nome = "Canalina PVC 25x25", UnitaMisura = "metri", ScortaMinima = 50 };
        service.SalvaMateriale(cavo);
        service.SalvaMateriale(interruttori);
        service.SalvaMateriale(canaline);

        service.RegistraMovimento(new MovimentoMateriale { MaterialeId = cavo.Id, Quantita = 300, Riferimento = "DDT 1042 - ElettroForniture S.r.l.", Data = oggi.AddDays(-10) });
        service.RegistraMovimento(new MovimentoMateriale { MaterialeId = cavo.Id, Quantita = -220, Riferimento = "Utilizzo cantiere EW-2026-001", CantiereId = c1, Data = oggi.AddDays(-1) });
        service.RegistraMovimento(new MovimentoMateriale { MaterialeId = interruttori.Id, Quantita = 15, Riferimento = "DDT 1042 - ElettroForniture S.r.l.", Data = oggi.AddDays(-10) });
        service.RegistraMovimento(new MovimentoMateriale { MaterialeId = interruttori.Id, Quantita = -8, Riferimento = "Utilizzo cantiere EW-2026-002", CantiereId = c2, Data = oggi.AddDays(-3) });
        service.RegistraMovimento(new MovimentoMateriale { MaterialeId = canaline.Id, Quantita = 40, Riferimento = "DDT 1077 - ElettroForniture S.r.l.", Data = oggi.AddDays(-2) });
    }
}

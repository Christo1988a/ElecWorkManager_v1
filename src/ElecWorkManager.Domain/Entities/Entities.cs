using ElecWorkManager.Domain.Enums;

namespace ElecWorkManager.Domain.Entities;

public class Operatore
{
    public int Id { get; set; }
    public int SquadraId { get; set; }
    public string Nome { get; set; } = "";
    public string Ruolo { get; set; } = "";
    public int Eta { get; set; }
    public LivelloEsperienza Livello { get; set; } = LivelloEsperienza.Apprendista;
}

public class Squadra
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Caposquadra { get; set; } = "";
    public List<Operatore> Operatori { get; } = [];
}

public class Cantiere
{
    public int Id { get; set; }
    public string Codice { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Cliente { get; set; } = "";
    public string Indirizzo { get; set; } = "";
    public StatoCantiere Stato { get; set; }
    public int? SquadraId { get; set; }
}

public class Intervento
{
    public int Id { get; set; }
    public int CantiereId { get; set; }
    public string Descrizione { get; set; } = "";
    public DateTime DataProgrammata { get; set; }
    public DateTime? DataCompletamento { get; set; }
    public StatoIntervento Stato { get; set; }
    public PrioritaIntervento Priorita { get; set; } = PrioritaIntervento.Media;
    public string TecnicoResponsabile { get; set; } = "";
    public Guid? SerieId { get; set; }

    public bool IsInRitardo => Stato != StatoIntervento.Completato && DataProgrammata.Date < DateTime.Today;
    public string StatoVisuale => IsInRitardo ? "In ritardo" : Stato.ToString();
}

public class Mezzo
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public TipoMezzo Tipo { get; set; }
    public int? SquadraId { get; set; }
}

public class Dpi
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public int SquadraId { get; set; }
    public DateTime DataScadenza { get; set; }

    public StatoScadenza GetStato(int giorniAnticipo = 30)
    {
        if (DataScadenza.Date < DateTime.Today) return StatoScadenza.Scaduta;
        if (DataScadenza.Date <= DateTime.Today.AddDays(giorniAnticipo)) return StatoScadenza.InScadenza;
        return StatoScadenza.Regolare;
    }

    public int GiorniAllaScadenza => (DataScadenza.Date - DateTime.Today).Days;
}

public class Materiale
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string UnitaMisura { get; set; } = "";
    public int ScortaMinima { get; set; }
    public int Giacenza { get; set; }
    public bool SottoScorta => Giacenza < ScortaMinima;
}

public class MovimentoMateriale
{
    public int Id { get; set; }
    public int MaterialeId { get; set; }
    public DateTime Data { get; set; } = DateTime.Today;
    public int Quantita { get; set; }
    public string Riferimento { get; set; } = "";
    public int? CantiereId { get; set; }
}

public class Certificazione
{
    public int Id { get; set; }
    public int CantiereId { get; set; }
    public string Titolo { get; set; } = "";
    public TipoCertificazione Tipo { get; set; }
    public string Intestatario { get; set; } = "";
    public DateTime DataScadenza { get; set; }

    public StatoScadenza GetStato(int giorniAnticipo = 30)
    {
        if (DataScadenza.Date < DateTime.Today) return StatoScadenza.Scaduta;
        if (DataScadenza.Date <= DateTime.Today.AddDays(giorniAnticipo)) return StatoScadenza.InScadenza;
        return StatoScadenza.Regolare;
    }

    public int GiorniAllaScadenza => (DataScadenza.Date - DateTime.Today).Days;
}

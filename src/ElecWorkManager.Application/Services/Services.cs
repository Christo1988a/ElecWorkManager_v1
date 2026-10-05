using ElecWorkManager.Domain.Entities;
using ElecWorkManager.Domain.Enums;

namespace ElecWorkManager.Application.Services;

public interface IGestionaleService
{
    IReadOnlyList<Cantiere> Cantieri();
    IReadOnlyList<Squadra> Squadre();
    IReadOnlyList<Operatore> Operatori();
    IReadOnlyList<Intervento> Interventi();
    IReadOnlyList<Certificazione> Certificazioni();
    IReadOnlyList<Mezzo> Mezzi();
    IReadOnlyList<Dpi> DpiList();
    IReadOnlyList<Materiale> Materiali();
    IReadOnlyList<MovimentoMateriale> Movimenti();
    void SalvaCantiere(Cantiere cantiere);
    void SalvaIntervento(Intervento intervento);
    void SalvaSquadra(Squadra squadra);
    void SalvaOperatore(Operatore operatore);
    void SalvaCertificazione(Certificazione certificazione);
    void SalvaMezzo(Mezzo mezzo);
    void SalvaDpi(Dpi dpi);
    void SalvaMateriale(Materiale materiale);
    void RegistraMovimento(MovimentoMateriale movimento);
    void EliminaCantiere(int id);
    void EliminaIntervento(int id);
    void EliminaSquadra(int id);
    void EliminaOperatore(int id);
    void EliminaCertificazione(int id);
    void EliminaMezzo(int id);
    void EliminaDpi(int id);
    void EliminaMateriale(int id);
}

public class ScadenzeService
{
    public IReadOnlyList<Certificazione> Critiche(IEnumerable<Certificazione> items, int giorniAnticipo = 30) =>
        items.Where(x => x.GetStato(giorniAnticipo) != StatoScadenza.Regolare).OrderBy(x => x.DataScadenza).ToList();

    public IReadOnlyList<Dpi> Critiche(IEnumerable<Dpi> items, int giorniAnticipo = 30) =>
        items.Where(x => x.GetStato(giorniAnticipo) != StatoScadenza.Regolare).OrderBy(x => x.DataScadenza).ToList();
}

public record DashboardKpi(int CantieriAttivi, int SquadreTotali, int SquadreDisponibili, int InterventiOggi, int InterventiInRitardo, int ScadenzeCritiche, int MaterialiSottoScorta);

public class DashboardService
{
    public DashboardKpi CreaKpi(IEnumerable<Cantiere> cantieri, IEnumerable<Squadra> squadre, IEnumerable<Intervento> interventi, IEnumerable<Certificazione> certificazioni, IEnumerable<Dpi>? dpi = null, IEnumerable<Materiale>? materiali = null)
    {
        var c = cantieri.ToList();
        var s = squadre.ToList();
        var i = interventi.ToList();
        var scadenzeDpi = dpi?.Count(x => x.GetStato() != StatoScadenza.Regolare) ?? 0;
        var sottoScorta = materiali?.Count(x => x.SottoScorta) ?? 0;
        return new(
            c.Count(x => x.Stato == StatoCantiere.InLavorazione),
            s.Count,
            s.Count(x => !c.Any(y => y.SquadraId == x.Id && y.Stato == StatoCantiere.InLavorazione)),
            i.Count(x => x.DataProgrammata.Date == DateTime.Today),
            i.Count(x => x.IsInRitardo),
            certificazioni.Count(x => x.GetStato() != StatoScadenza.Regolare) + scadenzeDpi,
            sottoScorta);
    }
}

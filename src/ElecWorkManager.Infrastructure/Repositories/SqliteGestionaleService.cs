using System.Globalization;
using ElecWorkManager.Application.Services;
using ElecWorkManager.Domain.Entities;
using ElecWorkManager.Domain.Enums;
using ElecWorkManager.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;

namespace ElecWorkManager.Infrastructure.Repositories;

public class SqliteGestionaleService : IGestionaleService
{
    private readonly Database _database;

    public SqliteGestionaleService(Database database)
    {
        _database = database;
        _database.Initialize();
    }

    public IReadOnlyList<Cantiere> Cantieri()
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT Id,Codice,Nome,Cliente,Indirizzo,Stato,SquadraId FROM Cantieri ORDER BY Id DESC";
        using var r = cmd.ExecuteReader();
        var list = new List<Cantiere>();
        while (r.Read())
        {
            list.Add(new Cantiere
            {
                Id = r.GetInt32(0),
                Codice = r.GetString(1),
                Nome = r.GetString(2),
                Cliente = r.GetString(3),
                Indirizzo = r.GetString(4),
                Stato = (StatoCantiere)r.GetInt32(5),
                SquadraId = r.IsDBNull(6) ? null : r.GetInt32(6)
            });
        }
        return list;
    }

    public IReadOnlyList<Squadra> Squadre()
    {
        using var db = _database.Open();
        var list = new List<Squadra>();
        using (var cmd = db.CreateCommand())
        {
            cmd.CommandText = "SELECT Id,Nome,Caposquadra FROM Squadre ORDER BY Nome";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Squadra
                {
                    Id = r.GetInt32(0),
                    Nome = r.GetString(1),
                    Caposquadra = r.GetString(2)
                });
            }
        }

        using (var cmd = db.CreateCommand())
        {
            cmd.CommandText = "SELECT Id,SquadraId,Nome,Ruolo,Eta,Livello FROM Operatori ORDER BY Nome";
            using var r = cmd.ExecuteReader();
            var bySquadra = list.ToDictionary(x => x.Id);
            while (r.Read())
            {
                var op = new Operatore
                {
                    Id = r.GetInt32(0),
                    SquadraId = r.GetInt32(1),
                    Nome = r.GetString(2),
                    Ruolo = r.GetString(3),
                    Eta = r.GetInt32(4),
                    Livello = (LivelloEsperienza)r.GetInt32(5)
                };
                if (bySquadra.TryGetValue(op.SquadraId, out var squadra))
                    squadra.Operatori.Add(op);
            }
        }

        return list;
    }

    public IReadOnlyList<Operatore> Operatori()
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT Id,SquadraId,Nome,Ruolo,Eta,Livello FROM Operatori ORDER BY Nome";
        using var r = cmd.ExecuteReader();
        var list = new List<Operatore>();
        while (r.Read())
        {
            list.Add(new Operatore
            {
                Id = r.GetInt32(0),
                SquadraId = r.GetInt32(1),
                Nome = r.GetString(2),
                Ruolo = r.GetString(3),
                Eta = r.GetInt32(4),
                Livello = (LivelloEsperienza)r.GetInt32(5)
            });
        }
        return list;
    }

    public IReadOnlyList<Intervento> Interventi()
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT Id,CantiereId,Descrizione,DataProgrammata,DataCompletamento,Stato,Priorita,TecnicoResponsabile,SerieId FROM Interventi ORDER BY DataProgrammata";
        using var r = cmd.ExecuteReader();
        var list = new List<Intervento>();
        while (r.Read())
        {
            list.Add(new Intervento
            {
                Id = r.GetInt32(0),
                CantiereId = r.GetInt32(1),
                Descrizione = r.GetString(2),
                DataProgrammata = ParseDate(r.GetString(3)),
                DataCompletamento = r.IsDBNull(4) ? null : ParseDate(r.GetString(4)),
                Stato = ParseStatoIntervento(r.GetInt32(5)),
                Priorita = (PrioritaIntervento)r.GetInt32(6),
                TecnicoResponsabile = r.GetString(7),
                SerieId = r.IsDBNull(8) ? null : Guid.Parse(r.GetString(8))
            });
        }
        return list;
    }

    public IReadOnlyList<Certificazione> Certificazioni()
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT Id,CantiereId,Titolo,Tipo,Intestatario,DataScadenza FROM Certificazioni ORDER BY DataScadenza";
        using var r = cmd.ExecuteReader();
        var list = new List<Certificazione>();
        while (r.Read())
        {
            list.Add(new Certificazione
            {
                Id = r.GetInt32(0),
                CantiereId = r.GetInt32(1),
                Titolo = r.GetString(2),
                Tipo = ParseTipoCertificazione(r.GetValue(3)),
                Intestatario = r.GetString(4),
                DataScadenza = ParseDate(r.GetString(5))
            });
        }
        return list;
    }

    public void SalvaCantiere(Cantiere c)
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        if (c.Id == 0)
        {
            cmd.CommandText = """
                INSERT INTO Cantieri(Codice,Nome,Cliente,Indirizzo,Stato,SquadraId)
                VALUES($codice,$nome,$cliente,$indirizzo,$stato,$squadra)
                RETURNING Id
                """;
        }
        else
        {
            cmd.CommandText = """
                UPDATE Cantieri
                SET Codice=$codice,Nome=$nome,Cliente=$cliente,Indirizzo=$indirizzo,Stato=$stato,SquadraId=$squadra
                WHERE Id=$id
                """;
            cmd.Parameters.AddWithValue("$id", c.Id);
        }
        cmd.Parameters.AddWithValue("$codice", c.Codice);
        cmd.Parameters.AddWithValue("$nome", c.Nome);
        cmd.Parameters.AddWithValue("$cliente", c.Cliente);
        cmd.Parameters.AddWithValue("$indirizzo", c.Indirizzo);
        cmd.Parameters.AddWithValue("$stato", (int)c.Stato);
        cmd.Parameters.AddWithValue("$squadra", (object?)c.SquadraId ?? DBNull.Value);
        if (c.Id == 0)
            c.Id = Convert.ToInt32(cmd.ExecuteScalar());
        else
            cmd.ExecuteNonQuery();
    }

    public void SalvaIntervento(Intervento i)
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        if (i.Id == 0)
        {
            cmd.CommandText = """
                INSERT INTO Interventi(CantiereId,Descrizione,DataProgrammata,DataCompletamento,Stato,Priorita,TecnicoResponsabile,SerieId)
                VALUES($c,$d,$dp,$dc,$s,$p,$t,$serie)
                RETURNING Id
                """;
        }
        else
        {
            cmd.CommandText = """
                UPDATE Interventi
                SET CantiereId=$c,Descrizione=$d,DataProgrammata=$dp,DataCompletamento=$dc,Stato=$s,Priorita=$p,TecnicoResponsabile=$t,SerieId=$serie
                WHERE Id=$id
                """;
            cmd.Parameters.AddWithValue("$id", i.Id);
        }
        cmd.Parameters.AddWithValue("$c", i.CantiereId);
        cmd.Parameters.AddWithValue("$d", i.Descrizione);
        cmd.Parameters.AddWithValue("$dp", i.DataProgrammata.ToString("O", CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("$dc", (object?)i.DataCompletamento?.ToString("O", CultureInfo.InvariantCulture) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$s", (int)i.Stato);
        cmd.Parameters.AddWithValue("$p", (int)i.Priorita);
        cmd.Parameters.AddWithValue("$t", i.TecnicoResponsabile);
        cmd.Parameters.AddWithValue("$serie", (object?)i.SerieId?.ToString() ?? DBNull.Value);
        if (i.Id == 0)
            i.Id = Convert.ToInt32(cmd.ExecuteScalar());
        else
            cmd.ExecuteNonQuery();
    }

    public void SalvaSquadra(Squadra squadra)
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        if (squadra.Id == 0)
        {
            cmd.CommandText = "INSERT INTO Squadre (Nome, Caposquadra) VALUES ($nome, $capo) RETURNING Id";
        }
        else
        {
            cmd.CommandText = "UPDATE Squadre SET Nome=$nome, Caposquadra=$capo WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", squadra.Id);
        }
        cmd.Parameters.AddWithValue("$nome", squadra.Nome);
        cmd.Parameters.AddWithValue("$capo", squadra.Caposquadra);
        if (squadra.Id == 0)
            squadra.Id = Convert.ToInt32(cmd.ExecuteScalar());
        else
            cmd.ExecuteNonQuery();
    }

    public void SalvaOperatore(Operatore o)
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        if (o.Id == 0)
        {
            cmd.CommandText = """
                INSERT INTO Operatori (SquadraId, Nome, Ruolo, Eta, Livello)
                VALUES ($squadra, $nome, $ruolo, $eta, $livello)
                RETURNING Id
                """;
        }
        else
        {
            cmd.CommandText = """
                UPDATE Operatori
                SET SquadraId=$squadra, Nome=$nome, Ruolo=$ruolo, Eta=$eta, Livello=$livello
                WHERE Id=$id
                """;
            cmd.Parameters.AddWithValue("$id", o.Id);
        }
        cmd.Parameters.AddWithValue("$squadra", o.SquadraId);
        cmd.Parameters.AddWithValue("$nome", o.Nome);
        cmd.Parameters.AddWithValue("$ruolo", o.Ruolo);
        cmd.Parameters.AddWithValue("$eta", o.Eta);
        cmd.Parameters.AddWithValue("$livello", (int)o.Livello);
        if (o.Id == 0)
            o.Id = Convert.ToInt32(cmd.ExecuteScalar());
        else
            cmd.ExecuteNonQuery();
    }

    public void SalvaCertificazione(Certificazione certificazione)
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        if (certificazione.Id == 0)
        {
            cmd.CommandText = """
                INSERT INTO Certificazioni (CantiereId, Titolo, Tipo, Intestatario, DataScadenza)
                VALUES ($cantiereId, $titolo, $tipo, $intestatario, $dataScadenza)
                RETURNING Id
                """;
        }
        else
        {
            cmd.CommandText = """
                UPDATE Certificazioni
                SET CantiereId=$cantiereId, Titolo=$titolo, Tipo=$tipo, Intestatario=$intestatario, DataScadenza=$dataScadenza
                WHERE Id=$id
                """;
            cmd.Parameters.AddWithValue("$id", certificazione.Id);
        }
        cmd.Parameters.AddWithValue("$cantiereId", certificazione.CantiereId);
        cmd.Parameters.AddWithValue("$titolo", certificazione.Titolo);
        cmd.Parameters.AddWithValue("$tipo", (int)certificazione.Tipo);
        cmd.Parameters.AddWithValue("$intestatario", certificazione.Intestatario);
        cmd.Parameters.AddWithValue("$dataScadenza", certificazione.DataScadenza.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (certificazione.Id == 0)
            certificazione.Id = Convert.ToInt32(cmd.ExecuteScalar());
        else
            cmd.ExecuteNonQuery();
    }

    public void EliminaCantiere(int id) => Delete("DELETE FROM Cantieri WHERE Id=$id", id);
    public void EliminaIntervento(int id) => Delete("DELETE FROM Interventi WHERE Id=$id", id);
    public void EliminaSquadra(int id) => Delete("DELETE FROM Squadre WHERE Id=$id", id);
    public void EliminaOperatore(int id) => Delete("DELETE FROM Operatori WHERE Id=$id", id);
    public void EliminaCertificazione(int id) => Delete("DELETE FROM Certificazioni WHERE Id=$id", id);
    public void EliminaMezzo(int id) => Delete("DELETE FROM Mezzi WHERE Id=$id", id);
    public void EliminaDpi(int id) => Delete("DELETE FROM Dpi WHERE Id=$id", id);
    public void EliminaMateriale(int id) => Delete("DELETE FROM Materiali WHERE Id=$id", id);

    public IReadOnlyList<Mezzo> Mezzi()
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT Id,Nome,Tipo,SquadraId FROM Mezzi ORDER BY Tipo,Nome";
        using var r = cmd.ExecuteReader();
        var list = new List<Mezzo>();
        while (r.Read())
            list.Add(new Mezzo { Id = r.GetInt32(0), Nome = r.GetString(1), Tipo = (TipoMezzo)r.GetInt32(2), SquadraId = r.IsDBNull(3) ? null : r.GetInt32(3) });
        return list;
    }

    public void SalvaMezzo(Mezzo m)
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        if (m.Id == 0)
        {
            cmd.CommandText = "INSERT INTO Mezzi (Nome, Tipo, SquadraId) VALUES ($nome, $tipo, $squadra) RETURNING Id";
        }
        else
        {
            cmd.CommandText = "UPDATE Mezzi SET Nome=$nome, Tipo=$tipo, SquadraId=$squadra WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", m.Id);
        }
        cmd.Parameters.AddWithValue("$nome", m.Nome);
        cmd.Parameters.AddWithValue("$tipo", (int)m.Tipo);
        cmd.Parameters.AddWithValue("$squadra", (object?)m.SquadraId ?? DBNull.Value);
        if (m.Id == 0)
            m.Id = Convert.ToInt32(cmd.ExecuteScalar());
        else
            cmd.ExecuteNonQuery();
    }

    public IReadOnlyList<Dpi> DpiList()
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT Id,Nome,SquadraId,DataScadenza FROM Dpi ORDER BY DataScadenza";
        using var r = cmd.ExecuteReader();
        var list = new List<Dpi>();
        while (r.Read())
            list.Add(new Dpi { Id = r.GetInt32(0), Nome = r.GetString(1), SquadraId = r.GetInt32(2), DataScadenza = ParseDate(r.GetString(3)) });
        return list;
    }

    public void SalvaDpi(Dpi d)
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        if (d.Id == 0)
        {
            cmd.CommandText = "INSERT INTO Dpi (Nome, SquadraId, DataScadenza) VALUES ($nome, $squadra, $scadenza) RETURNING Id";
        }
        else
        {
            cmd.CommandText = "UPDATE Dpi SET Nome=$nome, SquadraId=$squadra, DataScadenza=$scadenza WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", d.Id);
        }
        cmd.Parameters.AddWithValue("$nome", d.Nome);
        cmd.Parameters.AddWithValue("$squadra", d.SquadraId);
        cmd.Parameters.AddWithValue("$scadenza", d.DataScadenza.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (d.Id == 0)
            d.Id = Convert.ToInt32(cmd.ExecuteScalar());
        else
            cmd.ExecuteNonQuery();
    }

    public IReadOnlyList<Materiale> Materiali()
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = """
            SELECT m.Id, m.Nome, m.UnitaMisura, m.ScortaMinima, COALESCE(SUM(mm.Quantita), 0)
            FROM Materiali m LEFT JOIN MovimentiMateriale mm ON mm.MaterialeId = m.Id
            GROUP BY m.Id, m.Nome, m.UnitaMisura, m.ScortaMinima
            ORDER BY m.Nome
            """;
        using var r = cmd.ExecuteReader();
        var list = new List<Materiale>();
        while (r.Read())
            list.Add(new Materiale { Id = r.GetInt32(0), Nome = r.GetString(1), UnitaMisura = r.GetString(2), ScortaMinima = r.GetInt32(3), Giacenza = r.GetInt32(4) });
        return list;
    }

    public void SalvaMateriale(Materiale m)
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        if (m.Id == 0)
        {
            cmd.CommandText = "INSERT INTO Materiali (Nome, UnitaMisura, ScortaMinima) VALUES ($nome, $um, $scorta) RETURNING Id";
        }
        else
        {
            cmd.CommandText = "UPDATE Materiali SET Nome=$nome, UnitaMisura=$um, ScortaMinima=$scorta WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", m.Id);
        }
        cmd.Parameters.AddWithValue("$nome", m.Nome);
        cmd.Parameters.AddWithValue("$um", m.UnitaMisura);
        cmd.Parameters.AddWithValue("$scorta", m.ScortaMinima);
        if (m.Id == 0)
            m.Id = Convert.ToInt32(cmd.ExecuteScalar());
        else
            cmd.ExecuteNonQuery();
    }

    public IReadOnlyList<MovimentoMateriale> Movimenti()
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT Id,MaterialeId,Data,Quantita,Riferimento,CantiereId FROM MovimentiMateriale ORDER BY Data DESC, Id DESC";
        using var r = cmd.ExecuteReader();
        var list = new List<MovimentoMateriale>();
        while (r.Read())
            list.Add(new MovimentoMateriale { Id = r.GetInt32(0), MaterialeId = r.GetInt32(1), Data = ParseDate(r.GetString(2)), Quantita = r.GetInt32(3), Riferimento = r.GetString(4), CantiereId = r.IsDBNull(5) ? null : r.GetInt32(5) });
        return list;
    }

    public void RegistraMovimento(MovimentoMateriale mv)
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = """
            INSERT INTO MovimentiMateriale (MaterialeId, Data, Quantita, Riferimento, CantiereId)
            VALUES ($materiale, $data, $quantita, $riferimento, $cantiere)
            RETURNING Id
            """;
        cmd.Parameters.AddWithValue("$materiale", mv.MaterialeId);
        cmd.Parameters.AddWithValue("$data", mv.Data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("$quantita", mv.Quantita);
        cmd.Parameters.AddWithValue("$riferimento", mv.Riferimento);
        cmd.Parameters.AddWithValue("$cantiere", (object?)mv.CantiereId ?? DBNull.Value);
        mv.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    private void Delete(string sql, int id)
    {
        using var db = _database.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    private static DateTime ParseDate(string value)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var invariant))
            return invariant;
        return DateTime.Parse(value, CultureInfo.CurrentCulture);
    }

    private static StatoIntervento ParseStatoIntervento(int value) =>
        Enum.IsDefined(typeof(StatoIntervento), value) ? (StatoIntervento)value : StatoIntervento.Programmato;

    private static TipoCertificazione ParseTipoCertificazione(object value)
    {
        if (value is long number) return (TipoCertificazione)number;
        if (value is int intNumber) return (TipoCertificazione)intNumber;

        var text = Convert.ToString(value) ?? string.Empty;
        if (int.TryParse(text, out var parsed) && Enum.IsDefined(typeof(TipoCertificazione), parsed))
            return (TipoCertificazione)parsed;
        return Enum.TryParse<TipoCertificazione>(text, true, out var tipo)
            ? tipo
            : TipoCertificazione.DocumentazioneCantiere;
    }
}

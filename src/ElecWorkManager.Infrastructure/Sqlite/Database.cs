using Microsoft.Data.Sqlite;

namespace ElecWorkManager.Infrastructure.Sqlite;

public sealed class Database
{
    private readonly string _connectionString;

    public Database(string? path = null)
    {
        path ??= Path.Combine(AppContext.BaseDirectory, "elecwork.db");
        _connectionString = $"Data Source={path}";
    }

    public SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }

    public void Initialize()
    {
        using var db = Open();
        using (var cmd = db.CreateCommand())
        {
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS Squadre (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nome TEXT NOT NULL,
                    Caposquadra TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS Operatori (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SquadraId INTEGER NOT NULL,
                    Nome TEXT NOT NULL,
                    Ruolo TEXT NOT NULL,
                    FOREIGN KEY(SquadraId) REFERENCES Squadre(Id) ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS Cantieri (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Codice TEXT NOT NULL UNIQUE,
                    Nome TEXT NOT NULL,
                    Cliente TEXT NOT NULL,
                    Indirizzo TEXT NOT NULL,
                    Stato INTEGER NOT NULL,
                    SquadraId INTEGER NULL,
                    FOREIGN KEY(SquadraId) REFERENCES Squadre(Id) ON DELETE SET NULL
                );
                CREATE TABLE IF NOT EXISTS Interventi (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CantiereId INTEGER NOT NULL,
                    Descrizione TEXT NOT NULL,
                    DataProgrammata TEXT NOT NULL,
                    DataCompletamento TEXT NULL,
                    Stato INTEGER NOT NULL,
                    Priorita INTEGER NOT NULL,
                    TecnicoResponsabile TEXT NOT NULL,
                    FOREIGN KEY(CantiereId) REFERENCES Cantieri(Id) ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS Certificazioni (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CantiereId INTEGER NOT NULL,
                    Titolo TEXT NOT NULL,
                    Tipo INTEGER NOT NULL,
                    Intestatario TEXT NOT NULL,
                    DataScadenza TEXT NOT NULL,
                    FOREIGN KEY(CantiereId) REFERENCES Cantieri(Id) ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS Mezzi (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nome TEXT NOT NULL,
                    Tipo INTEGER NOT NULL,
                    SquadraId INTEGER NULL,
                    FOREIGN KEY(SquadraId) REFERENCES Squadre(Id) ON DELETE SET NULL
                );
                CREATE TABLE IF NOT EXISTS Dpi (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nome TEXT NOT NULL,
                    SquadraId INTEGER NOT NULL,
                    DataScadenza TEXT NOT NULL,
                    FOREIGN KEY(SquadraId) REFERENCES Squadre(Id) ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS Materiali (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nome TEXT NOT NULL,
                    UnitaMisura TEXT NOT NULL,
                    ScortaMinima INTEGER NOT NULL
                );
                CREATE TABLE IF NOT EXISTS MovimentiMateriale (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MaterialeId INTEGER NOT NULL,
                    Data TEXT NOT NULL,
                    Quantita INTEGER NOT NULL,
                    Riferimento TEXT NOT NULL,
                    CantiereId INTEGER NULL,
                    FOREIGN KEY(MaterialeId) REFERENCES Materiali(Id) ON DELETE CASCADE,
                    FOREIGN KEY(CantiereId) REFERENCES Cantieri(Id) ON DELETE SET NULL
                );
                """;
            cmd.ExecuteNonQuery();
        }

        // Colonne aggiunte in versioni successive dello schema: CREATE TABLE IF NOT EXISTS
        // non basta sui database già esistenti, va fatta una ALTER TABLE mirata.
        EnsureColumn(db, "Operatori", "Eta", "INTEGER NOT NULL DEFAULT 18");
        EnsureColumn(db, "Operatori", "Livello", "INTEGER NOT NULL DEFAULT 1");
        EnsureColumn(db, "Interventi", "SerieId", "TEXT NULL");

        // Stato 3 era l'enum persistito "InRitardo": il ritardo ora è solo calcolato.
        Execute(db, "UPDATE Interventi SET Stato = 0 WHERE Stato = 3");
        Execute(db, "DELETE FROM Interventi WHERE CantiereId NOT IN (SELECT Id FROM Cantieri)");
        Execute(db, "DELETE FROM Certificazioni WHERE CantiereId NOT IN (SELECT Id FROM Cantieri)");
        Execute(db, "DELETE FROM Operatori WHERE SquadraId NOT IN (SELECT Id FROM Squadre)");
        Execute(db, "UPDATE Cantieri SET SquadraId = NULL WHERE SquadraId IS NOT NULL AND SquadraId NOT IN (SELECT Id FROM Squadre)");
        Execute(db, "DELETE FROM Mezzi WHERE SquadraId IS NOT NULL AND SquadraId NOT IN (SELECT Id FROM Squadre)");
        Execute(db, "DELETE FROM Dpi WHERE SquadraId NOT IN (SELECT Id FROM Squadre)");
        Execute(db, "DELETE FROM MovimentiMateriale WHERE MaterialeId NOT IN (SELECT Id FROM Materiali)");
    }

    private static void EnsureColumn(SqliteConnection db, string table, string column, string definition)
    {
        using (var check = db.CreateCommand())
        {
            check.CommandText = $"PRAGMA table_info({table});";
            using var reader = check.ExecuteReader();
            while (reader.Read())
                if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                    return;
        }
        Execute(db, $"ALTER TABLE {table} ADD COLUMN {column} {definition};");
    }

    private static void Execute(SqliteConnection db, string sql)
    {
        using var cmd = db.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}

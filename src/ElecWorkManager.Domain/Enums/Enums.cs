namespace ElecWorkManager.Domain.Enums;

public enum StatoCantiere { Pianificato, InLavorazione, Sospeso, Completato }
public enum StatoIntervento { Programmato, InCorso, Completato }
public enum PrioritaIntervento { Bassa, Media, Alta, Critica }
public enum TipoCertificazione { Abilitazione, Formazione, Sicurezza, IdoneitaTecnica, DocumentazioneCantiere }
public enum StatoScadenza { Regolare, InScadenza, Scaduta }
public enum LivelloEsperienza { Apprendista = 1, Qualificato = 2, Specializzato = 3, Esperto = 4 }
public enum TipoMezzo { Automezzo, Attrezzatura }

using System.Windows.Forms;
using ElecWorkManager.Domain.Entities;
using ElecWorkManager.Domain.Enums;

namespace ElecWorkManager.WinForms;

public sealed class InterventoForm : Form
{
    public Intervento? Result { get; private set; }
    public int RicorrenzaGiorni { get; private set; } // 0 = nessuna, 7 = settimanale, 30 = mensile
    public int RicorrenzaOccorrenze { get; private set; }
    private readonly ComboBox cantiere = new(), stato = new(), priorita = new(), tecnico = new(), ricorrenza = new();
    private readonly TextBox descrizione = new();
    private readonly DateTimePicker data = new();
    private readonly NumericUpDown occorrenze = new() { Minimum = 2, Maximum = 52, Value = 4, Enabled = false };
    private readonly IReadOnlyList<Cantiere> _cantieri;
    private readonly IReadOnlyList<Squadra> _squadre;
    private readonly bool _isNew;

    public InterventoForm(IReadOnlyList<Cantiere> cantieri, IReadOnlyList<Squadra>? squadre = null, Intervento? source = null)
    {
        _cantieri = cantieri;
        _squadre = squadre ?? Array.Empty<Squadra>();
        _isNew = source == null;
        Result = source;
        Text = source == null ? "Nuovo intervento" : "Modifica intervento";
        Width = 560;
        Height = _isNew ? 490 : 420;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Padding = new Padding(24);
        Font = new Font("Segoe UI", 9.5f);

        cantiere.DropDownStyle = ComboBoxStyle.DropDownList;
        cantiere.DisplayMember = "Nome";
        cantiere.ValueMember = "Id";
        cantiere.DataSource = cantieri.ToList();
        cantiere.SelectedIndexChanged += (_, _) => FillTecnici();
        stato.DropDownStyle = ComboBoxStyle.DropDownList;
        stato.DataSource = Enum.GetValues<StatoIntervento>();
        priorita.DropDownStyle = ComboBoxStyle.DropDownList;
        priorita.DataSource = Enum.GetValues<PrioritaIntervento>();
        tecnico.DropDownStyle = ComboBoxStyle.DropDown;

        Add("Cantiere", cantiere, 0);
        Add("Descrizione", descrizione, 1);
        Add("Data", data, 2);
        Add("Priorità", priorita, 3);
        Add("Stato", stato, 4);
        Add("Tecnico", tecnico, 5);

        if (source != null)
        {
            cantiere.SelectedValue = source.CantiereId;
            descrizione.Text = source.Descrizione;
            data.Value = source.DataProgrammata == default ? DateTime.Today : source.DataProgrammata;
            priorita.SelectedItem = source.Priorita;
            stato.SelectedItem = Enum.IsDefined(source.Stato) ? source.Stato : StatoIntervento.Programmato;
        }
        else
        {
            priorita.SelectedItem = PrioritaIntervento.Media;
            stato.SelectedItem = StatoIntervento.Programmato;
        }

        if (_isNew)
        {
            ricorrenza.DropDownStyle = ComboBoxStyle.DropDownList;
            ricorrenza.Items.Add("Nessuna ripetizione");
            ricorrenza.Items.Add("Ripeti ogni settimana");
            ricorrenza.Items.Add("Ripeti ogni mese");
            ricorrenza.SelectedIndex = 0;
            ricorrenza.SelectedIndexChanged += (_, _) => occorrenze.Enabled = ricorrenza.SelectedIndex != 0;
            Add("Ricorrenza", ricorrenza, 6);
            Add("Numero di ripetizioni", occorrenze, 7);
        }

        FillTecnici();
        if (source != null && !string.IsNullOrWhiteSpace(source.TecnicoResponsabile))
            tecnico.Text = source.TecnicoResponsabile;

        var okTop = _isNew ? 440 : 330;
        var ok = new Button { Text = "Salva", Width = 100, Height = 34, Left = 400, Top = okTop };
        ok.Click += (_, _) => Save();
        var cancel = new Button { Text = "Annulla", DialogResult = DialogResult.Cancel, Width = 100, Height = 34, Left = 290, Top = okTop };
        Controls.Add(ok);
        Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    private void FillTecnici()
    {
        var current = tecnico.Text;
        var names = new List<string>();
        var cantiereId = FormValues.SelectedInt(cantiere);
        var squadraId = _cantieri.FirstOrDefault(x => x.Id == cantiereId)?.SquadraId;
        if (squadraId is int sid)
        {
            var squadra = _squadre.FirstOrDefault(x => x.Id == sid);
            if (squadra != null)
                names.AddRange(squadra.Operatori.Select(x => x.Nome).Where(x => !string.IsNullOrWhiteSpace(x)));
        }
        if (names.Count == 0)
            names.AddRange(_squadre.SelectMany(s => s.Operatori.Select(o => o.Nome)).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct());

        tecnico.Items.Clear();
        foreach (var name in names.Distinct())
            tecnico.Items.Add(name);
        if (!string.IsNullOrWhiteSpace(current))
            tecnico.Text = current;
        else if (tecnico.Items.Count > 0)
            tecnico.SelectedIndex = 0;
    }

    private void Save()
    {
        var cantiereId = FormValues.SelectedInt(cantiere);
        if (cantiereId is null or 0)
        {
            MessageBox.Show("Seleziona un cantiere.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (string.IsNullOrWhiteSpace(descrizione.Text))
        {
            MessageBox.Show("La descrizione dell'intervento è obbligatoria.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (string.IsNullOrWhiteSpace(tecnico.Text))
        {
            MessageBox.Show("Il tecnico responsabile è obbligatorio.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var statoValue = stato.SelectedItem is StatoIntervento s ? s : StatoIntervento.Programmato;
        var completato = statoValue == StatoIntervento.Completato;
        Result = new Intervento
        {
            Id = Result?.Id ?? 0,
            CantiereId = cantiereId.Value,
            Descrizione = descrizione.Text.Trim(),
            DataProgrammata = data.Value.Date,
            Priorita = priorita.SelectedItem is PrioritaIntervento p ? p : PrioritaIntervento.Media,
            Stato = statoValue,
            TecnicoResponsabile = tecnico.Text.Trim(),
            DataCompletamento = completato ? Result?.DataCompletamento ?? DateTime.Today : null
        };
        if (_isNew)
        {
            RicorrenzaGiorni = ricorrenza.SelectedIndex switch { 1 => 7, 2 => 30, _ => 0 };
            RicorrenzaOccorrenze = RicorrenzaGiorni > 0 ? (int)occorrenze.Value : 0;
        }
        DialogResult = DialogResult.OK;
    }

    private void Add(string label, Control c, int row)
    {
        var y = 18 + row * 48;
        Controls.Add(new Label { Text = label, Left = 0, Top = y + 5, Width = 90 });
        c.Left = 100;
        c.Top = y;
        c.Width = 390;
        c.Height = 30;
        Controls.Add(c);
    }
}

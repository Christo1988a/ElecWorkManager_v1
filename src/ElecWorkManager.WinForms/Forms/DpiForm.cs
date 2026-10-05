using System.Windows.Forms;
using ElecWorkManager.Domain.Entities;

namespace ElecWorkManager.WinForms;

public sealed class DpiForm : Form
{
    public Dpi? Result { get; private set; }
    private readonly TextBox nome = new();
    private readonly ComboBox squadra = new();
    private readonly DateTimePicker scadenza = new();
    private readonly IReadOnlyList<Squadra> _squadre;

    public DpiForm(IReadOnlyList<Squadra> squadre, Dpi? source = null)
    {
        _squadre = squadre;
        Result = source;
        Text = source == null ? "Nuovo DPI" : "Modifica DPI";
        Width = 560;
        Height = 340;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Padding = new Padding(24);
        Font = new Font("Segoe UI", 9.5f);

        squadra.DropDownStyle = ComboBoxStyle.DropDownList;
        squadra.DisplayMember = "Nome";
        squadra.ValueMember = "Id";
        squadra.DataSource = _squadre.ToList();

        Add("Dispositivo", nome, 0);
        Add("Squadra", squadra, 1);
        Add("Scadenza", scadenza, 2);

        if (source != null)
        {
            nome.Text = source.Nome;
            squadra.SelectedValue = source.SquadraId;
            scadenza.Value = source.DataScadenza == default ? DateTime.Today : source.DataScadenza;
        }

        var ok = new Button { Text = "Salva", Width = 100, Height = 34, Left = 400, Top = 250 };
        ok.Click += (_, _) => Save();
        var cancel = new Button { Text = "Annulla", DialogResult = DialogResult.Cancel, Width = 100, Height = 34, Left = 290, Top = 250 };
        Controls.Add(ok);
        Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(nome.Text))
        {
            MessageBox.Show("Il nome del dispositivo è obbligatorio.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var squadraId = FormValues.SelectedInt(squadra);
        if (squadraId is null)
        {
            MessageBox.Show("Crea prima almeno una squadra a cui assegnare il DPI.", "Nessuna squadra", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Result = new Dpi
        {
            Id = Result?.Id ?? 0,
            Nome = nome.Text.Trim(),
            SquadraId = squadraId.Value,
            DataScadenza = scadenza.Value.Date
        };
        DialogResult = DialogResult.OK;
    }

    private void Add(string label, Control control, int row)
    {
        var y = 18 + row * 48;
        Controls.Add(new Label { Text = label, Left = 0, Top = y + 5, Width = 100 });
        control.Left = 110;
        control.Top = y;
        control.Width = 390;
        control.Height = 30;
        Controls.Add(control);
    }
}

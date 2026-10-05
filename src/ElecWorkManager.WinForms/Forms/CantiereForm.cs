using System.Windows.Forms;
using ElecWorkManager.Domain.Entities;
using ElecWorkManager.Domain.Enums;

namespace ElecWorkManager.WinForms;

public sealed class CantiereForm : Form
{
    public Cantiere? Result { get; private set; }
    private readonly TextBox codice = new(), nome = new(), cliente = new(), indirizzo = new();
    private readonly ComboBox stato = new(), squadra = new();
    private readonly IReadOnlyList<Squadra> _squadre;

    public CantiereForm(IReadOnlyList<Squadra>? squadre = null, Cantiere? source = null)
    {
        _squadre = squadre ?? Array.Empty<Squadra>();
        Text = source == null ? "Nuovo cantiere" : "Modifica cantiere";
        Width = 560;
        Height = 420;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Padding = new Padding(24);
        Font = new Font("Segoe UI", 9.5f);

        stato.DropDownStyle = ComboBoxStyle.DropDownList;
        stato.DataSource = Enum.GetValues<StatoCantiere>();
        squadra.DropDownStyle = ComboBoxStyle.DropDownList;
        squadra.DisplayMember = "Nome";
        squadra.ValueMember = "Id";
        squadra.DataSource = _squadre.ToList();
        squadra.SelectedIndex = -1;

        Add("Codice", codice, 0);
        Add("Nome", nome, 1);
        Add("Cliente", cliente, 2);
        Add("Indirizzo", indirizzo, 3);
        Add("Stato", stato, 4);
        Add("Squadra", squadra, 5);

        if (source != null)
        {
            Result = source;
            codice.Text = source.Codice;
            nome.Text = source.Nome;
            cliente.Text = source.Cliente;
            indirizzo.Text = source.Indirizzo;
            stato.SelectedItem = source.Stato;
            if (source.SquadraId.HasValue)
                squadra.SelectedValue = source.SquadraId.Value;
        }

        var ok = new Button { Text = "Salva", Width = 100, Height = 34, Left = 400, Top = 330 };
        ok.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(codice.Text) || string.IsNullOrWhiteSpace(nome.Text))
            {
                MessageBox.Show("Codice e nome del cantiere sono obbligatori.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Result = new Cantiere
            {
                Id = Result?.Id ?? 0,
                Codice = codice.Text.Trim(),
                Nome = nome.Text.Trim(),
                Cliente = cliente.Text.Trim(),
                Indirizzo = indirizzo.Text.Trim(),
                Stato = stato.SelectedItem is StatoCantiere s ? s : StatoCantiere.Pianificato,
                SquadraId = FormValues.SelectedInt(squadra)
            };
            DialogResult = DialogResult.OK;
        };
        var cancel = new Button { Text = "Annulla", DialogResult = DialogResult.Cancel, Width = 100, Height = 34, Left = 290, Top = 330 };
        Controls.Add(ok);
        Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    private void Add(string label, Control control, int row)
    {
        var y = 18 + row * 48;
        Controls.Add(new Label { Text = label, Left = 0, Top = y + 5, Width = 90 });
        control.Left = 100;
        control.Top = y;
        control.Width = 390;
        control.Height = 30;
        Controls.Add(control);
    }
}

using System.Windows.Forms;
using ElecWorkManager.Domain.Entities;
using ElecWorkManager.Domain.Enums;

namespace ElecWorkManager.WinForms;

public sealed class OperatoreForm : Form
{
    public Operatore? Result { get; private set; }
    private readonly TextBox nome = new(), ruolo = new();
    private readonly NumericUpDown eta = new() { Minimum = 16, Maximum = 80, Value = 18 };
    private readonly ComboBox livello = new(), squadra = new();
    private readonly IReadOnlyList<Squadra> _squadre;

    public OperatoreForm(IReadOnlyList<Squadra> squadre, Operatore? source = null)
    {
        _squadre = squadre;
        Result = source;
        Text = source == null ? "Nuovo operaio" : "Modifica operaio";
        Width = 560;
        Height = 420;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Padding = new Padding(24);
        Font = new Font("Segoe UI", 9.5f);

        livello.DropDownStyle = ComboBoxStyle.DropDownList;
        livello.DataSource = Enum.GetValues<LivelloEsperienza>();
        squadra.DropDownStyle = ComboBoxStyle.DropDownList;
        squadra.DisplayMember = "Nome";
        squadra.ValueMember = "Id";
        squadra.DataSource = _squadre.ToList();

        Add("Nome e cognome", nome, 0);
        Add("Ruolo", ruolo, 1);
        Add("Età", eta, 2);
        Add("Livello esperienza", livello, 3);
        Add("Squadra", squadra, 4);

        if (source != null)
        {
            nome.Text = source.Nome;
            ruolo.Text = source.Ruolo;
            eta.Value = Math.Clamp(source.Eta, (int)eta.Minimum, (int)eta.Maximum);
            livello.SelectedItem = source.Livello;
            squadra.SelectedValue = source.SquadraId;
        }
        else
        {
            livello.SelectedItem = LivelloEsperienza.Apprendista;
        }

        var ok = new Button { Text = "Salva", Width = 100, Height = 34, Left = 400, Top = 330 };
        ok.Click += (_, _) => Save();
        var cancel = new Button { Text = "Annulla", DialogResult = DialogResult.Cancel, Width = 100, Height = 34, Left = 290, Top = 330 };
        Controls.Add(ok);
        Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(nome.Text) || string.IsNullOrWhiteSpace(ruolo.Text))
        {
            MessageBox.Show("Nome e ruolo dell'operaio sono obbligatori.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var squadraId = FormValues.SelectedInt(squadra);
        if (squadraId is null)
        {
            MessageBox.Show("Crea prima almeno una squadra a cui assegnare l'operaio.", "Nessuna squadra", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Result = new Operatore
        {
            Id = Result?.Id ?? 0,
            Nome = nome.Text.Trim(),
            Ruolo = ruolo.Text.Trim(),
            Eta = (int)eta.Value,
            Livello = livello.SelectedItem is LivelloEsperienza l ? l : LivelloEsperienza.Apprendista,
            SquadraId = squadraId.Value
        };
        DialogResult = DialogResult.OK;
    }

    private void Add(string label, Control control, int row)
    {
        var y = 18 + row * 48;
        Controls.Add(new Label { Text = label, Left = 0, Top = y + 5, Width = 130 });
        control.Left = 140;
        control.Top = y;
        control.Width = 360;
        control.Height = 30;
        Controls.Add(control);
    }
}

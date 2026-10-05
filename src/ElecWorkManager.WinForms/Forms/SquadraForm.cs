using System.Windows.Forms;
using ElecWorkManager.Domain.Entities;

namespace ElecWorkManager.WinForms;

public sealed class SquadraForm : Form
{
    public Squadra? Result { get; private set; }
    private readonly TextBox nome = new(), caposquadra = new();

    public SquadraForm(Squadra? source = null)
    {
        Result = source;
        Text = source == null ? "Nuova squadra" : "Modifica squadra";
        Width = 480;
        Height = 260;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Padding = new Padding(24);
        Font = new Font("Segoe UI", 9.5f);

        Add("Nome", nome, 0);
        Add("Caposquadra", caposquadra, 1);
        Controls.Add(new Label { Text = "Gli operai si gestiscono dalla scheda \"Operai\".", Left = 0, Top = 118, Width = 400, ForeColor = Color.Gray });

        if (source != null)
        {
            nome.Text = source.Nome;
            caposquadra.Text = source.Caposquadra;
        }

        var ok = new Button { Text = "Salva", Width = 100, Height = 34, Left = 320, Top = 170 };
        ok.Click += (_, _) => Save();
        var cancel = new Button { Text = "Annulla", DialogResult = DialogResult.Cancel, Width = 100, Height = 34, Left = 210, Top = 170 };
        Controls.Add(ok);
        Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(nome.Text))
        {
            MessageBox.Show("Il nome della squadra è obbligatorio.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Result = new Squadra
        {
            Id = Result?.Id ?? 0,
            Nome = nome.Text.Trim(),
            Caposquadra = caposquadra.Text.Trim()
        };
        DialogResult = DialogResult.OK;
    }

    private void Add(string label, Control control, int row)
    {
        var y = 18 + row * 48;
        Controls.Add(new Label { Text = label, Left = 0, Top = y + 5, Width = 110 });
        control.Left = 120;
        control.Top = y;
        control.Width = 300;
        control.Height = 30;
        Controls.Add(control);
    }
}

using System.Windows.Forms;
using ElecWorkManager.Domain.Entities;

namespace ElecWorkManager.WinForms;

public sealed class MaterialeForm : Form
{
    public Materiale? Result { get; private set; }
    private readonly TextBox nome = new(), unitaMisura = new();
    private readonly NumericUpDown scortaMinima = new() { Minimum = 0, Maximum = 100000, Value = 0 };

    public MaterialeForm(Materiale? source = null)
    {
        Result = source;
        Text = source == null ? "Nuovo materiale" : "Modifica materiale";
        Width = 480;
        Height = 300;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Padding = new Padding(24);
        Font = new Font("Segoe UI", 9.5f);

        Add("Nome materiale", nome, 0);
        Add("Unità di misura", unitaMisura, 1);
        Add("Scorta minima", scortaMinima, 2);

        if (source != null)
        {
            nome.Text = source.Nome;
            unitaMisura.Text = source.UnitaMisura;
            scortaMinima.Value = Math.Clamp(source.ScortaMinima, 0, 100000);
        }

        var ok = new Button { Text = "Salva", Width = 100, Height = 34, Left = 320, Top = 210 };
        ok.Click += (_, _) => Save();
        var cancel = new Button { Text = "Annulla", DialogResult = DialogResult.Cancel, Width = 100, Height = 34, Left = 210, Top = 210 };
        Controls.Add(ok);
        Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(nome.Text) || string.IsNullOrWhiteSpace(unitaMisura.Text))
        {
            MessageBox.Show("Nome e unità di misura sono obbligatori.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Result = new Materiale
        {
            Id = Result?.Id ?? 0,
            Nome = nome.Text.Trim(),
            UnitaMisura = unitaMisura.Text.Trim(),
            ScortaMinima = (int)scortaMinima.Value,
            Giacenza = Result?.Giacenza ?? 0
        };
        DialogResult = DialogResult.OK;
    }

    private void Add(string label, Control control, int row)
    {
        var y = 18 + row * 48;
        Controls.Add(new Label { Text = label, Left = 0, Top = y + 5, Width = 120 });
        control.Left = 130;
        control.Top = y;
        control.Width = 290;
        control.Height = 30;
        Controls.Add(control);
    }
}

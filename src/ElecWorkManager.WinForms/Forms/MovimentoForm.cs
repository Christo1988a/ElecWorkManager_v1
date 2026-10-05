using System.Windows.Forms;
using ElecWorkManager.Domain.Entities;

namespace ElecWorkManager.WinForms;

public sealed class MovimentoForm : Form
{
    public MovimentoMateriale? Result { get; private set; }
    private readonly ComboBox materiale = new(), tipoMovimento = new(), cantiere = new();
    private readonly NumericUpDown quantita = new() { Minimum = 1, Maximum = 1_000_000, Value = 1 };
    private readonly TextBox riferimento = new();
    private readonly DateTimePicker data = new();
    private readonly IReadOnlyList<Materiale> _materiali;

    public MovimentoForm(IReadOnlyList<Materiale> materiali, IReadOnlyList<Cantiere> cantieri)
    {
        _materiali = materiali;
        Text = "Registra movimento di magazzino";
        Width = 560;
        Height = 420;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Padding = new Padding(24);
        Font = new Font("Segoe UI", 9.5f);

        materiale.DropDownStyle = ComboBoxStyle.DropDownList;
        materiale.DisplayMember = "Nome";
        materiale.ValueMember = "Id";
        materiale.DataSource = _materiali.ToList();

        tipoMovimento.DropDownStyle = ComboBoxStyle.DropDownList;
        tipoMovimento.Items.Add("Carico (es. da DDT fornitore)");
        tipoMovimento.Items.Add("Utilizzo (scarico per un intervento)");
        tipoMovimento.SelectedIndex = 0;

        cantiere.DropDownStyle = ComboBoxStyle.DropDownList;
        cantiere.DisplayMember = "Nome";
        cantiere.ValueMember = "Id";
        cantiere.DataSource = cantieri.ToList();
        cantiere.SelectedIndex = -1;

        data.Value = DateTime.Today;

        Add("Materiale", materiale, 0);
        Add("Movimento", tipoMovimento, 1);
        Add("Quantità", quantita, 2);
        Add("Cantiere (opzionale)", cantiere, 3);
        Add("Riferimento (n. DDT, fornitore...)", riferimento, 4);
        Add("Data", data, 5);

        var ok = new Button { Text = "Registra", Width = 110, Height = 34, Left = 380, Top = 330 };
        ok.Click += (_, _) => Save();
        var cancel = new Button { Text = "Annulla", DialogResult = DialogResult.Cancel, Width = 100, Height = 34, Left = 260, Top = 330 };
        Controls.Add(ok);
        Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    private void Save()
    {
        var materialeId = FormValues.SelectedInt(materiale);
        if (materialeId is null)
        {
            MessageBox.Show("Crea prima almeno un materiale in anagrafica.", "Nessun materiale", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (string.IsNullOrWhiteSpace(riferimento.Text))
        {
            MessageBox.Show("Indica un riferimento (es. numero DDT o intervento).", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var isUtilizzo = tipoMovimento.SelectedIndex == 1;
        Result = new MovimentoMateriale
        {
            MaterialeId = materialeId.Value,
            Quantita = isUtilizzo ? -(int)quantita.Value : (int)quantita.Value,
            Riferimento = riferimento.Text.Trim(),
            CantiereId = FormValues.SelectedInt(cantiere),
            Data = data.Value.Date
        };
        DialogResult = DialogResult.OK;
    }

    private void Add(string label, Control control, int row)
    {
        var y = 18 + row * 48;
        Controls.Add(new Label { Text = label, Left = 0, Top = y + 5, Width = 190 });
        control.Left = 200;
        control.Top = y;
        control.Width = 300;
        control.Height = 30;
        Controls.Add(control);
    }
}

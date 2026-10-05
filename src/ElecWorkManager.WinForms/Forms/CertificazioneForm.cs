using System.Windows.Forms;
using ElecWorkManager.Domain.Entities;
using ElecWorkManager.Domain.Enums;

namespace ElecWorkManager.WinForms;

public sealed class CertificazioneForm : Form
{
    public Certificazione? Result { get; private set; }
    private readonly ComboBox cantiere = new(), tipo = new();
    private readonly TextBox titolo = new(), intestatario = new();
    private readonly DateTimePicker scadenza = new();

    public CertificazioneForm(IReadOnlyList<Cantiere> cantieri, Certificazione? source = null)
    {
        Result = source;
        Text = source == null ? "Nuova scadenza" : "Modifica scadenza";
        Width = 560;
        Height = 380;
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
        tipo.DropDownStyle = ComboBoxStyle.DropDownList;
        tipo.DataSource = Enum.GetValues<TipoCertificazione>();

        Add("Cantiere", cantiere, 0);
        Add("Documento", titolo, 1);
        Add("Tipo", tipo, 2);
        Add("Intestatario", intestatario, 3);
        Add("Scadenza", scadenza, 4);

        if (source != null)
        {
            cantiere.SelectedValue = source.CantiereId;
            titolo.Text = source.Titolo;
            tipo.SelectedItem = source.Tipo;
            intestatario.Text = source.Intestatario;
            scadenza.Value = source.DataScadenza == default ? DateTime.Today : source.DataScadenza;
        }

        var ok = new Button { Text = "Salva", Width = 100, Height = 34, Left = 400, Top = 280 };
        ok.Click += (_, _) => Save();
        var cancel = new Button { Text = "Annulla", DialogResult = DialogResult.Cancel, Width = 100, Height = 34, Left = 290, Top = 280 };
        Controls.Add(ok);
        Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    private void Save()
    {
        var cantiereId = FormValues.SelectedInt(cantiere);
        if (cantiereId is null or 0)
        {
            MessageBox.Show("Seleziona un cantiere.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (string.IsNullOrWhiteSpace(titolo.Text) || string.IsNullOrWhiteSpace(intestatario.Text))
        {
            MessageBox.Show("Documento e intestatario sono obbligatori.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Result = new Certificazione
        {
            Id = Result?.Id ?? 0,
            CantiereId = cantiereId.Value,
            Titolo = titolo.Text.Trim(),
            Tipo = tipo.SelectedItem is TipoCertificazione t ? t : TipoCertificazione.DocumentazioneCantiere,
            Intestatario = intestatario.Text.Trim(),
            DataScadenza = scadenza.Value.Date
        };
        DialogResult = DialogResult.OK;
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

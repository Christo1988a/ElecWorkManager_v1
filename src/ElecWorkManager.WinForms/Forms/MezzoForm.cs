using System.Windows.Forms;
using ElecWorkManager.Domain.Entities;
using ElecWorkManager.Domain.Enums;

namespace ElecWorkManager.WinForms;

public sealed class MezzoForm : Form
{
    public Mezzo? Result { get; private set; }
    private readonly TextBox nome = new();
    private readonly ComboBox tipo = new(), squadra = new();
    private readonly IReadOnlyList<Squadra> _squadre;

    public MezzoForm(IReadOnlyList<Squadra> squadre, Mezzo? source = null)
    {
        _squadre = squadre;
        Result = source;
        Text = source == null ? "Nuovo mezzo/attrezzatura" : "Modifica mezzo/attrezzatura";
        Width = 560;
        Height = 340;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Padding = new Padding(24);
        Font = new Font("Segoe UI", 9.5f);

        tipo.DropDownStyle = ComboBoxStyle.DropDownList;
        tipo.DataSource = Enum.GetValues<TipoMezzo>();
        squadra.DropDownStyle = ComboBoxStyle.DropDownList;
        squadra.DisplayMember = "Nome";
        squadra.ValueMember = "Id";
        squadra.DataSource = _squadre.ToList();
        squadra.SelectedIndex = -1;

        Add("Nome/descrizione", nome, 0);
        Add("Tipo", tipo, 1);
        Add("Squadra assegnata", squadra, 2);

        if (source != null)
        {
            nome.Text = source.Nome;
            tipo.SelectedItem = source.Tipo;
            if (source.SquadraId.HasValue) squadra.SelectedValue = source.SquadraId.Value;
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
            MessageBox.Show("Il nome è obbligatorio.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Result = new Mezzo
        {
            Id = Result?.Id ?? 0,
            Nome = nome.Text.Trim(),
            Tipo = tipo.SelectedItem is TipoMezzo t ? t : TipoMezzo.Automezzo,
            SquadraId = FormValues.SelectedInt(squadra)
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

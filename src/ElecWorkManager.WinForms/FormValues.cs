using System.Windows.Forms;

namespace ElecWorkManager.WinForms;

internal static class FormValues
{
    public static int? SelectedInt(ComboBox combo)
    {
        if (combo.SelectedIndex < 0 || combo.SelectedValue is null or DBNull)
            return null;
        try
        {
            return Convert.ToInt32(combo.SelectedValue);
        }
        catch (Exception)
        {
            return null;
        }
    }
}

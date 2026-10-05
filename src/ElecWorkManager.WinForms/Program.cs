using System.Windows.Forms;
using ElecWorkManager.Infrastructure;
using ElecWorkManager.Infrastructure.Repositories;
using ElecWorkManager.Infrastructure.Sqlite;

namespace ElecWorkManager.WinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        var service = new SqliteGestionaleService(new Database());
        DemoDataSeeder.EnsureSeeded(service);
        System.Windows.Forms.Application.Run(new MainForm(service));
    }
}

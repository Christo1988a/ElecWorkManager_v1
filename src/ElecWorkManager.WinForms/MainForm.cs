using System.Drawing;
using System.Windows.Forms;
using ElecWorkManager.Application.Services;
using ElecWorkManager.Domain;
using ElecWorkManager.Domain.Entities;
using ElecWorkManager.Domain.Enums;

namespace ElecWorkManager.WinForms;

public class MainForm : Form
{
    private readonly IGestionaleService _service;
    private readonly DashboardService _dashboard = new();
    private DataGridView _cantieriGrid = null!, _interventiGrid = null!, _squadreGrid = null!, _scadenzeGrid = null!, _operaiGrid = null!, _mezziGrid = null!, _dpiGrid = null!, _materialiGrid = null!, _movimentiGrid = null!;
    private Label _kpiCantieri = null!, _kpiSquadre = null!, _kpiOggi = null!, _kpiRitardi = null!, _kpiScadenze = null!, _kpiMateriali = null!;
    private TextBox _searchCantieri = null!, _searchInterventi = null!, _searchOperai = null!;
    private CheckBox _soloCritiche = null!;
    private MonthCalendar _calendario = null!;
    private ListBox _calendarioLista = null!;

    public MainForm(IGestionaleService service)
    {
        _service = service;
        Text = "ElecWork Manager · Gestionale operativo";
        Width = 1280;
        Height = 820;
        MinimumSize = new Size(1080, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5f);
        BackColor = Color.FromArgb(246, 247, 249);
        BuildUi();
        LoadData();
    }

    private void BuildUi()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 78, Padding = new Padding(28, 18, 28, 10), BackColor = Color.White };
        header.Controls.Add(new Label { Text = "ElecWork Manager", Font = new Font("Segoe UI Semibold", 20), AutoSize = true, Location = new Point(28, 16) });
        header.Controls.Add(new Label { Text = "Gestione cantieri, squadre, interventi e scadenze", ForeColor = Color.DimGray, AutoSize = true, Location = new Point(30, 48) });
        var actions = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 360, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 8, 0, 0) };
        actions.Controls.Add(Button("＋ Nuovo intervento", (_, _) => NewIntervento()));
        actions.Controls.Add(Button("＋ Nuovo cantiere", (_, _) => NewCantiere()));
        header.Controls.Add(actions);

        var kpis = new TableLayoutPanel { Dock = DockStyle.Top, Height = 118, Padding = new Padding(24, 16, 24, 10), ColumnCount = 6, RowCount = 1 };
        for (var n = 0; n < 6; n++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100.0f / 6));
        (_kpiCantieri, var c1) = KpiCard("Cantieri attivi");
        (_kpiSquadre, var c2) = KpiCard("Squadre libere");
        (_kpiOggi, var c3) = KpiCard("Interventi oggi");
        (_kpiRitardi, var c4) = KpiCard("In ritardo");
        (_kpiScadenze, var c5) = KpiCard("Scadenze critiche");
        (_kpiMateriali, var c6) = KpiCard("Materiali sotto scorta");
        kpis.Controls.Add(c1, 0, 0);
        kpis.Controls.Add(c2, 1, 0);
        kpis.Controls.Add(c3, 2, 0);
        kpis.Controls.Add(c4, 3, 0);
        kpis.Controls.Add(c5, 4, 0);
        kpis.Controls.Add(c6, 5, 0);

        var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(14, 8) };
        tabs.TabPages.Add(BuildCantieriTab());
        tabs.TabPages.Add(BuildSquadreTab());
        tabs.TabPages.Add(BuildOperaiTab());
        tabs.TabPages.Add(BuildInterventiTab());
        tabs.TabPages.Add(BuildCalendarioTab());
        tabs.TabPages.Add(BuildScadenzeTab());
        tabs.TabPages.Add(BuildMezziDpiTab());
        tabs.TabPages.Add(BuildMagazzinoTab());
        Controls.Add(tabs);
        Controls.Add(kpis);
        Controls.Add(header);
    }

    private TabPage BuildCantieriTab()
    {
        var page = Page("Cantieri");
        var bar = new Panel { Dock = DockStyle.Top, Height = 46 };
        _searchCantieri = SearchBox("Cerca per codice, nome, cliente o indirizzo...");
        _searchCantieri.TextChanged += (_, _) => RefreshCantieri(_searchCantieri.Text);
        bar.Controls.Add(_searchCantieri);
        var refresh = Button("Aggiorna", (_, _) => LoadData());
        refresh.Dock = DockStyle.Right;
        refresh.Width = 100;
        bar.Controls.Add(refresh);
        _cantieriGrid = Grid();
        HideId(_cantieriGrid);
        _cantieriGrid.Columns.Add("Codice", "Codice");
        _cantieriGrid.Columns.Add("Nome", "Cantiere");
        _cantieriGrid.Columns.Add("Cliente", "Cliente");
        _cantieriGrid.Columns.Add("Indirizzo", "Indirizzo");
        _cantieriGrid.Columns.Add("Stato", "Stato");
        _cantieriGrid.Columns.Add("Squadra", "Squadra");
        _cantieriGrid.DoubleClick += (_, _) => { if (ModifierKeys == Keys.Control) DeleteSelectedCantiere(); else EditSelectedCantiere(); };
        page.Controls.Add(_cantieriGrid);
        page.Controls.Add(Hint("Doppio clic per modificare · Ctrl + doppio clic per eliminare"));
        page.Controls.Add(bar);
        return page;
    }

    private TabPage BuildSquadreTab()
    {
        var page = Page("Squadre");
        var bar = new Panel { Dock = DockStyle.Top, Height = 46 };
        var nuovo = Button("＋ Nuova squadra", (_, _) => NewSquadra());
        nuovo.Dock = DockStyle.Left;
        var refresh = Button("Aggiorna", (_, _) => LoadData());
        refresh.Dock = DockStyle.Right;
        refresh.Width = 100;
        bar.Controls.Add(refresh);
        bar.Controls.Add(nuovo);
        _squadreGrid = Grid();
        HideId(_squadreGrid);
        _squadreGrid.Columns.Add("Nome", "Squadra");
        _squadreGrid.Columns.Add("Caposquadra", "Caposquadra");
        _squadreGrid.Columns.Add("Operatori", "Operatori");
        _squadreGrid.DoubleClick += (_, _) => { if (ModifierKeys == Keys.Control) DeleteSelectedSquadra(); else EditSelectedSquadra(); };
        page.Controls.Add(_squadreGrid);
        page.Controls.Add(Hint("Doppio clic per modificare · Ctrl + doppio clic per eliminare. I cantieri assegnati restano senza squadra; operai, mezzi e DPI collegati vengono eliminati."));
        page.Controls.Add(bar);
        return page;
    }

    private TabPage BuildOperaiTab()
    {
        var page = Page("Operai");
        var bar = new Panel { Dock = DockStyle.Top, Height = 46 };
        _searchOperai = SearchBox("Cerca per nome, ruolo o squadra...");
        _searchOperai.TextChanged += (_, _) => RefreshOperai(_searchOperai.Text);
        bar.Controls.Add(_searchOperai);
        var nuovo = Button("＋ Nuovo operaio", (_, _) => NewOperatore());
        nuovo.Dock = DockStyle.Right;
        var refresh = Button("Aggiorna", (_, _) => LoadData());
        refresh.Dock = DockStyle.Right;
        refresh.Width = 100;
        bar.Controls.Add(refresh);
        bar.Controls.Add(nuovo);
        _operaiGrid = Grid();
        HideId(_operaiGrid);
        _operaiGrid.Columns.Add("Nome", "Nome");
        _operaiGrid.Columns.Add("Ruolo", "Ruolo");
        _operaiGrid.Columns.Add("Eta", "Età");
        _operaiGrid.Columns.Add("Livello", "Livello");
        _operaiGrid.Columns.Add("Squadra", "Squadra");
        _operaiGrid.DoubleClick += (_, _) => { if (ModifierKeys == Keys.Control) DeleteSelectedOperatore(); else EditSelectedOperatore(); };
        page.Controls.Add(_operaiGrid);
        page.Controls.Add(Hint("Doppio clic per modificare · Ctrl + doppio clic per eliminare."));
        page.Controls.Add(bar);
        return page;
    }

    private TabPage BuildInterventiTab()
    {
        var page = Page("Interventi");
        var bar = new Panel { Dock = DockStyle.Top, Height = 46 };
        _searchInterventi = SearchBox("Cerca descrizione, tecnico o stato...");
        _searchInterventi.TextChanged += (_, _) => RefreshInterventi(_searchInterventi.Text);
        bar.Controls.Add(_searchInterventi);
        var refresh = Button("Aggiorna", (_, _) => LoadData());
        refresh.Dock = DockStyle.Right;
        refresh.Width = 100;
        bar.Controls.Add(refresh);
        _interventiGrid = Grid();
        HideId(_interventiGrid);
        var idxCId = _interventiGrid.Columns.Add("CantiereId", "CantiereId");
        _interventiGrid.Columns[idxCId].Visible = false;
        _interventiGrid.Columns.Add("Cantiere", "Cantiere");
        _interventiGrid.Columns.Add("Intervento", "Intervento");
        _interventiGrid.Columns.Add("Data", "Data");
        _interventiGrid.Columns.Add("Priorita", "Priorità");
        _interventiGrid.Columns.Add("Stato", "Stato");
        _interventiGrid.Columns.Add("Tecnico", "Tecnico");
        _interventiGrid.DoubleClick += (_, _) => { if (ModifierKeys == Keys.Control) DeleteSelectedIntervento(); else EditSelectedIntervento(); };
        page.Controls.Add(_interventiGrid);
        page.Controls.Add(Hint("Doppio clic per modificare · Ctrl + doppio clic per eliminare. Lo stato In ritardo è calcolato dalla data."));
        page.Controls.Add(bar);
        return page;
    }

    private TabPage BuildScadenzeTab()
    {
        var page = Page("Scadenze");
        var bar = new Panel { Dock = DockStyle.Top, Height = 46 };
        _soloCritiche = new CheckBox { Text = "Solo critiche (30 giorni)", AutoSize = true, Location = new Point(8, 12), Checked = false };
        _soloCritiche.CheckedChanged += (_, _) => RefreshScadenze();
        bar.Controls.Add(_soloCritiche);
        var nuovo = Button("＋ Nuova scadenza", (_, _) => NewCertificazione());
        nuovo.Dock = DockStyle.Right;
        var refresh = Button("Aggiorna", (_, _) => LoadData());
        refresh.Dock = DockStyle.Right;
        refresh.Width = 100;
        bar.Controls.Add(nuovo);
        bar.Controls.Add(refresh);
        _scadenzeGrid = Grid();
        HideId(_scadenzeGrid);
        _scadenzeGrid.Columns.Add("Cantiere", "Cantiere");
        _scadenzeGrid.Columns.Add("Titolo", "Documento");
        _scadenzeGrid.Columns.Add("Tipo", "Tipo");
        _scadenzeGrid.Columns.Add("Intestatario", "Intestatario");
        _scadenzeGrid.Columns.Add("Scadenza", "Scadenza");
        _scadenzeGrid.Columns.Add("Stato", "Stato");
        _scadenzeGrid.Columns.Add("Giorni", "Giorni");
        _scadenzeGrid.DoubleClick += (_, _) => { if (ModifierKeys == Keys.Control) DeleteSelectedCertificazione(); else EditSelectedCertificazione(); };
        page.Controls.Add(_scadenzeGrid);
        page.Controls.Add(Hint("Doppio clic per modificare · Ctrl + doppio clic per eliminare."));
        page.Controls.Add(bar);
        return page;
    }

    private TabPage BuildCalendarioTab()
    {
        var page = Page("Calendario");
        _calendario = new MonthCalendar { Dock = DockStyle.Left, MaxSelectionCount = 1 };
        _calendario.DateChanged += (_, _) => RefreshCalendario();
        _calendarioLista = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), IntegralHeight = false, BackColor = Color.White };
        var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0) };
        right.Controls.Add(_calendarioLista);
        right.Controls.Add(Hint("Un tecnico con più interventi lo stesso giorno è segnalato con ⚠. Qui il calendario è solo interno: nessuna sincronizzazione con Google/Outlook."));
        page.Controls.Add(right);
        page.Controls.Add(_calendario);
        return page;
    }

    private TabPage BuildMezziDpiTab()
    {
        var page = Page("Mezzi e DPI");
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 260 };

        var topBar = new Panel { Dock = DockStyle.Top, Height = 42 };
        var nuovoMezzo = Button("＋ Nuovo mezzo/attrezzatura", (_, _) => NewMezzo());
        nuovoMezzo.Dock = DockStyle.Right;
        topBar.Controls.Add(nuovoMezzo);
        _mezziGrid = Grid();
        HideId(_mezziGrid);
        _mezziGrid.Columns.Add("Nome", "Nome/descrizione");
        _mezziGrid.Columns.Add("Tipo", "Tipo");
        _mezziGrid.Columns.Add("Squadra", "Squadra");
        _mezziGrid.DoubleClick += (_, _) => { if (ModifierKeys == Keys.Control) DeleteSelectedMezzo(); else EditSelectedMezzo(); };
        split.Panel1.Controls.Add(_mezziGrid);
        split.Panel1.Controls.Add(Hint("Mezzi e attrezzature · doppio clic per modificare, Ctrl + doppio clic per eliminare."));
        split.Panel1.Controls.Add(topBar);

        var bottomBar = new Panel { Dock = DockStyle.Top, Height = 42 };
        var nuovoDpi = Button("＋ Nuovo DPI", (_, _) => NewDpi());
        nuovoDpi.Dock = DockStyle.Right;
        bottomBar.Controls.Add(nuovoDpi);
        _dpiGrid = Grid();
        HideId(_dpiGrid);
        _dpiGrid.Columns.Add("Nome", "Dispositivo");
        _dpiGrid.Columns.Add("Squadra", "Squadra");
        _dpiGrid.Columns.Add("Scadenza", "Scadenza");
        _dpiGrid.Columns.Add("Stato", "Stato");
        _dpiGrid.Columns.Add("Giorni", "Giorni");
        _dpiGrid.DoubleClick += (_, _) => { if (ModifierKeys == Keys.Control) DeleteSelectedDpi(); else EditSelectedDpi(); };
        split.Panel2.Controls.Add(_dpiGrid);
        split.Panel2.Controls.Add(Hint("DPI assegnati alle squadre · doppio clic per modificare, Ctrl + doppio clic per eliminare."));
        split.Panel2.Controls.Add(bottomBar);

        page.Controls.Add(split);
        return page;
    }

    private TabPage BuildMagazzinoTab()
    {
        var page = Page("Magazzino");
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 240 };

        var topBar = new Panel { Dock = DockStyle.Top, Height = 42 };
        var nuovoMateriale = Button("＋ Nuovo materiale", (_, _) => NewMateriale());
        nuovoMateriale.Dock = DockStyle.Right;
        topBar.Controls.Add(nuovoMateriale);
        _materialiGrid = Grid();
        HideId(_materialiGrid);
        _materialiGrid.Columns.Add("Nome", "Materiale");
        _materialiGrid.Columns.Add("UM", "Unità");
        _materialiGrid.Columns.Add("Giacenza", "Giacenza");
        _materialiGrid.Columns.Add("ScortaMinima", "Scorta minima");
        _materialiGrid.Columns.Add("Stato", "Stato");
        _materialiGrid.DoubleClick += (_, _) => { if (ModifierKeys == Keys.Control) DeleteSelectedMateriale(); else EditSelectedMateriale(); };
        split.Panel1.Controls.Add(_materialiGrid);
        split.Panel1.Controls.Add(Hint("Anagrafica materiali · doppio clic per modificare, Ctrl + doppio clic per eliminare. Sotto scorta evidenziato in rosso."));
        split.Panel1.Controls.Add(topBar);

        var bottomBar = new Panel { Dock = DockStyle.Top, Height = 42 };
        var nuovoMovimento = Button("＋ Registra movimento", (_, _) => NewMovimento());
        nuovoMovimento.Dock = DockStyle.Right;
        bottomBar.Controls.Add(nuovoMovimento);
        _movimentiGrid = Grid();
        _movimentiGrid.Columns.Add("Data", "Data");
        _movimentiGrid.Columns.Add("Materiale", "Materiale");
        _movimentiGrid.Columns.Add("Quantita", "Quantità");
        _movimentiGrid.Columns.Add("Riferimento", "Riferimento");
        _movimentiGrid.Columns.Add("Cantiere", "Cantiere");
        split.Panel2.Controls.Add(_movimentiGrid);
        split.Panel2.Controls.Add(Hint("Storico movimenti (carichi da DDT e utilizzi su cantiere)."));
        split.Panel2.Controls.Add(bottomBar);

        page.Controls.Add(split);
        return page;
    }

    private static void HideId(DataGridView grid)
    {
        var idx = grid.Columns.Add("Id", "Id");
        grid.Columns[idx].Visible = false;
    }

    private static TabPage Page(string title) => new(title) { BackColor = Color.FromArgb(246, 247, 249), Padding = new Padding(16) };

    private static (Label, Panel) KpiCard(string caption)
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(5), Padding = new Padding(16, 10, 16, 10) };
        var value = new Label { Text = "—", Font = new Font("Segoe UI Semibold", 24), AutoSize = true, Location = new Point(16, 10) };
        p.Controls.Add(value);
        p.Controls.Add(new Label { Text = caption, ForeColor = Color.DimGray, AutoSize = true, Location = new Point(18, 55) });
        return (value, p);
    }

    private static Button Button(string text, EventHandler handler)
    {
        var b = new Button { Text = text, AutoSize = true, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(35, 40, 48), ForeColor = Color.White, Margin = new Padding(6, 0, 0, 0), Padding = new Padding(12, 0, 12, 0) };
        b.FlatAppearance.BorderSize = 0;
        b.Click += handler;
        return b;
    }

    private static TextBox SearchBox(string placeholder) => new() { Width = 420, Height = 32, Font = new Font("Segoe UI", 10), PlaceholderText = placeholder };
    private static Label Hint(string text) => new() { Text = text, Dock = DockStyle.Bottom, Height = 28, ForeColor = Color.Gray, Padding = new Padding(4, 6, 0, 0) };
    private static DataGridView Grid() => new() { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, MultiSelect = false, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal };

    private void LoadData()
    {
        var c = _service.Cantieri();
        var s = _service.Squadre();
        var i = _service.Interventi();
        var d = _service.Certificazioni();
        var dpi = _service.DpiList();
        var materiali = _service.Materiali();
        var k = _dashboard.CreaKpi(c, s, i, d, dpi, materiali);
        _kpiCantieri.Text = k.CantieriAttivi.ToString();
        _kpiSquadre.Text = $"{k.SquadreDisponibili}/{k.SquadreTotali}";
        _kpiOggi.Text = k.InterventiOggi.ToString();
        _kpiRitardi.Text = k.InterventiInRitardo.ToString();
        _kpiScadenze.Text = k.ScadenzeCritiche.ToString();
        _kpiMateriali.Text = k.MaterialiSottoScorta.ToString();
        RefreshCantieri(_searchCantieri?.Text ?? "");
        RefreshSquadre();
        RefreshOperai(_searchOperai?.Text ?? "");
        RefreshInterventi(_searchInterventi?.Text ?? "");
        RefreshScadenze();
        RefreshMezzi();
        RefreshDpi();
        RefreshMateriali();
        RefreshMovimenti();
        RefreshCalendario();
    }

    private void RefreshCantieri(string query)
    {
        if (_cantieriGrid == null) return;
        _cantieriGrid.Rows.Clear();
        var squadre = _service.Squadre().ToDictionary(x => x.Id);
        foreach (var c in _service.Cantieri().Where(c => string.IsNullOrWhiteSpace(query) || $"{c.Codice} {c.Nome} {c.Cliente} {c.Indirizzo}".Contains(query, StringComparison.OrdinalIgnoreCase)))
            _cantieriGrid.Rows.Add(c.Id, c.Codice, c.Nome, c.Cliente, c.Indirizzo, c.Stato, c.SquadraId.HasValue && squadre.TryGetValue(c.SquadraId.Value, out var s) ? s.Nome : "—");
    }

    private void RefreshSquadre()
    {
        if (_squadreGrid == null) return;
        _squadreGrid.Rows.Clear();
        foreach (var s in _service.Squadre())
            _squadreGrid.Rows.Add(s.Id, s.Nome, s.Caposquadra, s.Operatori.Count == 0 ? "—" : string.Join(", ", s.Operatori.Select(o => o.Nome)));
    }

    private void RefreshInterventi(string query)
    {
        if (_interventiGrid == null) return;
        _interventiGrid.Rows.Clear();
        var cantieri = _service.Cantieri().ToDictionary(x => x.Id);
        foreach (var i in _service.Interventi().Where(i =>
            string.IsNullOrWhiteSpace(query) ||
            $"{i.Descrizione} {i.TecnicoResponsabile} {i.Stato} {i.StatoVisuale}".Contains(query, StringComparison.OrdinalIgnoreCase)))
            _interventiGrid.Rows.Add(i.Id, i.CantiereId, cantieri.GetValueOrDefault(i.CantiereId)?.Nome ?? $"#{i.CantiereId}", i.Descrizione, i.DataProgrammata.ToString("dd/MM/yyyy"), i.Priorita, i.StatoVisuale, i.TecnicoResponsabile);
    }

    private void RefreshScadenze()
    {
        if (_scadenzeGrid == null) return;
        _scadenzeGrid.Rows.Clear();
        var cantieri = _service.Cantieri().ToDictionary(x => x.Id);
        var items = _service.Certificazioni();
        if (_soloCritiche?.Checked == true)
            items = new ScadenzeService().Critiche(items);
        foreach (var x in items)
            _scadenzeGrid.Rows.Add(x.Id, cantieri.GetValueOrDefault(x.CantiereId)?.Nome ?? $"#{x.CantiereId}", x.Titolo, x.Tipo, x.Intestatario, x.DataScadenza.ToString("dd/MM/yyyy"), x.GetStato(), x.GiorniAllaScadenza);
    }

    private void RefreshOperai(string query)
    {
        if (_operaiGrid == null) return;
        _operaiGrid.Rows.Clear();
        var squadre = _service.Squadre().ToDictionary(x => x.Id);
        foreach (var o in _service.Operatori().Where(o =>
            string.IsNullOrWhiteSpace(query) ||
            $"{o.Nome} {o.Ruolo} {(squadre.TryGetValue(o.SquadraId, out var sq) ? sq.Nome : "")}".Contains(query, StringComparison.OrdinalIgnoreCase)))
            _operaiGrid.Rows.Add(o.Id, o.Nome, o.Ruolo, o.Eta, o.Livello, squadre.TryGetValue(o.SquadraId, out var s) ? s.Nome : "—");
    }

    private void RefreshMezzi()
    {
        if (_mezziGrid == null) return;
        _mezziGrid.Rows.Clear();
        var squadre = _service.Squadre().ToDictionary(x => x.Id);
        foreach (var m in _service.Mezzi())
            _mezziGrid.Rows.Add(m.Id, m.Nome, m.Tipo, m.SquadraId.HasValue && squadre.TryGetValue(m.SquadraId.Value, out var s) ? s.Nome : "—");
    }

    private void RefreshDpi()
    {
        if (_dpiGrid == null) return;
        _dpiGrid.Rows.Clear();
        var squadre = _service.Squadre().ToDictionary(x => x.Id);
        foreach (var x in _service.DpiList())
            _dpiGrid.Rows.Add(x.Id, x.Nome, squadre.TryGetValue(x.SquadraId, out var s) ? s.Nome : "—", x.DataScadenza.ToString("dd/MM/yyyy"), x.GetStato(), x.GiorniAllaScadenza);
    }

    private void RefreshMateriali()
    {
        if (_materialiGrid == null) return;
        _materialiGrid.Rows.Clear();
        foreach (var m in _service.Materiali())
        {
            var idx = _materialiGrid.Rows.Add(m.Id, m.Nome, m.UnitaMisura, m.Giacenza, m.ScortaMinima, m.SottoScorta ? "Sotto scorta" : "Regolare");
            if (m.SottoScorta) _materialiGrid.Rows[idx].DefaultCellStyle.ForeColor = Color.Firebrick;
        }
    }

    private void RefreshMovimenti()
    {
        if (_movimentiGrid == null) return;
        _movimentiGrid.Rows.Clear();
        var materiali = _service.Materiali().ToDictionary(x => x.Id);
        var cantieri = _service.Cantieri().ToDictionary(x => x.Id);
        foreach (var mv in _service.Movimenti())
            _movimentiGrid.Rows.Add(mv.Data.ToString("dd/MM/yyyy"), materiali.GetValueOrDefault(mv.MaterialeId)?.Nome ?? $"#{mv.MaterialeId}", mv.Quantita, mv.Riferimento, mv.CantiereId.HasValue && cantieri.TryGetValue(mv.CantiereId.Value, out var c) ? c.Nome : "—");
    }

    private void RefreshCalendario()
    {
        if (_calendarioLista == null) return;
        _calendarioLista.Items.Clear();
        var giorno = _calendario.SelectionStart.Date;
        var cantieri = _service.Cantieri().ToDictionary(x => x.Id);
        var interventi = _service.Interventi().Where(i => i.DataProgrammata.Date == giorno).OrderBy(i => i.TecnicoResponsabile).ToList();
        if (interventi.Count == 0)
        {
            _calendarioLista.Items.Add("Nessun intervento programmato per questo giorno.");
            return;
        }
        var perTecnico = interventi.GroupBy(i => i.TecnicoResponsabile);
        foreach (var gruppo in perTecnico)
        {
            var doppia = gruppo.Count() > 1;
            foreach (var i in gruppo)
            {
                var cantiereNome = cantieri.GetValueOrDefault(i.CantiereId)?.Nome ?? $"#{i.CantiereId}";
                var prefisso = doppia ? "⚠ " : "";
                _calendarioLista.Items.Add($"{prefisso}{i.TecnicoResponsabile} · {cantiereNome} · {i.Descrizione} ({i.Priorita})");
            }
        }
    }

    private void NewCantiere() => SaveDialog(() =>
    {
        using var dlg = new CantiereForm(_service.Squadre());
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
            _service.SalvaCantiere(dlg.Result);
        else return false;
        return true;
    });

    private void EditSelectedCantiere()
    {
        var c = _service.Cantieri().FirstOrDefault(x => x.Id == SelectedId(_cantieriGrid));
        if (c == null) return;
        SaveDialog(() =>
        {
            using var dlg = new CantiereForm(_service.Squadre(), c);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                _service.SalvaCantiere(dlg.Result);
            else return false;
            return true;
        });
    }

    private void DeleteSelectedCantiere()
    {
        var c = _service.Cantieri().FirstOrDefault(x => x.Id == SelectedId(_cantieriGrid));
        if (c == null) return;
        if (MessageBox.Show($"Eliminare '{c.Nome}'? Interventi e scadenze collegati verranno eliminati.", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        SaveDialog(() => { _service.EliminaCantiere(c.Id); return true; });
    }

    private void NewSquadra() => SaveDialog(() =>
    {
        using var dlg = new SquadraForm();
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
            _service.SalvaSquadra(dlg.Result);
        else return false;
        return true;
    });

    private void EditSelectedSquadra()
    {
        var s = _service.Squadre().FirstOrDefault(x => x.Id == SelectedId(_squadreGrid));
        if (s == null) return;
        SaveDialog(() =>
        {
            using var dlg = new SquadraForm(s);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                _service.SalvaSquadra(dlg.Result);
            else return false;
            return true;
        });
    }

    private void DeleteSelectedSquadra()
    {
        var s = _service.Squadre().FirstOrDefault(x => x.Id == SelectedId(_squadreGrid));
        if (s == null) return;
        if (MessageBox.Show($"Eliminare la squadra '{s.Nome}'? Operai, DPI e mezzi assegnati verranno eliminati o rimarranno senza squadra.", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        SaveDialog(() => { _service.EliminaSquadra(s.Id); return true; });
    }

    private void NewOperatore()
    {
        var squadre = _service.Squadre();
        if (squadre.Count == 0)
        {
            MessageBox.Show("Crea prima almeno una squadra.", "Nessuna squadra", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        SaveDialog(() =>
        {
            using var dlg = new OperatoreForm(squadre);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                _service.SalvaOperatore(dlg.Result);
            else return false;
            return true;
        });
    }

    private void EditSelectedOperatore()
    {
        var o = _service.Operatori().FirstOrDefault(x => x.Id == SelectedId(_operaiGrid));
        if (o == null) return;
        SaveDialog(() =>
        {
            using var dlg = new OperatoreForm(_service.Squadre(), o);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                _service.SalvaOperatore(dlg.Result);
            else return false;
            return true;
        });
    }

    private void DeleteSelectedOperatore()
    {
        var o = _service.Operatori().FirstOrDefault(x => x.Id == SelectedId(_operaiGrid));
        if (o == null) return;
        if (MessageBox.Show($"Eliminare l'operaio '{o.Nome}'?", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        SaveDialog(() => { _service.EliminaOperatore(o.Id); return true; });
    }

    private void NewIntervento()
    {
        var cantieri = _service.Cantieri();
        if (cantieri.Count == 0)
        {
            MessageBox.Show("Crea prima almeno un cantiere.", "Nessun cantiere", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        SaveDialog(() =>
        {
            using var dlg = new InterventoForm(cantieri, _service.Squadre());
            if (dlg.ShowDialog(this) != DialogResult.OK || dlg.Result == null) return false;

            if (dlg.RicorrenzaGiorni == 0)
            {
                if (!ConfermaSeOccupato(dlg.Result.TecnicoResponsabile, dlg.Result.DataProgrammata)) return false;
                _service.SalvaIntervento(dlg.Result);
            }
            else
            {
                var serie = Guid.NewGuid();
                for (var n = 0; n < dlg.RicorrenzaOccorrenze; n++)
                {
                    var occorrenza = new Intervento
                    {
                        CantiereId = dlg.Result.CantiereId,
                        Descrizione = dlg.Result.Descrizione,
                        DataProgrammata = dlg.Result.DataProgrammata.AddDays(dlg.RicorrenzaGiorni * n),
                        Priorita = dlg.Result.Priorita,
                        Stato = StatoIntervento.Programmato,
                        TecnicoResponsabile = dlg.Result.TecnicoResponsabile,
                        SerieId = serie
                    };
                    _service.SalvaIntervento(occorrenza);
                }
            }
            return true;
        });
    }

    private bool ConfermaSeOccupato(string tecnico, DateTime data, int escludiId = 0)
    {
        if (string.IsNullOrWhiteSpace(tecnico)) return true;
        var occupato = _service.Interventi().Any(i => i.Id != escludiId && i.TecnicoResponsabile.Equals(tecnico, StringComparison.OrdinalIgnoreCase) && i.DataProgrammata.Date == data.Date);
        if (!occupato) return true;
        return MessageBox.Show($"{tecnico} ha già un intervento in programma il {data:dd/MM/yyyy}. Salvare comunque?", "Doppia prenotazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
    }

    private void EditSelectedIntervento()
    {
        var i = _service.Interventi().FirstOrDefault(x => x.Id == SelectedId(_interventiGrid));
        if (i == null) return;
        SaveDialog(() =>
        {
            using var dlg = new InterventoForm(_service.Cantieri(), _service.Squadre(), i);
            if (dlg.ShowDialog(this) != DialogResult.OK || dlg.Result == null) return false;
            if (!ConfermaSeOccupato(dlg.Result.TecnicoResponsabile, dlg.Result.DataProgrammata, i.Id)) return false;
            _service.SalvaIntervento(dlg.Result);
            return true;
        });
    }

    private void DeleteSelectedIntervento()
    {
        var i = _service.Interventi().FirstOrDefault(x => x.Id == SelectedId(_interventiGrid));
        if (i == null) return;
        if (MessageBox.Show($"Eliminare l'intervento '{i.Descrizione}'?", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        SaveDialog(() => { _service.EliminaIntervento(i.Id); return true; });
    }

    private void NewCertificazione()
    {
        var cantieri = _service.Cantieri();
        if (cantieri.Count == 0)
        {
            MessageBox.Show("Crea prima almeno un cantiere.", "Nessun cantiere", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        SaveDialog(() =>
        {
            using var dlg = new CertificazioneForm(cantieri);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                _service.SalvaCertificazione(dlg.Result);
            else return false;
            return true;
        });
    }

    private void EditSelectedCertificazione()
    {
        var x = _service.Certificazioni().FirstOrDefault(c => c.Id == SelectedId(_scadenzeGrid));
        if (x == null) return;
        SaveDialog(() =>
        {
            using var dlg = new CertificazioneForm(_service.Cantieri(), x);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                _service.SalvaCertificazione(dlg.Result);
            else return false;
            return true;
        });
    }

    private void DeleteSelectedCertificazione()
    {
        var x = _service.Certificazioni().FirstOrDefault(c => c.Id == SelectedId(_scadenzeGrid));
        if (x == null) return;
        if (MessageBox.Show($"Eliminare '{x.Titolo}'?", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        SaveDialog(() => { _service.EliminaCertificazione(x.Id); return true; });
    }

    private void NewMezzo() => SaveDialog(() =>
    {
        using var dlg = new MezzoForm(_service.Squadre());
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
            _service.SalvaMezzo(dlg.Result);
        else return false;
        return true;
    });

    private void EditSelectedMezzo()
    {
        var m = _service.Mezzi().FirstOrDefault(x => x.Id == SelectedId(_mezziGrid));
        if (m == null) return;
        SaveDialog(() =>
        {
            using var dlg = new MezzoForm(_service.Squadre(), m);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                _service.SalvaMezzo(dlg.Result);
            else return false;
            return true;
        });
    }

    private void DeleteSelectedMezzo()
    {
        var m = _service.Mezzi().FirstOrDefault(x => x.Id == SelectedId(_mezziGrid));
        if (m == null) return;
        if (MessageBox.Show($"Eliminare '{m.Nome}'?", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        SaveDialog(() => { _service.EliminaMezzo(m.Id); return true; });
    }

    private void NewDpi()
    {
        var squadre = _service.Squadre();
        if (squadre.Count == 0)
        {
            MessageBox.Show("Crea prima almeno una squadra.", "Nessuna squadra", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        SaveDialog(() =>
        {
            using var dlg = new DpiForm(squadre);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                _service.SalvaDpi(dlg.Result);
            else return false;
            return true;
        });
    }

    private void EditSelectedDpi()
    {
        var d = _service.DpiList().FirstOrDefault(x => x.Id == SelectedId(_dpiGrid));
        if (d == null) return;
        SaveDialog(() =>
        {
            using var dlg = new DpiForm(_service.Squadre(), d);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                _service.SalvaDpi(dlg.Result);
            else return false;
            return true;
        });
    }

    private void DeleteSelectedDpi()
    {
        var d = _service.DpiList().FirstOrDefault(x => x.Id == SelectedId(_dpiGrid));
        if (d == null) return;
        if (MessageBox.Show($"Eliminare '{d.Nome}'?", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        SaveDialog(() => { _service.EliminaDpi(d.Id); return true; });
    }

    private void NewMateriale() => SaveDialog(() =>
    {
        using var dlg = new MaterialeForm();
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
            _service.SalvaMateriale(dlg.Result);
        else return false;
        return true;
    });

    private void EditSelectedMateriale()
    {
        var m = _service.Materiali().FirstOrDefault(x => x.Id == SelectedId(_materialiGrid));
        if (m == null) return;
        SaveDialog(() =>
        {
            using var dlg = new MaterialeForm(m);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                _service.SalvaMateriale(dlg.Result);
            else return false;
            return true;
        });
    }

    private void DeleteSelectedMateriale()
    {
        var m = _service.Materiali().FirstOrDefault(x => x.Id == SelectedId(_materialiGrid));
        if (m == null) return;
        if (MessageBox.Show($"Eliminare '{m.Nome}'? Anche lo storico movimenti verrà eliminato.", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        SaveDialog(() => { _service.EliminaMateriale(m.Id); return true; });
    }

    private void NewMovimento()
    {
        var materiali = _service.Materiali();
        if (materiali.Count == 0)
        {
            MessageBox.Show("Crea prima almeno un materiale in anagrafica.", "Nessun materiale", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        SaveDialog(() =>
        {
            using var dlg = new MovimentoForm(materiali, _service.Cantieri());
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                _service.RegistraMovimento(dlg.Result);
            else return false;
            return true;
        });
    }

    private void SaveDialog(Func<bool> action)
    {
        try
        {
            if (action()) LoadData();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private static int SelectedId(DataGridView grid) =>
        grid.CurrentRow?.Cells["Id"].Value is int id ? id : Convert.ToInt32(grid.CurrentRow?.Cells["Id"].Value ?? 0);

    private static void ShowError(Exception ex) =>
        MessageBox.Show($"Operazione non riuscita.\n\n{ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
}

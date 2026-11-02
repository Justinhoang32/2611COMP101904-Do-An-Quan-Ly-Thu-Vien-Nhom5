using QuanLyThuVien.Demo;
using QuanLyThuVien.GiaoDien;

namespace QuanLyThuVien.Forms;

public partial class frmDanhSachPhieuMuon : LibraryPage
{
    private TextBox query = null!; private DateTimePicker from = null!, to = null!;
    private DataGridView grid = null!; private Pager pager = null!; private int state; private List<Button> tabs = [];
    public frmDanhSachPhieuMuon() : this(new LibraryDemo()) { }
    public frmDanhSachPhieuMuon(LibraryDemo store)
    {
        InitializeComponent();
        Setup(store, "Phiếu mượn");
        Header("Phiếu mượn", "Theo dõi mượn – trả, thời hạn và lịch sử luân chuyển sách.", Ui.Button("Tạo phiếu mượn", () => Go(LibraryScreen.CreateLoan), true, "plus"));
        var overdue = store.Overdue(store.Today).ToList();
        var previousMonth = store.Today.AddMonths(-1);
        Add(Ui.Columns([25, 25, 25, 25],
            Ui.Metric("Đang mượn đúng hạn", store.Loans.Count(x => x.Remaining > 0 && x.Due >= store.Today).ToString(), "Sách đang lưu hành"),
            Ui.Metric("Phiếu quá hạn", overdue.Count.ToString(), $"{overdue.Sum(x => x.Remaining)} cuốn cần nhắc trả", true),
            Ui.Metric("Đã trả trong tháng " + previousMonth.Month, store.Loans.Count(x => x.Remaining == 0 && x.Lines.Any(l => l.Returned?.Month == previousMonth.Month)).ToString(), "Phiếu đã hoàn tất"),
            Ui.Metric("Đến hạn hôm nay", store.Loans.Count(x => x.Remaining > 0 && x.Due == store.Today).ToString(), "Cần tiếp nhận tại quầy")));
        var nav = Ui.Actions();
        string[] labels = ["Tất cả", "Đang mượn", "Đã trả", "Quá hạn"];
        for (int i = 0; i < labels.Length; i++)
        {
            int index = i;
            var b = Ui.Button(labels[i], () => { state = index; RefreshLoans(true); });
            tabs.Add(b);
            nav.Controls.Add(b);
        }
        query = Ui.Input("Mã phiếu, mã thẻ, họ tên...");
        from = Ui.Date(new(2026, 10, 1));
        to = Ui.Date(store.Today);
        grid = Ui.Grid(555);
        Ui.Column(grid, "code", "MÃ PHIẾU", 110);
        Ui.Column(grid, "reader", "ĐỘC GIẢ", 150);
        Ui.Column(grid, "borrow", "NGÀY MƯỢN", 95);
        Ui.Column(grid, "due", "HẸN TRẢ", 95);
        Ui.Column(grid, "count", "CUỐN / ĐÃ TRẢ", 85);
        Ui.Column(grid, "status", "TRẠNG THÁI", 95);
        Ui.Column(grid, "return", "", 75, true);
        Ui.Column(grid, "print", "", 45, true);
        pager = new Pager(7, _ => RefreshLoans());
        grid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            var loan = (DemoLoan)grid.Rows[e.RowIndex].Tag!;
            if (grid.Columns[e.ColumnIndex].Name == "print")
                Go(LibraryScreen.Print, loan);
            if (grid.Columns[e.ColumnIndex].Name == "return")
                Go(loan.Remaining > 0 ? LibraryScreen.Returns : LibraryScreen.Print, loan);
        };
        var apply = Ui.Button("Áp dụng", () => RefreshLoans(true));
        AcceptButton = apply;
        Add(Ui.Card("Tất cả phiếu mượn", "", Ui.Actions(Ui.Button("Xuất danh sách", () => Ui.Export("phieu-muon.xlsx", ["Mã phiếu", "Độc giả", "Ngày mượn", "Hẹn trả", "Số cuốn", "Đã trả", "Trạng thái"], Filtered().Select(x => new[] { x.Code, x.Reader.Name, x.Borrowed.ToString("dd/MM/yyyy"), x.Due.ToString("dd/MM/yyyy"), x.Lines.Count.ToString(), (x.Lines.Count - x.Remaining).ToString(), x.Status(Store.Today) })), icon: "download")),
            nav, Ui.Columns([55, 17, 17, 11], Ui.Field("Tìm phiếu hoặc độc giả", query), Ui.Field("Từ ngày", from), Ui.Field("Đến ngày", to), Ui.Actions(apply)), grid, pager.Control));
        Add(Ui.Notice("Một phiếu có thể được trả nhiều lần. Phiếu chuyển sang Đã trả khi không còn cuốn đang mượn."));
        RefreshLoans(true);
        Footer();
    }
    private List<DemoLoan> Filtered() => Store.Loans.Where(x => x.Borrowed >= from.Value.Date && x.Borrowed <= to.Value.Date && (LibraryDemo.Matches(x.Code, query.Text) || LibraryDemo.Matches(x.Reader.Code, query.Text) || LibraryDemo.Matches(x.Reader.Name, query.Text)) && (state == 0 || state == 1 && x.Remaining > 0 || state == 2 && x.Remaining == 0 || state == 3 && x.Remaining > 0 && x.Due < Store.Today)).OrderByDescending(x => x.Borrowed).ToList();
    private void RefreshLoans(bool reset = false)
    {
        if (from.Value.Date > to.Value.Date)
        {
            Ui.Error("Từ ngày không được sau đến ngày.");
            return;
        }
        var list = Filtered();
        pager.Update(list.Count, reset);
        grid.Rows.Clear();
        foreach (var l in list.Skip(pager.Skip).Take(7))
        {
            int i = grid.Rows.Add(l.Code, l.Reader.Name + "\n" + l.Reader.Code + " · " + l.Reader.Group, l.Borrowed.ToString("dd/MM/yyyy"), l.Due.ToString("dd/MM/yyyy"), $"{l.Lines.Count} / {l.Lines.Count - l.Remaining}", l.Status(Store.Today), l.Remaining > 0 ? "Trả sách" : "Chi tiết", "In");
            grid.Rows[i].Tag = l;
            Ui.Tone(grid.Rows[i].Cells["status"], l.Status(Store.Today));
        }
        for (int i = 0; i < tabs.Count; i++)
            tabs[i].BackColor = i == state ? Ui.Blue : Color.White;
    }

    private void frmDanhSachPhieuMuon_Load(object sender, EventArgs e)
    {

    }
}

using QuanLyThuVien.Demo;
using QuanLyThuVien.GiaoDien;

namespace QuanLyThuVien.Forms;

public partial class frmBaoCaoQuaHan : LibraryPage
{
    private DateTimePicker date = null!, month = null!; private ComboBox group = null!, severity = null!; private TextBox query = null!;
    private DataGridView grid = null!; private Pager pager = null!; private Label count = null!, monthly = null!;
    private Panel metrics = null!; private BarChart chart = null!; private List<DemoLoan> filtered = [];
    private DateTime asOf;
    public frmBaoCaoQuaHan() : this(new LibraryDemo()) { }
    public frmBaoCaoQuaHan(LibraryDemo store)
    {
        InitializeComponent();
        Setup(store, "Báo cáo quá hạn");
        Header("Báo cáo quá hạn", "Danh sách sách chưa trả sau ngày hẹn.",
            Ui.Button("Xuất Excel", Export, icon: "download"), Ui.Button("In danh sách quá hạn", Print, true, "printer"));
        metrics = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        Add(metrics);
        date = Ui.Date(store.Today);
        group = Ui.Select("Tất cả nhóm", "Sinh viên", "Giảng viên", "Cán bộ");
        severity = Ui.Select("Tất cả số ngày trễ", "1–3 ngày", "4–7 ngày", "8–14 ngày", "Trên 14 ngày");
        query = Ui.Input("Mã phiếu, họ tên...");
        count = Ui.Label("", true, Ui.Red, 9);
        grid = Ui.Grid(510);
        Ui.Column(grid, "code", "PHIẾU / NGÀY MƯỢN", 110);
        Ui.Column(grid, "reader", "ĐỘC GIẢ / LIÊN HỆ", 150);
        Ui.Column(grid, "books", "SÁCH CHƯA TRẢ", 210);
        Ui.Column(grid, "due", "HẸN TRẢ", 85);
        Ui.Column(grid, "days", "SỐ NGÀY TRỄ", 75);
        Ui.Column(grid, "quantity", "SỐ CUỐN", 60);
        Ui.Column(grid, "view", "THAO TÁC", 75, true);
        grid.RowTemplate.Height = 76;
        pager = new Pager(6, _ => FillRows());
        grid.CellContentClick += (_, e) => { if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "view") Go(LibraryScreen.Print, grid.Rows[e.RowIndex].Tag); };
        var filter = Ui.Button("Lọc báo cáo", () => Apply(true));
        AcceptButton = filter;
        Add(Ui.Card("Danh sách cần nhắc trả", "", count,
            Ui.Columns([17, 17, 20, 33, 13], Ui.Field("Ngày đối chiếu", date), Ui.Field("Nhóm độc giả", group), Ui.Field("Mức độ quá hạn", severity), Ui.Field("Tìm độc giả / phiếu", query), Ui.Actions(filter)), grid, pager.Control));
        month = Ui.Date(new(2026, 10, 1));
        month.CustomFormat = "MM/yyyy";
        month.ShowUpDown = true;
        monthly = Ui.Label("", true, Ui.Navy, 13);
        chart = new BarChart { Height = 150 };
        Add(Ui.Card("Thống kê theo tháng", "Theo dõi hiệu quả nhắc trả và xu hướng phát sinh quá hạn.",
            Ui.Columns([70, 30], monthly, Ui.Field("Tháng thống kê", month)), chart));
        month.ValueChanged += (_, _) => UpdateMonthly();
        Apply(true);
        Footer();
    }
    private void Apply(bool reset)
    {
        asOf = date.Value.Date;
        (int min, int max) = severity.SelectedIndex switch
        {
            1 => (1, 3),
            2 => (4, 7),
            3 => (8, 14),
            4 => (15, int.MaxValue),
            _ => (0, int.MaxValue)
        };
        filtered = Store.Overdue(asOf.Date, group.Text, min, max, query.Text).ToList();
        pager.Update(filtered.Count, reset);
        count.Text = $"{filtered.Count} phiếu quá hạn · Đối chiếu ngày {asOf:dd/MM/yyyy}";
        while (metrics.Controls.Count > 0)
            metrics.Controls[0].Dispose();
        metrics.Controls.Add(Ui.Columns([25, 25, 25, 25],
            Ui.Metric("Phiếu chưa hoàn tất", filtered.Count.ToString(), $"Tính đến ngày {asOf:dd/MM/yyyy}", true),
            Ui.Metric("Sách chưa thu hồi", filtered.Sum(x => x.RemainingAt(asOf)).ToString(), $"Thuộc {filtered.Count} phiếu quá hạn"),
            Ui.Metric("Độc giả cần liên hệ", filtered.Select(x => x.Reader.Code).Distinct().Count().ToString(), "Sinh viên · Giảng viên · Cán bộ"),
            Ui.Metric("Trễ nhiều nhất", (filtered.Count == 0 ? 0 : filtered.Max(x => x.LateDays(asOf))) + " ngày", "Ưu tiên nhắc trả trong hôm nay", true)));
        FillRows();
        UpdateMonthly();
    }
    private void FillRows()
    {
        grid.Rows.Clear();
        foreach (var l in filtered.Skip(pager.Skip).Take(6))
        {
            var books = l.Lines.Where(x => x.Returned == null || x.Returned.Value.Date > asOf.Date).GroupBy(x => x.Book.Code)
                .Select(g => g.Key + " · " + g.First().Book.Title + (g.Count() > 1 ? $" ({g.Count()})" : ""));
            int i = grid.Rows.Add(l.Code + "\nMượn: " + l.Borrowed.ToString("dd/MM/yyyy"), l.Reader.Name + "\n" + l.Reader.Code + " · " + l.Reader.Phone, string.Join("\n", books), l.Due.ToString("dd/MM/yyyy"), l.LateDays(asOf) + " ngày", l.RemainingAt(asOf), "Chi tiết →");
            grid.Rows[i].Tag = l;
            grid.Rows[i].Cells["days"].Style.ForeColor = Ui.Red;
            grid.Rows[i].Cells["books"].Style.Font = Ui.SmallFont;
        }
    }
    private void UpdateMonthly()
    {
        var selected = new DateTime(month.Value.Year, month.Value.Month, 1);
        var occurred = Store.Loans.Where(x => x.Due.AddDays(1).Year == selected.Year && x.Due.AddDays(1).Month == selected.Month && x.RemainingAt(x.Due) > 0 && x.Due < asOf.Date).ToList();
        int completed = occurred.Count(x => x.RemainingAt(asOf) == 0);
        monthly.Text = $"{occurred.Count}  Phiếu phát sinh quá hạn     {completed}  Đã thu hồi đủ     {occurred.Count - completed}  Cần theo dõi";
        var months = Enumerable.Range(-5, 6).Select(i => selected.AddMonths(i)).ToArray();
        chart.Labels = months.Select(x => "T" + x.Month).ToArray();
        chart.Values = months.Select(m => Store.Loans.Count(x => x.Due.AddDays(1).Year == m.Year && x.Due.AddDays(1).Month == m.Month && x.RemainingAt(x.Due) > 0 && x.Due < asOf.Date)).ToArray();
        chart.Invalidate();
    }
    private string[][] Rows() => filtered.Select(l => new[] { l.Code, l.Reader.Name, l.Reader.Code, l.Reader.Phone, string.Join("; ", l.Lines.Where(x => x.Returned == null || x.Returned.Value.Date > asOf.Date).Select(x => x.Book.Title)), l.Due.ToString("dd/MM/yyyy"), l.LateDays(asOf).ToString(), l.RemainingAt(asOf).ToString() }).ToArray();
    private void Export() => Ui.Export("bao-cao-qua-han.xlsx", ["Mã phiếu", "Độc giả", "Mã thẻ", "Điện thoại", "Sách chưa trả", "Hẹn trả", "Số ngày trễ", "Số cuốn"], Rows());
    private void Print() => PrintSupport.PrintText("Báo cáo sách quá hạn", $"Ngày đối chiếu: {asOf:dd/MM/yyyy}\nSố phiếu: {filtered.Count}\n\n" + string.Join("\n\n", Rows().Select(r => $"{r[0]} · {r[1]} · {r[2]}\nLiên hệ: {r[3]}\n{r[4]}\nHẹn trả: {r[5]} · Trễ {r[6]} ngày · {r[7]} cuốn")));

    private void frmBaoCaoQuaHan_Load(object sender, EventArgs e)
    {

    }
}

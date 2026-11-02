using QuanLyThuVien.Demo;
using QuanLyThuVien.GiaoDien;

namespace QuanLyThuVien.Forms;

public partial class frmTongQuan : LibraryPage
{
    public frmTongQuan(LibraryDemo store)
    {
        Setup(store, "Tổng quan");
        Header("Tổng quan thư viện", $"Chào {store.Session.Name}. Cùng bắt đầu một ngày làm việc hiệu quả.", Ui.Button("Lập phiếu mượn", () => Go(LibraryScreen.CreateLoan), true, "plus"));
        var overdue = store.Overdue(store.Today).ToList();
        Add(Ui.Columns([25, 25, 25, 25],
            Ui.Metric("Đầu sách trong thư viện", store.Books.Count.ToString(), $"{store.Books.Sum(x => x.Total)} bản sách · {store.Categories.Count} thể loại"),
            Ui.Metric("Độc giả đang hoạt động", store.Readers.Count(x => x.Expires >= store.Today).ToString(), "Hồ sơ độc giả trong dữ liệu mẫu"),
            Ui.Metric("Phiếu đang mượn", store.Loans.Count(x => x.Remaining > 0).ToString(), $"{store.Loans.Sum(x => x.Remaining)} cuốn đang lưu hành"),
            Ui.Metric("Phiếu quá hạn", overdue.Count.ToString(), $"{overdue.Sum(x => x.Remaining)} cuốn cần liên hệ thu hồi", true)));
        var months = Enumerable.Range(1, 10).ToArray();
        var values = months.Select(m => store.Loans.Where(x => x.Borrowed.Year == 2026 && x.Borrowed.Month == m).Sum(x => x.Lines.Count)).ToArray();
        var work = Ui.Card("Việc cần xử lý", "",
            Ui.Heading(overdue.Count + "  Phiếu mượn quá hạn", 12), Ui.Label("Liên hệ độc giả và nhắc trả sách", color: Ui.Muted, size: 9), Ui.Button("Báo cáo quá hạn", () => Go(LibraryScreen.Overdue), icon: "arrow-right"),
            new Panel { Height = 20 }, Ui.Heading(store.Readers.Count(x => x.Expires >= store.Today && x.Expires <= store.Today.AddDays(7)) + "  Thẻ độc giả sắp hết hạn", 12),
            Ui.Button("Xem độc giả", () => Go(LibraryScreen.Readers), icon: "arrow-right"));
        Add(Ui.Columns([66, 34], Ui.Card("Lượt mượn theo tháng", "Tổng lượt sách được mượn trong năm 2026",
            Ui.Heading(values.Sum().ToString(), 22), new BarChart { Labels = months.Select(x => "T" + x).ToArray(), Values = values }), work));
        var grid = Ui.Grid(330);
        Ui.Column(grid, "code", "MÃ PHIẾU", 110);
        Ui.Column(grid, "reader", "ĐỘC GIẢ", 145);
        Ui.Column(grid, "borrow", "NGÀY MƯỢN");
        Ui.Column(grid, "due", "HẸN TRẢ");
        Ui.Column(grid, "count", "SỐ CUỐN", 70);
        Ui.Column(grid, "status", "TRẠNG THÁI", 100);
        Ui.Column(grid, "open", "THAO TÁC", 80, true);
        foreach (var l in store.Loans.OrderByDescending(x => x.Borrowed).Take(4))
        {
            int i = grid.Rows.Add(l.Code, l.Reader.Name, l.Borrowed.ToString("dd/MM/yyyy"), l.Due.ToString("dd/MM/yyyy"), l.Lines.Count, l.Status(store.Today), "Chi tiết →");
            grid.Rows[i].Tag = l;
            Ui.Tone(grid.Rows[i].Cells["status"], l.Status(store.Today));
        }
        grid.CellContentClick += (_, e) => { if (e.RowIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "open") Go(LibraryScreen.Print, grid.Rows[e.RowIndex].Tag); };
        Add(Ui.Card("Phiếu mượn gần đây", "Theo dõi giao dịch mới nhất tại quầy thư viện.", Ui.Actions(Ui.Button("Tất cả phiếu mượn", () => Go(LibraryScreen.Loans), icon: "arrow-right")), grid));
        Footer();
    }

    
    

    private void frmTongQuan_Load(object sender, EventArgs e)
    {

    }
}

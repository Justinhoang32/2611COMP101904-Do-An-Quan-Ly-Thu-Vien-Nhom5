using QuanLyThuVien.Demo;
using QuanLyThuVien.GiaoDien;

namespace QuanLyThuVien.Forms;

public partial class frmDocGia : LibraryPage
{
    private TextBox query = null!, card = null!; private ComboBox group = null!, editGroup = null!;
    private DataGridView grid = null!; private Pager pager = null!; private Label profile = null!, cardPreview = null!;
    private DateTimePicker issued = null!, expires = null!; private DemoReader editing = null!;
    public frmDocGia() : this(new LibraryDemo()) { }
    public frmDocGia(LibraryDemo store)
    {
        InitializeComponent();
        Setup(store, "Độc giả");
        Header("Độc giả", "Quản lý hồ sơ, nhóm độc giả và thời hạn sử dụng thẻ thư viện.", Ui.Button("Thêm độc giả", AddReader, true, "user-round"));
        Add(Ui.Columns([25, 25, 25, 25], Ui.Metric("Đang hoạt động", store.Readers.Count(x => x.Expires >= store.Today).ToString(), "Thẻ còn hiệu lực"),
            Ui.Metric("Sinh viên", store.Readers.Count(x => x.Group == "Sinh viên").ToString(), "Nhóm độc giả"),
            Ui.Metric("Giảng viên / cán bộ", store.Readers.Count(x => x.Group != "Sinh viên").ToString(), "Nhóm độc giả"),
            Ui.Metric("Thẻ sắp hết hạn", store.Readers.Count(x => x.Expires >= store.Today && x.Expires <= store.Today.AddDays(7)).ToString(), "Trong vòng 7 ngày")));
        query = Ui.Input("Mã thẻ, họ tên, mã sinh viên...");
        group = Ui.Select("Tất cả nhóm", "Sinh viên", "Giảng viên", "Cán bộ");
        grid = Ui.Grid(470);
        Ui.Column(grid, "name", "ĐỘC GIẢ", 180);
        Ui.Column(grid, "group", "NHÓM", 90);
        Ui.Column(grid, "expires", "HẠN THẺ", 100);
        Ui.Column(grid, "state", "TRẠNG THÁI", 110);
        Ui.Column(grid, "view", "", 60, true);
        pager = new Pager(6, _ => RefreshReaders());
        query.TextChanged += (_, _) => RefreshReaders(true);
        group.SelectedIndexChanged += (_, _) => RefreshReaders(true);
        grid.CellContentClick += (_, e) => { if (e.RowIndex >= 0) LoadReader((DemoReader)grid.Rows[e.RowIndex].Tag!); };
        profile = Ui.Label("");
        profile.MaximumSize = new Size(320, 0);
        cardPreview = Ui.Label("", true, Ui.Navy, 10);
        card = Ui.Input();
        card.ReadOnly = true;
        editGroup = Ui.Select("Sinh viên", "Giảng viên", "Cán bộ");
        issued = Ui.Date(store.Today);
        expires = Ui.Date(new(2027, 9, 30));
        var edit = Ui.Card("Hồ sơ & cấp thẻ độc giả", "", profile,
            Ui.Label("Cấp / gia hạn thẻ", true), Ui.Columns([50, 50], Ui.Field("Mã thẻ *", card), Ui.Field("Nhóm *", editGroup)),
            Ui.Columns([50, 50], Ui.Field("Ngày cấp *", issued), Ui.Field("Hạn thẻ *", expires)), Ui.Notice("THƯ VIỆN HCMUE · THẺ ĐỘC GIẢ"), cardPreview,
            Ui.Actions(Ui.Button("In thẻ", () => PrintSupport.PrintText("Thẻ độc giả", cardPreview.Text), icon: "printer"), Ui.Button("Lưu & cấp thẻ", Save, true, "check")));
        var list = Ui.Card("Danh sách độc giả", "", Ui.Actions(Ui.Button("Xuất danh sách", () => Ui.Export("doc-gia.xlsx", ["Mã thẻ", "Họ tên", "Nhóm", "Hạn thẻ", "Điện thoại"], Filtered().Select(x => new[] { x.Code, x.Name, x.Group, x.Expires.ToString("dd/MM/yyyy"), x.Phone })), icon: "download")), Ui.Columns([70, 30], Ui.Field("Tìm độc giả", query), Ui.Field("Nhóm", group)), grid, pager.Control);
        Add(Ui.Columns([65, 35], list, edit));
        RefreshReaders(true);
        LoadReader(store.Readers[0]);
        Footer();
    }
    private List<DemoReader> Filtered() => Store.Readers.Where(x => (LibraryDemo.Matches(x.Name, query.Text) || LibraryDemo.Matches(x.Code, query.Text) || LibraryDemo.Matches(x.StudentId, query.Text)) && (group.SelectedIndex == 0 || x.Group == group.Text)).ToList();
    private void RefreshReaders(bool reset = false)
    {
        var list = Filtered();
        pager.Update(list.Count, reset);
        grid.Rows.Clear();
        foreach (var r in list.Skip(pager.Skip).Take(6))
        {
            string status = r.Expires < Store.Today ? "Hết hạn" : Store.Overdue(Store.Today).Any(x => x.Reader.Code == r.Code) ? "Có quá hạn" : r.Expires <= Store.Today.AddDays(7) ? "Sắp hết hạn" : "Hoạt động";
            int i = grid.Rows.Add(r.Name + "\n" + r.Code + " · " + r.StudentId, r.Group, r.Expires.ToString("dd/MM/yyyy"), status, "Xem");
            grid.Rows[i].Tag = r;
            Ui.Tone(grid.Rows[i].Cells["state"], status);
        }
    }
    private void LoadReader(DemoReader r)
    {
        editing = r;
        profile.Text = $"{r.Name}\n\nKhoa: {r.Faculty}\nMã sinh viên: {r.StudentId}\nLiên hệ: {r.Phone}\n{r.Email}\n\nĐang mượn: {Store.Loans.Where(x => x.Reader == r).Sum(x => x.Remaining)} / 5 cuốn\n";
        card.Text = r.Code;
        editGroup.SelectedItem = r.Group;
        issued.Value = r.Issued;
        expires.Value = r.Expires;
        cardPreview.Text = $"{r.Name.ToUpperInvariant()}\n{r.Code} · {r.Group}\nHạn thẻ: {r.Expires:dd/MM/yyyy}";
    }
    private void Save()
    {
        if (expires.Value.Date < issued.Value.Date)
        {
            Ui.Error("Hạn thẻ phải từ ngày cấp trở đi.");
            return;
        }
        editing.Group = editGroup.Text;
        editing.Issued = issued.Value.Date;
        editing.Expires = expires.Value.Date;
        LoadReader(editing);
        RefreshReaders();
        Ui.Info("Đã cập nhật thẻ trong dữ liệu mẫu.");
    }
    private void AddReader()
    {
        var values = Ui.Ask("Thêm độc giả", ("Họ và tên *", ""), ("Mã sinh viên / cán bộ *", ""), ("Khoa", ""), ("Điện thoại", ""), ("Email", ""));
        if (values == null)
            return;
        if (values[0] == "" || values[1] == "")
        {
            Ui.Error("Họ tên và mã sinh viên không được để trống.");
            return;
        }
        if (Store.Readers.Any(x => x.StudentId.Equals(values[1], StringComparison.OrdinalIgnoreCase)))
        {
            Ui.Error("Mã sinh viên / cán bộ đã tồn tại.");
            return;
        }
        var reader = new DemoReader { Code = "DG" + (Store.Readers.Count + 1).ToString("D4"), Name = values[0], StudentId = values[1], Faculty = values[2], Phone = values[3], Email = values[4], Issued = Store.Today };
        Store.Readers.Add(reader);
        RefreshReaders(true);
        LoadReader(reader);
    }

    private void frmDocGia_Load(object sender, EventArgs e)
    {

    }
}

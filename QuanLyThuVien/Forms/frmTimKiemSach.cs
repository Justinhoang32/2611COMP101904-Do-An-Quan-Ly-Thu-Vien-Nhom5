using QuanLyThuVien.Demo;
using QuanLyThuVien.GiaoDien;

namespace QuanLyThuVien.Forms;

public partial class frmTimKiemSach : LibraryPage
{
    private TextBox code = null!, title = null!, author = null!;
    private ComboBox category = null!, state = null!;
    private CheckBox available = null!;
    private DataGridView grid = null!;
    private Pager pager = null!;
    private Label result = null!;
    private readonly bool readOnly;
    public frmTimKiemSach() : this(new LibraryDemo()) { }
    public frmTimKiemSach(LibraryDemo store, bool readOnly = false)
    {
        this.readOnly = readOnly;
        InitializeComponent();
        Setup(store, "Tìm kiếm sách");
        BuildView();
        RefreshResults(true);
    }
    private void BuildView()
    {
        Header("Tìm kiếm sách", "Tra cứu học liệu theo mã, tên sách, tác giả và tình trạng có sẵn.", Ui.Button(readOnly ? "Về đăng nhập" : "Về danh mục sách", () => { if (readOnly) Parent?.FindForm()?.Close(); else Go(LibraryScreen.Books); }, icon: "arrow-left"));
        code = Ui.Input("Nhập mã sách, ví dụ S001");
        title = Ui.Input("Nhập toàn bộ hoặc một phần tên sách");
        author = Ui.Input("Nhập tên tác giả");
        category = Ui.Select(new[] { "Tất cả thể loại" }.Concat(Store.Categories.Select(c => c.Name)).ToArray());
        state = Ui.Select("Tất cả tình trạng", "Được phép cho mượn", "Không được phép cho mượn");
        state.SelectedIndex = 1;
        available = new CheckBox { Text = "Chỉ hiển thị sách có sẵn (còn ít nhất 1 bản)", Checked = true, AutoSize = true };
        var search = Ui.Button("Tìm kiếm", () => RefreshResults(true), true, "search");
        AcceptButton = search;
        Add(Ui.Card("Điều kiện tìm kiếm", "Có thể kết hợp nhiều tiêu chí. Để trống trường không cần lọc.",
            Ui.Columns([23, 49, 28], Ui.Field("Mã sách", code), Ui.Field("Tên sách", title), Ui.Field("Tác giả", author)),
            Ui.Columns([30, 28, 42], Ui.Field("Thể loại", category), Ui.Field("Tình trạng lưu hành", state), Ui.Actions(Ui.Button("Xóa bộ lọc", Clear), search)),
            available));
        grid = BookTable.Create(true);
        pager = new Pager(5, _ => RefreshResults());
        result = Ui.Label("", color: Ui.Muted, size: 9);
        grid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            var b = (DemoBook)grid.Rows[e.RowIndex].Tag!;
            if (grid.Columns[e.ColumnIndex].Name == "action")
                Ui.Info($"{b.Title}\nMã: {b.Code}\nTác giả: {b.Author}\nThể loại: {b.Category}\nVị trí: {b.Location}\nCòn: {b.Available}/{b.Total}");
            if (grid.Columns[e.ColumnIndex].Name == "add" && b.Available > 0 && b.Borrowable)
            {
                if (readOnly)
                {
                    Ui.Info("Vui lòng đăng nhập để lập phiếu mượn.");
                    return;
                }
                int next = Store.Draft.GetValueOrDefault(b.Code) + 1;
                if (next > b.Available)
                {
                    Ui.Error("Số bản đang chọn đã bằng số bản còn sẵn.");
                    return;
                }
                Store.Draft[b.Code] = next;
                Go(LibraryScreen.CreateLoan);
            }
        };
        Add(Ui.Card("Kết quả tìm kiếm", "", result, grid, pager.Control));
        Footer();
    }
    private void RefreshResults(bool reset = false)
    {
        var books = Store.Search(code.Text, title.Text, author.Text, category.Text, state.SelectedIndex, available.Checked).ToList();
        pager.Update(books.Count, reset);
        BookTable.Fill(grid, books.Skip(pager.Skip).Take(5), true);
        result.Text = books.Count == 0 ? "Không tìm thấy sách phù hợp." : $"Tìm thấy {books.Count} đầu sách phù hợp · Sắp xếp theo mã sách";
    }
    private void Clear()
    {
        code.Clear();
        title.Clear();
        author.Clear();
        category.SelectedIndex = 0;
        state.SelectedIndex = 0;
        available.Checked = false;
        RefreshResults(true);
    }

    private void frmTimKiemSach_Load(object sender, EventArgs e)
    {

    }
}

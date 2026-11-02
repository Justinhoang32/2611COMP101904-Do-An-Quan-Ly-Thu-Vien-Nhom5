using QuanLyThuVien.Demo;
using QuanLyThuVien.GiaoDien;

namespace QuanLyThuVien.Forms;

public partial class frmSach : LibraryPage
{
    private TextBox query = null!, name = null!, location = null!;
    private ComboBox category = null!, status = null!, editCategory = null!, editAuthor = null!;
    private NumericUpDown year = null!, total = null!;
    private DataGridView grid = null!; private Pager pager = null!;
    private DemoBook? editing; private Label editorTitle = null!, counts = null!; private Surface editor = null!;
    private PictureBox cover = null!; private byte[]? pendingCover; private CheckBox borrowable = null!;
    public frmSach() : this(new LibraryDemo()) { }
    public frmSach(LibraryDemo store)
    {
        InitializeComponent();
        Setup(store, "Quản lý sách");
        BuildView();
        RefreshBooks(true);
        LoadBook(store.Books[2]);
    }
    private void BuildView()
    {
        var add = Ui.Button("Thêm đầu sách", () => LoadBook(null), true, "plus");
        add.Enabled = Store.Session.IsManager;
        var import = Ui.Button("Nhập danh mục", Import, icon: "upload");
        import.Enabled = Store.Session.IsManager;
        Header("Quản lý sách", "Danh mục học liệu, số lượng bản sách và tình trạng lưu hành.", import, add);
        query = Ui.Input("Tìm mã sách, tên sách, tác giả...");
        category = Ui.Select(new[] { "Tất cả thể loại" }.Concat(Store.Categories.Select(x => x.Name)).ToArray());
        status = Ui.Select("Tất cả trạng thái", "Có sẵn", "Hết sách");
        counts = Ui.Label("", color: Ui.Muted, size: 9);
        grid = BookTable.Create(false);
        pager = new Pager(6, _ => RefreshBooks());
        grid.CellContentClick += (_, e) => { if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "action") LoadBook((DemoBook)grid.Rows[e.RowIndex].Tag!); };
        var filter = Ui.Button("Lọc", () => RefreshBooks(true), icon: "sliders-horizontal");
        AcceptButton = filter;
        Add(Ui.Card("", "", Ui.Columns([50, 20, 20, 10], Ui.Field("Tìm trong danh mục", query), Ui.Field("Thể loại", category), Ui.Field("Tình trạng", status), Ui.Actions(filter)), counts, grid, pager.Control));
        editorTitle = Ui.Heading("", 14);
        name = Ui.Input();
        location = Ui.Input();
        editAuthor = Ui.Select(Store.Authors.Select(x => x.Name).ToArray());
        editCategory = Ui.Select(Store.Categories.Select(x => x.Name).ToArray());
        year = Ui.Number(2023, 1500, 2100);
        total = Ui.Number(1);
        cover = new PictureBox { Height = 120, SizeMode = PictureBoxSizeMode.Zoom };
        cover.Disposed += (_, _) => cover.Image?.Dispose();
        borrowable = new CheckBox { Text = "Được phép cho mượn", Checked = true, AutoSize = true };
        var fields = Ui.Stack(Ui.Columns([70, 30], Ui.Field("Tên sách *", name), Ui.Field("Tác giả *", editAuthor)),
            Ui.Columns([50, 16, 17, 17], Ui.Field("Thể loại *", editCategory), Ui.Field("Năm xuất bản", year), Ui.Field("Tổng số bản *", total), Ui.Field("Vị trí kệ", location)), borrowable);
        editor = Ui.Card("", "", editorTitle, Ui.Columns([14, 86], Ui.Stack(cover, Ui.Actions(Ui.Button("Đổi bìa", ChangeCover, icon: "upload"))), fields),
            Ui.Actions(Ui.Button("Hủy", () => LoadBook(editing)), Ui.Button("Lưu thay đổi", Save, true, "check")));
        editor.Enabled = Store.Session.IsManager;
        Add(editor);
        Footer();
    }
    private void RefreshBooks(bool reset = false)
    {
        var list = Store.Books.Where(b => (LibraryDemo.Matches(b.Code, query.Text) || LibraryDemo.Matches(b.Title, query.Text) || LibraryDemo.Matches(b.Author, query.Text)) && (category.SelectedIndex == 0 || b.Category == category.Text) && (status.SelectedIndex == 0 || (status.SelectedIndex == 1 ? b.Available > 0 : b.Available == 0))).OrderBy(b => b.Code).ToList();
        pager.Update(list.Count, reset);
        BookTable.Fill(grid, list.Skip(pager.Skip).Take(6), false);
        counts.Text = $"{list.Count} đầu sách                      {list.Sum(x => x.Total)} bản sách";
    }
    private void LoadBook(DemoBook? b)
    {
        editing = b;
        editorTitle.Text = b == null ? "Thêm đầu sách" : "Chỉnh sửa đầu sách · " + b.Code;
        name.Text = b?.Title ?? "";
        location.Text = b?.Location ?? "";
        year.Value = b?.Year ?? 2026;
        total.Value = b?.Total ?? 1;
        editAuthor.SelectedItem = b?.Author ?? Store.Authors[0].Name;
        editCategory.SelectedItem = b?.Category ?? Store.Categories[0].Name;
        borrowable.Checked = b?.Borrowable ?? true;
        pendingCover = b?.CoverImage;
        cover.Image?.Dispose();
        cover.Image = BookTable.Cover(b ?? new DemoBook { Title = "Sách mới", Code = "S001" }, 80, 112);
    }
    private void ChangeCover()
    {
        using var dialog = new OpenFileDialog { Filter = "Ảnh bìa (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg" };
        if (dialog.ShowDialog() != DialogResult.OK)
            return;
        Ui.Try(() =>
        {
            if (new FileInfo(dialog.FileName).Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("Ảnh bìa tối đa 5 MB.");
            var bytes = File.ReadAllBytes(dialog.FileName);
            using var stream = new MemoryStream(bytes);
            using var image = Image.FromStream(stream);
            var preview = new Bitmap(image, 80, 112);
            pendingCover = bytes;
            cover.Image?.Dispose();
            cover.Image = preview;
        });
    }
    private void Save()
    {
        if (!Store.Session.IsManager)
            return;
        if (string.IsNullOrWhiteSpace(name.Text))
        {
            Ui.Error("Vui lòng nhập tên sách.");
            return;
        }
        int unavailable = editing == null ? 0 : editing.Total - editing.Available;
        if (total.Value < unavailable)
        {
            Ui.Error($"Tổng số bản phải từ {unavailable} vì đang có bản chưa sẵn sàng.");
            return;
        }
        var b = editing ?? new DemoBook { Code = "S" + (Store.Books.Select(x => int.TryParse(x.Code.TrimStart('S'), out int n) ? n : 0).Max() + 1).ToString("D3") };
        b.Title = name.Text.Trim();
        b.Author = editAuthor.Text;
        b.Category = editCategory.Text;
        b.Year = (int)year.Value;
        b.Total = (int)total.Value;
        b.Available = b.Total - unavailable;
        b.Location = location.Text.Trim();
        b.Borrowable = borrowable.Checked;
        b.CoverImage = pendingCover;
        if (editing == null)
            Store.Books.Add(b);
        LoadBook(b);
        RefreshBooks();
        Ui.Info("Đã lưu thay đổi trong dữ liệu mẫu của phiên này.");
    }
    private void Import()
    {
        if (!Store.Session.IsManager)
            return;
        using var dialog = new OpenFileDialog { Filter = "Danh mục CSV (*.csv)|*.csv" };
        if (dialog.ShowDialog() != DialogResult.OK)
            return;
        Ui.Try(() =>
        {
            using var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(dialog.FileName) { TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited, HasFieldsEnclosedInQuotes = true };
            parser.SetDelimiters(",");
            var header = parser.ReadFields();
            string[] expected = ["MaSach", "TenSach", "TacGia", "TheLoai", "TongBan", "Nam", "ViTri"];
            if (header == null || !header.SequenceEqual(expected))
                throw new InvalidOperationException("CSV cần các cột: " + string.Join(",", expected));
            List<DemoBook> pending = [];
            while (!parser.EndOfData)
            {
                var row = parser.ReadFields()!;
                if (row.Length != 7 || string.IsNullOrWhiteSpace(row[0]) || string.IsNullOrWhiteSpace(row[1]) || !int.TryParse(row[4], out int count) || count < 0 || !int.TryParse(row[5], out int y) || y < 1500 || y > 2100)
                    throw new InvalidOperationException("Có dòng dữ liệu không hợp lệ.");
                if (Store.Books.Concat(pending).Any(x => x.Code.Equals(row[0], StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("Mã sách trùng: " + row[0]);
                if (!Store.Authors.Any(x => x.Name == row[2]) || !Store.Categories.Any(x => x.Name == row[3]))
                    throw new InvalidOperationException("Tác giả/thể loại phải có trong danh mục trước khi nhập.");
                pending.Add(new()
                {
                    Code = row[0],
                    Title = row[1],
                    Author = row[2],
                    Category = row[3],
                    Total = count,
                    Available = count,
                    Year = y,
                    Location = row[6]
                });
            }
            Store.Books.AddRange(pending);
            RefreshBooks(true);
            Ui.Info($"Đã nhập {pending.Count} đầu sách vào dữ liệu mẫu.");
        });
    }

    private void frmSach_Load(object sender, EventArgs e)
    {

    }
}

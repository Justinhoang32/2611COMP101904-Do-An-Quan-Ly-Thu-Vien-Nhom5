using QuanLyThuVien.Demo;
using QuanLyThuVien.GiaoDien;

namespace QuanLyThuVien.Forms;

public partial class frmTacGiaTheLoai : LibraryPage
{
    public frmTacGiaTheLoai() : this(new LibraryDemo()) { }
    public frmTacGiaTheLoai(LibraryDemo store)
    {
        InitializeComponent();
        Setup(store, "Tác giả & thể loại");
        Header("Tác giả & thể loại", "Chuẩn hóa danh mục dùng chung để tra cứu và biên mục nhất quán.");
        Add(Ui.Notice("Quyền chỉnh sửa danh mục: Quản trị viên. Thay đổi tên được áp dụng cho các đầu sách liên quan."));
        Add(Ui.Columns([50, 50], BuildCatalog(true), BuildCatalog(false)));
        Footer();
    }
    private Control BuildCatalog(bool author)
    {
        var data = author ? Store.Authors : Store.Categories;
        var filter = Ui.Input(author ? "Tìm tên hoặc mã tác giả" : "Tìm tên hoặc mã thể loại");
        var grid = Ui.Grid(435);
        Ui.Column(grid, "code", "MÃ", 70);
        Ui.Column(grid, "name", author ? "TÊN TÁC GIẢ" : "TÊN THỂ LOẠI", 170);
        Ui.Column(grid, "count", "ĐẦU SÁCH", 70);
        Ui.Column(grid, "edit", "", 55, true);
        var name = Ui.Input();
        var description = Ui.Input();
        description.Multiline = true;
        description.Height = 78;
        var selected = Ui.Label("", true, Ui.Muted, 9);
        DemoCatalogItem? editing = null;
        Pager pager = null!;
        void Load(DemoCatalogItem? item)
        {
            editing = item;
            selected.Text = item?.Code ?? "Mục mới";
            name.Text = item?.Name ?? "";
            description.Text = item?.Description ?? "";
        }
        void Refresh(bool reset = false)
        {
            var list = data.Where(x => LibraryDemo.Matches(x.Name, filter.Text) || LibraryDemo.Matches(x.Code, filter.Text)).ToList();
            pager.Update(list.Count, reset);
            grid.Rows.Clear();
            foreach (var item in list.Skip(pager.Skip).Take(6))
            {
                int i = grid.Rows.Add(item.Code, item.Name, Store.Books.Count(b => (author ? b.Author : b.Category) == item.Name), "Sửa");
                grid.Rows[i].Tag = item;
            }
        }
        pager = new Pager(6, _ => Refresh());
        filter.TextChanged += (_, _) => Refresh(true);
        grid.CellContentClick += (_, e) => { if (e.RowIndex >= 0) Load((DemoCatalogItem)grid.Rows[e.RowIndex].Tag!); };
        var save = Ui.Button(author ? "Lưu tác giả" : "Lưu thể loại", () =>
        {
            if (!Store.Session.IsManager)
                return;
            if (string.IsNullOrWhiteSpace(name.Text))
            {
                Ui.Error("Vui lòng nhập tên.");
                return;
            }
            if (data.Any(x => x != editing && x.Name.Equals(name.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                Ui.Error("Tên này đã tồn tại.");
                return;
            }
            var item = editing ?? new DemoCatalogItem { Code = (author ? "TG" : "TL") + (data.Count + 1).ToString("D3") };
            string old = item.Name;
            item.Name = name.Text.Trim();
            item.Description = description.Text.Trim();
            if (editing == null)
                data.Add(item);
            foreach (var b in Store.Books)
            {
                if (author && b.Author == old)
                    b.Author = item.Name;
                if (!author && b.Category == old)
                    b.Category = item.Name;
            }
            Load(item);
            Refresh();
            Ui.Info("Đã lưu danh mục trong dữ liệu mẫu.");
        }, true, "check");
        save.Enabled = Store.Session.IsManager;
        name.ReadOnly = !Store.Session.IsManager;
        description.ReadOnly = !Store.Session.IsManager;
        var add = Ui.Button(author ? "Thêm tác giả" : "Thêm thể loại", () => Load(null), icon: "plus");
        add.Enabled = Store.Session.IsManager;
        var editor = Ui.Stack(Ui.Label(author ? "Chỉnh sửa tác giả" : "Chỉnh sửa thể loại", true), selected, Ui.Field(author ? "Họ và tên tác giả *" : "Tên thể loại *", name), new Panel { Height = 12 }, Ui.Field(author ? "Giới thiệu" : "Mô tả", description), Ui.Actions(Ui.Button("Hủy", () => Load(editing)), save));
        editor.Padding = new Padding(14);
        editor.BackColor = Ui.Background;
        Refresh();
        Load(data.ElementAtOrDefault(2));
        return Ui.Card(author ? "Tác giả" : "Thể loại", "", Ui.Actions(add), Ui.Field(author ? "Tra cứu tác giả" : "Tra cứu thể loại", filter), grid, pager.Control, editor);
    }

    private void frmTacGiaTheLoai_Load(object sender, EventArgs e)
    {

    }
}

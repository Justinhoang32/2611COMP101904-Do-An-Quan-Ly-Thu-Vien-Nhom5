using QuanLyThuVien.Demo;
using QuanLyThuVien.GiaoDien;

namespace QuanLyThuVien.Forms;

public partial class frmLapPhieuMuon : LibraryPage
{
    private ComboBox reader = null!; private DateTimePicker borrowed = null!, due = null!;
    private TextBox search = null!, note = null!; private DataGridView matches = null!, cart = null!;
    private Label summary = null!, validation = null!, readerInfo = null!; private Button create = null!;
    public frmLapPhieuMuon() : this(new LibraryDemo()) { }
    public frmLapPhieuMuon(LibraryDemo store)
    {
        InitializeComponent();
        Setup(store, "Lập phiếu mượn");
        Header("Lập phiếu mượn", "Chọn độc giả, thêm sách và kiểm tra số lượng trước khi tạo phiếu.");
        Add(Ui.Label("01  Thông tin mượn                         02  Chọn sách                         03  Kiểm tra & tạo phiếu", true, Ui.Navy, 9));
        reader = Ui.Select(store.Readers.Select(x => x.Code + " — " + x.Name).ToArray());
        reader.SelectedIndex = Math.Max(0, store.Readers.FindIndex(x => x.Code == store.DraftReaderCode));
        borrowed = Ui.Date(store.DraftBorrowed);
        due = Ui.Date(store.DraftDue);
        readerInfo = Ui.Label("", color: Ui.Muted, size: 9);
        Add(Ui.Card("Thông tin mượn", "", Ui.Columns([60, 20, 20], Ui.Field("Độc giả *", reader), Ui.Field("Ngày mượn *", borrowed), Ui.Field("Ngày hẹn trả *", due)), readerInfo));
        search = Ui.Input("Tìm theo mã, tên sách hoặc tác giả");
        search.Text = "Dạy học phát triển năng lực";
        matches = Ui.Grid(142);
        Ui.Column(matches, "book", "SÁCH", 190);
        Ui.Column(matches, "stock", "CÒN / TỔNG", 75);
        Ui.Column(matches, "add", "", 60, true);
        search.TextChanged += (_, _) => RefreshMatches();
        matches.CellContentClick += (_, e) => { if (e.RowIndex < 0 || e.ColumnIndex < 0 || matches.Columns[e.ColumnIndex].Name != "add") return; var b = (DemoBook)matches.Rows[e.RowIndex].Tag!; if (b.Available <= Store.Draft.GetValueOrDefault(b.Code) || !b.Borrowable) return; Store.Draft[b.Code] = Store.Draft.GetValueOrDefault(b.Code) + 1; RefreshCart(); };
        cart = Ui.Grid(330);
        Ui.Column(cart, "book", "SÁCH", 200);
        Ui.Column(cart, "available", "CÒN", 60);
        Ui.Column(cart, "minus", "", 30, true);
        Ui.Column(cart, "quantity", "SỐ LƯỢNG", 75);
        Ui.Column(cart, "plus", "", 30, true);
        Ui.Column(cart, "remove", "", 40, true);
        cart.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            var b = (DemoBook)cart.Rows[e.RowIndex].Tag!;
            string action = cart.Columns[e.ColumnIndex].Name;
            int count = Store.Draft[b.Code];
            if (action == "remove")
                Store.Draft.Remove(b.Code);
            else if (action == "minus" && count > 1)
                Store.Draft[b.Code]--;
            else if (action == "plus" && count < b.Available)
                Store.Draft[b.Code]++;
            else
                return;
            RefreshCart();
        };
        validation = Ui.Label("", color: Ui.Red, size: 9);
        summary = Ui.Label("");
        note = Ui.Input();
        note.Multiline = true;
        note.Height = 90;
        note.MaxLength = 200;
        note.Text = store.DraftNote;
        note.TextChanged += (_, _) => store.DraftNote = note.Text;
        create = Ui.Button("Tạo phiếu & xem bản in", Create, true, "printer");
        var left = Ui.Stack(Ui.Card("Tìm & thêm sách", "Tìm theo mã, tên sách hoặc tác giả.", search, matches),
            Ui.Card("Sách trong phiếu", "Số lượng từng đầu sách không vượt số bản còn.", Ui.Actions(Ui.Button("Thêm từ danh mục", () => Go(LibraryScreen.Search))), cart, validation));
        var right = Ui.Card("Kiểm tra phiếu", "", summary, Ui.Field("Ghi chú phiếu", note), Ui.Actions(create), Ui.Actions(Ui.Button("Hủy lập phiếu", () => { if (MessageBox.Show("Hủy các sách đang chọn?", "Lập phiếu mượn", MessageBoxButtons.YesNo) == DialogResult.Yes) { Store.Draft.Clear(); Go(LibraryScreen.Loans); } })));
        Add(Ui.Columns([66, 34], left, right));
        reader.SelectedIndexChanged += (_, _) => RefreshSummary();
        borrowed.ValueChanged += (_, _) => RefreshSummary();
        due.ValueChanged += (_, _) => RefreshSummary();
        RefreshMatches();
        RefreshCart();
        Footer();
    }
    private DemoReader SelectedReader => Store.Readers[reader.SelectedIndex];
    private void RefreshMatches()
    {
        matches.Rows.Clear();
        foreach (var b in Store.Books.Where(x => LibraryDemo.Matches(x.Code, search.Text) || LibraryDemo.Matches(x.Title, search.Text) || LibraryDemo.Matches(x.Author, search.Text)).Take(4))
        {
            int i = matches.Rows.Add(b.Title + "\n" + b.Code + " · " + b.Author, $"{b.Available} / {b.Total}", b.Available > 0 && b.Borrowable ? "Thêm" : "Hết sách");
            matches.Rows[i].Tag = b;
        }
        matches.Height = 40 + Math.Max(1, matches.Rows.Count) * 66 + 4;
    }
    private void RefreshCart()
    {
        cart.Rows.Clear();
        foreach (var (code, count) in Store.Draft)
        {
            var b = Store.Books.Single(x => x.Code == code);
            int i = cart.Rows.Add(b.Title + "\n" + b.Code + " · " + b.Author, b.Available, count > 1 ? "−" : "", count, count < b.Available ? "+" : "", "×");
            cart.Rows[i].Tag = b;
            if (count > b.Available)
                cart.Rows[i].Cells["quantity"].Style.ForeColor = Ui.Red;
        }
        RefreshSummary();
    }
    private void RefreshSummary()
    {
        var r = SelectedReader;
        Store.DraftReaderCode = r.Code;
        Store.DraftBorrowed = borrowed.Value.Date;
        Store.DraftDue = due.Value.Date;
        int current = Store.Loans.Where(x => x.Reader.Code == r.Code).Sum(x => x.Remaining);
        readerInfo.Text = $"{r.Group} · Khoa {r.Faculty}     Thẻ hết hạn: {r.Expires:dd/MM/yyyy}     Trước lập phiếu: {current} / 5 cuốn";
        summary.Text = $"Độc giả: {r.Name}\n\nSố đầu sách: {Store.Draft.Count}\n\nTổng số cuốn đang chọn: {Store.Draft.Values.Sum()}\n\nHạn mức: tối đa 5 cuốn\n\nThời gian mượn: {(due.Value.Date - borrowed.Value.Date).Days} ngày\n";
        var errors = Store.ValidateDraft(r, borrowed.Value.Date, due.Value.Date);
        validation.Text = string.Join("\n", errors);
        create.Enabled = errors.Count == 0;
    }
    private void Create() => Ui.Try(() => { var loan = Store.CreateLoan(SelectedReader, borrowed.Value.Date, due.Value.Date, note.Text.Trim()); Go(LibraryScreen.Print, loan); });

    private void frmLapPhieuMuon_Load(object sender, EventArgs e)
    {

    }
}

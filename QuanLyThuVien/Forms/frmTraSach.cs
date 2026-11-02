using QuanLyThuVien.Demo;
using QuanLyThuVien.GiaoDien;

namespace QuanLyThuVien.Forms;

public partial class frmTraSach : LibraryPage
{
    private DemoLoan loan; private TextBox code = null!, note = null!; private DateTimePicker date = null!;
    private DataGridView grid = null!; private Label details = null!, summary = null!; private Button confirm = null!;
    public frmTraSach() : this(new LibraryDemo()) { }
    public frmTraSach(LibraryDemo store, DemoLoan? loan = null)
    {
        this.loan = loan ?? store.Loans[0];
        InitializeComponent();
        Setup(store, "Ghi nhận trả sách");
        Header("Ghi nhận trả sách", "Kiểm tra từng bản sách và xác nhận số cuốn thực nhận tại quầy.", Ui.Button("Về danh sách phiếu", () => Go(LibraryScreen.Loans), icon: "arrow-left"));
        code = Ui.Input("Nhập mã phiếu");
        code.Text = this.loan.Code;
        details = Ui.Label("");
        date = Ui.Date(store.Today);
        var find = Ui.Button("Tìm phiếu", Find, icon: "search");
        AcceptButton = find;
        Add(Ui.Card("Phiếu mượn", "", Ui.Columns([26, 14, 40, 20], Ui.Field("Quét / nhập mã phiếu", code), Ui.Actions(find), details, Ui.Field("Ngày ghi nhận trả *", date))));
        grid = Ui.Grid(370);
        grid.ReadOnly = false;
        grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "selected", HeaderText = "CHỌN", Width = 55, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        Ui.Column(grid, "book", "SÁCH / MÃ", 180);
        Ui.Column(grid, "copy", "MÃ BẢN", 95);
        var condition = new DataGridViewComboBoxColumn { Name = "condition", HeaderText = "TÌNH TRẠNG KHI TRẢ", FillWeight = 110, FlatStyle = FlatStyle.Flat };
        condition.Items.AddRange("Tốt", "Hư hỏng", "Mất");
        grid.Columns.Add(condition);
        Ui.Column(grid, "note", "GHI CHÚ TỪNG CUỐN", 160);
        grid.Columns["book"].ReadOnly = true;
        grid.Columns["copy"].ReadOnly = true;
        grid.CurrentCellDirtyStateChanged += (_, _) => { if (grid.IsCurrentCellDirty) grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };
        grid.CellValueChanged += (_, _) => { if (confirm != null) RefreshSummary(); };
        grid.DataError += (_, e) => { e.ThrowException = false; };
        Add(Ui.Card("Chọn sách trả trong lần này", "Có thể trả một phần hoặc toàn bộ phiếu.", Ui.Actions(Ui.Button("Chọn tất cả", () => SelectAll(true)), Ui.Button("Bỏ chọn", () => SelectAll(false))), grid));
        note = Ui.Input();
        note.Multiline = true;
        note.Height = 95;
        note.Text = "Độc giả trả trước hạn. Kiểm tra tình trạng từng bản sách.";
        summary = Ui.Label("");
        confirm = Ui.Button("Xác nhận trả sách", Return, true, "check");
        Add(Ui.Columns([65, 35], Ui.Card("Ghi chú lần trả", "", Ui.Field("Ghi chú chung", note), Ui.Label("Thủ thư tiếp nhận: " + store.Session.Name, color: Ui.Muted, size: 9)),
            Ui.Card("Tóm tắt trước xác nhận", "", summary, Ui.Actions(confirm))));
        Add(Ui.Notice("Bản sách Tốt được cộng lại vào số có sẵn. Bản Hư hỏng hoặc Mất cần ghi lý do và chờ xử lý."));
        LoadLoan();
        Footer();
    }
    private void Find()
    {
        var found = Store.Loans.FirstOrDefault(x => x.Code.Equals(code.Text.Trim(), StringComparison.OrdinalIgnoreCase));
        if (found == null)
        {
            Ui.Error("Không tìm thấy phiếu mượn.");
            return;
        }
        loan = found;
        LoadLoan();
    }
    private void LoadLoan()
    {
        details.Text = $"{loan.Reader.Name}\n{loan.Reader.Code} · {loan.Reader.Group} · {loan.Reader.Phone}\nMượn: {loan.Borrowed:dd/MM/yyyy}   Hẹn trả: {loan.Due:dd/MM/yyyy}";
        grid.Rows.Clear();
        foreach (var line in loan.Lines.Where(x => x.Returned == null))
        {
            int i = grid.Rows.Add(false, line.Book.Title + "\n" + line.Book.Code, line.CopyCode, "Tốt", "");
            grid.Rows[i].Tag = line;
        }
        if (grid.Rows.Count > 0)
            grid.Rows[0].Cells["selected"].Value = true;
        if (grid.Rows.Count > 1)
        {
            grid.Rows[1].Cells["selected"].Value = true;
            grid.Rows[1].Cells["condition"].Value = "Hư hỏng";
            grid.Rows[1].Cells["note"].Value = "Rách bìa sau, cần phục hồi.";
        }
        RefreshSummary();
    }
    private void SelectAll(bool value)
    {
        foreach (DataGridViewRow row in grid.Rows)
            row.Cells["selected"].Value = value;
        RefreshSummary();
    }
    private List<(DemoLoanLine Line, string Condition, string Note)> Selection()
        => grid.Rows.Cast<DataGridViewRow>().Where(r => r.Cells["selected"].Value is true).Select(r => ((DemoLoanLine)r.Tag!, r.Cells["condition"].Value?.ToString() ?? "Tốt", r.Cells["note"].Value?.ToString() ?? "")).ToList();
    private void RefreshSummary()
    {
        var selected = Selection();
        summary.Text = $"Số bản trong phiếu: {loan.Lines.Count} cuốn\n\nĐược chọn trả lần này: {selected.Count} cuốn\n\nTốt · trở lại kho: {selected.Count(x => x.Condition == "Tốt")} cuốn\n\nHư hỏng · chờ xử lý: {selected.Count(x => x.Condition == "Hư hỏng")} cuốn\n\nBáo mất: {selected.Count(x => x.Condition == "Mất")} cuốn\n\nCòn đang mượn: {loan.Remaining - selected.Count} cuốn\n";
        confirm.Text = $"Xác nhận trả {selected.Count} cuốn";
        confirm.Enabled = selected.Count > 0;
    }
    private void Return()
    {
        grid.EndEdit();
        var selected = Selection();
        if (MessageBox.Show($"Xác nhận nhận lại {selected.Count} cuốn sách?", "Ghi nhận trả", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        Ui.Try(() =>
        {
            if (selected.Any(x => x.Condition != "Tốt" && string.IsNullOrWhiteSpace(x.Note)))
                throw new InvalidOperationException("Ghi rõ lý do riêng cho từng cuốn hư hỏng hoặc mất.");
            var withNotes = selected.Select(x => (x.Line, x.Condition, x.Note + (string.IsNullOrWhiteSpace(note.Text) ? "" : "\n" + note.Text.Trim()))).ToList();
            Store.ReturnBooks(loan, withNotes, date.Value.Date);
            LoadLoan();
            Ui.Info("Đã ghi nhận trong dữ liệu mẫu. " + loan.Status(Store.Today));
        });
    }

    private void frmTraSach_Load(object sender, EventArgs e)
    {

    }
}

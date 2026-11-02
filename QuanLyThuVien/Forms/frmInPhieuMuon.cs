using System.Drawing.Printing;
using QuanLyThuVien.Demo;
using QuanLyThuVien.GiaoDien;

namespace QuanLyThuVien.Forms;

public partial class frmInPhieuMuon : LibraryPage
{
    private DemoLoan loan; private ReceiptCanvas canvas = null!;
    private ComboBox printer = null!, paper = null!, orientation = null!, range = null!;
    private CheckBox notes = null!; private NumericUpDown copies = null!; private Label pages = null!;
    private Button previous = null!, next = null!; private int page;
    public frmInPhieuMuon() : this(new LibraryDemo()) { }
    public frmInPhieuMuon(LibraryDemo store, DemoLoan? loan = null)
    {
        this.loan = loan ?? store.Loans[0];
        InitializeComponent();
        Setup(store, "Xem trước phiếu mượn");
        Header("Xem trước phiếu mượn", $"Phiếu {this.loan.Code} · {this.loan.Lines.Select(x => x.Book.Code).Distinct().Count()} đầu sách, {this.loan.Lines.Count} cuốn.",
            Ui.Button("Quay lại phiếu", () => Go(LibraryScreen.Loans), icon: "arrow-left"), Ui.Button("In phiếu mượn", () => Print(false), true, "printer"));
        canvas = new ReceiptCanvas { Loan = this.loan, Width = 760 };
        var names = new List<string>();
        try
        {
            foreach (string name in PrinterSettings.InstalledPrinters)
                names.Add(name);
        }
        catch (System.ComponentModel.Win32Exception) { }
        printer = Ui.Select(names.Count == 0 ? ["Chưa cài máy in"] : names.ToArray());
        printer.Enabled = names.Count > 0;
        paper = Ui.Select("A4", "A5");
        orientation = Ui.Select("Dọc", "Ngang");
        range = Ui.Select("Tất cả các trang", "Trang đang xem");
        copies = Ui.Number(1, 1, 99);
        notes = new CheckBox { Text = "Bao gồm ghi chú phiếu", AutoSize = true, Checked = true, Margin = new Padding(0, 16, 0, 16) };
        pages = Ui.Label("");
        previous = Ui.Button("", () => { page--; RefreshPreview(); }, icon: "chevron-left");
        previous.AccessibleName = "Trang trước";
        next = Ui.Button("", () => { page++; RefreshPreview(); }, icon: "chevron-right");
        next.AccessibleName = "Trang sau";
        paper.SelectedIndexChanged += (_, _) => RefreshPreview();
        orientation.SelectedIndexChanged += (_, _) => { page = 0; RefreshPreview(); };
        notes.CheckedChanged += (_, _) => RefreshPreview();
        var print = Ui.Button("In phiếu", () => Print(false), true, "printer");
        print.Enabled = names.Count > 0;
        var pdf = Ui.Button("Lưu PDF", () => Print(true), icon: "download");
        var settings = Ui.Card("Thiết lập bản in", "", Ui.Label("Phiếu đã tạo hợp lệ", true, Ui.Green, 9),
            Ui.Field("Máy in", printer), Ui.Columns([50, 50], Ui.Field("Khổ giấy", paper), Ui.Field("Hướng giấy", orientation)),
            Ui.Field("Số bản in", copies), Ui.Field("Phạm vi trang", range), notes, Ui.Label("Lề giấy: Tiêu chuẩn", color: Ui.Muted, size: 9),
            Ui.Actions(print, pdf), Ui.Actions(previous, pages, next));
        Add(Ui.Columns([70, 30], canvas, Ui.Stack(settings, Ui.Notice("Phiếu nhiều cuốn\nTất cả sách trong một phiếu được tổng hợp trên cùng chứng từ."))));
        RefreshPreview();
        Footer();
    }
    private void Form9_Load(object? sender, EventArgs e)
    {
    }
    private void RefreshPreview()
    {
        bool landscape = orientation.SelectedIndex == 1;
        int count = ReceiptRenderer.PageCount(loan, landscape);
        page = Math.Clamp(page, 0, count - 1);
        canvas.Landscape = landscape;
        canvas.IncludeNotes = notes.Checked;
        canvas.Page = page;
        canvas.Height = (int)(canvas.Width * (landscape ? 794F / 1123 : 1123F / 794));
        canvas.Invalidate();
        pages.Text = $"{page + 1} / {count}";
        previous.Enabled = page > 0;
        next.Enabled = page < count - 1;
    }
    private void Print(bool pdf)
    {
        try
        {
            string name = printer.Text;
            string? path = null;
            if (pdf)
            {
                name = PrinterSettings.InstalledPrinters.Cast<string>().FirstOrDefault(x => x.Equals("Microsoft Print to PDF", StringComparison.OrdinalIgnoreCase)) ?? "";
                if (name == "")
                {
                    Ui.Error("Chưa có máy in Microsoft Print to PDF. Hãy bật tính năng này trên Windows hoặc chọn máy in PDF trong hộp thoại in.");
                    return;
                }
                using var save = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", FileName = loan.Code + ".pdf", DefaultExt = "pdf", AddExtension = true };
                if (save.ShowDialog() != DialogResult.OK)
                    return;
                path = save.FileName;
            }
            using var doc = new PrintDocument { DocumentName = loan.Code };
            doc.PrinterSettings.PrinterName = name;
            if (!doc.PrinterSettings.IsValid)
            {
                Ui.Error("Chưa có máy in hợp lệ. Bản xem trước vẫn có thể sử dụng.");
                return;
            }
            doc.PrinterSettings.Copies = (short)copies.Value;
            var kind = paper.SelectedIndex == 0 ? PaperKind.A4 : PaperKind.A5;
            var selectedPaper = doc.PrinterSettings.PaperSizes.Cast<PaperSize>().FirstOrDefault(x => x.Kind == kind);
            doc.DefaultPageSettings.PaperSize = selectedPaper ?? (kind == PaperKind.A4 ? new PaperSize("A4", 827, 1169) : new PaperSize("A5", 583, 827));
            doc.DefaultPageSettings.Landscape = orientation.SelectedIndex == 1;
            doc.DefaultPageSettings.Margins = new Margins(20, 20, 20, 20);
            bool landscape = orientation.SelectedIndex == 1;
            int current = range.SelectedIndex == 1 ? page : 0;
            int last = range.SelectedIndex == 1 ? page : ReceiptRenderer.PageCount(loan, landscape) - 1;
            doc.PrintPage += (_, e) => { ReceiptRenderer.Draw(e.Graphics!, e.MarginBounds, loan, notes.Checked, landscape, current); e.HasMorePages = current++ < last; };
            if (pdf)
            {
                doc.PrinterSettings.PrintToFile = true;
                doc.PrinterSettings.PrintFileName = path!;
                doc.Print();
                Ui.Info("Đã gửi bản in tới Microsoft Print to PDF.");
            }
            else
            {
                using var dialog = new PrintDialog { Document = doc, UseEXDialog = true };
                if (dialog.ShowDialog() == DialogResult.OK)
                    doc.Print();
            }
        }
        catch (Exception ex) when (ex is InvalidPrinterException or System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException or InvalidOperationException) { Ui.Error("Không thể in: " + ex.Message); }
    }
}

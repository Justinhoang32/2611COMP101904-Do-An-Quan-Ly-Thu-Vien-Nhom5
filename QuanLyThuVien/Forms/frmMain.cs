using QuanLyThuVien.Demo;
using QuanLyThuVien.GiaoDien;
using System.Globalization;

namespace QuanLyThuVien.Forms;

public partial class frmMain : Form
{
    private readonly LibraryDemo store;
    private readonly Panel content = new() { Dock = DockStyle.Fill };
    private readonly Label breadcrumb = Ui.Label("Thư viện / Tổng quan", color: Ui.Muted, size: 9);
    private readonly Dictionary<LibraryScreen, Button> navigation = [];
    private LibraryPage? current;
    public bool LoggedOut
    {
        get; private set;
    }

    public frmMain() : this(new LibraryDemo()) { }
    public frmMain(LibraryDemo store)
    {
        this.store = store;
        InitializeComponent();
        BuildShell();
        OpenScreen(LibraryScreen.Overview);
    }

    private void BuildShell()
    {
        Text = "Thư viện HCMUE";
        Font = Ui.BodyFont;
        BackColor = Ui.Background;
        MinimumSize = new Size(1160, 760);
        Size = new Size(1460, 960);
        StartPosition = FormStartPosition.CenterScreen;
        var sidebar = new Panel { Width = 232, Dock = DockStyle.Left, BackColor = Color.White, Padding = new Padding(16), AutoScroll = true };
        var stack = Ui.Stack();
        var logo = new PictureBox { Image = Ui.Asset("hcmue-logo.png"), SizeMode = PictureBoxSizeMode.Zoom, Height = 70, Width = 148, Dock = DockStyle.Top, Margin = new Padding(12, 6, 30, 16) };
        stack.Controls.Add(logo);
        stack.Controls.Add(Ui.Heading("Thư viện HCMUE", 14));
        stack.Controls.Add(Ui.Label("HỆ THỐNG QUẢN LÝ THƯ VIỆN", color: Ui.Muted, size: 7.5F));
        var caption = Ui.Label("KHÔNG GIAN LÀM VIỆC", true, Ui.Muted, 7.5F);
        caption.Margin = new Padding(0, 24, 0, 12);
        stack.Controls.Add(caption);
        (LibraryScreen Screen, string Text, string Icon)[] links =
        [
            (LibraryScreen.Overview,"Tổng quan","layout-dashboard"), (LibraryScreen.Books,"Quản lý sách","book-open"),
            (LibraryScreen.Catalog,"Tác giả & thể loại","tags"), (LibraryScreen.Readers,"Độc giả","users"),
            (LibraryScreen.Loans,"Phiếu mượn","clipboard-list"), (LibraryScreen.Returns,"Ghi nhận trả sách","clipboard-check"),
            (LibraryScreen.Search,"Tìm kiếm sách","search"), (LibraryScreen.Overdue,"Báo cáo quá hạn","chart-no-axes-combined")
        ];
        foreach (var link in links)
        {
            var button = Ui.Button(link.Text, () => OpenScreen(link.Screen), icon: link.Icon);
            button.AutoSize = false;
            button.Size = new Size(200, 46);
            button.Dock = DockStyle.Top;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Font = Ui.BodyFont;
            button.FlatAppearance.BorderSize = 0;
            button.Margin = new Padding(0, 0, 0, 5);
            navigation[link.Screen] = button;
            stack.Controls.Add(button);
        }
        var admin = Ui.Label("QUẢN TRỊ HỆ THỐNG", true, Ui.Muted, 7.5F);
        admin.Margin = new Padding(0, 28, 0, 12);
        stack.Controls.Add(admin);
        var account = Ui.Button("Tài khoản & phân quyền", () => Ui.Info("Phần quản lý tài khoản chưa nằm trong 11 giao diện được cung cấp."), icon: "shield-check");
        account.Font = Ui.SmallFont;
        account.FlatAppearance.BorderSize = 0;
        account.Enabled = store.Session.IsManager;
        stack.Controls.Add(account);
        var motto = Ui.Stack(Ui.Label("━━━━", color: Ui.Red), Ui.Label("Chất lượng – Sáng tạo – Nhân văn", color: Ui.Navy, size: 8), Ui.Label("Năm học 2026 – 2027", color: Ui.Muted, size: 8));
        motto.Margin = new Padding(8, 60, 0, 12);
        stack.Controls.Add(motto);
        sidebar.Controls.Add(stack);
        var workspace = new Panel { Dock = DockStyle.Fill };
        var topbar = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.White, Padding = new Padding(28, 18, 24, 12) };
        var left = Ui.Actions(Ui.Label("HCMUE", true, Ui.Navy, 9), breadcrumb);
        left.Dock = DockStyle.Left;
        var right = Ui.Actions(Ui.Label(store.Today.ToString("dddd, dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN")), color: Ui.Muted, size: 8),
            Ui.Button("", () => OpenScreen(LibraryScreen.Overdue), icon: "bell"), Ui.Label(store.Session.Name + "\n" + (store.Session.IsManager ? "Quản trị viên" : "Thủ thư") + " · Cơ sở An Dương Vương", true, size: 8),
            Ui.Button("", () => { LoggedOut = true; Close(); }, icon: "log-out"));
        right.Dock = DockStyle.Right;
        foreach (Control c in right.Controls)
            if (c is Button b && b.Image != null && b.Text == "")
            {
                b.AccessibleName = b == right.Controls[1] ? "Báo cáo quá hạn" : "Đăng xuất";
            }
        topbar.Controls.Add(left);
        topbar.Controls.Add(right);
        workspace.Controls.Add(content);
        workspace.Controls.Add(topbar);
        Controls.Add(workspace);
        Controls.Add(sidebar);
    }

    public void OpenScreen(LibraryScreen screen, object? value = null)
    {
        current?.Dispose();
        content.Controls.Clear();
        current = screen switch
        {
            LibraryScreen.Overview => new frmTongQuan(store),
            LibraryScreen.Books => new frmSach(store),
            LibraryScreen.Catalog => new frmTacGiaTheLoai(store),
            LibraryScreen.Readers => new frmDocGia(store),
            LibraryScreen.Loans => new frmDanhSachPhieuMuon(store),
            LibraryScreen.CreateLoan => new frmLapPhieuMuon(store),
            LibraryScreen.Returns => new frmTraSach(store, value as DemoLoan),
            LibraryScreen.Search => new frmTimKiemSach(store),
            LibraryScreen.Overdue => new frmBaoCaoQuaHan(store),
            LibraryScreen.Print => new frmInPhieuMuon(store, value as DemoLoan ?? store.Loans[0]),
            _ => throw new ArgumentOutOfRangeException(nameof(screen))
        };
        current.Navigate += OpenScreen;
        content.Controls.Add(current);
        current.Show();
        breadcrumb.Text = "Thư viện / " + current.Text;
        var selected = screen is LibraryScreen.CreateLoan or LibraryScreen.Print ? LibraryScreen.Loans : screen;
        foreach (var (key, button) in navigation)
        {
            button.BackColor = key == selected ? Ui.Blue : Color.White;
            button.Font = key == selected ? Ui.BoldFont : Ui.BodyFont;
        }
    }

    private void frmMain_Load(object sender, EventArgs e)
    {

    }
}

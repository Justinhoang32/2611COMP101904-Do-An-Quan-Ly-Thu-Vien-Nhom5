using QuanLyThuVien.Demo;
using QuanLyThuVien.GiaoDien;

namespace QuanLyThuVien.Forms;

public partial class frmDangNhap : Form
{
    private readonly LibraryDemo store;
    private TextBox username = null!, password = null!;
    private CheckBox remember = null!;
    private Label validation = null!;
    private readonly string preference = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HcmueWinFormsDemo", "username.txt");
    public frmDangNhap() : this(new LibraryDemo()) { }
    public frmDangNhap(LibraryDemo store)
    {
        this.store = store;
        InitializeComponent();
        BuildLogin();
    }

    private void BuildLogin()
    {
        Text = "Đăng nhập · Thư viện HCMUE";
        MinimumSize = new Size(1050, 760);
        Size = new Size(1400, 940);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Ui.Background;
        Font = Ui.BodyFont;
        var header = new Panel { Dock = DockStyle.Top, Height = 112, BackColor = Color.White, Padding = new Padding(48, 18, 48, 16) };
        var logo = new PictureBox { Image = Ui.Asset("hcmue-logo.png"), SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Left, Width = 136 };
        var brand = Ui.Stack(Ui.Heading("Thư viện HCMUE", 21), Ui.Label("HỆ THỐNG QUẢN LÝ THƯ VIỆN", color: Ui.Muted, size: 8));
        brand.Dock = DockStyle.Left;
        brand.Padding = new Padding(28, 10, 0, 0);
        brand.Width = 390;
        var lookup = Ui.Button("Tra cứu thư viện", () => OpenLookup(), icon: "book-open");
        lookup.Dock = DockStyle.Right;
        header.Controls.Add(lookup);
        header.Controls.Add(brand);
        header.Controls.Add(logo);
        var footer = Ui.Label("© 2026 Thư viện · Trường Đại học Sư phạm Thành phố Hồ Chí Minh                        Năm học 2026 – 2027", color: Ui.Muted, size: 8);
        footer.Dock = DockStyle.Bottom;
        footer.AutoSize = false;
        footer.Height = 55;
        footer.Padding = new Padding(48, 15, 0, 0);
        var center = new Panel { Dock = DockStyle.Fill, Padding = new Padding(70, 48, 70, 40), AutoScroll = true };
        var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.White, MinimumSize = new Size(850, 560) };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        var photo = new LoginPhoto { Dock = DockStyle.Fill, Margin = Padding.Empty };
        var fields = new Panel { Dock = DockStyle.Fill, Padding = new Padding(60, 65, 60, 35), AutoScroll = true };
        username = Ui.Input("Nhập tên tài khoản");
        password = Ui.Input("Nhập mật khẩu");
        password.UseSystemPasswordChar = true;
        var reveal = Ui.Button("", () => password.UseSystemPasswordChar = !password.UseSystemPasswordChar, icon: "eye");
        reveal.AccessibleName = "Hiện hoặc ẩn mật khẩu";
        var passwordRow = Ui.Columns([85, 15], password, reveal);
        remember = new CheckBox { Text = "Ghi nhớ tài khoản", AutoSize = true, Font = Ui.SmallFont };
        try
        {
            if (File.Exists(preference))
            {
                username.Text = File.ReadAllText(preference);
                remember.Checked = true;
            }
        }
        catch (IOException) { }
        validation = Ui.Label("", color: Ui.Red, size: 9);
        var login = Ui.Button("Đăng nhập", Login, true, "arrow-right");
        login.Dock = DockStyle.Top;
        login.Height = 50;
        AcceptButton = login;
        var stack = Ui.Stack(Ui.Label("CHÀO MỪNG BẠN TRỞ LẠI", true, Ui.Muted, 8),
            Ui.Heading("Đăng nhập tài khoản", 25),
            Ui.Label("Sử dụng tài khoản được cấp để truy cập hệ thống quản lý thư viện.", color: Ui.Muted, size: 9),
            new Panel { Height = 20 }, Ui.Field("Tài khoản", username), new Panel { Height = 18 }, Ui.Field("Mật khẩu", passwordRow),
            Ui.Columns([50, 50], remember, Ui.Button("Quên mật khẩu?", () => Ui.Info("Liên hệ quản trị viên thư viện để được cấp lại tài khoản."))),
            validation, login, new Panel { Height = 20 }, Ui.Button("Liên hệ thư viện", () => Ui.Info("Liên hệ thủ thư tại quầy thư viện, cơ sở An Dương Vương."), icon: "circle-help"));
        fields.Controls.Add(stack);
        split.Controls.Add(photo, 0, 0);
        split.Controls.Add(fields, 1, 0);
        center.Controls.Add(split);
        Controls.Add(center);
        Controls.Add(footer);
        Controls.Add(header);
    }

    private void Login()
    {
        bool manager = username.Text.Trim().Equals("quanly", StringComparison.OrdinalIgnoreCase) && password.Text == "Admin@123";
        bool librarian = username.Text.Trim().Equals("thuthu", StringComparison.OrdinalIgnoreCase) && password.Text == "ThuThu@123";
        if (!manager && !librarian)
        {
            validation.Text = "Tài khoản hoặc mật khẩu không đúng.";
            return;
        }
        store.Session.IsManager = manager;
        store.Session.Name = manager ? "Trần Minh Anh" : "Nguyễn Lan Hương";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(preference)!);
            File.WriteAllText(preference, remember.Checked ? username.Text.Trim() : "");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        validation.Text = "";
        Hide();
        using var main = new frmMain(store);
        main.ShowDialog(this);
        password.Clear();
        if (main.LoggedOut)
            Show();
        else
            Close();
    }

    private void OpenLookup()
    {
        using var window = new Form { Text = "Tra cứu thư viện", Size = new Size(1200, 850), StartPosition = FormStartPosition.CenterParent };
        var page = new frmTimKiemSach(store, readOnly: true);
        window.Controls.Add(page);
        page.Show();
        window.ShowDialog(this);
    }

    private void frmDangNhap_Load(object sender, EventArgs e)
    {

    }
}

internal sealed class LoginPhoto : Control
{
    public LoginPhoto()
    {
        DoubleBuffered = true;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var image = Ui.Asset("library.jpg");
        if (image != null)
        {
            float scale = Math.Max((float)Width / image.Width, (float)Height / image.Height);
            e.Graphics.DrawImage(image, new RectangleF((Width - image.Width * scale) / 2, (Height - image.Height * scale) / 2, image.Width * scale, image.Height * scale));
        }
        using var overlay = new SolidBrush(Color.FromArgb(180, 10, 48, 77));
        e.Graphics.FillRectangle(overlay, ClientRectangle);
        using var white = new SolidBrush(Color.White);
        using var soft = new SolidBrush(Color.FromArgb(215, 230, 241));
        using var title = new Font("Georgia", 26, FontStyle.Bold);
        using var small = new Font("Segoe UI", 10);
        e.Graphics.DrawString("THƯ VIỆN HCMUE", Ui.BoldFont, soft, new PointF(34, 42));
        e.Graphics.DrawString("Kết nối tri thức,\nnuôi dưỡng tương lai.", title, white, new RectangleF(34, 92, Width - 68, 180));
        e.Graphics.DrawString("Đồng hành cùng hoạt động học tập, giảng dạy và nghiên cứu của cộng đồng HCMUE.", small, soft, new RectangleF(34, 270, Width - 68, 100));
        e.Graphics.DrawString("Chất lượng – Sáng tạo – Nhân văn", small, white, new RectangleF(34, Height - 100, Width - 68, 52));
        e.Graphics.DrawString("TRƯỜNG ĐẠI HỌC SƯ PHẠM TP. HỒ CHÍ MINH", Ui.SmallFont, soft, new RectangleF(34, Height - 52, Width - 68, 42));
    }
}

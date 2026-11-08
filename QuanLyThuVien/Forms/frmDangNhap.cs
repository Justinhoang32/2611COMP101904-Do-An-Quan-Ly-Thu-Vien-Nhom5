using QuanLyThuVien.Controls;
using QuanLyThuVien.Presenters;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Forms;

/// <summary>
/// Màn hình 1 – Đăng nhập. Chỉ hiển thị và chuyển sự kiện cho Presenter (không có nghiệp vụ, không truy cập database).
/// Bố cục: header (logo + "Tra cứu thư viện") / thẻ giữa (ảnh nền bên trái, form bên phải) / footer.
/// </summary>
public partial class frmDangNhap : Form, IDangNhapView
{
    private readonly DangNhapPresenter _presenter;

    // ----- IDangNhapView -----
    public event EventHandler Loaded;
    public event EventHandler DangNhapClicked;
    public event EventHandler QuenMatKhauClicked;
    public event EventHandler LienHeClicked;
    public event EventHandler TraCuuClicked;

    public string TenDangNhap { get => txtTaiKhoan.Text; set => txtTaiKhoan.Text = value; }
    public string MatKhau => txtMatKhau.Text;
    public bool GhiNho { get => chkGhiNho.Checked; set => chkGhiNho.Checked = value; }

    public frmDangNhap()
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không tạo Presenter
        GanSuKien();
        _presenter = new DangNhapPresenter(this);   // Presenter đăng ký sự kiện của View
    }

    #region Gắn sự kiện và phần không biểu diễn được trong Designer
    // Giao diện tĩnh nằm trong frmDangNhap.Designer.cs (kéo-thả được); ở đây gắn sự kiện, ảnh nền, viền thẻ và căn giữa lúc chạy.
    private void GanSuKien()
    {
        pnlAnh.AnhNen = CauHinhUngDung.TaiAnhNenDangNhap();

        // "Bạn cần hỗ trợ? Liên hệ thư viện" – chỉ phần "Liên hệ thư viện" là liên kết
        lnkLienHe.Links.Clear();
        int batDau = lnkLienHe.Text.IndexOf("Liên hệ", StringComparison.Ordinal);
        lnkLienHe.Links.Add(batDau, lnkLienHe.Text.Length - batDau);

        lnkTraCuu.LinkClicked += (s, e) => TraCuuClicked?.Invoke(this, EventArgs.Empty);
        lnkQuenMatKhau.LinkClicked += (s, e) => QuenMatKhauClicked?.Invoke(this, EventArgs.Empty);
        lnkLienHe.LinkClicked += (s, e) => LienHeClicked?.Invoke(this, EventArgs.Empty);
        btnDangNhap.Click += (s, e) => DangNhapClicked?.Invoke(this, EventArgs.Empty);

        // Viền mỏng quanh thẻ
        pnlThe.Paint += (s, e) =>
        {
            using var pen = new Pen(UiTheme.VienNhat);
            e.Graphics.DrawRectangle(pen, 0, 0, pnlThe.Width - 1, pnlThe.Height - 1);
        };
        // Thẻ có kích thước tối đa và luôn nằm giữa vùng (tính lúc chạy nên nhân theo DPI thật)
        pnlTrungTam.Resize += (s, e) => CanGiuaThe();
        pnlTrungTam.HandleCreated += (s, e) => CanGiuaThe();
        // Căn form giữa theo chiều dọc khi còn dư chỗ; thiếu chỗ thì AutoScroll cho cuộn
        pnlPhai.Resize += (s, e) => CanGiuaDoc();
        tlpForm.SizeChanged += (s, e) => CanGiuaDoc();

        Load += (s, e) => { Loaded?.Invoke(this, EventArgs.Empty); txtTaiKhoan.Focus(); };
    }

    // Căn thẻ giữa vùng, tối đa 1080x660 (96 DPI)
    private void CanGiuaThe()
    {
        var vung = pnlTrungTam.ClientRectangle;
        vung.Inflate(-pnlTrungTam.Padding.Left, -pnlTrungTam.Padding.Top);
        int rong = Math.Min(DpiHelper.Px(pnlTrungTam, 1080), vung.Width);
        int cao = Math.Min(DpiHelper.Px(pnlTrungTam, 660), vung.Height);
        if (rong <= 0 || cao <= 0) return;
        pnlThe.SetBounds(vung.Left + (vung.Width - rong) / 2, vung.Top + (vung.Height - cao) / 2, rong, cao);
    }

    private void CanGiuaDoc()
    {
        int toiThieu = DpiHelper.Px(pnlPhai, 40);
        int tren = Math.Max(toiThieu, (pnlPhai.ClientSize.Height - tlpForm.Height) / 2);
        var moi = new Padding(pnlPhai.Padding.Left, tren, pnlPhai.Padding.Right, toiThieu);
        if (pnlPhai.Padding != moi) pnlPhai.Padding = moi;   // chỉ gán khi khác để không lặp bố cục
    }
    #endregion

    // ----- Phương thức IDangNhapView -----
    public void HienThiLoi(string thongBao) =>
        MessageBox.Show(this, thongBao, "Không thể đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public void HienThiThongTin(string thongBao) =>
        MessageBox.Show(this, thongBao, "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public void DatTrangThaiDangXuLy(bool dangXuLy)
    {
        if (IsDisposed) return;
        btnDangNhap.Enabled = !dangXuLy;
        txtTaiKhoan.Enabled = !dangXuLy;
        txtMatKhau.Enabled = !dangXuLy;
        btnDangNhap.Text = dangXuLy ? "Đang đăng nhập..." : "Đăng nhập";
        UseWaitCursor = dangXuLy;
    }

    public void XoaMatKhau()
    {
        txtMatKhau.Clear();
        txtMatKhau.Focus();
    }

    public void DangNhapThanhCong()
    {
        DialogResult = DialogResult.OK;   // Program.cs sẽ mở khung chính
        Close();
    }
}

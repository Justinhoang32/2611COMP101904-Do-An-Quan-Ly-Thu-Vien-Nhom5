using QuanLyThuVien.Controls;
using QuanLyThuVien.Presenters;
using QuanLyThuVien.UserControls;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Forms;

/// <summary>
/// Khung chính: sidebar (8 mục + mục quản trị) / header (breadcrumb, ngày, người dùng) / vùng nội dung / footer.
/// Các màn hình con là UserControl được nạp vào pnlNoiDung. Form này không có nghiệp vụ, mọi xử lý do MainPresenter đảm nhiệm.
/// </summary>
public partial class frmMain : Form, IMainView, IDieuHuong
{
    private readonly Dictionary<MucMenu, MenuItemSidebar> _menu = new();
    private Control _manHinhHienTai;
    private MucMenu _mucDangChon;

    private readonly MainPresenter _presenter;

    // ----- IMainView -----
    public event EventHandler Loaded;
    public event EventHandler<YeuCauDieuHuong> MenuDuocChon;
    public event EventHandler DangXuatClicked;
    public event EventHandler YeuCauLamMoiHuyHieu;

    public frmMain()
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không tạo Presenter
        GanSuKien();
        _presenter = new MainPresenter(this);
    }

    #region Gắn sự kiện và phần không biểu diễn được trong Designer
    // Khung giao diện (sidebar, header, footer, các mục menu) nằm trong frmMain.Designer.cs (kéo-thả được).
    // Ở đây ánh xạ mục menu <-> MucMenu, gắn sự kiện và vẽ các đường viền.
    private void GanSuKien()
    {
        _menu[MucMenu.TongQuan] = mnuTongQuan;
        _menu[MucMenu.QuanLySach] = mnuQuanLySach;
        _menu[MucMenu.TacGiaTheLoai] = mnuTacGiaTheLoai;
        _menu[MucMenu.DocGia] = mnuDocGia;
        _menu[MucMenu.PhieuMuon] = mnuPhieuMuon;
        _menu[MucMenu.GhiNhanTraSach] = mnuGhiNhanTraSach;
        _menu[MucMenu.TimKiemSach] = mnuTimKiemSach;
        _menu[MucMenu.BaoCaoQuaHan] = mnuBaoCaoQuaHan;
        foreach (var kv in _menu)
        {
            var muc = kv.Key;
            kv.Value.Click += (s, e) => MenuDuocChon?.Invoke(this, new YeuCauDieuHuong(muc));
        }
        lnkDangXuat.LinkClicked += (s, e) => DangXuatClicked?.Invoke(this, EventArgs.Empty);

        pnlSidebar.Paint += (s, e) =>
        {
            using var pen = new Pen(UiTheme.VienNhat);
            e.Graphics.DrawLine(pen, pnlSidebar.Width - 1, 0, pnlSidebar.Width - 1, pnlSidebar.Height);   // viền phải
        };
        tlpHeader.Paint += (s, e) =>
        {
            using var pen = new Pen(UiTheme.VienNhat);
            e.Graphics.DrawLine(pen, 0, tlpHeader.Height - 1, tlpHeader.Width, tlpHeader.Height - 1);
        };

        Load += (s, e) => Loaded?.Invoke(this, EventArgs.Empty);
        FormClosing += (s, e) => _manHinhHienTai?.Dispose();
    }
    #endregion

    // ----- IMainView -----
    public void HienThiNguoiDung(string hoTen, string chucVu, string kyTuVietTat)
    {
        lblHoTen.Text = hoTen;
        lblChucVu.Text = chucVu;
        avatar.KyTu = kyTuVietTat;
    }

    public void HienThiNgayHienTai(string ngay) => lblNgay.Text = ngay;
    public void HienThiNamHoc(string namHoc) => lblNamHoc.Text = namHoc;
    public void HienThiNgayDuLieu(string noiDung) => lblDuLieu.Text = noiDung;

    public void HienThiSoQuaHan(int soPhieu) =>
        _menu[MucMenu.BaoCaoQuaHan].HuyHieu = soPhieu > 0 ? soPhieu.ToString() : null;

    /// <summary>Nạp màn hình tương ứng vào vùng nội dung, đổi mục menu đang chọn và breadcrumb.</summary>
    public void HienThiManHinh(YeuCauDieuHuong yeuCau, string duongDan)
    {
        var moi = TaoManHinh(yeuCau.Muc);
        moi.Dock = DockStyle.Fill;

        // Bỏ màn hình cũ, nạp màn hình mới
        pnlNoiDung.SuspendLayout();
        var cu = _manHinhHienTai;
        pnlNoiDung.Controls.Clear();
        cu?.Dispose();
        pnlNoiDung.Controls.Add(moi);
        _manHinhHienTai = moi;
        pnlNoiDung.ResumeLayout(true);

        if (moi is INhanThamSo nhan) nhan.NhanThamSo(yeuCau.ThamSo);

        // Màn hình phụ tô sáng mục cha trên sidebar
        _mucDangChon = yeuCau.Muc;
        var mucSidebar = yeuCau.Muc is MucMenu.LapPhieuMuon or MucMenu.XemTruocPhieu ? MucMenu.PhieuMuon : yeuCau.Muc;
        foreach (var kv in _menu) kv.Value.DangChon = kv.Key == mucSidebar;

        lblBreadcrumb.Text = "HCMUE  |  Thư viện  /  " + duongDan;
        YeuCauLamMoiHuyHieu?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Nhà máy tạo màn hình. Mỗi mục menu tương ứng một UserControl.
    /// </summary>
    private Control TaoManHinh(MucMenu muc) => muc switch
    {
        MucMenu.TongQuan => new ucTongQuan(this),
        MucMenu.QuanLySach => new ucQuanLySach(),
        MucMenu.TacGiaTheLoai => new ucTacGiaTheLoai(),
        MucMenu.DocGia => new ucDocGia(),
        MucMenu.PhieuMuon => new ucPhieuMuon(this),
        MucMenu.LapPhieuMuon => new ucLapPhieuMuon(this),
        MucMenu.TimKiemSach => new ucTimKiemSach(this),
        MucMenu.GhiNhanTraSach => new ucGhiNhanTraSach(this),
        MucMenu.XemTruocPhieu => new ucXemTruocPhieu(this),
        MucMenu.BaoCaoQuaHan => new ucBaoCaoQuaHan(this),
        _ => throw new ArgumentOutOfRangeException(nameof(muc), muc, "Mục menu chưa được hỗ trợ.")
    };

    // ----- IDieuHuong: màn hình con gọi để chuyển màn hình -----
    public void DieuHuong(MucMenu muc, object thamSo = null) =>
        MenuDuocChon?.Invoke(this, new YeuCauDieuHuong(muc, thamSo));

    public void LamMoiHuyHieuQuaHan() => YeuCauLamMoiHuyHieu?.Invoke(this, EventArgs.Empty);

    public void HienThiThongTin(string thongBao) =>
        MessageBox.Show(this, thongBao, "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public void HienThiLoi(string thongBao) =>
        MessageBox.Show(this, thongBao, "Có lỗi xảy ra", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public bool XacNhan(string cauHoi) =>
        MessageBox.Show(this, cauHoi, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    /// <summary>Đóng khung chính; đăng xuất thì trả Retry để Program.cs mở lại màn hình đăng nhập.</summary>
    public void KetThuc(bool dangXuat)
    {
        DialogResult = dangXuat ? DialogResult.Retry : DialogResult.Cancel;
        Close();
    }
}

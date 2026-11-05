using System.Globalization;
using DTO;
using DTO.Models;
using QuanLyThuVien.Controls;
using QuanLyThuVien.Presenters;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.UserControls;

/// <summary>
/// Màn hình 7 – Phiếu mượn. Thống kê, banner hướng dẫn, các tab trạng thái, bộ lọc từ khóa/ngày và lưới phiếu có phân trang.
/// Chỉ hiển thị, mọi xử lý do PhieuMuonPresenter.
/// </summary>
public partial class ucPhieuMuon : UserControl, IPhieuMuonView
{
    private static readonly CultureInfo Vi = new("vi-VN");
    private static readonly string[] CacTab = { HangSo.TatCa, HangSo.PhieuDangMuon, HangSo.PhieuDaTra, HangSo.PhieuQuaHan };

    private Button[] _nutTab;
    private int _tabHienTai;
    private Label lblTrong;
    private Font _fontDam;

    private readonly PhieuMuonPresenter _presenter;

    #region IPhieuMuonView – thuộc tính và sự kiện
    public string TuKhoa => txtTimKiem.Text.Trim();
    public DateTime? TuNgay => dtpTu.Checked ? dtpTu.Value.Date : null;
    public DateTime? DenNgay => dtpDen.Checked ? dtpDen.Value.Date : null;

    public event EventHandler Loaded;
    public event EventHandler ApDungClicked;
    public event EventHandler<string> TabChanged;
    public event EventHandler<int> TrangChanged;
    public event EventHandler TaoPhieuClicked;
    public event EventHandler<string> TraSachClicked;
    public event EventHandler<string> ChiTietClicked;
    public event EventHandler<string> InClicked;
    public event EventHandler XuatClicked;
    #endregion

    public ucPhieuMuon(IDieuHuong dieuHuong)
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không tạo Presenter
        GanSuKien();
        _presenter = new PhieuMuonPresenter(this, dieuHuong);
    }

    #region Gắn sự kiện và phần không biểu diễn được trong Designer
    // Giao diện tĩnh nằm trong ucPhieuMuon.Designer.cs; ở đây chỉ gắn sự kiện và cấu hình lưới theo DPI.
    private void GanSuKien()
    {
        _fontDam = DpiHelper.TaoFont(9.5f, FontStyle.Bold);
        _nutTab = new Button[] { btnTab0, btnTab1, btnTab2, btnTab3 };
        for (int i = 0; i < 4; i++)
        {
            int chiSo = i;
            _nutTab[i].Click += (s, e) => ChonTab(chiSo, true);
        }
        ChonTab(0, false);

        btnTao.Click += (s, e) => TaoPhieuClicked?.Invoke(this, EventArgs.Empty);
        btnXuat.Click += (s, e) => XuatClicked?.Invoke(this, EventArgs.Empty);
        btnApDung.Click += (s, e) => ApDungClicked?.Invoke(this, EventArgs.Empty);
        txtTimKiem.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApDungClicked?.Invoke(this, EventArgs.Empty); } };
        pagination.TrangChanged += (s, t) => TrangChanged?.Invoke(this, t);

        dgv.CellFormatting += DgvCellFormatting;
        dgv.CellContentClick += DgvCellContentClick;
        dgv.RowHeightChanged += (s, e) => UiForm.CanChieuCao(cardLuoi, 1, dgv, 60);
        lblTrong = UiTheme.TaoNhan("Không có phiếu mượn phù hợp.", 10f, FontStyle.Regular, UiTheme.ChuPhu);
        lblTrong.AutoSize = false; lblTrong.Height = 60; lblTrong.Dock = DockStyle.Bottom; lblTrong.TextAlign = ContentAlignment.MiddleCenter; lblTrong.Visible = false;
        dgv.Controls.Add(lblTrong);

        Load += (s, e) =>
        {
            DpiHelper.CauHinhDataGridView(dgv, 56, 40);
            DpiHelper.DatDoRongToiThieuCot(colPhieu, 110);
            DpiHelper.DatDoRongToiThieuCot(colDocGia, 150);
            DpiHelper.DatDoRongToiThieuCot(colNgay, 120);
            DpiHelper.DatDoRongToiThieuCot(colSoCuon, 70);
            DpiHelper.DatDoRongToiThieuCot(colTrangThai, 110);
            DpiHelper.DatDoRongToiThieuCot(colTra, 70);
            DpiHelper.DatDoRongToiThieuCot(colChiTiet, 70);
            DpiHelper.DatDoRongToiThieuCot(colIn, 40);
            Loaded?.Invoke(this, EventArgs.Empty);
        };
        Disposed += (s, e) => _fontDam?.Dispose();
    }

    // Tô đậm nút tab đang chọn
    private void ChonTab(int chiSo, bool phatSuKien)
    {
        _tabHienTai = chiSo;
        for (int i = 0; i < 4; i++)
        {
            bool chon = i == chiSo;
            _nutTab[i].BackColor = chon ? UiTheme.XanhChinh : Color.White;
            _nutTab[i].ForeColor = chon ? Color.White : UiTheme.XanhChinh;
        }
        if (phatSuKien) TabChanged?.Invoke(this, CacTab[chiSo]);
    }
    #endregion

    #region Lưới
    private void DgvCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != colTrangThai.Index) return;
        e.CellStyle.ForeColor = MauTrangThai(Convert.ToString(e.Value));
        e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
        e.CellStyle.Font = _fontDam;
    }

    private static Color MauTrangThai(string tt) =>
        tt != null && tt.StartsWith("Trễ") ? UiTheme.Do :
        tt == HangSo.PhieuDaTra ? UiTheme.XanhLa :
        tt == HangSo.PhieuTraMotPhan ? UiTheme.Cam : UiTheme.XanhChinh;

    private void DgvCellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || dgv.Rows[e.RowIndex].Tag is not PhieuMuonHienThi p) return;
        if (e.ColumnIndex == colTra.Index && p.SoCuonConMuon > 0) TraSachClicked?.Invoke(this, p.MaPhieu);
        else if (e.ColumnIndex == colChiTiet.Index) ChiTietClicked?.Invoke(this, p.MaPhieu);
        else if (e.ColumnIndex == colIn.Index) InClicked?.Invoke(this, p.MaPhieu);
    }
    #endregion

    #region IPhieuMuonView – hiển thị
    public void HienThiThongKe(ThongKePhieu tk)
    {
        cardDangMuon.GiaTri = tk.DungHan.ToString("N0", Vi);
        cardDangMuon.GhiChu = $"{tk.CuonLuuHanh.ToString("N0", Vi)} cuốn đang lưu hành";
        cardQuaHan.GiaTri = tk.QuaHan.ToString("N0", Vi);
        cardQuaHan.GhiChu = $"{tk.CuonQuaHan.ToString("N0", Vi)} cuốn quá hạn";
        cardDaTra.GiaTri = tk.DaTraTrongThang.ToString("N0", Vi);
        cardDaTra.GhiChu = $"{tk.CuonDaTraTrongThang.ToString("N0", Vi)} cuốn đã nhận lại";
        cardDenHan.GiaTri = tk.DenHanHomNay.ToString("N0", Vi);
        cardDenHan.GhiChu = $"{tk.CuonDenHan.ToString("N0", Vi)} cuốn đến hạn";
        // Số phiếu trên từng tab
        string[] dem = { tk.TongPhieu.ToString(), tk.DungHan.ToString(), tk.DaTraTatCa.ToString(), tk.QuaHan.ToString() };
        for (int i = 0; i < 4; i++) _nutTab[i].Text = $"{CacTab[i]} ({dem[i]})";
    }

    public void HienThiDanhSach(KetQuaPhanTrang<PhieuMuonHienThi> kq)
    {
        dgv.SuspendLayout();
        dgv.Rows.Clear();
        foreach (var p in kq.DuLieu)
        {
            string tt = p.SoNgayTre > 0 && p.TrangThaiHienThi == HangSo.PhieuQuaHan ? $"Trễ {p.SoNgayTre} ngày" : p.TrangThaiHienThi;
            string cuon = p.SoCuonDaTra > 0 ? $"{p.TongSoCuon} ({p.SoCuonDaTra} đã trả)" : p.TongSoCuon.ToString();
            int i = dgv.Rows.Add(
                p.MaPhieu,
                p.HoTenDocGia + "\n" + p.MaDocGia,
                $"{p.NgayMuon:dd/MM/yyyy}\n{p.HenTra:dd/MM/yyyy}",
                cuon, tt,
                p.SoCuonConMuon > 0 ? "Trả sách" : "", "Chi tiết", "In");
            dgv.Rows[i].Tag = p;
            dgv.Rows[i].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }
        dgv.ResumeLayout(true);
        lblTrong.Visible = kq.DuLieu.Count == 0;
        lblHienThi.Text = kq.TongSoDong == 0 ? "Không có kết quả" : $"Hiển thị {kq.TuDong}–{kq.DenDong} / {kq.TongSoDong.ToString("N0", Vi)} phiếu";
        pagination.DatTrang(kq.TrangHienTai, kq.TongSoTrang);
        UiForm.CanChieuCao(cardLuoi, 1, dgv, 60);
    }

    public void XuatExcel(string tenGoiY, string tieuDe, string[] cot, List<string[]> dong)
    {
        try
        {
            string tep = QuanLyThuVien.XuatExcel.Xuat(this, tenGoiY, tieuDe, cot, dong);
            if (tep != null) HienThiThongTin("Đã xuất danh sách ra tệp:\n" + tep);
        }
        catch (IOException) { HienThiLoi("Không thể ghi tệp. Hãy đóng tệp Excel nếu đang mở rồi thử lại."); }
        catch (UnauthorizedAccessException) { HienThiLoi("Không có quyền ghi vào vị trí đã chọn. Vui lòng chọn thư mục khác."); }
    }

    public void DatTrangThaiDangXuLy(bool dangXuLy)
    {
        if (IsDisposed) return;
        UseWaitCursor = dangXuLy;
        btnApDung.Enabled = btnTao.Enabled = !dangXuLy;
    }

    public void HienThiThongTin(string thongBao) => MessageBox.Show(this, thongBao, "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public void HienThiLoi(string thongBao) => MessageBox.Show(this, thongBao, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    #endregion

}

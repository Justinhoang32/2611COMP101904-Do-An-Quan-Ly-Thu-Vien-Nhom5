using DTO;
using DTO.Models;
using QuanLyThuVien.Controls;
using QuanLyThuVien.Presenters;
using QuanLyThuVien.Views;
using BLL;

namespace QuanLyThuVien.UserControls;

/// <summary>
/// Màn hình 9 – Ghi nhận trả sách. Nhập/tải phiếu, chọn từng bản sách trả kèm tình trạng + ghi chú,
/// xem tóm tắt trước khi xác nhận. Chỉ hiển thị; nghiệp vụ nằm ở GhiNhanTraPresenter/TraSachBLL.
/// </summary>
public partial class ucGhiNhanTraSach : UserControl, IGhiNhanTraView, INhanThamSo
{
    private static readonly string[] CacTinhTrang = { HangSo.TinhTrangTot, HangSo.TinhTrangHuHong, HangSo.TinhTrangMat };

    private sealed class DongTra
    {
        public ChiTietPhieuHienThi Ct;
        public CheckBox Chon;
        public ComboBox TinhTrang;
        public TextBox GhiChu;
    }
    private readonly List<DongTra> _dong = new();
    private bool _dangNap, _daLoad;
    private string _maCho;

    private readonly GhiNhanTraPresenter _presenter;

    #region IGhiNhanTraView
    public string MaPhieuNhap => txtMaPhieu.Text.Trim();
    public DateTime NgayTra => dtpNgayTra.Value.Date;
    public string GhiChuChung => txtGhiChuChung.Text.Trim();
    public List<ChiTietTraSach> DanhSachChon => _dong.Where(d => d.Chon.Checked).Select(d => new ChiTietTraSach
    {
        MaSach = d.Ct.MaSach, MaBanSach = d.Ct.MaBanSach, TinhTrang = d.TinhTrang.Text, GhiChu = d.GhiChu.Text
    }).ToList();

    public event EventHandler Loaded;
    public event EventHandler TaiPhieuClicked;
    public event EventHandler VeDanhSachClicked;
    public event EventHandler LuaChonThayDoi;
    public event EventHandler XacNhanClicked;
    public event EventHandler<string> MoPhieu;
    #endregion

    public ucGhiNhanTraSach(IDieuHuong dieuHuong)
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không tạo Presenter
        GanSuKien();
        _presenter = new GhiNhanTraPresenter(this, dieuHuong);
    }

    /// <summary>Nhận mã phiếu khi được mở từ màn Phiếu mượn (có thể đến trước hoặc sau khi màn hình nạp xong).</summary>
    public void NhanThamSo(object thamSo)
    {
        _maCho = thamSo as string;
        PhatMaCho();
    }

    private void PhatMaCho()
    {
        if (!_daLoad || string.IsNullOrEmpty(_maCho)) return;
        string ma = _maCho; _maCho = null;
        txtMaPhieu.Text = ma;
        MoPhieu?.Invoke(this, ma);
    }

    #region Gắn sự kiện và phần không biểu diễn được trong Designer
    // Giao diện tĩnh nằm trong ucGhiNhanTraSach.Designer.cs; các dòng sách là nội dung động nên được dựng khi tải phiếu (xem ThemDong).
    private void GanSuKien()
    {
        dtpNgayTra.MaxDate = DateTime.Today;
        dtpNgayTra.Value = DateTime.Today;
        txtMaPhieu.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; TaiPhieuClicked?.Invoke(this, EventArgs.Empty); } };
        btnTai.Click += (s, e) => TaiPhieuClicked?.Invoke(this, EventArgs.Empty);
        btnVe.Click += (s, e) => VeDanhSachClicked?.Invoke(this, EventArgs.Empty);
        dtpNgayTra.ValueChanged += (s, e) => { if (!_dangNap) LuaChonThayDoi?.Invoke(this, EventArgs.Empty); };
        chkTatCa.CheckedChanged += (s, e) =>
        {
            if (_dangNap) return;
            _dangNap = true;
            foreach (var d in _dong) if (d.Ct.DangMuon) d.Chon.Checked = chkTatCa.Checked;
            _dangNap = false;
            CapNhatBatTat();
            LuaChonThayDoi?.Invoke(this, EventArgs.Empty);
        };
        lnkBoChon.LinkClicked += (s, e) => chkTatCa.Checked = false;
        btnXacNhan.Click += (s, e) => XacNhanClicked?.Invoke(this, EventArgs.Empty);
        Load += (s, e) => { _daLoad = true; Loaded?.Invoke(this, EventArgs.Empty); PhatMaCho(); };
        XoaPhieu();
    }
    #endregion

    #region IGhiNhanTraView – hiển thị
    public void XoaPhieu()
    {
        _dong.Clear();
        tlpDong.Controls.Clear(); tlpDong.RowStyles.Clear(); tlpDong.RowCount = 0;
        tdkPhieu.TieuDe = "Phiếu mượn";
        lblChipTrangThai.Visible = false;
        lblTenDocGia.Text = ""; lblThongTinDocGia.Text = "Nhập mã phiếu rồi bấm \"Tải phiếu\".";
        lblNgayMuon.Text = lblHenTra.Text = "—";
        tdkChon.MoTa = "Chưa có phiếu nào được tải.";
        chkTatCa.Text = "Chọn tất cả";
        btnXacNhan.Enabled = false;
        btnXacNhan.Text = "✓  Xác nhận trả";
        hopThongTin.Visible = false;
        lblTong.Text = lblChonLanNay.Text = lblTot.Text = lblHuHong.Text = lblMat.Text = lblConMuon.Text = "0 cuốn";
        lblSau.Text = "—";
        lblThuThu.Text = "";
    }

    public void HienThiPhieu(PhieuMuonDayDu p)
    {
        _dangNap = true;
        try
        {
            XoaPhieu();
            txtMaPhieu.Text = p.Phieu.MaPhieu;
            tdkPhieu.TieuDe = "Phiếu mượn · " + p.Phieu.MaPhieu;
            lblChipTrangThai.Text = "• " + p.Phieu.TrangThaiHienThi;
            var mau = p.Phieu.TrangThaiHienThi == HangSo.PhieuQuaHan ? UiTheme.Do : p.Phieu.TrangThaiHienThi == HangSo.PhieuTraMotPhan ? UiTheme.Cam : UiTheme.XanhChinh;
            lblChipTrangThai.ForeColor = mau; lblChipTrangThai.BackColor = UiForm.Pha(mau, 0.88f); lblChipTrangThai.Visible = true;
            lblTenDocGia.Text = p.Phieu.HoTenDocGia;
            lblThongTinDocGia.Text = $"{p.Phieu.MaDocGia} · {p.Phieu.NhomDocGia} · {p.DocGia?.DienThoai}";
            lblNgayMuon.Text = p.Phieu.NgayMuon?.ToString("dd/MM/yyyy");
            lblHenTra.Text = p.Phieu.HenTra?.ToString("dd/MM/yyyy");
            lblThuThu.Text = "Thủ thư tiếp nhận: " + (PhienDangNhap.NguoiDungHienTai?.HoTen ?? "") + " · " + DateTime.Now.ToString("dd/MM/yyyy · HH:mm");
            // Ngày trả không được trước ngày mượn
            dtpNgayTra.MinDate = p.Phieu.NgayMuon ?? DateTimePicker.MinimumDateTime;
            if (dtpNgayTra.Value < dtpNgayTra.MinDate) dtpNgayTra.Value = dtpNgayTra.MinDate;

            int dangMuon = p.ChiTiet.Count(c => c.DangMuon);
            tdkChon.MoTa = $"{dangMuon} bản đang mượn · Có thể trả một phần hoặc toàn bộ phiếu.";
            chkTatCa.Text = $"Chọn tất cả ({dangMuon})";

            // Tiêu đề cột
            string[] td = { "CHỌN", "SÁCH / MÃ", "MÃ BẢN", "TÌNH TRẠNG KHI TRẢ", "GHI CHÚ TỪNG CUỐN" };
            tlpDong.SuspendLayout();
            tlpDong.RowStyles.Add(new RowStyle(SizeType.AutoSize)); tlpDong.RowCount = 1;
            for (int i = 0; i < 5; i++)
            {
                var l = UiTheme.TaoNhan(td[i], 8.5f, FontStyle.Bold, UiTheme.ChuPhu);
                l.Dock = DockStyle.Fill; l.Padding = new Padding(8, 10, 4, 10); l.BackColor = UiTheme.NenHover; l.TextAlign = ContentAlignment.MiddleLeft;
                tlpDong.Controls.Add(l, i, 0);
            }
            foreach (var ct in p.ChiTiet) ThemDong(ct);
            tlpDong.ResumeLayout(true);
            CapNhatBatTat();
        }
        finally { _dangNap = false; }
    }

    // Thêm một dòng bản sách vào bảng
    private void ThemDong(ChiTietPhieuHienThi ct)
    {
        int r = tlpDong.RowCount;
        tlpDong.RowStyles.Add(new RowStyle(SizeType.AutoSize)); tlpDong.RowCount = r + 1;
        var d = new DongTra { Ct = ct };
        var chon = new CheckBox { AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(12, 14, 8, 14), Enabled = ct.DangMuon };
        var ten = UiTheme.TaoBangMotCot(); ten.AutoSize = true; ten.Dock = DockStyle.Fill; ten.Margin = new Padding(0, 8, 4, 8);
        UiTheme.ThemHang(ten, UiTheme.TaoNhan(ct.TenSach, 9.5f, FontStyle.Bold));
        UiTheme.ThemHang(ten, UiTheme.TaoNhan($"{ct.MaSach} · {ct.TenTacGia}", 8.5f, FontStyle.Regular, UiTheme.ChuPhu));
        var ma = UiTheme.TaoNhan(ct.MaBanSach, 10f); ma.Anchor = AnchorStyles.Left; ma.Margin = new Padding(8, 0, 4, 0);
        tlpDong.Controls.Add(chon, 0, r); tlpDong.Controls.Add(ten, 1, r); tlpDong.Controls.Add(ma, 2, r);

        if (ct.DangMuon)
        {
            var cbo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Font = DpiHelper.TaoFont(9.5f), Anchor = AnchorStyles.Left | AnchorStyles.Right, Margin = new Padding(8, 8, 8, 8), Enabled = false };
            cbo.Items.AddRange(CacTinhTrang); cbo.SelectedIndex = 0;
            var txt = new TextBox { Font = DpiHelper.TaoFont(9.5f), PlaceholderText = "Không có ghi chú", Anchor = AnchorStyles.Left | AnchorStyles.Right, Margin = new Padding(0, 8, 12, 8), Enabled = false };
            d.Chon = chon; d.TinhTrang = cbo; d.GhiChu = txt;
            chon.CheckedChanged += (s, e) => { if (_dangNap) return; CapNhatBatTat(); LuaChonThayDoi?.Invoke(this, EventArgs.Empty); };
            cbo.SelectedIndexChanged += (s, e) => { TinhTrangMau(d); if (!_dangNap) LuaChonThayDoi?.Invoke(this, EventArgs.Empty); };
            tlpDong.Controls.Add(cbo, 3, r); tlpDong.Controls.Add(txt, 4, r);
            TinhTrangMau(d);
        }
        else
        {
            // Bản đã trả ở lần trước: chỉ hiển thị
            d.Chon = chon; d.TinhTrang = new ComboBox(); d.GhiChu = new TextBox();
            var tt = UiTheme.TaoNhan(ct.TinhTrangKhiTra + (ct.NgayTra.HasValue ? " · " + ct.NgayTra.Value.ToString("dd/MM/yyyy") : ""), 9.5f, FontStyle.Regular, UiTheme.XanhLa);
            tt.Anchor = AnchorStyles.Left; tt.Margin = new Padding(8, 0, 4, 0);
            var gc = UiTheme.TaoNhan("Đã trả", 9f, FontStyle.Regular, UiTheme.ChuPhu); gc.Anchor = AnchorStyles.Left;
            tlpDong.Controls.Add(tt, 3, r); tlpDong.Controls.Add(gc, 4, r);
        }
        if (ct.DangMuon) _dong.Add(d);       // dòng đã trả không tham gia lựa chọn
    }

    // Tô màu ô tình trạng (Tốt = xanh lá, Hư hỏng = cam, Mất = đỏ)
    private static void TinhTrangMau(DongTra d)
    {
        var mau = d.TinhTrang.Text == HangSo.TinhTrangMat ? UiTheme.Do : d.TinhTrang.Text == HangSo.TinhTrangHuHong ? UiTheme.Cam : UiTheme.XanhLa;
        d.TinhTrang.BackColor = UiForm.Pha(mau, 0.88f);
        d.TinhTrang.ForeColor = mau;
    }

    // Bật/tắt ô tình trạng + ghi chú theo ô chọn; cập nhật "Chọn tất cả"
    private void CapNhatBatTat()
    {
        foreach (var d in _dong) { d.TinhTrang.Enabled = d.Chon.Checked; d.GhiChu.Enabled = d.Chon.Checked; }
    }

    public void HienThiTomTat(PhieuMuonDayDu p, KetQuaTraSach t, int soChon)
    {
        int tong = p.ChiTiet.Count;
        lblTong.Text = tong + " cuốn";
        lblChonLanNay.Text = soChon + " cuốn";
        lblTot.Text = t.SoTot + " cuốn";
        lblHuHong.Text = t.SoHuHong + " cuốn";
        lblMat.Text = t.SoMat + " cuốn";
        lblConMuon.Text = t.SoConMuon + " cuốn";
        var mau = soChon == 0 ? UiTheme.ChuPhu : t.SoConMuon == 0 ? UiTheme.XanhLa : UiTheme.Cam;
        lblSau.Text = soChon == 0 ? "—" : "• " + t.TrangThaiPhieu;
        lblSau.ForeColor = mau; lblSau.BackColor = UiForm.Pha(mau, 0.88f);
        btnXacNhan.Enabled = soChon > 0;
        btnXacNhan.Text = soChon > 0 ? $"✓  Xác nhận trả {soChon} cuốn" : "✓  Xác nhận trả";
        if (soChon > 0 && t.SoConMuon > 0)
        {
            hopThongTin.Text = $"Trả một phần · {t.SoConMuon} cuốn tiếp tục được mượn\nSau xác nhận, phiếu chuyển sang Trả một phần. Hạn trả giữ nguyên {p.Phieu.HenTra:dd/MM/yyyy}. Chỉ bản có tình trạng Tốt được bổ sung lại vào số lượng có sẵn.";
            hopThongTin.Visible = true;
        }
        else if (soChon > 0)
        {
            hopThongTin.Text = "Trả toàn bộ phiếu\nSau xác nhận, phiếu chuyển sang Đã trả. Chỉ bản có tình trạng Tốt được bổ sung lại vào số lượng có sẵn.";
            hopThongTin.Visible = true;
        }
        else hopThongTin.Visible = false;
    }

    public bool XacNhan(string cauHoi) =>
        MessageBox.Show(this, cauHoi, "Xác nhận trả sách", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public void DatTrangThaiDangXuLy(bool dangXuLy)
    {
        if (IsDisposed) return;
        UseWaitCursor = dangXuLy;
        btnTai.Enabled = !dangXuLy;
        btnXacNhan.Enabled = !dangXuLy && _dong.Any(d => d.Chon.Checked);
    }

    public void HienThiThongTin(string thongBao) => MessageBox.Show(this, thongBao, "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public void HienThiLoi(string thongBao) => MessageBox.Show(this, thongBao, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    #endregion
}

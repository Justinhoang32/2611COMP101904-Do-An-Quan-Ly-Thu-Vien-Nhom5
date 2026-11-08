using System.Globalization;
using DTO;
using DTO.Entities;
using DTO.Models;
using QuanLyThuVien.Controls;
using QuanLyThuVien.Forms;
using QuanLyThuVien.Presenters;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.UserControls;

/// <summary>
/// Màn hình 4 – Độc giả. Bên trái: thống kê + danh sách có lọc/phân trang. Bên phải: hồ sơ, cấp/gia hạn thẻ, xem trước và in thẻ.
/// Chỉ hiển thị và chuyển sự kiện cho DocGiaPresenter.
/// </summary>
public partial class ucDocGia : UserControl, IDocGiaView
{
    private static readonly CultureInfo Vi = new("vi-VN");

    private Label lblTrong;
    private Font _fontDam;
    // Panel hồ sơ
    private DocGiaHienThi _hoSo;

    private readonly DocGiaPresenter _presenter;

    #region IDocGiaView – thuộc tính và sự kiện
    public string TuKhoaLoc => txtTimKiem.Text.Trim();
    public string NhomLoc => cboNhomLoc.SelectedIndex <= 0 ? null : cboNhomLoc.Text;
    public string TrangThaiLoc => cboTrangThaiLoc.SelectedIndex <= 0 ? null : cboTrangThaiLoc.Text;
    public string NhomThe => cboNhomThe.Text;
    public DateTime NgayCap => dtpNgayCap.Value.Date;
    public DateTime HanThe => dtpHanThe.Value.Date;

    public event EventHandler Loaded;
    public event EventHandler LocClicked;
    public event EventHandler<int> TrangChanged;
    public event EventHandler<string> DocGiaDuocChon;
    public event EventHandler ThemClicked;
    public event EventHandler SuaClicked;
    public event EventHandler CapTheClicked;
    public event EventHandler InTheClicked;
    public event EventHandler XuatClicked;
    #endregion

    public ucDocGia()
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không tạo Presenter
        GanSuKien();
        _presenter = new DocGiaPresenter(this);
    }

    #region Gắn sự kiện và phần không biểu diễn được trong Designer
    // Giao diện tĩnh nằm trong ucDocGia.Designer.cs; ở đây gắn sự kiện, cấu hình lưới theo DPI và các phần vẽ/dữ liệu động.
    private void GanSuKien()
    {
        _fontDam = DpiHelper.TaoFont(9.5f, FontStyle.Bold);
        cardSapHetHan.MauGiaTri = UiTheme.Do;
        dtpHanThe.Value = DateTime.Today.AddYears(4);

        btnThem.Click += (s, e) => ThemClicked?.Invoke(this, EventArgs.Empty);
        btnXuat.Click += (s, e) => XuatClicked?.Invoke(this, EventArgs.Empty);
        btnLoc.Click += (s, e) => LocClicked?.Invoke(this, EventArgs.Empty);
        txtTimKiem.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LocClicked?.Invoke(this, EventArgs.Empty); } };
        lnkSua.LinkClicked += (s, e) => SuaClicked?.Invoke(this, EventArgs.Empty);
        btnInThe.Click += (s, e) => InTheClicked?.Invoke(this, EventArgs.Empty);
        btnCapThe.Click += (s, e) => CapTheClicked?.Invoke(this, EventArgs.Empty);
        pagination.TrangChanged += (s, t) => TrangChanged?.Invoke(this, t);

        // Xem trước thẻ cập nhật ngay khi sửa các ô
        EventHandler capNhatThe = (s, e) => CapNhatTheXemTruoc();
        cboNhomThe.SelectedIndexChanged += capNhatThe;
        dtpHanThe.ValueChanged += capNhatThe;

        dgv.CellPainting += DgvCellPainting;
        dgv.CellFormatting += DgvCellFormatting;
        dgv.CellClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && dgv.Rows[e.RowIndex].Tag is DocGiaHienThi d) DocGiaDuocChon?.Invoke(this, d.MaDocGia);
        };
        dgv.RowHeightChanged += (s, e) => UiForm.CanChieuCao(cardLuoi, 1, dgv, 60);
        lblTrong = UiTheme.TaoNhan("Không tìm thấy độc giả phù hợp.", 10f, FontStyle.Regular, UiTheme.ChuPhu);
        lblTrong.AutoSize = false; lblTrong.Height = 60; lblTrong.Dock = DockStyle.Bottom; lblTrong.TextAlign = ContentAlignment.MiddleCenter; lblTrong.Visible = false;
        dgv.Controls.Add(lblTrong);

        Load += (s, e) =>
        {
            DpiHelper.CauHinhDataGridView(dgv, 60, 40);
            DpiHelper.DatDoRongToiThieuCot(colDocGia, 170);
            DpiHelper.DatDoRongToiThieuCot(colNhom, 80);
            DpiHelper.DatDoRongToiThieuCot(colHan, 90);
            DpiHelper.DatDoRongToiThieuCot(colTrangThai, 100);
            DpiHelper.DatDoRongToiThieuCot(colXem, 50);
            Loaded?.Invoke(this, EventArgs.Empty);
        };
        Disposed += (s, e) => _fontDam?.Dispose();
    }
    #endregion

    #region Lưới
    private void DgvCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != colDocGia.Index || dgv.Rows[e.RowIndex].Tag is not DocGiaHienThi d) return;
        e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
        var vung = Rectangle.Inflate(e.CellBounds, -DpiHelper.Px(dgv, 8), -DpiHelper.Px(dgv, 4));
        const TextFormatFlags co = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis;
        using var fNho = DpiHelper.TaoFont(8.5f);
        string phu = d.MaDocGia + (string.IsNullOrWhiteSpace(d.MaSinhVien) ? "" : "  •  " + d.MaSinhVien);
        var k1 = TextRenderer.MeasureText(e.Graphics, d.HoTen, _fontDam, new Size(vung.Width, int.MaxValue), co);
        var k2 = TextRenderer.MeasureText(e.Graphics, phu, fNho, new Size(vung.Width, int.MaxValue), co);
        int y = vung.Top + Math.Max(0, (vung.Height - k1.Height - k2.Height - 2) / 2);
        TextRenderer.DrawText(e.Graphics, d.HoTen, _fontDam, new Rectangle(vung.Left, y, vung.Width, k1.Height), UiTheme.ChuChinh, co);
        TextRenderer.DrawText(e.Graphics, phu, fNho, new Rectangle(vung.Left, y + k1.Height + 2, vung.Width, k2.Height), UiTheme.ChuPhu, co);
        e.Handled = true;
    }

    private void DgvCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != colTrangThai.Index) return;
        e.CellStyle.ForeColor = MauTrangThai(Convert.ToString(e.Value));
        e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
        e.CellStyle.Font = _fontDam;
    }

    private static Color MauTrangThai(string tt) => tt switch
    {
        HangSo.TheHoatDong => UiTheme.XanhLa,
        HangSo.TheSapHetHan => UiTheme.Cam,
        _ => UiTheme.Do
    };
    #endregion

    #region IDocGiaView – hiển thị
    public void HienThiThongKe(ThongKeDocGia tk)
    {
        cardHoatDong.GiaTri = tk.DangHoatDong.ToString("N0", Vi);
        cardSinhVien.GiaTri = tk.SinhVien.ToString("N0", Vi);
        cardGvCb.GiaTri = tk.GiangVienCanBo.ToString("N0", Vi);
        cardSapHetHan.GiaTri = tk.SapHetHan.ToString("00");
    }

    public void HienThiDanhSach(KetQuaPhanTrang<DocGiaHienThi> kq, string maDangChon)
    {
        dgv.SuspendLayout();
        dgv.Rows.Clear();
        foreach (var d in kq.DuLieu)
        {
            int i = dgv.Rows.Add(d.HoTen + "\n" + d.MaDocGia, d.NhomDocGia, d.HanThe?.ToString("dd/MM/yyyy"), d.TrangThaiHienThi, null);
            dgv.Rows[i].Tag = d;
            if (d.MaDocGia == maDangChon) dgv.Rows[i].Selected = true;
        }
        dgv.ResumeLayout(true);
        lblTrong.Visible = kq.DuLieu.Count == 0;
        lblHienThi.Text = kq.TongSoDong == 0 ? "Không có kết quả" : $"Hiển thị {kq.TuDong}–{kq.DenDong} / {kq.TongSoDong.ToString("N0", Vi)} kết quả";
        pagination.DatTrang(kq.TrangHienTai, kq.TongSoTrang);
        UiForm.CanChieuCao(cardLuoi, 1, dgv, 60);
    }

    public void HienThiHoSo(DocGiaHienThi d)
    {
        _hoSo = d;
        lblChuaChon.Visible = d == null;
        tlpNoiDungHoSo.Visible = d != null;
        if (d == null) return;

        lblDangChon.Text = "Đang chọn " + d.MaDocGia;
        lblTen.Text = d.HoTen;
        string chip = d.TrangThaiHienThi == HangSo.TheHoatDong ? "Hợp lệ" : d.TrangThaiHienThi;
        var mau = MauTrangThai(d.TrangThaiHienThi);
        lblChipTrangThai.Text = chip;
        lblChipTrangThai.ForeColor = mau;
        lblChipTrangThai.BackColor = UiForm.Pha(mau, 0.88f);
        lblKhoa.Text = string.IsNullOrWhiteSpace(d.Khoa) ? "—" : d.Khoa;
        lblMaSv.Text = string.IsNullOrWhiteSpace(d.MaSinhVien) ? "—" : d.MaSinhVien;
        lblLienHe.Text = string.IsNullOrWhiteSpace(d.DienThoai) ? "—" : d.DienThoai;
        lblEmail.Text = string.IsNullOrWhiteSpace(d.Email) ? "—" : d.Email;
        lblDangMuon.Text = $"Đang mượn: {d.SoSachDangMuon} / {HangSo.HanMucSachMoiDocGia} cuốn";
        lblQuaHan.Text = $"Quá hạn: {d.SoPhieuQuaHan}";
        var mauQh = d.SoPhieuQuaHan > 0 ? UiTheme.Do : UiTheme.XanhLa;
        lblQuaHan.ForeColor = mauQh;
        lblQuaHan.BackColor = UiForm.Pha(mauQh, 0.9f);

        txtMaThe.Text = d.MaDocGia;
        int i = cboNhomThe.FindStringExact(d.NhomDocGia ?? "");
        cboNhomThe.SelectedIndex = i >= 0 ? i : 0;
        dtpNgayCap.Value = d.NgayCap ?? DateTime.Today;
        dtpHanThe.Value = d.HanThe ?? DateTime.Today.AddYears(4);
        CapNhatTheXemTruoc();
    }

    private void CapNhatTheXemTruoc()
    {
        if (_hoSo == null) return;
        theXemTruoc.DatNoiDung(_hoSo.HoTen, _hoSo.MaDocGia, cboNhomThe.Text, dtpHanThe.Value.Date);
    }

    public DocGia NhapHoSo(DocGia khoiTao, bool them)
    {
        using var hop = new frmHoSoDocGia(khoiTao, them);
        return hop.ShowDialog(this) == DialogResult.OK ? hop.KetQua : null;
    }

    // In thẻ độc giả đang chọn (thẻ vẽ cùng hàm với bản xem trước, kích thước 85,6 x 54 mm)
    public void InThe()
    {
        if (_hoSo == null) return;
        string ten = _hoSo.HoTen, ma = _hoSo.MaDocGia, nhom = cboNhomThe.Text, han = "Hạn: " + dtpHanThe.Value.ToString("dd/MM/yyyy");
        InHelper.In(this, "Thẻ độc giả " + ma, (g, vung) =>
        {
            const float rong = 337f, cao = 213f;   // 1/100 inch ≈ 85,6 x 54 mm
            UcTheDocGia.Ve(g, new RectangleF(vung.Left, vung.Top, rong, cao), ten, ma, nhom, han);
        });
    }

    public void XuatExcel(string tenGoiY, string tieuDe, string[] cot, List<string[]> dong)
    {
        try
        {
            string tep = GhiExcel(tenGoiY, tieuDe, cot, dong);
            if (tep != null) HienThiThongTin("Đã xuất danh sách ra tệp:\n" + tep);
        }
        catch (IOException) { HienThiLoi("Không thể ghi tệp. Hãy đóng tệp Excel nếu đang mở rồi thử lại."); }
        catch (UnauthorizedAccessException) { HienThiLoi("Không có quyền ghi vào vị trí đã chọn. Vui lòng chọn thư mục khác."); }
    }

    private string GhiExcel(string tenGoiY, string tieuDe, string[] cot, List<string[]> dong) =>
        QuanLyThuVien.XuatExcel.Xuat(this, tenGoiY, tieuDe, cot, dong);

    public void DatTrangThaiDangXuLy(bool dangXuLy)
    {
        if (IsDisposed) return;
        UseWaitCursor = dangXuLy;
        btnLoc.Enabled = btnThem.Enabled = !dangXuLy;
    }

    public void HienThiThongTin(string thongBao) => MessageBox.Show(this, thongBao, "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public void HienThiLoi(string thongBao) => MessageBox.Show(this, thongBao, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    #endregion

}

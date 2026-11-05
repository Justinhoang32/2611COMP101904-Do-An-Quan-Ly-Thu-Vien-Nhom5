using System.Globalization;
using DTO;
using DTO.Entities;
using DTO.Models;
using QuanLyThuVien.Controls;
using QuanLyThuVien.Presenters;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.UserControls;

/// <summary>
/// Màn hình 3 – Quản lý sách. Chỉ hiển thị và chuyển sự kiện cho SachPresenter.
/// Bố cục (cuộn dọc được): tiêu đề + 2 nút / bộ lọc / 3 thẻ thống kê / lưới sách + phân trang / panel thêm-sửa đầu sách.
/// </summary>
public partial class ucQuanLySach : UserControl, ISachView
{
    private static readonly CultureInfo Vi = new("vi-VN");

    private Label lblTrong;
    private readonly List<Image> _anhThuNho = new();
    private Font _fontDam, _fontThuong;
    private string _maDangSua;
    private byte[] _anhBiaMoi;

    private readonly SachPresenter _presenter;

    #region ISachView – thuộc tính và sự kiện
    public string TuKhoaLoc => txtTimKiem.Text.Trim();
    public string MaTheLoaiLoc { get { var v = cboTheLoaiLoc.SelectedValue as string; return string.IsNullOrEmpty(v) ? null : v; } }
    public string TinhTrangLoc => cboTinhTrangLoc.SelectedIndex <= 0 ? null : cboTinhTrangLoc.Text;

    public string MaSachDangSua => _maDangSua;
    public string TenSach => txtTenSach.Text;
    public string MaTacGia => cboTacGia.SelectedValue as string;
    public string MaTheLoai => cboTheLoai.SelectedValue as string;
    public string NamXuatBan => txtNamXB.Text;
    public int TongSoBan => (int)nudTongSo.Value;
    public string ViTriKe => txtViTri.Text;
    public string NhaXuatBan => txtNXB.Text;
    public string NgonNgu => txtNgonNgu.Text;
    public string TinhTrangLuuHanh => cboLuuHanh.Text;
    public byte[] AnhBiaMoi => _anhBiaMoi;

    public event EventHandler Loaded;
    public event EventHandler LocClicked;
    public event EventHandler<int> TrangChanged;
    public event EventHandler ThemClicked;
    public event EventHandler<string> SuaClicked;
    public event EventHandler LuuClicked;
    public event EventHandler HuyClicked;
    public event EventHandler NhapDanhMucClicked;
    public event EventHandler AnhBiaDaChon;
    #endregion

    public ucQuanLySach()
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không tạo Presenter
        GanSuKien();
        _presenter = new SachPresenter(this);
    }

    #region Gắn sự kiện và phần không biểu diễn được trong Designer
    // Giao diện tĩnh nằm trong ucQuanLySach.Designer.cs; ở đây gắn sự kiện, cấu hình lưới theo DPI và các phần vẽ/dữ liệu động.
    private void GanSuKien()
    {
        _fontDam = DpiHelper.TaoFont(9.5f, FontStyle.Bold);
        _fontThuong = DpiHelper.TaoFont(9.5f);

        btnNhapDanhMuc.Click += (s, e) => NhapDanhMucClicked?.Invoke(this, EventArgs.Empty);
        btnThemDauSach.Click += (s, e) => ThemClicked?.Invoke(this, EventArgs.Empty);
        btnLoc.Click += (s, e) => LocClicked?.Invoke(this, EventArgs.Empty);
        txtTimKiem.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LocClicked?.Invoke(this, EventArgs.Empty); }
        };
        btnDoiAnh.Click += (s, e) => ChonAnhBia();
        btnHuy.Click += (s, e) => HuyClicked?.Invoke(this, EventArgs.Empty);
        btnLuu.Click += (s, e) => LuuClicked?.Invoke(this, EventArgs.Empty);
        pagination.TrangChanged += (s, trang) => TrangChanged?.Invoke(this, trang);

        // Vẽ ô "SÁCH / MÃ": tên sách đậm, dòng dưới là mã và nhà xuất bản
        dgvSach.CellPainting += DgvSach_CellPainting;
        dgvSach.CellFormatting += DgvSach_CellFormatting;
        dgvSach.CellContentClick += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == colThaoTac.Index) MoSuaDong(e.RowIndex); };
        dgvSach.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) MoSuaDong(e.RowIndex); };
        dgvSach.RowHeightChanged += (s, e) => CanChieuCaoLuoi();

        // Thông báo khi không có dữ liệu (nằm trong lưới)
        lblTrong = UiTheme.TaoNhan("Không tìm thấy sách phù hợp với điều kiện lọc.", 10f, FontStyle.Regular, UiTheme.ChuPhu);
        lblTrong.Visible = false;
        lblTrong.Dock = DockStyle.Bottom;
        lblTrong.TextAlign = ContentAlignment.MiddleCenter;
        lblTrong.AutoSize = false;
        lblTrong.Height = 60;
        dgvSach.Controls.Add(lblTrong);

        Load += (s, e) =>
        {
            // Cấu hình lưới sau khi control đã co giãn theo DPI
            DpiHelper.CauHinhDataGridView(dgvSach, 64, 40);
            DpiHelper.DatDoRongToiThieuCot(colAnh, 56);
            DpiHelper.DatDoRongToiThieuCot(colSach, 180);
            DpiHelper.DatDoRongToiThieuCot(colTacGia, 120);
            DpiHelper.DatDoRongToiThieuCot(colTheLoai, 100);
            DpiHelper.DatDoRongToiThieuCot(colSoLuong, 80);
            DpiHelper.DatDoRongToiThieuCot(colTrangThai, 90);
            DpiHelper.DatDoRongToiThieuCot(colThaoTac, 60);
            Loaded?.Invoke(this, EventArgs.Empty);
        };
        Disposed += (s, e) =>
        {
            foreach (var a in _anhThuNho) a.Dispose();
            picBia?.Image?.Dispose();
            _fontDam?.Dispose();
            _fontThuong?.Dispose();
        };
    }
    #endregion

    #region Lưới: vẽ và xử lý
    private void DgvSach_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != colSach.Index) return;
        if (dgvSach.Rows[e.RowIndex].Tag is not SachHienThi s) return;

        // Vẽ nền, viền và màu chọn mặc định; phần chữ tự vẽ để có hai kiểu chữ
        e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
        var vung = Rectangle.Inflate(e.CellBounds, -DpiHelper.Px(dgvSach, 8), -DpiHelper.Px(dgvSach, 4));
        const TextFormatFlags co = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis;
        using var fNho = DpiHelper.TaoFont(8.5f);
        string phu = s.MaSach + (string.IsNullOrWhiteSpace(s.NhaXuatBan) ? "" : "  •  " + s.NhaXuatBan);
        var kt1 = TextRenderer.MeasureText(e.Graphics, s.TenSach, _fontDam, new Size(vung.Width, int.MaxValue), co);
        var kt2 = TextRenderer.MeasureText(e.Graphics, phu, fNho, new Size(vung.Width, int.MaxValue), co);
        int y = vung.Top + Math.Max(0, (vung.Height - kt1.Height - kt2.Height - 2) / 2);   // căn giữa theo chiều dọc
        TextRenderer.DrawText(e.Graphics, s.TenSach, _fontDam, new Rectangle(vung.Left, y, vung.Width, kt1.Height), UiTheme.ChuChinh, co);
        TextRenderer.DrawText(e.Graphics, phu, fNho, new Rectangle(vung.Left, y + kt1.Height + 2, vung.Width, kt2.Height), UiTheme.ChuPhu, co);
        e.Handled = true;
    }

    private void DgvSach_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (e.ColumnIndex == colTrangThai.Index)
        {
            // Có sẵn = xanh lá, Hết sách = đỏ
            bool coSan = Convert.ToString(e.Value) == HangSo.SachCoSan;
            e.CellStyle.ForeColor = coSan ? UiTheme.XanhLa : UiTheme.Do;
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            e.CellStyle.Font = _fontDam;
        }
    }

    private void MoSuaDong(int chiSoDong)
    {
        if (dgvSach.Rows[chiSoDong].Tag is SachHienThi s) SuaClicked?.Invoke(this, s.MaSach);
    }

    // Đặt chiều cao thẻ lưới vừa đủ chứa tiêu đề cột + các dòng (tính theo chiều cao thật nên đúng ở mọi DPI)
    private void CanChieuCaoLuoi()
    {
        if (cardLuoi == null || dgvSach == null) return;
        int noiDung = dgvSach.ColumnHeadersHeight + dgvSach.Rows.GetRowsHeight(DataGridViewElementStates.Visible);
        if (dgvSach.Rows.Count == 0) noiDung += lblTrong.Height;
        int cao = Math.Max(noiDung + 2, DpiHelper.Px(dgvSach, 110));
        if (cardLuoi.RowStyles[0].Height != cao) cardLuoi.RowStyles[0].Height = cao;
    }
    #endregion

    #region ISachView – hiển thị
    public void NapTheLoai(List<TheLoai> ds)
    {
        // Bộ lọc có thêm mục "Tất cả thể loại" ở đầu
        var locDs = new List<TheLoai> { new TheLoai { MaTheLoai = "", TenTheLoai = "Tất cả thể loại" } };
        locDs.AddRange(ds);
        cboTheLoaiLoc.DataSource = locDs;
        cboTheLoai.DataSource = ds.ToList();
        cboTheLoai.SelectedIndex = -1;
    }

    public void NapTacGia(List<TacGia> ds)
    {
        cboTacGia.DataSource = ds.ToList();
        cboTacGia.SelectedIndex = -1;
    }

    public void HienThiThongKe(ThongKeSach tk)
    {
        cardDauSach.GiaTri = tk.SoDauSach.ToString("N0", Vi);
        cardBanSach.GiaTri = tk.SoBanSach.ToString("N0", Vi);
        cardHetBan.GiaTri = tk.SoDauSachHetBan.ToString("N0", Vi);
    }

    public void HienThiDanhSach(KetQuaPhanTrang<SachHienThi> kq)
    {
        dgvSach.SuspendLayout();
        dgvSach.Rows.Clear();
        foreach (var a in _anhThuNho) a.Dispose();   // giải phóng ảnh thu nhỏ của trang trước
        _anhThuNho.Clear();

        var kichThuocAnh = DpiHelper.Px(dgvSach, new Size(40, 54));
        foreach (var s in kq.DuLieu)
        {
            var anh = AnhHelper.TaoAnhThu(s.AnhBia, s.TenSach, kichThuocAnh);
            _anhThuNho.Add(anh);
            // Giá trị cột "SÁCH / MÃ" chỉ để tính chiều cao dòng; phần chữ thật được vẽ trong CellPainting
            int i = dgvSach.Rows.Add(anh, s.TenSach + "\n" + s.MaSach, s.TenTacGia, s.TenTheLoai, $"{s.TongSoBan} / {s.SoBanCoSan}", s.TrangThaiHienThi, null);
            dgvSach.Rows[i].Tag = s;
        }
        dgvSach.ClearSelection();
        dgvSach.ResumeLayout(true);

        lblTrong.Visible = kq.DuLieu.Count == 0;
        lblHienThi.Text = kq.TongSoDong == 0 ? "Không có kết quả"
            : $"Hiển thị {kq.TuDong}–{kq.DenDong} / {kq.TongSoDong.ToString("N0", Vi)} kết quả";
        pagination.DatTrang(kq.TrangHienTai, kq.TongSoTrang);
        CanChieuCaoLuoi();
    }

    public void MoFormThem()
    {
        _maDangSua = null;
        _anhBiaMoi = null;
        lblTieuDeForm.Text = "Thêm đầu sách mới";
        lblThongTinBan.Text = "Mã sách được tạo tự động.";
        btnLuu.Text = "Thêm đầu sách";
        txtTenSach.Clear(); txtNamXB.Clear(); txtViTri.Clear(); txtNXB.Text = "NXB Đại học Sư phạm"; txtNgonNgu.Text = "Tiếng Việt";
        nudTongSo.Value = 1;
        cboTacGia.SelectedIndex = -1;
        cboTheLoai.SelectedIndex = -1;
        cboLuuHanh.SelectedIndex = 0;
        DatAnhXemTruoc(null, "");
        HienForm();
    }

    public void MoFormSua(SachHienThi s)
    {
        _maDangSua = s.MaSach;
        _anhBiaMoi = null;
        lblTieuDeForm.Text = "Chỉnh sửa đầu sách " + s.MaSach;
        lblThongTinBan.Text = $"{s.SoBanDangMuon} bản đang mượn, {s.SoBanCoSan} bản có sẵn.";
        btnLuu.Text = "Lưu thay đổi";
        txtTenSach.Text = s.TenSach;
        cboTacGia.SelectedValue = s.MaTacGia ?? "";
        cboTheLoai.SelectedValue = s.MaTheLoai ?? "";
        txtNamXB.Text = s.NamXuatBan > 0 ? s.NamXuatBan.ToString() : "";
        nudTongSo.Value = Math.Min(nudTongSo.Maximum, Math.Max(nudTongSo.Minimum, s.TongSoBan));
        txtViTri.Text = s.ViTriKe;
        txtNXB.Text = s.NhaXuatBan;
        txtNgonNgu.Text = s.NgonNgu;
        int viTri = cboLuuHanh.FindStringExact(s.TinhTrangLuuHanh ?? "");
        cboLuuHanh.SelectedIndex = viTri >= 0 ? viTri : 0;
        DatAnhXemTruoc(s.AnhBia, s.TenSach);
        HienForm();
    }

    private void HienForm()
    {
        cardForm.Visible = true;
        pnlCuon.ScrollControlIntoView(cardForm);   // cuộn xuống panel để người dùng thấy ngay
        txtTenSach.Focus();
    }

    public void DongForm()
    {
        cardForm.Visible = false;
        _maDangSua = null;
        _anhBiaMoi = null;
    }

    public void XoaAnhBiaMoi()
    {
        _anhBiaMoi = null;
        DatAnhXemTruoc(null, txtTenSach.Text);
    }

    // Hiển thị ảnh bìa (hoặc ảnh thay thế có chữ cái đầu) trong ô xem trước
    private void DatAnhXemTruoc(byte[] duLieu, string tenSach)
    {
        var cu = picBia.Image;
        picBia.Image = AnhHelper.TaoAnhThu(duLieu, tenSach, DpiHelper.Px(picBia, new Size(240, 320)));   // vẽ lớn gấp đôi để rõ nét khi Scale lớn
        cu?.Dispose();
    }

    // Chọn ảnh bìa từ máy tính
    private void ChonAnhBia()
    {
        using var hop = new OpenFileDialog
        {
            Title = "Chọn ảnh bìa sách",
            Filter = "Tệp ảnh (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
            CheckFileExists = true
        };
        if (hop.ShowDialog(this) != DialogResult.OK) return;
        var du = AnhHelper.DocFileAnh(hop.FileName, out string loi);
        if (du == null) { HienThiLoi(loi); return; }
        _anhBiaMoi = du;
        DatAnhXemTruoc(du, txtTenSach.Text);
        AnhBiaDaChon?.Invoke(this, EventArgs.Empty);   // Presenter kiểm tra dung lượng
    }

    public string ChonFileNhap()
    {
        using var hop = new OpenFileDialog
        {
            Title = "Chọn tệp danh mục sách (CSV, mã hóa UTF-8)",
            Filter = "Tệp CSV (*.csv)|*.csv|Tất cả tệp (*.*)|*.*",
            CheckFileExists = true
        };
        return hop.ShowDialog(this) == DialogResult.OK ? hop.FileName : null;
    }

    public void DatTrangThaiDangXuLy(bool dangXuLy)
    {
        if (IsDisposed) return;
        UseWaitCursor = dangXuLy;
        btnLoc.Enabled = btnLuu.Enabled = btnThemDauSach.Enabled = btnNhapDanhMuc.Enabled = !dangXuLy;
    }

    public void HienThiThongTin(string thongBao) =>
        MessageBox.Show(this, thongBao, "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public void HienThiLoi(string thongBao) =>
        MessageBox.Show(this, thongBao, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    #endregion

}

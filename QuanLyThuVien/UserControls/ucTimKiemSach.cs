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
/// Màn hình 8 – Tìm kiếm sách. Thẻ "Điều kiện tìm kiếm" (lọc kết hợp), thẻ kết quả có phân trang
/// và thao tác "+ Thêm vào phiếu" / "Chi tiết sách". Chỉ hiển thị, xử lý do TimKiemSachPresenter.
/// </summary>
public partial class ucTimKiemSach : UserControl, ITimKiemSachView
{
    private static readonly CultureInfo Vi = new("vi-VN");

    private Label lblTrong;
    private Font _fontDam;
    private readonly TimKiemSachPresenter _presenter;

    #region ITimKiemSachView – thuộc tính và sự kiện
    public string MaSach => txtMa.Text.Trim();
    public string TenSach => txtTen.Text.Trim();
    public string TacGia => txtTacGia.Text.Trim();
    public string MaTheLoai => (cboTheLoai.SelectedItem as TheLoai)?.MaTheLoai;
    public string LuuHanh => cboLuuHanh.SelectedIndex <= 0 ? null : cboLuuHanh.Text;
    public bool ChiConSan => chkChiCoSan.Checked;

    public event EventHandler Loaded;
    public event EventHandler TimClicked;
    public event EventHandler XoaLocClicked;
    public event EventHandler<int> TrangChanged;
    public event EventHandler<string> ThemVaoPhieuClicked;
    public event EventHandler<string> ChiTietClicked;
    public event EventHandler MoPhieuClicked;
    public event EventHandler VeDanhMucClicked;
    #endregion

    public ucTimKiemSach(IDieuHuong dieuHuong)
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không tạo Presenter
        GanSuKien();
        _presenter = new TimKiemSachPresenter(this, dieuHuong);
    }

    #region Gắn sự kiện và phần không biểu diễn được trong Designer
    // Giao diện tĩnh nằm trong ucTimKiemSach.Designer.cs; ở đây chỉ gắn sự kiện và cấu hình lưới theo DPI.
    private void GanSuKien()
    {
        _fontDam = DpiHelper.TaoFont(9.5f, FontStyle.Bold);
        lblChipPhieu.Margin = new Padding(0, 4, 8, 0);
        lnkMoPhieu.Margin = new Padding(0, 6, 14, 0);
        lnkMoPhieu.LinkClicked += (s, e) => MoPhieuClicked?.Invoke(this, EventArgs.Empty);
        btnVeDanhMuc.Click += (s, e) => VeDanhMucClicked?.Invoke(this, EventArgs.Empty);
        foreach (var t in new[] { txtMa, txtTen, txtTacGia })
            t.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; TimClicked?.Invoke(this, EventArgs.Empty); } };
        btnTim.Click += (s, e) => TimClicked?.Invoke(this, EventArgs.Empty);
        btnXoa.Click += (s, e) => XoaLocClicked?.Invoke(this, EventArgs.Empty);
        pagination.TrangChanged += (s, t) => TrangChanged?.Invoke(this, t);

        dgv.CellPainting += DgvCellPainting;
        dgv.CellFormatting += DgvCellFormatting;
        dgv.CellContentClick += (s, e) =>
        {
            if (e.RowIndex < 0 || dgv.Rows[e.RowIndex].Tag is not SachHienThi sach) return;
            if (e.ColumnIndex == colThem.Index && PhepThem(sach)) ThemVaoPhieuClicked?.Invoke(this, sach.MaSach);
            else if (e.ColumnIndex == colChiTiet.Index) ChiTietClicked?.Invoke(this, sach.MaSach);
        };
        dgv.RowHeightChanged += (s, e) => UiForm.CanChieuCao(cardLuoi, 1, dgv, 60);
        lblTrong = UiTheme.TaoNhan("Không tìm thấy sách phù hợp.", 10f, FontStyle.Regular, UiTheme.ChuPhu);
        lblTrong.AutoSize = false; lblTrong.Height = 60; lblTrong.Dock = DockStyle.Bottom; lblTrong.TextAlign = ContentAlignment.MiddleCenter; lblTrong.Visible = false;
        dgv.Controls.Add(lblTrong);

        Load += (s, e) =>
        {
            DpiHelper.CauHinhDataGridView(dgv, 60, 40);
            DpiHelper.DatDoRongToiThieuCot(colSach, 200);
            DpiHelper.DatDoRongToiThieuCot(colTheLoai, 100);
            DpiHelper.DatDoRongToiThieuCot(colTinhTrang, 110);
            DpiHelper.DatDoRongToiThieuCot(colKe, 70);
            DpiHelper.DatDoRongToiThieuCot(colThem, 110);
            DpiHelper.DatDoRongToiThieuCot(colChiTiet, 80);
            Loaded?.Invoke(this, EventArgs.Empty);
        };
        Disposed += (s, e) => { _presenter?.HuyDangKy(); _fontDam?.Dispose(); };
    }
    #endregion

    #region Lưới
    // Sách được phép thêm vào phiếu khi còn bản và cho mượn
    private static bool PhepThem(SachHienThi s) => s.SoBanCoSan > 0 && s.TinhTrangLuuHanh != HangSo.LuuHanhDocTaiCho;

    private void DgvCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != colSach.Index || dgv.Rows[e.RowIndex].Tag is not SachHienThi s) return;
        e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
        var vung = Rectangle.Inflate(e.CellBounds, -DpiHelper.Px(dgv, 8), -DpiHelper.Px(dgv, 4));
        const TextFormatFlags co = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis;
        using var fNho = DpiHelper.TaoFont(8.5f);
        string phu = $"{s.MaSach}  •  {s.TenTacGia}";
        var k1 = TextRenderer.MeasureText(e.Graphics, s.TenSach, _fontDam, new Size(vung.Width, int.MaxValue), co);
        var k2 = TextRenderer.MeasureText(e.Graphics, phu, fNho, new Size(vung.Width, int.MaxValue), co);
        int y = vung.Top + Math.Max(0, (vung.Height - k1.Height - k2.Height - 2) / 2);
        TextRenderer.DrawText(e.Graphics, s.TenSach, _fontDam, new Rectangle(vung.Left, y, vung.Width, k1.Height), UiTheme.ChuChinh, co);
        TextRenderer.DrawText(e.Graphics, phu, fNho, new Rectangle(vung.Left, y + k1.Height + 2, vung.Width, k2.Height), UiTheme.ChuPhu, co);
        e.Handled = true;
    }

    private void DgvCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || dgv.Rows[e.RowIndex].Tag is not SachHienThi s) return;
        if (e.ColumnIndex == colTinhTrang.Index)
        {
            e.CellStyle.ForeColor = s.SoBanCoSan > 0 ? UiTheme.XanhLa : UiTheme.Do;
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            e.CellStyle.Font = _fontDam;
        }
    }
    #endregion

    #region ITimKiemSachView – hiển thị
    public void NapTheLoai(List<TheLoai> ds)
    {
        cboTheLoai.Items.Clear();
        cboTheLoai.Items.Add(new TheLoai { MaTheLoai = null, TenTheLoai = "Tất cả" });
        foreach (var t in ds) cboTheLoai.Items.Add(t);
        cboTheLoai.DisplayMember = nameof(TheLoai.TenTheLoai);
        cboTheLoai.SelectedIndex = 0;
    }

    public void DatLaiBoLoc()
    {
        txtMa.Clear(); txtTen.Clear(); txtTacGia.Clear();
        if (cboTheLoai.Items.Count > 0) cboTheLoai.SelectedIndex = 0;
        cboLuuHanh.SelectedIndex = 0;
        chkChiCoSan.Checked = false;
        lblTrangThaiNhanh.Text = "";
    }

    public void HienThiKetQua(KetQuaPhanTrang<SachHienThi> kq)
    {
        dgv.SuspendLayout();
        dgv.Rows.Clear();
        foreach (var s in kq.DuLieu)
        {
            bool docTaiCho = s.TinhTrangLuuHanh == HangSo.LuuHanhDocTaiCho;
            string tinhTrang = $"{s.TrangThaiHienThi}\n{s.SoBanCoSan}/{s.TongSoBan} bản" + (docTaiCho ? " · đọc tại chỗ" : "");
            int i = dgv.Rows.Add(s.TenSach + "\n" + s.MaSach, s.TenTheLoai, tinhTrang, s.ViTriKe, PhepThem(s) ? "+ Thêm vào phiếu" : "", "Chi tiết sách");
            dgv.Rows[i].Tag = s;
            dgv.Rows[i].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }
        dgv.ResumeLayout(true);
        lblTrong.Visible = kq.DuLieu.Count == 0;
        lblHienThi.Text = kq.TongSoDong == 0 ? "Không có kết quả" : $"Hiển thị {kq.TuDong}–{kq.DenDong} / {kq.TongSoDong.ToString("N0", Vi)} đầu sách";
        pagination.DatTrang(kq.TrangHienTai, kq.TongSoTrang);
        UiForm.CanChieuCao(cardLuoi, 1, dgv, 60);
    }

    public void HienThiPhieuNhap(int soDauSach, int tongCuon)
    {
        if (IsDisposed) return;
        lblChipPhieu.Text = tongCuon == 0 ? "Phiếu nháp: trống" : $"Phiếu nháp: {tongCuon} cuốn / {soDauSach} đầu sách";
        var mau = tongCuon == 0 ? UiTheme.ChuPhu : UiTheme.XanhChinh;
        lblChipPhieu.ForeColor = mau;
        lblChipPhieu.BackColor = UiForm.Pha(mau, 0.88f);
    }

    public void HienThiTrangThaiNhanh(string noiDung, bool thanhCong)
    {
        lblTrangThaiNhanh.ForeColor = thanhCong ? UiTheme.XanhLa : UiTheme.Do;
        lblTrangThaiNhanh.Text = noiDung;
    }

    public void MoChiTiet(SachHienThi sach)
    {
        using var f = new frmChiTietSach(sach);
        f.ShowDialog(this);
    }

    public void DatTrangThaiDangXuLy(bool dangXuLy)
    {
        if (IsDisposed) return;
        UseWaitCursor = dangXuLy;
        btnTim.Enabled = btnXoa.Enabled = !dangXuLy;
    }

    public void HienThiThongTin(string thongBao) => MessageBox.Show(this, thongBao, "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public void HienThiLoi(string thongBao) => MessageBox.Show(this, thongBao, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    #endregion

}

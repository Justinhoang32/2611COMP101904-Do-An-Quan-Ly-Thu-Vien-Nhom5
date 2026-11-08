using System.ComponentModel;
using System.Globalization;
using DTO.Models;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Controls;

/// <summary>
/// Một khối danh mục (Tác giả hoặc Thể loại): tiêu đề + đếm, nút Thêm, ô tra cứu, lưới 3 cột, phân trang, panel chỉnh sửa.
/// Hai khối trên màn hình "Tác giả &amp; thể loại" dùng chung lớp này, chỉ khác chữ hiển thị (đặt qua các thuộc tính
/// TieuDe, TuMot, NhanTen, NhanMoTa ngay trong Designer). Giao diện tĩnh nằm ở ucDanhMucPanel.Designer.cs.
/// </summary>
public partial class ucDanhMucPanel : UserControl, IDanhMucView
{
    private static readonly CultureInfo Vi = new("vi-VN");

    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 350 };
    private const int HangLuoi = 2;          // chỉ số hàng chứa lưới trong thẻ `the`
    private Label lblTrong;                  // dòng "không có kết quả" nằm trong lưới (tạo lúc chạy)
    private string _tieuDe = "Danh mục", _tuMot = "danh mục", _nhanTen = "Tên", _nhanMoTa = "Mô tả";
    private string _maDangSua;
    private bool _choPhepSua = true;

    #region Thuộc tính đặt chữ hiển thị (chỉnh được trong cửa sổ Properties)
    /// <summary>Tiêu đề khối, ví dụ "Tác giả".</summary>
    [Category("Nội dung"), DefaultValue("Danh mục")]
    public string TieuDe { get => _tieuDe; set { _tieuDe = value ?? ""; CapNhatChu(); } }

    /// <summary>Từ chỉ đối tượng viết thường, ví dụ "tác giả".</summary>
    [Category("Nội dung"), DefaultValue("danh mục")]
    public string TuMot { get => _tuMot; set { _tuMot = value ?? ""; CapNhatChu(); } }

    /// <summary>Nhãn ô tên trong form chỉnh sửa, ví dụ "Họ và tên tác giả".</summary>
    [Category("Nội dung"), DefaultValue("Tên")]
    public string NhanTen { get => _nhanTen; set { _nhanTen = value ?? ""; CapNhatChu(); } }

    /// <summary>Nhãn ô mô tả trong form chỉnh sửa, ví dụ "Giới thiệu".</summary>
    [Category("Nội dung"), DefaultValue("Mô tả")]
    public string NhanMoTa { get => _nhanMoTa; set { _nhanMoTa = value ?? ""; CapNhatChu(); } }

    private string TieuDeThem => "Thêm " + _tuMot;
    private string TieuDeSua => "Chỉnh sửa " + _tuMot;
    private string NhanLuu => "Lưu " + _tuMot;

    // Cập nhật mọi chữ phụ thuộc bốn thuộc tính trên
    private void CapNhatChu()
    {
        lblTieuDe.Text = _tieuDe;
        btnThem.Text = TieuDeThem;
        btnLuu.Text = NhanLuu;
        truongTim.NhanText = "Tra cứu " + _tuMot;
        txtTim.PlaceholderText = "Tìm tên hoặc mã " + _tuMot;
        colTen.HeaderText = "TÊN " + _tuMot.ToUpperInvariant();
        truongTen.NhanText = _nhanTen;
        truongMoTa.NhanText = _nhanMoTa;
    }
    #endregion

    public string TuKhoa => txtTim.Text.Trim();
    public string MaDangSua => _maDangSua;
    public string Ten => txtTen.Text;
    public string MoTa => txtMoTa.Text;

    public event EventHandler TimKiem;
    public event EventHandler<int> TrangChanged;
    public event EventHandler ThemClicked;
    public event EventHandler<string> SuaClicked;
    public event EventHandler LuuClicked;
    public event EventHandler HuyClicked;

    public ucDanhMucPanel()
    {
        InitializeComponent();
        CapNhatChu();
        colDauSach.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không gắn xử lý chạy thật
        GanSuKien();
    }

    #region Gắn sự kiện và phần không biểu diễn được trong Designer
    private void GanSuKien()
    {
        btnThem.Click += (s, e) => ThemClicked?.Invoke(this, EventArgs.Empty);
        btnHuy.Click += (s, e) => HuyClicked?.Invoke(this, EventArgs.Empty);
        btnLuu.Click += (s, e) => LuuClicked?.Invoke(this, EventArgs.Empty);
        pagination.TrangChanged += (s, t) => TrangChanged?.Invoke(this, t);

        // Ô tra cứu: tìm sau khi ngừng gõ 350 ms hoặc nhấn Enter
        txtTim.TextChanged += (s, e) => { _timer.Stop(); _timer.Start(); };
        txtTim.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _timer.Stop(); TimKiem?.Invoke(this, EventArgs.Empty); } };
        _timer.Tick += (s, e) => { _timer.Stop(); TimKiem?.Invoke(this, EventArgs.Empty); };

        dgv.CellContentClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == colSua.Index && dgv.Rows[e.RowIndex].Tag is DanhMucItem d) SuaClicked?.Invoke(this, d.Ma);
        };
        dgv.CellDoubleClick += (s, e) =>
        {
            if (_choPhepSua && e.RowIndex >= 0 && dgv.Rows[e.RowIndex].Tag is DanhMucItem d) SuaClicked?.Invoke(this, d.Ma);
        };
        dgv.RowHeightChanged += (s, e) => UiForm.CanChieuCao(the, HangLuoi, dgv, 56);
        lblTrong = UiTheme.TaoNhan("Không tìm thấy kết quả phù hợp.", 10f, FontStyle.Regular, UiTheme.ChuPhu);
        lblTrong.AutoSize = false; lblTrong.Height = 56; lblTrong.Dock = DockStyle.Bottom; lblTrong.TextAlign = ContentAlignment.MiddleCenter; lblTrong.Visible = false;
        dgv.Controls.Add(lblTrong);

        // Đường kẻ phía trên panel chỉnh sửa
        tlpForm.Paint += (s, e) => { using var pen = new Pen(UiTheme.VienNhat); e.Graphics.DrawLine(pen, 0, 0, tlpForm.Width, 0); };
        HandleCreated += (s, e) => { DpiHelper.CauHinhDataGridView(dgv, 44, 38); };
        Disposed += (s, e) => _timer.Dispose();
    }
    #endregion

    #region IDanhMucView – hiển thị
    public void HienThiDanhSach(KetQuaPhanTrang<DanhMucItem> kq, string tenDoiTuong)
    {
        dgv.SuspendLayout();
        dgv.Rows.Clear();
        foreach (var d in kq.DuLieu)
        {
            int i = dgv.Rows.Add(d.Ma, d.Ten, d.SoDauSach.ToString(), null);
            dgv.Rows[i].Tag = d;
        }
        dgv.ClearSelection();
        dgv.ResumeLayout(true);
        lblTrong.Visible = kq.DuLieu.Count == 0;
        lblHienThi.Text = kq.TongSoDong == 0 ? "Không có kết quả" : $"{kq.TuDong}–{kq.DenDong} / {kq.TongSoDong.ToString("N0", Vi)} {tenDoiTuong}";
        pagination.DatTrang(kq.TrangHienTai, kq.TongSoTrang);
        UiForm.CanChieuCao(the, HangLuoi, dgv, 56);
    }

    public void HienThiTong(string noiDung) => lblTong.Text = noiDung;

    public void MoFormThem()
    {
        if (!_choPhepSua) return;
        _maDangSua = null;
        lblTieuDeForm.Text = TieuDeThem;
        txtTen.Clear(); txtMoTa.Clear();
        btnLuu.Text = NhanLuu;
        tlpForm.Visible = true;
        txtTen.Focus();
    }

    public void MoFormSua(string ma, string ten, string moTa)
    {
        if (!_choPhepSua) return;
        _maDangSua = ma;
        lblTieuDeForm.Text = $"{TieuDeSua} {ma}";
        txtTen.Text = ten;
        txtMoTa.Text = moTa;
        btnLuu.Text = NhanLuu;
        tlpForm.Visible = true;
        txtTen.Focus();
    }

    public void DongForm() { tlpForm.Visible = false; _maDangSua = null; }

    public void DatQuyenSua(bool choPhep)
    {
        _choPhepSua = choPhep;
        btnThem.Visible = choPhep;
        colSua.Visible = choPhep;
        chipChiXem.Visible = !choPhep;
        if (!choPhep) DongForm();
    }

    public void DatTrangThaiDangXuLy(bool dangXuLy) { if (!IsDisposed) UseWaitCursor = dangXuLy; }
    public void HienThiThongTin(string thongBao) => MessageBox.Show(FindForm(), thongBao, "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public void HienThiLoi(string thongBao) => MessageBox.Show(FindForm(), thongBao, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    #endregion
}

using QuanLyThuVien.Controls;
using QuanLyThuVien.Presenters;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.UserControls;

/// <summary>
/// Màn hình 5 – Tác giả &amp; thể loại. Hai khối danh mục đặt cạnh nhau (ucDanhMucPanel); chỉ Quản trị viên được thêm/sửa,
/// Thủ thư chỉ xem. Có dải thông báo quyền chỉnh sửa ở đầu trang. Giao diện tĩnh nằm trong ucTacGiaTheLoai.Designer.cs.
/// </summary>
public partial class ucTacGiaTheLoai : UserControl, ITacGiaTheLoaiView
{
    private readonly TacGiaTheLoaiPresenter _presenter;

    public IDanhMucView TacGia => pnlTacGia;
    public IDanhMucView TheLoai => pnlTheLoai;
    public event EventHandler Loaded;

    public ucTacGiaTheLoai()
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không tạo Presenter
        Load += (s, e) => Loaded?.Invoke(this, EventArgs.Empty);
        _presenter = new TacGiaTheLoaiPresenter(this);
    }

    /// <summary>Áp dụng quyền: Quản trị viên được sửa; Thủ thư chỉ xem (nút Thêm/Sửa bị ẩn).</summary>
    public void DatQuyen(bool laQuanTri)
    {
        pnlTacGia.DatQuyenSua(laQuanTri);
        pnlTheLoai.DatQuyenSua(laQuanTri);
        hopQuyen.Text = "Quyền chỉnh sửa danh mục: Quản trị viên.\n" +
            "Thủ thư được tra cứu và sử dụng danh mục khi biên mục sách. Thay đổi tại đây áp dụng cho các đầu sách liên quan." +
            (laQuanTri ? "" : "\nBạn đang đăng nhập với quyền Thủ thư nên chỉ có thể xem danh mục.");
    }
}

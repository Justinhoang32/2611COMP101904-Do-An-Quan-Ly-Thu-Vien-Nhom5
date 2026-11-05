using DTO;
using DTO.Models;

namespace QuanLyThuVien.Forms;

/// <summary>
/// Hộp thoại xem chi tiết một đầu sách (chỉ đọc): ảnh bìa và thông tin đầy đủ.
/// Giao diện tĩnh nằm trong frmChiTietSach.Designer.cs; ở đây chỉ đổ dữ liệu vào các nhãn.
/// </summary>
public partial class frmChiTietSach : Form
{
    public frmChiTietSach(SachHienThi s)
    {
        InitializeComponent();

        picBia.Size = DpiHelper.Px(picBia, new Size(150, 210));
        picBia.Image = AnhHelper.TaoAnhThu(s.AnhBia, s.TenSach, DpiHelper.Px(picBia, new Size(300, 420)));
        Disposed += (_, _) => picBia.Image?.Dispose();

        lblTenSach.Text = s.TenSach;
        lblMaSach.Text = Hoac(s.MaSach);
        lblTacGia.Text = Hoac(s.TenTacGia);
        lblTheLoai.Text = Hoac(s.TenTheLoai);
        lblNamXb.Text = Hoac(s.NamXuatBan > 0 ? s.NamXuatBan.ToString() : null);
        lblNhaXb.Text = Hoac(s.NhaXuatBan);
        lblNgonNgu.Text = Hoac(s.NgonNgu);
        lblViTri.Text = Hoac(s.ViTriKe);
        lblSoBan.Text = $"còn {s.SoBanCoSan} / {s.TongSoBan} (đang mượn {s.SoBanDangMuon})";
        lblLuuHanh.Text = Hoac(s.TinhTrangLuuHanh);
    }

    // Giá trị trống hiển thị dấu gạch ngang
    private static string Hoac(string v) => string.IsNullOrWhiteSpace(v) ? "—" : v;
}

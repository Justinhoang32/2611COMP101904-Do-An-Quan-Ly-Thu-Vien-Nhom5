using DTO.Models;
using static DAL.DatabaseHelper;

namespace DAL;

/// <summary>Truy cập bảng TaiKhoan và VaiTro.</summary>
public static class TaiKhoanDAL
{
    /// <summary>Lấy tài khoản theo tên đăng nhập (kèm tên vai trò). Trả về null nếu không có.</summary>
    public static NguoiDung LayTheoTenDangNhap(string tenDangNhap)
    {
        const string sql = @"SELECT tk.MaTaiKhoan, tk.TenDangNhap, tk.MatKhau, tk.HoTen, tk.MaVaiTro, vt.TenVaiTro
                             FROM TaiKhoan tk LEFT JOIN VaiTro vt ON vt.MaVaiTro = tk.MaVaiTro
                             WHERE tk.TenDangNhap = @ten";
        var dt = ExecuteQuery(sql, Tham("@ten", tenDangNhap));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new NguoiDung
        {
            MaTaiKhoan = r.So("MaTaiKhoan"),
            TenDangNhap = r.Chuoi("TenDangNhap"),
            MatKhau = r.Chuoi("MatKhau"),
            HoTen = r.Chuoi("HoTen"),
            MaVaiTro = r.So("MaVaiTro"),
            TenVaiTro = r.Chuoi("TenVaiTro")
        };
    }
}

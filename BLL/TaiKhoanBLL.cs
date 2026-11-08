using DAL;
using DTO.Models;

namespace BLL;

/// <summary>Nghiệp vụ đăng nhập.</summary>
public static class TaiKhoanBLL
{
    /// <summary>Kiểm tra tài khoản/mật khẩu; thành công thì mở phiên đăng nhập và trả về người dùng.</summary>
    public static NguoiDung DangNhap(string tenDangNhap, string matKhau)
    {
        // Kiểm tra dữ liệu nhập
        if (string.IsNullOrWhiteSpace(tenDangNhap)) throw new NghiepVuException("Vui lòng nhập tên tài khoản.");
        if (string.IsNullOrEmpty(matKhau)) throw new NghiepVuException("Vui lòng nhập mật khẩu.");

        var nd = TaiKhoanDAL.LayTheoTenDangNhap(tenDangNhap.Trim());
        // Lưu ý: mật khẩu đang lưu dạng chữ thường (theo dữ liệu mẫu). Thực tế nên băm (hash) mật khẩu.
        if (nd == null || !string.Equals(nd.MatKhau, matKhau, StringComparison.Ordinal))
            throw new NghiepVuException("Tài khoản hoặc mật khẩu không đúng.");

        nd.MatKhau = null;   // không giữ mật khẩu trong bộ nhớ
        PhienDangNhap.BatDau(nd);
        return nd;
    }

    public static void DangXuat() => PhienDangNhap.KetThuc();
}

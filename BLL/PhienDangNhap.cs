using DTO;
using DTO.Models;

namespace BLL;

/// <summary>Lưu người dùng đang đăng nhập trong suốt phiên làm việc và kiểm tra quyền.</summary>
public static class PhienDangNhap
{
    /// <summary>Người dùng hiện tại (null nếu chưa đăng nhập).</summary>
    public static NguoiDung NguoiDungHienTai { get; private set; }

    public static bool DaDangNhap => NguoiDungHienTai != null;
    public static bool LaQuanTri => NguoiDungHienTai?.MaVaiTro == HangSo.VaiTroQuanTri;

    public static void BatDau(NguoiDung nguoiDung) => NguoiDungHienTai = nguoiDung;
    public static void KetThuc() => NguoiDungHienTai = null;

    /// <summary>Ném lỗi nghiệp vụ nếu chưa đăng nhập.</summary>
    public static void YeuCauDangNhap()
    {
        if (!DaDangNhap) throw new NghiepVuException("Phiên đăng nhập đã hết. Vui lòng đăng nhập lại.");
    }

    /// <summary>Ném lỗi nghiệp vụ nếu người dùng không phải Quản trị viên.</summary>
    public static void YeuCauQuanTri()
    {
        YeuCauDangNhap();
        if (!LaQuanTri) throw new NghiepVuException("Chỉ Quản trị viên mới được thay đổi danh mục này.");
    }
}

using DAL;
using DTO;
using DTO.Entities;
using DTO.Models;

namespace BLL;

/// <summary>Nghiệp vụ danh mục Tác giả. Thêm/sửa chỉ dành cho Quản trị viên.</summary>
public static class TacGiaBLL
{
    public static List<TacGia> LayTatCa() => TacGiaDAL.LayTatCa();

    public static KetQuaPhanTrang<TacGiaHienThi> TimKiem(string tuKhoa, int trang, int kichThuoc = HangSo.KichThuocTrangMacDinh)
    {
        PhienDangNhap.YeuCauDangNhap();
        return TacGiaDAL.TimKiem(tuKhoa, Math.Max(1, trang), kichThuoc);
    }

    public static TacGia LayTheoMa(string ma) => TacGiaDAL.LayTheoMa(ma);

    /// <summary>Thêm tác giả mới, mã tự sinh dạng TGxxx. Trả về mã vừa tạo.</summary>
    public static string Them(string ten, string gioiThieu)
    {
        PhienDangNhap.YeuCauQuanTri();
        var t = KiemTra(ten, gioiThieu, null);
        t.MaTacGia = SinhMa.TiepTheo(TacGiaDAL.LayMaCuoi(), "TG", 3);
        TacGiaDAL.Them(t);
        return t.MaTacGia;
    }

    public static void Sua(string ma, string ten, string gioiThieu)
    {
        PhienDangNhap.YeuCauQuanTri();
        if (TacGiaDAL.LayTheoMa(ma) == null) throw new NghiepVuException("Không tìm thấy tác giả cần sửa.");
        var t = KiemTra(ten, gioiThieu, ma);
        t.MaTacGia = ma;
        TacGiaDAL.Sua(t);
    }

    private static TacGia KiemTra(string ten, string gioiThieu, string maDangSua)
    {
        ten = (ten ?? "").Trim();
        if (ten.Length == 0) throw new NghiepVuException("Vui lòng nhập họ và tên tác giả.");
        if (ten.Length > 100) throw new NghiepVuException("Tên tác giả tối đa 100 ký tự.");
        if (TacGiaDAL.TonTaiTen(ten, maDangSua)) throw new NghiepVuException("Tên tác giả đã tồn tại.");
        gioiThieu = (gioiThieu ?? "").Trim();
        if (gioiThieu.Length > 500) throw new NghiepVuException("Phần giới thiệu tối đa 500 ký tự.");
        return new TacGia { TenTacGia = ten, GioiThieu = gioiThieu };
    }
}

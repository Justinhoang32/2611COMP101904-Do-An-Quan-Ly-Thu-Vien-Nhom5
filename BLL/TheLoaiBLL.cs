using DAL;
using DTO;
using DTO.Entities;
using DTO.Models;

namespace BLL;

/// <summary>Nghiệp vụ danh mục Thể loại. Thêm/sửa chỉ dành cho Quản trị viên.</summary>
public static class TheLoaiBLL
{
    public static List<TheLoai> LayTatCa() => TheLoaiDAL.LayTatCa();

    public static KetQuaPhanTrang<TheLoaiHienThi> TimKiem(string tuKhoa, int trang, int kichThuoc = HangSo.KichThuocTrangMacDinh)
    {
        PhienDangNhap.YeuCauDangNhap();
        return TheLoaiDAL.TimKiem(tuKhoa, Math.Max(1, trang), kichThuoc);
    }

    public static TheLoai LayTheoMa(string ma) => TheLoaiDAL.LayTheoMa(ma);

    /// <summary>Thêm thể loại mới, mã tự sinh dạng TLxxx. Trả về mã vừa tạo.</summary>
    public static string Them(string ten, string moTa)
    {
        PhienDangNhap.YeuCauQuanTri();
        var t = KiemTra(ten, moTa, null);
        t.MaTheLoai = SinhMa.TiepTheo(TheLoaiDAL.LayMaCuoi(), "TL", 3);
        TheLoaiDAL.Them(t);
        return t.MaTheLoai;
    }

    public static void Sua(string ma, string ten, string moTa)
    {
        PhienDangNhap.YeuCauQuanTri();
        if (TheLoaiDAL.LayTheoMa(ma) == null) throw new NghiepVuException("Không tìm thấy thể loại cần sửa.");
        var t = KiemTra(ten, moTa, ma);
        t.MaTheLoai = ma;
        TheLoaiDAL.Sua(t);
    }

    // Kiểm tra dữ liệu nhập và trả về đối tượng đã làm sạch
    private static TheLoai KiemTra(string ten, string moTa, string maDangSua)
    {
        ten = (ten ?? "").Trim();
        if (ten.Length == 0) throw new NghiepVuException("Vui lòng nhập tên thể loại.");
        if (ten.Length > 100) throw new NghiepVuException("Tên thể loại tối đa 100 ký tự.");
        if (TheLoaiDAL.TonTaiTen(ten, maDangSua)) throw new NghiepVuException("Tên thể loại đã tồn tại.");
        moTa = (moTa ?? "").Trim();
        if (moTa.Length > 500) throw new NghiepVuException("Mô tả tối đa 500 ký tự.");
        return new TheLoai { TenTheLoai = ten, MoTa = moTa };
    }
}

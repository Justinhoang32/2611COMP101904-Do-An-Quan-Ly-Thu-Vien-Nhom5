using System.Text.RegularExpressions;
using DAL;
using DTO;
using DTO.Entities;
using DTO.Models;

namespace BLL;

/// <summary>Nghiệp vụ độc giả: tìm kiếm, thêm, sửa, cấp / gia hạn thẻ.</summary>
public static class DocGiaBLL
{
    public static KetQuaPhanTrang<DocGiaHienThi> TimKiem(DocGiaBoLoc boLoc, int trang, int kichThuoc = HangSo.KichThuocTrangMacDinh)
    {
        PhienDangNhap.YeuCauDangNhap();
        return DocGiaDAL.TimKiem(boLoc, DateTime.Today, Math.Max(1, trang), kichThuoc);
    }

    /// <summary>Lấy toàn bộ kết quả (không phân trang) để xuất danh sách.</summary>
    public static List<DocGiaHienThi> LayTatCaDeXuat(DocGiaBoLoc boLoc) =>
        DocGiaDAL.TimKiem(boLoc, DateTime.Today, 1, 1_000_000).DuLieu;

    public static DocGiaHienThi LayTheoMa(string ma) => DocGiaDAL.LayTheoMa(ma, DateTime.Today);
    public static ThongKeDocGia ThongKe() => ThongKeDAL.LayDocGia(DateTime.Today);

    /// <summary>Thêm độc giả mới (mã tự sinh DGxxxx, thẻ mặc định 4 năm). Trả về mã vừa tạo.</summary>
    public static string Them(DocGia d)
    {
        PhienDangNhap.YeuCauDangNhap();
        KiemTra(d);
        d.MaDocGia = SinhMa.TiepTheo(DocGiaDAL.LayMaCuoi(), "DG", 4);
        d.NgayCap ??= DateTime.Today;
        d.HanThe ??= d.NgayCap.Value.AddYears(4);
        d.TrangThai = HangSo.TheHoatDong;
        if (d.HanThe.Value.Date <= d.NgayCap.Value.Date) throw new NghiepVuException("Hạn thẻ phải sau ngày cấp.");
        DocGiaDAL.Them(d);
        return d.MaDocGia;
    }

    public static void Sua(DocGia d)
    {
        PhienDangNhap.YeuCauDangNhap();
        if (!DocGiaDAL.TonTai(d.MaDocGia)) throw new NghiepVuException("Không tìm thấy độc giả cần sửa.");
        KiemTra(d);
        DocGiaDAL.Sua(d);
    }

    /// <summary>Cấp mới / gia hạn thẻ cho độc giả.</summary>
    public static void CapThe(string maDocGia, string nhom, DateTime ngayCap, DateTime hanThe)
    {
        PhienDangNhap.YeuCauDangNhap();
        if (!DocGiaDAL.TonTai(maDocGia)) throw new NghiepVuException("Không tìm thấy độc giả.");
        if (Array.IndexOf(HangSo.CacNhomDocGia, nhom) < 0) throw new NghiepVuException("Vui lòng chọn nhóm độc giả hợp lệ.");
        if (hanThe.Date <= ngayCap.Date) throw new NghiepVuException("Hạn thẻ phải sau ngày cấp.");
        DocGiaDAL.CapThe(maDocGia, nhom, ngayCap, hanThe);
    }

    // Kiểm tra dữ liệu nhập: họ tên, nhóm, mã sinh viên, điện thoại, email
    private static void KiemTra(DocGia d)
    {
        d.HoTen = (d.HoTen ?? "").Trim();
        d.MaSinhVien = (d.MaSinhVien ?? "").Trim();
        d.Khoa = (d.Khoa ?? "").Trim();
        d.DienThoai = (d.DienThoai ?? "").Trim();
        d.Email = (d.Email ?? "").Trim();

        if (d.HoTen.Length == 0) throw new NghiepVuException("Vui lòng nhập họ tên độc giả.");
        if (d.HoTen.Length > 100) throw new NghiepVuException("Họ tên tối đa 100 ký tự.");
        if (Array.IndexOf(HangSo.CacNhomDocGia, d.NhomDocGia) < 0) throw new NghiepVuException("Vui lòng chọn nhóm độc giả.");
        if (d.NhomDocGia == HangSo.NhomSinhVien && d.MaSinhVien.Length == 0)
            throw new NghiepVuException("Sinh viên cần nhập mã sinh viên.");
        if (d.MaSinhVien.Length > 20) throw new NghiepVuException("Mã sinh viên tối đa 20 ký tự.");
        if (d.DienThoai.Length > 0 && !Regex.IsMatch(d.DienThoai, @"^[0-9 .+\-]{9,15}$"))
            throw new NghiepVuException("Số điện thoại không hợp lệ (9–15 chữ số).");
        if (d.Email.Length > 0 && (d.Email.Length > 100 || !Regex.IsMatch(d.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")))
            throw new NghiepVuException("Địa chỉ email không hợp lệ.");
    }
}

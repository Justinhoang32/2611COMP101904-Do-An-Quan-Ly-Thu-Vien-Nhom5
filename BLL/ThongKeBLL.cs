using DAL;
using DTO;
using DTO.Models;

namespace BLL;

/// <summary>Nghiệp vụ thống kê: Tổng quan và Báo cáo quá hạn.</summary>
public static class ThongKeBLL
{
    /// <summary>Gom toàn bộ số liệu của màn Tổng quan.</summary>
    public static ThongKeTongQuan LayTongQuan()
    {
        PhienDangNhap.YeuCauDangNhap();
        DateTime hom = DateTime.Today;
        var tq = ThongKeDAL.LayTongQuanCoBan(hom);
        tq.Sach = ThongKeDAL.LaySach();
        tq.DocGia = ThongKeDAL.LayDocGia(hom);
        tq.Phieu = ThongKeDAL.LayPhieu(hom);
        tq.LuotMuonTheoThang = ThongKeDAL.LayLuotMuonTheoThang(hom.Year);
        return tq;
    }

    /// <summary>Số phiếu quá hạn (hiện trên huy hiệu của sidebar).</summary>
    public static int DemPhieuQuaHan() => ThongKeDAL.LayPhieu(DateTime.Today).QuaHan;

    /// <summary>Lấy danh sách quá hạn theo bộ lọc (lọc trong bộ nhớ vì số lượng nhỏ) rồi phân trang.</summary>
    public static KetQuaPhanTrang<QuaHanHienThi> TimQuaHan(QuaHanBoLoc f, int trang, int kichThuoc = 6)
    {
        PhienDangNhap.YeuCauDangNhap();
        var ds = LocQuaHan(f);
        trang = Math.Max(1, trang);
        return new KetQuaPhanTrang<QuaHanHienThi>
        {
            TongSoDong = ds.Count, TrangHienTai = trang, KichThuocTrang = kichThuoc,
            DuLieu = ds.Skip((trang - 1) * kichThuoc).Take(kichThuoc).ToList()
        };
    }

    /// <summary>Toàn bộ kết quả đã lọc (dùng cho xuất Excel và in danh sách).</summary>
    public static List<QuaHanHienThi> LayTatCaQuaHanDeXuat(QuaHanBoLoc f)
    {
        PhienDangNhap.YeuCauDangNhap();
        return LocQuaHan(f);
    }

    /// <summary>Thống kê 3 thẻ số liệu đầu trang Báo cáo quá hạn (tính trên toàn bộ danh sách chưa lọc).</summary>
    public static ThongKeQuaHan ThongKeQuaHan(DateTime ngayDoiChieu)
    {
        var ds = ThongKeDAL.LayQuaHan(ngayDoiChieu);
        var docGia = ds.GroupBy(x => x.MaDocGia).Select(g => g.First()).ToList();
        return new ThongKeQuaHan
        {
            SoPhieu = ds.Count,
            SoCuon = ds.Sum(x => x.SoCuon),
            SoDocGia = docGia.Count,
            SoSinhVien = docGia.Count(x => x.NhomDocGia == HangSo.NhomSinhVien),
            SoGiangVienCanBo = docGia.Count(x => x.NhomDocGia != HangSo.NhomSinhVien),
            SoNgayTreNhieuNhat = ds.Count == 0 ? 0 : ds.Max(x => x.SoNgayTre)
        };
    }

    /// <summary>Biểu đồ "Phiếu phát sinh quá hạn theo tháng" (GiaTri) và số phiếu đã thu hồi đủ (GiaTri2).</summary>
    public static List<ThongKeThang> ThongKeQuaHanTheoThang(int nam) => ThongKeDAL.LayQuaHanTheoThang(nam, DateTime.Today);

    // Áp dụng bộ lọc: nhóm, mức độ trễ, từ khóa
    private static List<QuaHanHienThi> LocQuaHan(QuaHanBoLoc f)
    {
        f ??= new QuaHanBoLoc();
        IEnumerable<QuaHanHienThi> q = ThongKeDAL.LayQuaHan(f.NgayDoiChieu);
        if (!string.IsNullOrWhiteSpace(f.Nhom) && !f.Nhom.StartsWith("Tất cả"))
            q = q.Where(x => x.NhomDocGia == f.Nhom);
        q = q.Where(x => x.SoNgayTre >= f.SoNgayTreToiThieu);
        if (f.SoNgayTreToiDa.HasValue) q = q.Where(x => x.SoNgayTre <= f.SoNgayTreToiDa.Value);
        if (!string.IsNullOrWhiteSpace(f.TuKhoa))
        {
            string k = f.TuKhoa.Trim();
            q = q.Where(x => Chua(x.MaPhieu, k) || Chua(x.HoTen, k) || Chua(x.MaDocGia, k));
        }
        return q.ToList();
    }

    private static bool Chua(string nguon, string tu) =>
        nguon != null && nguon.Contains(tu, StringComparison.OrdinalIgnoreCase);
}

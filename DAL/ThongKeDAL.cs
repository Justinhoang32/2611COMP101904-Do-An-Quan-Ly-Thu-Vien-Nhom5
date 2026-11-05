using System.Data;
using DTO;
using DTO.Models;
using static DAL.DatabaseHelper;

namespace DAL;

/// <summary>Các truy vấn thống kê cho Tổng quan, các thẻ số liệu và Báo cáo quá hạn.</summary>
public static class ThongKeDAL
{
    /// <summary>Thống kê sách: số đầu sách, số bản, số đầu sách hết bản.</summary>
    public static ThongKeSach LaySach()
    {
        var r = ExecuteQuery(@"SELECT COUNT(*) AS SoDauSach, ISNULL(SUM(TongSoBan), 0) AS SoBanSach,
                                      ISNULL(SUM(CASE WHEN SoBanCoSan <= 0 THEN 1 ELSE 0 END), 0) AS HetBan FROM Sach").Rows[0];
        return new ThongKeSach { SoDauSach = r.So("SoDauSach"), SoBanSach = r.So("SoBanSach"), SoDauSachHetBan = r.So("HetBan") };
    }

    /// <summary>Thống kê độc giả theo ngày hôm nay.</summary>
    public static ThongKeDocGia LayDocGia(DateTime hom)
    {
        var r = ExecuteQuery(@"SELECT
                ISNULL(SUM(CASE WHEN HanThe IS NULL OR HanThe >= @Hom THEN 1 ELSE 0 END), 0) AS DangHoatDong,
                ISNULL(SUM(CASE WHEN NhomDocGia = N'Sinh viên' THEN 1 ELSE 0 END), 0) AS SinhVien,
                ISNULL(SUM(CASE WHEN NhomDocGia IN (N'Giảng viên', N'Cán bộ') THEN 1 ELSE 0 END), 0) AS GvCb,
                ISNULL(SUM(CASE WHEN HanThe >= @Hom AND HanThe <= DATEADD(DAY, @Ngay, @Hom) THEN 1 ELSE 0 END), 0) AS SapHetHan
            FROM DocGia", Tham("@Hom", hom.Date), Tham("@Ngay", HangSo.SoNgayBaoSapHetHan)).Rows[0];
        return new ThongKeDocGia
        {
            DangHoatDong = r.So("DangHoatDong"), SinhVien = r.So("SinhVien"),
            GiangVienCanBo = r.So("GvCb"), SapHetHan = r.So("SapHetHan")
        };
    }

    /// <summary>Thống kê phiếu mượn (số phiếu và số cuốn) theo ngày hôm nay và tháng hiện tại.</summary>
    public static ThongKePhieu LayPhieu(DateTime hom)
    {
        DateTime dauThang = new DateTime(hom.Year, hom.Month, 1);
        const string sql = @"
WITH p AS (
    SELECT pm.MaPhieu, pm.HenTra,
           SUM(CASE WHEN c.TinhTrangKhiTra = N'Chưa ghi nhận' THEN 1 ELSE 0 END) AS ConMuon,
           MAX(c.NgayTra) AS NgayTraCuoi,
           SUM(CASE WHEN c.TinhTrangKhiTra <> N'Chưa ghi nhận' AND c.NgayTra >= @DauThang AND c.NgayTra < @CuoiThang THEN 1 ELSE 0 END) AS DaTraThang
    FROM PhieuMuon pm JOIN ChiTietMuon c ON c.MaPhieu = pm.MaPhieu
    GROUP BY pm.MaPhieu, pm.HenTra)
SELECT
  ISNULL(SUM(CASE WHEN ConMuon > 0 AND HenTra >= @Hom THEN 1 ELSE 0 END), 0) AS DungHan,
  ISNULL(SUM(CASE WHEN ConMuon > 0 AND HenTra < @Hom THEN 1 ELSE 0 END), 0) AS QuaHan,
  ISNULL(SUM(CASE WHEN ConMuon = 0 THEN 1 ELSE 0 END), 0) AS DaTraTatCa,
  ISNULL(SUM(CASE WHEN ConMuon = 0 AND NgayTraCuoi >= @DauThang AND NgayTraCuoi < @CuoiThang THEN 1 ELSE 0 END), 0) AS DaTraThang,
  ISNULL(SUM(CASE WHEN ConMuon > 0 AND HenTra = @Hom THEN 1 ELSE 0 END), 0) AS DenHan,
  ISNULL(SUM(CASE WHEN HenTra >= @Hom THEN ConMuon ELSE 0 END), 0) AS CuonLuuHanh,
  ISNULL(SUM(CASE WHEN HenTra < @Hom THEN ConMuon ELSE 0 END), 0) AS CuonQuaHan,
  ISNULL(SUM(DaTraThang), 0) AS CuonDaTra,
  ISNULL(SUM(CASE WHEN HenTra = @Hom THEN ConMuon ELSE 0 END), 0) AS CuonDenHan
FROM p";
        var r = ExecuteQuery(sql, Tham("@Hom", hom.Date), Tham("@DauThang", dauThang), Tham("@CuoiThang", dauThang.AddMonths(1))).Rows[0];
        return new ThongKePhieu
        {
            DungHan = r.So("DungHan"), QuaHan = r.So("QuaHan"), DaTraTatCa = r.So("DaTraTatCa"),
            DaTraTrongThang = r.So("DaTraThang"), DenHanHomNay = r.So("DenHan"),
            CuonLuuHanh = r.So("CuonLuuHanh"), CuonQuaHan = r.So("CuonQuaHan"),
            CuonDaTraTrongThang = r.So("CuonDaTra"), CuonDenHan = r.So("CuonDenHan")
        };
    }

    /// <summary>Các số liệu lẻ của màn Tổng quan (thể loại, độc giả mới, sách hư hỏng/mất).</summary>
    public static ThongKeTongQuan LayTongQuanCoBan(DateTime hom)
    {
        DateTime dauThang = new DateTime(hom.Year, hom.Month, 1);
        var r = ExecuteQuery(@"SELECT
                (SELECT COUNT(*) FROM TheLoai) AS SoTheLoai,
                (SELECT COUNT(*) FROM DocGia) AS TongDocGia,
                (SELECT COUNT(*) FROM DocGia WHERE NgayCap >= @DauThang AND NgayCap < @CuoiThang) AS DocGiaMoi,
                (SELECT COUNT(*) FROM ChiTietMuon WHERE TinhTrangKhiTra = N'Hư hỏng') AS HuHong,
                (SELECT COUNT(*) FROM ChiTietMuon WHERE TinhTrangKhiTra = N'Mất') AS Mat",
            Tham("@DauThang", dauThang), Tham("@CuoiThang", dauThang.AddMonths(1))).Rows[0];
        return new ThongKeTongQuan
        {
            SoTheLoai = r.So("SoTheLoai"), TongDocGia = r.So("TongDocGia"), DocGiaMoiTrongThang = r.So("DocGiaMoi"),
            SachHuHong = r.So("HuHong"), SachMat = r.So("Mat")
        };
    }

    /// <summary>Số lượt sách được mượn theo từng tháng của một năm (luôn trả đủ 12 tháng).</summary>
    public static List<ThongKeThang> LayLuotMuonTheoThang(int nam)
    {
        var dt = ExecuteQuery(@"SELECT MONTH(pm.NgayMuon) AS Thang, COUNT(*) AS SoLuot
                                FROM PhieuMuon pm JOIN ChiTietMuon c ON c.MaPhieu = pm.MaPhieu
                                WHERE YEAR(pm.NgayMuon) = @Nam GROUP BY MONTH(pm.NgayMuon)", Tham("@Nam", nam));
        return DayDu12Thang(dt, "SoLuot", null);
    }

    /// <summary>Số phiếu phát sinh quá hạn theo tháng (GiaTri) và số phiếu đã thu hồi đủ sách (GiaTri2).</summary>
    public static List<ThongKeThang> LayQuaHanTheoThang(int nam, DateTime hom)
    {
        const string sql = @"
SELECT Thang, COUNT(*) AS SoPhieu, SUM(CASE WHEN ConMuon = 0 THEN 1 ELSE 0 END) AS DaThuHoi
FROM (SELECT pm.MaPhieu, MONTH(pm.HenTra) AS Thang,
             SUM(CASE WHEN c.TinhTrangKhiTra = N'Chưa ghi nhận' THEN 1 ELSE 0 END) AS ConMuon,
             MAX(CASE WHEN (c.TinhTrangKhiTra = N'Chưa ghi nhận' AND pm.HenTra < @Hom) OR c.NgayTra > pm.HenTra THEN 1 ELSE 0 END) AS TungQuaHan
      FROM PhieuMuon pm JOIN ChiTietMuon c ON c.MaPhieu = pm.MaPhieu
      WHERE YEAR(pm.HenTra) = @Nam
      GROUP BY pm.MaPhieu, MONTH(pm.HenTra)) t
WHERE TungQuaHan = 1 GROUP BY Thang";
        var dt = ExecuteQuery(sql, Tham("@Nam", nam), Tham("@Hom", hom.Date));
        return DayDu12Thang(dt, "SoPhieu", "DaThuHoi");
    }

    private static List<ThongKeThang> DayDu12Thang(DataTable dt, string cot1, string cot2)
    {
        var ds = Enumerable.Range(1, 12).Select(t => new ThongKeThang { Thang = t }).ToList();
        foreach (DataRow r in dt.Rows)
        {
            var t = ds[r.So("Thang") - 1];
            t.GiaTri = r.So(cot1);
            if (cot2 != null) t.GiaTri2 = r.So(cot2);
        }
        return ds;
    }

    /// <summary>Danh sách phiếu quá hạn (còn bản chưa trả và hẹn trả trước ngày đối chiếu).</summary>
    public static List<QuaHanHienThi> LayQuaHan(DateTime ngayDoiChieu)
    {
        const string sql = @"
SELECT pm.MaPhieu, pm.NgayMuon, pm.HenTra, dg.MaDocGia, dg.HoTen, dg.NhomDocGia, dg.DienThoai,
       DATEDIFF(DAY, pm.HenTra, @Ngay) AS SoNgayTre,
       COUNT(*) AS SoCuon,
       STRING_AGG(CONCAT(c.MaSach, N' ', s.TenSach), CHAR(10)) AS DanhSachSach
FROM PhieuMuon pm
JOIN DocGia dg ON dg.MaDocGia = pm.MaDocGia
JOIN ChiTietMuon c ON c.MaPhieu = pm.MaPhieu AND c.TinhTrangKhiTra = N'Chưa ghi nhận'
JOIN Sach s ON s.MaSach = c.MaSach
WHERE pm.HenTra < @Ngay
GROUP BY pm.MaPhieu, pm.NgayMuon, pm.HenTra, dg.MaDocGia, dg.HoTen, dg.NhomDocGia, dg.DienThoai
ORDER BY SoNgayTre DESC, pm.MaPhieu";
        var dt = ExecuteQuery(sql, Tham("@Ngay", ngayDoiChieu.Date));
        return dt.Rows.Cast<DataRow>().Select(r => new QuaHanHienThi
        {
            MaPhieu = r.Chuoi("MaPhieu"), NgayMuon = r.Ngay("NgayMuon") ?? DateTime.MinValue,
            HenTra = r.Ngay("HenTra") ?? DateTime.MinValue, MaDocGia = r.Chuoi("MaDocGia"),
            HoTen = r.Chuoi("HoTen"), NhomDocGia = r.Chuoi("NhomDocGia"), DienThoai = r.Chuoi("DienThoai"),
            SoNgayTre = r.So("SoNgayTre"), SoCuon = r.So("SoCuon"), DanhSachSach = r.Chuoi("DanhSachSach")
        }).ToList();
    }
}

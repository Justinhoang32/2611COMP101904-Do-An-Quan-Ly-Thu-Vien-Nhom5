using System.Data;
using DTO;
using DTO.Entities;
using DTO.Models;
using Microsoft.Data.SqlClient;
using static DAL.DatabaseHelper;

namespace DAL;

/// <summary>Truy cập bảng DocGia (kèm số sách đang mượn, phiếu quá hạn, trạng thái thẻ).</summary>
public static class DocGiaDAL
{
    // Truy vấn nền: tính số sách đang mượn, số phiếu quá hạn và trạng thái thẻ (theo ngày @Hom).
    private const string TruyVanNen = @"
SELECT dg.MaDocGia, dg.HoTen, dg.NhomDocGia, dg.MaSinhVien, dg.Khoa, dg.DienThoai, dg.Email,
       dg.NgayCap, dg.HanThe, dg.TrangThai,
       ISNULL(dm.SoSach, 0) AS SoSachDangMuon,
       ISNULL(qh.SoPhieu, 0) AS SoPhieuQuaHan,
       CASE WHEN dg.HanThe < @Hom THEN N'Hết hạn'
            WHEN ISNULL(qh.SoPhieu, 0) > 0 THEN N'Có quá hạn'
            WHEN dg.HanThe <= DATEADD(DAY, @NgayCanhBao, @Hom) THEN N'Sắp hết hạn'
            ELSE N'Hoạt động' END AS TrangThaiHienThi
FROM DocGia dg
OUTER APPLY (SELECT COUNT(*) AS SoSach
             FROM PhieuMuon pm JOIN ChiTietMuon c ON c.MaPhieu = pm.MaPhieu
             WHERE pm.MaDocGia = dg.MaDocGia AND c.TinhTrangKhiTra = N'Chưa ghi nhận') dm
OUTER APPLY (SELECT COUNT(DISTINCT pm.MaPhieu) AS SoPhieu
             FROM PhieuMuon pm JOIN ChiTietMuon c ON c.MaPhieu = pm.MaPhieu
             WHERE pm.MaDocGia = dg.MaDocGia AND pm.HenTra < @Hom AND c.TinhTrangKhiTra = N'Chưa ghi nhận') qh";

    private static SqlParameter[] ThamNgay(DateTime hom) => new[]
    {
        Tham("@Hom", hom.Date), Tham("@NgayCanhBao", HangSo.SoNgayBaoSapHetHan)
    };

    /// <summary>Tìm độc giả theo từ khóa, nhóm, trạng thái; có phân trang.</summary>
    public static KetQuaPhanTrang<DocGiaHienThi> TimKiem(DocGiaBoLoc f, DateTime hom, int trang, int kichThuoc)
    {
        f ??= new DocGiaBoLoc();
        var ds = new List<SqlParameter>(ThamNgay(hom));
        var dk = new List<string>();
        if (!string.IsNullOrWhiteSpace(f.TuKhoa))
        {
            dk.Add("(MaDocGia LIKE @tk OR HoTen LIKE @tk OR MaSinhVien LIKE @tk)");
            ds.Add(Tham("@tk", DangLike(f.TuKhoa)));
        }
        if (!string.IsNullOrWhiteSpace(f.Nhom)) { dk.Add("NhomDocGia = @nhom"); ds.Add(Tham("@nhom", f.Nhom)); }
        if (!string.IsNullOrWhiteSpace(f.TrangThai)) { dk.Add("TrangThaiHienThi = @tt"); ds.Add(Tham("@tt", f.TrangThai)); }
        string where = dk.Count == 0 ? "" : "WHERE " + string.Join(" AND ", dk);

        string cte = "WITH v AS (" + TruyVanNen + ") ";
        int tong = LaySo(ExecuteScalar(cte + "SELECT COUNT(*) FROM v " + where, SaoChep(ds)));

        ds.Add(Tham("@bd", (trang - 1) * kichThuoc));
        ds.Add(Tham("@sl", kichThuoc));
        var dt = ExecuteQuery(cte + "SELECT * FROM v " + where + " ORDER BY MaDocGia OFFSET @bd ROWS FETCH NEXT @sl ROWS ONLY", ds.ToArray());
        return new KetQuaPhanTrang<DocGiaHienThi>
        {
            TongSoDong = tong, TrangHienTai = trang, KichThuocTrang = kichThuoc,
            DuLieu = dt.Rows.Cast<DataRow>().Select(Map).ToList()
        };
    }

    /// <summary>Lấy một độc giả theo mã; null nếu không có.</summary>
    public static DocGiaHienThi LayTheoMa(string maDocGia, DateTime hom)
    {
        var ds = new List<SqlParameter>(ThamNgay(hom)) { Tham("@ma", maDocGia) };
        var dt = ExecuteQuery("WITH v AS (" + TruyVanNen + ") SELECT * FROM v WHERE MaDocGia = @ma", ds.ToArray());
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    private static DocGiaHienThi Map(DataRow r) => new DocGiaHienThi
    {
        MaDocGia = r.Chuoi("MaDocGia"), HoTen = r.Chuoi("HoTen"), NhomDocGia = r.Chuoi("NhomDocGia"),
        MaSinhVien = r.Chuoi("MaSinhVien"), Khoa = r.Chuoi("Khoa"), DienThoai = r.Chuoi("DienThoai"),
        Email = r.Chuoi("Email"), NgayCap = r.Ngay("NgayCap"), HanThe = r.Ngay("HanThe"),
        TrangThai = r.Chuoi("TrangThai"), SoSachDangMuon = r.So("SoSachDangMuon"),
        SoPhieuQuaHan = r.So("SoPhieuQuaHan"), TrangThaiHienThi = r.Chuoi("TrangThaiHienThi")
    };

    public static bool TonTai(string maDocGia) =>
        LaySo(ExecuteScalar("SELECT COUNT(*) FROM DocGia WHERE MaDocGia = @ma", Tham("@ma", maDocGia))) > 0;

    public static string LayMaCuoi() =>
        Convert.ToString(ExecuteScalar("SELECT TOP 1 MaDocGia FROM DocGia ORDER BY LEN(MaDocGia) DESC, MaDocGia DESC"));

    public static int Them(DocGia d) =>
        ExecuteNonQuery(@"INSERT INTO DocGia (MaDocGia, HoTen, NhomDocGia, MaSinhVien, Khoa, DienThoai, Email, NgayCap, HanThe, TrangThai)
                          VALUES (@ma, @ten, @nhom, @msv, @khoa, @sdt, @email, @nc, @ht, @tt)",
            Tham("@ma", d.MaDocGia), Tham("@ten", d.HoTen), Tham("@nhom", d.NhomDocGia), Tham("@msv", d.MaSinhVien),
            Tham("@khoa", d.Khoa), Tham("@sdt", d.DienThoai), Tham("@email", d.Email),
            Tham("@nc", d.NgayCap), Tham("@ht", d.HanThe), Tham("@tt", d.TrangThai));

    public static int Sua(DocGia d) =>
        ExecuteNonQuery(@"UPDATE DocGia SET HoTen = @ten, NhomDocGia = @nhom, MaSinhVien = @msv, Khoa = @khoa,
                                 DienThoai = @sdt, Email = @email WHERE MaDocGia = @ma",
            Tham("@ma", d.MaDocGia), Tham("@ten", d.HoTen), Tham("@nhom", d.NhomDocGia), Tham("@msv", d.MaSinhVien),
            Tham("@khoa", d.Khoa), Tham("@sdt", d.DienThoai), Tham("@email", d.Email));

    /// <summary>Cấp mới / gia hạn thẻ: cập nhật nhóm, ngày cấp, hạn thẻ và đặt trạng thái Hoạt động.</summary>
    public static int CapThe(string maDocGia, string nhom, DateTime ngayCap, DateTime hanThe) =>
        ExecuteNonQuery(@"UPDATE DocGia SET NhomDocGia = @nhom, NgayCap = @nc, HanThe = @ht, TrangThai = N'Hoạt động'
                          WHERE MaDocGia = @ma",
            Tham("@ma", maDocGia), Tham("@nhom", nhom), Tham("@nc", ngayCap.Date), Tham("@ht", hanThe.Date));
}

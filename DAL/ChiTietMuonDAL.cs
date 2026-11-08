using DTO.Models;
using static DAL.DatabaseHelper;

namespace DAL;

/// <summary>Truy cập bảng ChiTietMuon (từng bản sách trong phiếu).</summary>
public static class ChiTietMuonDAL
{
    /// <summary>Lấy các bản sách của một phiếu, kèm tên sách và tác giả.</summary>
    public static List<ChiTietPhieuHienThi> LayTheoPhieu(string maPhieu)
    {
        const string sql = @"SELECT c.MaPhieu, c.MaSach, c.MaBanSach, c.TinhTrangKhiTra, c.GhiChu, c.NgayTra,
                                    s.TenSach, tg.TenTacGia
                             FROM ChiTietMuon c
                             JOIN Sach s ON s.MaSach = c.MaSach
                             LEFT JOIN TacGia tg ON tg.MaTacGia = s.MaTacGia
                             WHERE c.MaPhieu = @ma ORDER BY c.MaSach, c.MaBanSach";
        var dt = ExecuteQuery(sql, Tham("@ma", maPhieu));
        return dt.Rows.Cast<System.Data.DataRow>().Select(r => new ChiTietPhieuHienThi
        {
            MaPhieu = r.Chuoi("MaPhieu"), MaSach = r.Chuoi("MaSach"), MaBanSach = r.Chuoi("MaBanSach"),
            TinhTrangKhiTra = r.Chuoi("TinhTrangKhiTra"), GhiChu = r.Chuoi("GhiChu"), NgayTra = r.Ngay("NgayTra"),
            TenSach = r.Chuoi("TenSach"), TenTacGia = r.Chuoi("TenTacGia")
        }).ToList();
    }
}

using DTO.Entities;
using DTO.Models;
using Microsoft.Data.SqlClient;
using static DAL.DatabaseHelper;

namespace DAL;

/// <summary>Truy cập bảng TacGia.</summary>
public static class TacGiaDAL
{
    /// <summary>Lấy toàn bộ tác giả (dùng cho ComboBox).</summary>
    public static List<TacGia> LayTatCa()
    {
        var dt = ExecuteQuery("SELECT MaTacGia, TenTacGia, GioiThieu FROM TacGia ORDER BY TenTacGia");
        return dt.Rows.Cast<System.Data.DataRow>().Select(r => new TacGia
        {
            MaTacGia = r.Chuoi("MaTacGia"), TenTacGia = r.Chuoi("TenTacGia"), GioiThieu = r.Chuoi("GioiThieu")
        }).ToList();
    }

    /// <summary>Tìm theo mã/tên, có phân trang, kèm số đầu sách của tác giả.</summary>
    public static KetQuaPhanTrang<TacGiaHienThi> TimKiem(string tuKhoa, int trang, int kichThuoc)
    {
        const string tu = "FROM TacGia tg WHERE (tg.MaTacGia LIKE @tk OR tg.TenTacGia LIKE @tk)";
        var tham = new SqlParameter[] { Tham("@tk", DangLike(tuKhoa)) };
        int tong = LaySo(ExecuteScalar("SELECT COUNT(*) " + tu, SaoChep(tham)));

        string sql = @"SELECT tg.MaTacGia, tg.TenTacGia, tg.GioiThieu,
                              (SELECT COUNT(*) FROM Sach s WHERE s.MaTacGia = tg.MaTacGia) AS SoDauSach " + tu +
                     " ORDER BY tg.MaTacGia OFFSET @bd ROWS FETCH NEXT @sl ROWS ONLY";
        var dt = ExecuteQuery(sql, tham.Concat(new[] { Tham("@bd", (trang - 1) * kichThuoc), Tham("@sl", kichThuoc) }).ToArray());
        return new KetQuaPhanTrang<TacGiaHienThi>
        {
            TongSoDong = tong, TrangHienTai = trang, KichThuocTrang = kichThuoc,
            DuLieu = dt.Rows.Cast<System.Data.DataRow>().Select(r => new TacGiaHienThi
            {
                MaTacGia = r.Chuoi("MaTacGia"), TenTacGia = r.Chuoi("TenTacGia"),
                GioiThieu = r.Chuoi("GioiThieu"), SoDauSach = r.So("SoDauSach")
            }).ToList()
        };
    }

    public static TacGia LayTheoMa(string ma)
    {
        var dt = ExecuteQuery("SELECT MaTacGia, TenTacGia, GioiThieu FROM TacGia WHERE MaTacGia = @ma", Tham("@ma", ma));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new TacGia { MaTacGia = r.Chuoi("MaTacGia"), TenTacGia = r.Chuoi("TenTacGia"), GioiThieu = r.Chuoi("GioiThieu") };
    }

    public static bool TonTaiTen(string ten, string maLoaiTru)
    {
        return LaySo(ExecuteScalar("SELECT COUNT(*) FROM TacGia WHERE TenTacGia = @ten AND MaTacGia <> @ma",
            Tham("@ten", ten), Tham("@ma", maLoaiTru ?? ""))) > 0;
    }

    public static string LayMaCuoi() =>
        Convert.ToString(ExecuteScalar("SELECT TOP 1 MaTacGia FROM TacGia ORDER BY LEN(MaTacGia) DESC, MaTacGia DESC"));

    public static int Them(TacGia t) =>
        ExecuteNonQuery("INSERT INTO TacGia (MaTacGia, TenTacGia, GioiThieu) VALUES (@ma, @ten, @gt)",
            Tham("@ma", t.MaTacGia), Tham("@ten", t.TenTacGia), Tham("@gt", t.GioiThieu));

    public static int Sua(TacGia t) =>
        ExecuteNonQuery("UPDATE TacGia SET TenTacGia = @ten, GioiThieu = @gt WHERE MaTacGia = @ma",
            Tham("@ma", t.MaTacGia), Tham("@ten", t.TenTacGia), Tham("@gt", t.GioiThieu));
}

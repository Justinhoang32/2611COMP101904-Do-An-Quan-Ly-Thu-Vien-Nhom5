using DTO.Entities;
using DTO.Models;
using Microsoft.Data.SqlClient;
using static DAL.DatabaseHelper;

namespace DAL;

/// <summary>Truy cập bảng TheLoai.</summary>
public static class TheLoaiDAL
{
    /// <summary>Lấy toàn bộ thể loại (dùng cho ComboBox).</summary>
    public static List<TheLoai> LayTatCa()
    {
        var dt = ExecuteQuery("SELECT MaTheLoai, TenTheLoai, MoTa FROM TheLoai ORDER BY TenTheLoai");
        return dt.Rows.Cast<System.Data.DataRow>().Select(r => new TheLoai
        {
            MaTheLoai = r.Chuoi("MaTheLoai"), TenTheLoai = r.Chuoi("TenTheLoai"), MoTa = r.Chuoi("MoTa")
        }).ToList();
    }

    /// <summary>Tìm theo mã/tên, có phân trang, kèm số đầu sách thuộc thể loại.</summary>
    public static KetQuaPhanTrang<TheLoaiHienThi> TimKiem(string tuKhoa, int trang, int kichThuoc)
    {
        const string tu = "FROM TheLoai tl WHERE (tl.MaTheLoai LIKE @tk OR tl.TenTheLoai LIKE @tk)";
        var tham = new SqlParameter[] { Tham("@tk", DangLike(tuKhoa)) };
        int tong = LaySo(ExecuteScalar("SELECT COUNT(*) " + tu, SaoChep(tham)));

        string sql = @"SELECT tl.MaTheLoai, tl.TenTheLoai, tl.MoTa,
                              (SELECT COUNT(*) FROM Sach s WHERE s.MaTheLoai = tl.MaTheLoai) AS SoDauSach " + tu +
                     " ORDER BY tl.MaTheLoai OFFSET @bd ROWS FETCH NEXT @sl ROWS ONLY";
        var dt = ExecuteQuery(sql, tham.Concat(new[] { Tham("@bd", (trang - 1) * kichThuoc), Tham("@sl", kichThuoc) }).ToArray());
        return new KetQuaPhanTrang<TheLoaiHienThi>
        {
            TongSoDong = tong, TrangHienTai = trang, KichThuocTrang = kichThuoc,
            DuLieu = dt.Rows.Cast<System.Data.DataRow>().Select(r => new TheLoaiHienThi
            {
                MaTheLoai = r.Chuoi("MaTheLoai"), TenTheLoai = r.Chuoi("TenTheLoai"),
                MoTa = r.Chuoi("MoTa"), SoDauSach = r.So("SoDauSach")
            }).ToList()
        };
    }

    public static TheLoai LayTheoMa(string ma)
    {
        var dt = ExecuteQuery("SELECT MaTheLoai, TenTheLoai, MoTa FROM TheLoai WHERE MaTheLoai = @ma", Tham("@ma", ma));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new TheLoai { MaTheLoai = r.Chuoi("MaTheLoai"), TenTheLoai = r.Chuoi("TenTheLoai"), MoTa = r.Chuoi("MoTa") };
    }

    /// <summary>Kiểm tra trùng tên (bỏ qua chính mã đang sửa).</summary>
    public static bool TonTaiTen(string ten, string maLoaiTru)
    {
        return LaySo(ExecuteScalar("SELECT COUNT(*) FROM TheLoai WHERE TenTheLoai = @ten AND MaTheLoai <> @ma",
            Tham("@ten", ten), Tham("@ma", maLoaiTru ?? ""))) > 0;
    }

    /// <summary>Mã lớn nhất hiện có (để BLL sinh mã kế tiếp).</summary>
    public static string LayMaCuoi() =>
        Convert.ToString(ExecuteScalar("SELECT TOP 1 MaTheLoai FROM TheLoai ORDER BY LEN(MaTheLoai) DESC, MaTheLoai DESC"));

    public static int Them(TheLoai t) =>
        ExecuteNonQuery("INSERT INTO TheLoai (MaTheLoai, TenTheLoai, MoTa) VALUES (@ma, @ten, @mota)",
            Tham("@ma", t.MaTheLoai), Tham("@ten", t.TenTheLoai), Tham("@mota", t.MoTa));

    public static int Sua(TheLoai t) =>
        ExecuteNonQuery("UPDATE TheLoai SET TenTheLoai = @ten, MoTa = @mota WHERE MaTheLoai = @ma",
            Tham("@ma", t.MaTheLoai), Tham("@ten", t.TenTheLoai), Tham("@mota", t.MoTa));
}

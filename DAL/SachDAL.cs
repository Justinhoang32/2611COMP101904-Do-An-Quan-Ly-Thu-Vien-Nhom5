using System.Data;
using DTO.Entities;
using DTO.Models;
using Microsoft.Data.SqlClient;
using static DAL.DatabaseHelper;

namespace DAL;

/// <summary>Truy cập bảng Sach (kèm TacGia, TheLoai khi hiển thị).</summary>
public static class SachDAL
{
    private const string CotChon =
        @"s.MaSach, s.TenSach, s.MaTacGia, tg.TenTacGia, s.MaTheLoai, tl.TenTheLoai, s.NamXuatBan, s.TongSoBan,
          s.SoBanCoSan, s.ViTriKe, s.NhaXuatBan, s.NgonNgu, s.AnhBia, s.TinhTrangLuuHanh";

    private const string TuBang =
        @"FROM Sach s LEFT JOIN TacGia tg ON tg.MaTacGia = s.MaTacGia
                      LEFT JOIN TheLoai tl ON tl.MaTheLoai = s.MaTheLoai";

    /// <summary>Ghép điều kiện WHERE từ bộ lọc (chỉ thêm điều kiện khi người dùng có nhập).</summary>
    private static string TaoDieuKien(SachBoLoc f, List<SqlParameter> ds)
    {
        var dk = new List<string>();
        if (!string.IsNullOrWhiteSpace(f.TuKhoa))
        {
            dk.Add("(s.MaSach LIKE @tk OR s.TenSach LIKE @tk OR tg.TenTacGia LIKE @tk)");
            ds.Add(Tham("@tk", DangLike(f.TuKhoa)));
        }
        if (!string.IsNullOrWhiteSpace(f.MaSach)) { dk.Add("s.MaSach LIKE @ma"); ds.Add(Tham("@ma", DangLike(f.MaSach))); }
        if (!string.IsNullOrWhiteSpace(f.TenSach)) { dk.Add("s.TenSach LIKE @ten"); ds.Add(Tham("@ten", DangLike(f.TenSach))); }
        if (!string.IsNullOrWhiteSpace(f.TenTacGia)) { dk.Add("tg.TenTacGia LIKE @tgia"); ds.Add(Tham("@tgia", DangLike(f.TenTacGia))); }
        if (!string.IsNullOrWhiteSpace(f.MaTheLoai)) { dk.Add("s.MaTheLoai = @tl"); ds.Add(Tham("@tl", f.MaTheLoai)); }
        if (!string.IsNullOrWhiteSpace(f.TinhTrangLuuHanh)) { dk.Add("s.TinhTrangLuuHanh = @luu"); ds.Add(Tham("@luu", f.TinhTrangLuuHanh)); }
        if (f.ChiConSach || f.TinhTrang == HangSoSachCoSan) dk.Add("s.SoBanCoSan > 0");
        else if (f.TinhTrang == HangSoSachHetSach) dk.Add("s.SoBanCoSan <= 0");
        return dk.Count == 0 ? "" : "WHERE " + string.Join(" AND ", dk);
    }

    private const string HangSoSachCoSan = DTO.HangSo.SachCoSan;
    private const string HangSoSachHetSach = DTO.HangSo.SachHetSach;

    /// <summary>Tìm sách theo bộ lọc, có phân trang.</summary>
    public static KetQuaPhanTrang<SachHienThi> TimKiem(SachBoLoc boLoc, int trang, int kichThuoc)
    {
        var ds = new List<SqlParameter>();
        string where = TaoDieuKien(boLoc ?? new SachBoLoc(), ds);

        // Đếm tổng số dòng (dùng bản sao tham số)
        int tong = LaySo(ExecuteScalar($"SELECT COUNT(*) {TuBang} {where}", SaoChep(ds)));

        // Lấy dữ liệu trang hiện tại
        ds.Add(Tham("@bd", (trang - 1) * kichThuoc));
        ds.Add(Tham("@sl", kichThuoc));
        var dt = ExecuteQuery($"SELECT {CotChon} {TuBang} {where} ORDER BY s.MaSach OFFSET @bd ROWS FETCH NEXT @sl ROWS ONLY", ds.ToArray());

        return new KetQuaPhanTrang<SachHienThi>
        {
            TongSoDong = tong, TrangHienTai = trang, KichThuocTrang = kichThuoc,
            DuLieu = dt.Rows.Cast<DataRow>().Select(Map).ToList()
        };
    }

    /// <summary>Lấy một sách theo mã (đầy đủ cả ảnh bìa). Null nếu không có.</summary>
    public static SachHienThi LayTheoMa(string maSach)
    {
        var dt = ExecuteQuery($"SELECT {CotChon} {TuBang} WHERE s.MaSach = @ma", Tham("@ma", maSach));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    private static SachHienThi Map(DataRow r) => new SachHienThi
    {
        MaSach = r.Chuoi("MaSach"), TenSach = r.Chuoi("TenSach"),
        MaTacGia = r.Chuoi("MaTacGia"), TenTacGia = r.Chuoi("TenTacGia"),
        MaTheLoai = r.Chuoi("MaTheLoai"), TenTheLoai = r.Chuoi("TenTheLoai"),
        NamXuatBan = r.So("NamXuatBan"), TongSoBan = r.So("TongSoBan"), SoBanCoSan = r.So("SoBanCoSan"),
        ViTriKe = r.Chuoi("ViTriKe"), NhaXuatBan = r.Chuoi("NhaXuatBan"), NgonNgu = r.Chuoi("NgonNgu"),
        AnhBia = r.NhiPhan("AnhBia"), TinhTrangLuuHanh = r.Chuoi("TinhTrangLuuHanh")
    };

    public static bool TonTai(string maSach) =>
        LaySo(ExecuteScalar("SELECT COUNT(*) FROM Sach WHERE MaSach = @ma", Tham("@ma", maSach))) > 0;

    public static string LayMaCuoi() =>
        Convert.ToString(ExecuteScalar("SELECT TOP 1 MaSach FROM Sach ORDER BY LEN(MaSach) DESC, MaSach DESC"));

    /// <summary>Thêm đầu sách mới; số bản có sẵn ban đầu bằng tổng số bản.</summary>
    public static int Them(Sach s)
    {
        const string sql = @"INSERT INTO Sach (MaSach, TenSach, MaTacGia, MaTheLoai, NamXuatBan, TongSoBan, SoBanCoSan,
                                               ViTriKe, NhaXuatBan, NgonNgu, AnhBia, TinhTrangLuuHanh)
                             VALUES (@MaSach, @TenSach, @MaTacGia, @MaTheLoai, @Nam, @Tong, @Tong,
                                     @ViTri, @Nxb, @NgonNgu, @Anh, @Luu)";
        return ExecuteNonQuery(sql,
            Tham("@MaSach", s.MaSach), Tham("@TenSach", s.TenSach), Tham("@MaTacGia", s.MaTacGia),
            Tham("@MaTheLoai", s.MaTheLoai), Tham("@Nam", s.NamXuatBan > 0 ? (object)s.NamXuatBan : null),
            Tham("@Tong", s.TongSoBan), Tham("@ViTri", s.ViTriKe), Tham("@Nxb", s.NhaXuatBan),
            Tham("@NgonNgu", s.NgonNgu), ThamNhiPhan("@Anh", s.AnhBia), Tham("@Luu", s.TinhTrangLuuHanh));
    }

    /// <summary>
    /// Sửa đầu sách. Khi đổi tổng số bản, số bản có sẵn được điều chỉnh theo chênh lệch
    /// (không làm thay đổi số bản đang được mượn). capNhatAnh = false thì giữ nguyên ảnh cũ.
    /// </summary>
    public static int Sua(Sach s, bool capNhatAnh)
    {
        string sql = @"UPDATE Sach SET TenSach = @TenSach, MaTacGia = @MaTacGia, MaTheLoai = @MaTheLoai, NamXuatBan = @Nam,
                              SoBanCoSan = SoBanCoSan + (@Tong - TongSoBan), TongSoBan = @Tong,
                              ViTriKe = @ViTri, NhaXuatBan = @Nxb, NgonNgu = @NgonNgu, TinhTrangLuuHanh = @Luu"
                     + (capNhatAnh ? ", AnhBia = @Anh" : "") + " WHERE MaSach = @MaSach";
        var ds = new List<SqlParameter>
        {
            Tham("@MaSach", s.MaSach), Tham("@TenSach", s.TenSach), Tham("@MaTacGia", s.MaTacGia),
            Tham("@MaTheLoai", s.MaTheLoai), Tham("@Nam", s.NamXuatBan > 0 ? (object)s.NamXuatBan : null),
            Tham("@Tong", s.TongSoBan), Tham("@ViTri", s.ViTriKe), Tham("@Nxb", s.NhaXuatBan),
            Tham("@NgonNgu", s.NgonNgu), Tham("@Luu", s.TinhTrangLuuHanh)
        };
        if (capNhatAnh) ds.Add(ThamNhiPhan("@Anh", s.AnhBia));
        return ExecuteNonQuery(sql, ds.ToArray());
    }
}

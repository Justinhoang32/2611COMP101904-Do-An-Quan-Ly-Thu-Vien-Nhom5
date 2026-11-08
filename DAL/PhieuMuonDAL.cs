using System.Data;
using DTO;
using DTO.Entities;
using DTO.Models;
using Microsoft.Data.SqlClient;
using static DAL.DatabaseHelper;

namespace DAL;

/// <summary>Truy cập bảng PhieuMuon; lập phiếu và ghi nhận trả sách (có transaction).</summary>
public static class PhieuMuonDAL
{
    // Truy vấn nền: ghép độc giả, thủ thư, đếm cuốn và TÍNH trạng thái phiếu theo ngày @Hom.
    private const string TruyVanNen = @"
SELECT pm.MaPhieu, pm.MaDocGia, dg.HoTen AS HoTenDocGia, dg.NhomDocGia, dg.DienThoai,
       pm.MaTaiKhoan, tk.HoTen AS TenThuThu, pm.NgayMuon, pm.HenTra, pm.TrangThai, pm.GhiChu,
       ct.Tong AS TongSoCuon, ISNULL(ct.DaTra, 0) AS SoCuonDaTra,
       CASE WHEN ct.Tong - ISNULL(ct.DaTra, 0) = 0 THEN N'Đã trả'
            WHEN pm.HenTra < @Hom THEN N'Quá hạn'
            WHEN ISNULL(ct.DaTra, 0) > 0 THEN N'Trả một phần'
            ELSE N'Đang mượn' END AS TrangThaiHienThi,
       CASE WHEN ct.Tong - ISNULL(ct.DaTra, 0) > 0 AND pm.HenTra < @Hom
            THEN DATEDIFF(DAY, pm.HenTra, @Hom) ELSE 0 END AS SoNgayTre
FROM PhieuMuon pm
JOIN DocGia dg ON dg.MaDocGia = pm.MaDocGia
LEFT JOIN TaiKhoan tk ON tk.MaTaiKhoan = pm.MaTaiKhoan
CROSS APPLY (SELECT COUNT(*) AS Tong,
                    SUM(CASE WHEN c.TinhTrangKhiTra <> N'Chưa ghi nhận' THEN 1 ELSE 0 END) AS DaTra
             FROM ChiTietMuon c WHERE c.MaPhieu = pm.MaPhieu) ct";

    private static PhieuMuonHienThi Map(DataRow r) => new PhieuMuonHienThi
    {
        MaPhieu = r.Chuoi("MaPhieu"), MaDocGia = r.Chuoi("MaDocGia"), HoTenDocGia = r.Chuoi("HoTenDocGia"),
        NhomDocGia = r.Chuoi("NhomDocGia"), DienThoai = r.Chuoi("DienThoai"),
        MaTaiKhoan = r.So("MaTaiKhoan"), TenThuThu = r.Chuoi("TenThuThu"),
        NgayMuon = r.Ngay("NgayMuon"), HenTra = r.Ngay("HenTra"), TrangThai = r.Chuoi("TrangThai"),
        GhiChu = r.Chuoi("GhiChu"), TongSoCuon = r.So("TongSoCuon"), SoCuonDaTra = r.So("SoCuonDaTra"),
        TrangThaiHienThi = r.Chuoi("TrangThaiHienThi"), SoNgayTre = r.So("SoNgayTre")
    };

    /// <summary>Tìm phiếu theo từ khóa, tab trạng thái, khoảng ngày mượn; có phân trang.</summary>
    public static KetQuaPhanTrang<PhieuMuonHienThi> TimKiem(PhieuBoLoc f, DateTime hom, int trang, int kichThuoc)
    {
        f ??= new PhieuBoLoc();
        var ds = new List<SqlParameter> { Tham("@Hom", hom.Date) };
        var dk = new List<string>();
        if (!string.IsNullOrWhiteSpace(f.TuKhoa))
        {
            dk.Add("(MaPhieu LIKE @tk OR MaDocGia LIKE @tk OR HoTenDocGia LIKE @tk)");
            ds.Add(Tham("@tk", DangLike(f.TuKhoa)));
        }
        switch (f.TrangThai)
        {
            case HangSo.PhieuDangMuon: dk.Add("TrangThaiHienThi IN (N'Đang mượn', N'Trả một phần')"); break;
            case HangSo.PhieuDaTra: dk.Add("TrangThaiHienThi = N'Đã trả'"); break;
            case HangSo.PhieuQuaHan: dk.Add("TrangThaiHienThi = N'Quá hạn'"); break;
        }
        if (f.TuNgay.HasValue) { dk.Add("NgayMuon >= @tu"); ds.Add(Tham("@tu", f.TuNgay.Value.Date)); }
        if (f.DenNgay.HasValue) { dk.Add("NgayMuon <= @den"); ds.Add(Tham("@den", f.DenNgay.Value.Date)); }
        string where = dk.Count == 0 ? "" : "WHERE " + string.Join(" AND ", dk);

        string cte = "WITH v AS (" + TruyVanNen + ") ";
        int tong = LaySo(ExecuteScalar(cte + "SELECT COUNT(*) FROM v " + where, SaoChep(ds)));

        ds.Add(Tham("@bd", (trang - 1) * kichThuoc));
        ds.Add(Tham("@sl", kichThuoc));
        var dt = ExecuteQuery(cte + "SELECT * FROM v " + where +
                              " ORDER BY NgayMuon DESC, MaPhieu DESC OFFSET @bd ROWS FETCH NEXT @sl ROWS ONLY", ds.ToArray());
        return new KetQuaPhanTrang<PhieuMuonHienThi>
        {
            TongSoDong = tong, TrangHienTai = trang, KichThuocTrang = kichThuoc,
            DuLieu = dt.Rows.Cast<DataRow>().Select(Map).ToList()
        };
    }

    /// <summary>Lấy một phiếu theo mã; null nếu không có.</summary>
    public static PhieuMuonHienThi LayTheoMa(string maPhieu, DateTime hom)
    {
        var dt = ExecuteQuery("WITH v AS (" + TruyVanNen + ") SELECT * FROM v WHERE MaPhieu = @ma",
            Tham("@Hom", hom.Date), Tham("@ma", maPhieu));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    /// <summary>Mã phiếu lớn nhất trong ngày (để BLL sinh số thứ tự kế tiếp).</summary>
    public static string LayMaPhieuCuoiTrongNgay(DateTime ngay)
    {
        string tienTo = "PM" + ngay.ToString("yyMMdd") + "-";
        return Convert.ToString(ExecuteScalar("SELECT MAX(MaPhieu) FROM PhieuMuon WHERE MaPhieu LIKE @p",
            Tham("@p", tienTo + "%")));
    }

    /// <summary>
    /// Lập phiếu mượn trong MỘT transaction: thêm phiếu, sinh mã bản sách, thêm chi tiết và trừ số bản có sẵn.
    /// Ném InvalidOperationException nếu tồn kho không đủ (BLL sẽ đổi thành thông báo thân thiện).
    /// </summary>
    public static List<ChiTietMuon> TaoPhieu(PhieuMuon pm, List<DongSachMuon> dong)
    {
        using var cn = TaoKetNoi();
        cn.Open();
        using var tx = cn.BeginTransaction();
        try
        {
            // 1. Thêm phiếu
            ExecuteNonQuery(cn, tx,
                @"INSERT INTO PhieuMuon (MaPhieu, MaDocGia, MaTaiKhoan, NgayMuon, HenTra, TrangThai, GhiChu)
                  VALUES (@ma, @dg, @tk, @nm, @ht, @tt, @gc)",
                Tham("@ma", pm.MaPhieu), Tham("@dg", pm.MaDocGia), Tham("@tk", pm.MaTaiKhoan),
                Tham("@nm", pm.NgayMuon), Tham("@ht", pm.HenTra), Tham("@tt", pm.TrangThai), Tham("@gc", pm.GhiChu));

            var ketQua = new List<ChiTietMuon>();
            foreach (var d in dong)
            {
                // 2. Khóa dòng sách và kiểm tra tồn kho lần cuối (tránh hai thủ thư mượn cùng lúc)
                var dtSach = ExecuteQuery(cn, tx,
                    "SELECT SoBanCoSan, TongSoBan FROM Sach WITH (UPDLOCK, ROWLOCK) WHERE MaSach = @ma", Tham("@ma", d.MaSach));
                if (dtSach.Rows.Count == 0)
                    throw new InvalidOperationException($"Không tìm thấy sách {d.MaSach}.");
                int conLai = dtSach.Rows[0].So("SoBanCoSan");
                int tongBan = dtSach.Rows[0].So("TongSoBan");
                if (conLai < d.SoLuong)
                    throw new InvalidOperationException($"Sách {d.MaSach} chỉ còn {conLai} bản, không đủ {d.SoLuong} bản.");

                // 3. Tìm các số bản đang bị chiếm (đang mượn / hư hỏng / mất) để chọn số nhỏ nhất còn trống
                var dtBan = ExecuteQuery(cn, tx,
                    "SELECT MaBanSach FROM ChiTietMuon WHERE MaSach = @ma AND TinhTrangKhiTra <> N'Tốt'", Tham("@ma", d.MaSach));
                var daDung = new HashSet<int>();
                foreach (DataRow r in dtBan.Rows)
                {
                    string ma = r.Chuoi("MaBanSach") ?? "";
                    int viTri = ma.LastIndexOf('-');
                    if (viTri >= 0 && int.TryParse(ma.Substring(viTri + 1), out int so)) daDung.Add(so);
                }

                // 4. Thêm từng bản sách vào chi tiết
                int soHienTai = 1;
                for (int i = 0; i < d.SoLuong; i++)
                {
                    while (daDung.Contains(soHienTai)) soHienTai++;
                    if (soHienTai > tongBan)
                        throw new InvalidOperationException($"Không tìm được mã bản trống cho sách {d.MaSach}.");
                    string maBan = $"{d.MaSach}-{soHienTai:000}";
                    daDung.Add(soHienTai);

                    ExecuteNonQuery(cn, tx,
                        @"INSERT INTO ChiTietMuon (MaPhieu, MaSach, MaBanSach, TinhTrangKhiTra, GhiChu, NgayTra)
                          VALUES (@p, @s, @b, N'Chưa ghi nhận', N'', NULL)",
                        Tham("@p", pm.MaPhieu), Tham("@s", d.MaSach), Tham("@b", maBan));
                    ketQua.Add(new ChiTietMuon { MaPhieu = pm.MaPhieu, MaSach = d.MaSach, MaBanSach = maBan, TinhTrangKhiTra = HangSo.ChuaGhiNhan, GhiChu = "" });
                }

                // 5. Trừ số bản có sẵn
                ExecuteNonQuery(cn, tx, "UPDATE Sach SET SoBanCoSan = SoBanCoSan - @sl WHERE MaSach = @ma",
                    Tham("@sl", d.SoLuong), Tham("@ma", d.MaSach));
            }
            tx.Commit();
            return ketQua;
        }
        catch
        {
            tx.Rollback();   // lỗi ở bước nào cũng hoàn tác toàn bộ
            throw;
        }
    }

    /// <summary>
    /// Ghi nhận trả sách trong MỘT transaction: cập nhật từng bản, cộng lại tồn kho cho bản "Tốt",
    /// rồi cập nhật trạng thái phiếu. ghiChuThem đã được BLL ghép sẵn (có thể rỗng).
    /// </summary>
    public static KetQuaTraSach TraSach(string maPhieu, List<ChiTietTraSach> ds, DateTime ngayTra, string ghiChuThem, DateTime hom)
    {
        using var cn = TaoKetNoi();
        cn.Open();
        using var tx = cn.BeginTransaction();
        try
        {
            var kq = new KetQuaTraSach();
            foreach (var b in ds)
            {
                // Chỉ cập nhật bản đang mượn (chặn trả hai lần)
                int n = ExecuteNonQuery(cn, tx,
                    @"UPDATE ChiTietMuon SET TinhTrangKhiTra = @tt, GhiChu = @gc, NgayTra = @nt
                      WHERE MaPhieu = @p AND MaSach = @s AND MaBanSach = @b AND TinhTrangKhiTra = N'Chưa ghi nhận'",
                    Tham("@tt", b.TinhTrang), Tham("@gc", b.GhiChu ?? ""), Tham("@nt", ngayTra.Date),
                    Tham("@p", maPhieu), Tham("@s", b.MaSach), Tham("@b", b.MaBanSach));
                if (n != 1)
                    throw new InvalidOperationException($"Bản sách {b.MaBanSach} không còn ở trạng thái đang mượn.");

                if (b.TinhTrang == HangSo.TinhTrangTot)
                {
                    // Chỉ bản Tốt mới quay lại kho
                    ExecuteNonQuery(cn, tx, "UPDATE Sach SET SoBanCoSan = SoBanCoSan + 1 WHERE MaSach = @s", Tham("@s", b.MaSach));
                    kq.SoTot++;
                }
                else if (b.TinhTrang == HangSo.TinhTrangHuHong) kq.SoHuHong++;
                else kq.SoMat++;
            }

            // Tính lại trạng thái phiếu
            var dt = ExecuteQuery(cn, tx,
                @"SELECT SUM(CASE WHEN TinhTrangKhiTra = N'Chưa ghi nhận' THEN 1 ELSE 0 END) AS ConMuon,
                         SUM(CASE WHEN TinhTrangKhiTra <> N'Chưa ghi nhận' THEN 1 ELSE 0 END) AS DaTra
                  FROM ChiTietMuon WHERE MaPhieu = @p", Tham("@p", maPhieu));
            int conMuon = dt.Rows[0].So("ConMuon");
            int daTra = dt.Rows[0].So("DaTra");
            DateTime henTra = Convert.ToDateTime(ExecuteScalar(cn, tx, "SELECT HenTra FROM PhieuMuon WHERE MaPhieu = @p", Tham("@p", maPhieu)));

            string trangThai;
            if (conMuon == 0) trangThai = HangSo.PhieuDaTra;
            else if (henTra.Date < hom.Date) trangThai = HangSo.PhieuQuaHan;
            else if (daTra > 0) trangThai = HangSo.PhieuTraMotPhan;
            else trangThai = HangSo.PhieuDangMuon;

            ExecuteNonQuery(cn, tx,
                "UPDATE PhieuMuon SET TrangThai = @tt, GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), @gc), 255) WHERE MaPhieu = @p",
                Tham("@tt", trangThai), Tham("@gc", ghiChuThem ?? ""), Tham("@p", maPhieu));

            tx.Commit();
            kq.SoConMuon = conMuon;
            kq.TrangThaiPhieu = trangThai;
            return kq;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}

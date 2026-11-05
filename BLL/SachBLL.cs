using DAL;
using DTO;
using DTO.Entities;
using DTO.Models;

namespace BLL;

/// <summary>Nghiệp vụ quản lý sách: tìm kiếm, thêm, sửa, kiểm tra ảnh bìa.</summary>
public static class SachBLL
{
    public static KetQuaPhanTrang<SachHienThi> TimKiem(SachBoLoc boLoc, int trang, int kichThuoc = HangSo.KichThuocTrangMacDinh)
    {
        PhienDangNhap.YeuCauDangNhap();
        return SachDAL.TimKiem(boLoc, Math.Max(1, trang), kichThuoc);
    }

    public static SachHienThi LayTheoMa(string maSach) => SachDAL.LayTheoMa(maSach);
    public static ThongKeSach ThongKe() => ThongKeDAL.LaySach();

    /// <summary>Kiểm tra ảnh bìa người dùng chọn (dung lượng tối đa 2 MB). Định dạng ảnh do View kiểm tra khi nạp.</summary>
    public static void KiemTraAnhBia(byte[] duLieu)
    {
        if (duLieu == null || duLieu.Length == 0) throw new NghiepVuException("Tệp ảnh bìa rỗng.");
        if (duLieu.Length > HangSo.DungLuongAnhBiaToiDa)
            throw new NghiepVuException("Ảnh bìa tối đa 2 MB. Vui lòng chọn ảnh nhỏ hơn.");
    }

    /// <summary>Thêm đầu sách mới (mã tự sinh Sxxx). Trả về mã vừa tạo.</summary>
    public static string Them(Sach s)
    {
        PhienDangNhap.YeuCauDangNhap();
        KiemTraChung(s, soBanKhongKhaDung: 0);
        s.MaSach = SinhMa.TiepTheo(SachDAL.LayMaCuoi(), "S", 3);
        s.TinhTrangLuuHanh = string.IsNullOrWhiteSpace(s.TinhTrangLuuHanh) ? HangSo.LuuHanhChoMuon : s.TinhTrangLuuHanh;
        SachDAL.Them(s);
        return s.MaSach;
    }

    /// <summary>
    /// Sửa đầu sách. Tổng số bản không được thấp hơn số bản đang được mượn
    /// (thông tin danh mục không làm thay đổi số bản đang mượn).
    /// </summary>
    public static void Sua(Sach s, bool capNhatAnhBia)
    {
        PhienDangNhap.YeuCauDangNhap();
        var cu = SachDAL.LayTheoMa(s.MaSach);
        if (cu == null) throw new NghiepVuException("Không tìm thấy đầu sách cần sửa.");
        KiemTraChung(s, cu.SoBanDangMuon);
        s.TinhTrangLuuHanh = string.IsNullOrWhiteSpace(s.TinhTrangLuuHanh) ? cu.TinhTrangLuuHanh : s.TinhTrangLuuHanh;
        SachDAL.Sua(s, capNhatAnhBia);
    }

    // Kiểm tra dữ liệu nhập chung cho thêm/sửa
    private static void KiemTraChung(Sach s, int soBanKhongKhaDung)
    {
        s.TenSach = (s.TenSach ?? "").Trim();
        s.ViTriKe = (s.ViTriKe ?? "").Trim();
        s.NhaXuatBan = (s.NhaXuatBan ?? "").Trim();
        s.NgonNgu = (s.NgonNgu ?? "").Trim();

        if (s.TenSach.Length == 0) throw new NghiepVuException("Vui lòng nhập tên sách.");
        if (s.TenSach.Length > 200) throw new NghiepVuException("Tên sách tối đa 200 ký tự.");
        if (string.IsNullOrWhiteSpace(s.MaTacGia)) throw new NghiepVuException("Vui lòng chọn tác giả.");
        if (TacGiaDAL.LayTheoMa(s.MaTacGia) == null) throw new NghiepVuException("Tác giả không tồn tại.");
        if (string.IsNullOrWhiteSpace(s.MaTheLoai)) throw new NghiepVuException("Vui lòng chọn thể loại.");
        if (TheLoaiDAL.LayTheoMa(s.MaTheLoai) == null) throw new NghiepVuException("Thể loại không tồn tại.");
        if (s.NamXuatBan != 0 && (s.NamXuatBan < 1000 || s.NamXuatBan > DateTime.Today.Year + 1))
            throw new NghiepVuException("Năm xuất bản không hợp lệ.");
        if (s.TongSoBan < 1) throw new NghiepVuException("Tổng số bản phải từ 1 trở lên.");
        if (s.TongSoBan < soBanKhongKhaDung)
            throw new NghiepVuException($"Tổng số bản không được nhỏ hơn số bản đang được mượn ({soBanKhongKhaDung}).");
        if (s.ViTriKe.Length > 50) throw new NghiepVuException("Vị trí kệ tối đa 50 ký tự.");
        if (s.NhaXuatBan.Length > 100) throw new NghiepVuException("Nhà xuất bản tối đa 100 ký tự.");
        if (s.AnhBia != null) KiemTraAnhBia(s.AnhBia);
    }

    /// <summary>
    /// Nhập danh mục sách từ các dòng đã đọc từ CSV. Cột: Tên sách; Tác giả; Thể loại; Năm xuất bản; Tổng số bản;
    /// Vị trí kệ; Nhà xuất bản; Ngôn ngữ (hai cột cuối có thể bỏ trống). Tác giả và thể loại phải đã có trong danh mục (khớp theo tên).
    /// Dòng lỗi được bỏ qua và ghi lại, các dòng đúng vẫn được thêm. soDongDau là số dòng của phần tử đầu trong file (để báo lỗi đúng dòng).
    /// </summary>
    public static KetQuaNhapDanhMuc NhapDanhMuc(List<string[]> dong, int soDongDau)
    {
        PhienDangNhap.YeuCauDangNhap();
        var kq = new KetQuaNhapDanhMuc();
        if (dong == null || dong.Count == 0) throw new NghiepVuException("Tệp không có dữ liệu để nhập.");

        // Tra cứu tác giả / thể loại theo tên (không phân biệt hoa thường)
        var tacGia = TacGiaDAL.LayTatCa().GroupBy(t => t.TenTacGia.Trim().ToLowerInvariant()).ToDictionary(g => g.Key, g => g.First().MaTacGia);
        var theLoai = TheLoaiDAL.LayTatCa().GroupBy(t => t.TenTheLoai.Trim().ToLowerInvariant()).ToDictionary(g => g.Key, g => g.First().MaTheLoai);

        for (int i = 0; i < dong.Count; i++)
        {
            var c = dong[i];
            int soDong = soDongDau + i;
            if (c == null || c.All(x => string.IsNullOrWhiteSpace(x))) continue;   // bỏ qua dòng trống
            string Lay(int k) => k < c.Length ? (c[k] ?? "").Trim() : "";
            try
            {
                if (c.Length < 5) throw new NghiepVuException("thiếu cột (cần tối thiểu 5 cột).");
                if (!tacGia.TryGetValue(Lay(1).ToLowerInvariant(), out string maTg))
                    throw new NghiepVuException($"tác giả \"{Lay(1)}\" chưa có trong danh mục.");
                if (!theLoai.TryGetValue(Lay(2).ToLowerInvariant(), out string maTl))
                    throw new NghiepVuException($"thể loại \"{Lay(2)}\" chưa có trong danh mục.");
                int nam = 0;
                if (Lay(3).Length > 0 && !int.TryParse(Lay(3), out nam)) throw new NghiepVuException("năm xuất bản không phải số.");
                if (!int.TryParse(Lay(4), out int tong)) throw new NghiepVuException("tổng số bản không phải số.");

                Them(new Sach
                {
                    TenSach = Lay(0), MaTacGia = maTg, MaTheLoai = maTl, NamXuatBan = nam, TongSoBan = tong,
                    ViTriKe = Lay(5), NhaXuatBan = Lay(6), NgonNgu = Lay(7)
                });
                kq.SoThanhCong++;
            }
            catch (NghiepVuException ex)
            {
                kq.Loi.Add($"Dòng {soDong}: {ex.Message}");
            }
        }
        return kq;
    }
}

/// <summary>Kết quả nhập danh mục: số dòng thành công và danh sách lỗi theo từng dòng.</summary>
public class KetQuaNhapDanhMuc
{
    public int SoThanhCong { get; set; }
    public List<string> Loi { get; } = new();
}

namespace DTO;

/// <summary>Các hằng số dùng chung toàn hệ thống (chuỗi trạng thái, hạn mức, vai trò...). Gom một nơi để không gõ sai chuỗi.</summary>
public static class HangSo
{
    // Vai trò (bảng VaiTro)
    public const int VaiTroQuanTri = 1;
    public const int VaiTroThuThu = 2;

    // Tình trạng từng bản sách (cột ChiTietMuon.TinhTrangKhiTra)
    public const string ChuaGhiNhan = "Chưa ghi nhận";   // bản sách còn đang được mượn
    public const string TinhTrangTot = "Tốt";
    public const string TinhTrangHuHong = "Hư hỏng";
    public const string TinhTrangMat = "Mất";

    // Trạng thái phiếu mượn
    public const string PhieuDangMuon = "Đang mượn";
    public const string PhieuTraMotPhan = "Trả một phần";
    public const string PhieuDaTra = "Đã trả";
    public const string PhieuQuaHan = "Quá hạn";
    public const string TatCa = "Tất cả";

    // Trạng thái hiển thị của thẻ độc giả (tính theo hạn thẻ và phiếu quá hạn)
    public const string TheHoatDong = "Hoạt động";
    public const string TheCoQuaHan = "Có quá hạn";
    public const string TheSapHetHan = "Sắp hết hạn";
    public const string TheHetHan = "Hết hạn";

    // Nhóm độc giả
    public const string NhomSinhVien = "Sinh viên";
    public const string NhomGiangVien = "Giảng viên";
    public const string NhomCanBo = "Cán bộ";
    public static readonly string[] CacNhomDocGia = { NhomSinhVien, NhomGiangVien, NhomCanBo };

    // Tình trạng sách trên kệ / lưu hành
    public const string SachCoSan = "Có sẵn";
    public const string SachHetSach = "Hết sách";
    public const string LuuHanhChoMuon = "Được phép cho mượn";
    public const string LuuHanhDocTaiCho = "Chỉ đọc tại chỗ";
    public static readonly string[] CacTinhTrangLuuHanh = { LuuHanhChoMuon, LuuHanhDocTaiCho };

    // Quy tắc nghiệp vụ
    public const int HanMucSachMoiDocGia = 5;     // tối đa 5 cuốn / độc giả
    public const int SoNgayMuonMacDinh = 14;      // mượn 14 ngày
    public const int SoNgayBaoSapHetHan = 7;      // cảnh báo thẻ sắp hết hạn trong 7 ngày
    public const int KichThuocTrangMacDinh = 6;   // số dòng mỗi trang của lưới
    public const int DungLuongAnhBiaToiDa = 2 * 1024 * 1024; // 2 MB
}

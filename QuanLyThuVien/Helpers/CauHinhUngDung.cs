namespace QuanLyThuVien;

/// <summary>
/// Cấu hình dùng chung: font và ĐƯỜNG DẪN TƯƠNG ĐỐI của logo / ảnh nền.
/// Chỉ cần chép file ảnh vào thư mục Resources của project giao diện (tên đúng như bên dưới) là hiển thị,
/// không cần sửa code. Không dùng đường dẫn tuyệt đối nên cả nhóm dùng chung được.
/// </summary>
public static class CauHinhUngDung
{
    public const string TenFont = "Segoe UI";
    public const float CoChu = 9.5F;   // đơn vị Point

    // >>> CHỪA SẴN ĐƯỜNG DẪN: đặt logo HCMUE tại QuanLyThuVien\Resources\logo_hcmue.png <<<
    public const string DuongDanLogo = @"Resources\logo_hcmue.png";
    // Ảnh nền bên trái màn hình đăng nhập (thư viện): QuanLyThuVien\Resources\anh_nen_dang_nhap.jpg
    public const string DuongDanAnhNenDangNhap = @"Resources\anh_thu_vien.jpg";

    /// <summary>Nạp logo; trả về null nếu chưa có file (View sẽ hiện chữ "HCMUE" thay thế).</summary>
    public static Image TaiLogo() => TaiAnh(DuongDanLogo);

    /// <summary>Nạp ảnh nền đăng nhập; null nếu chưa có file.</summary>
    public static Image TaiAnhNenDangNhap() => TaiAnh(DuongDanAnhNenDangNhap);

    // Đọc ảnh vào bộ nhớ (không khóa file trên đĩa)
    private static Image TaiAnh(string duongDanTuongDoi)
    {
        try
        {
            string duongDan = Path.Combine(AppContext.BaseDirectory, duongDanTuongDoi);
            if (!File.Exists(duongDan)) return null;
            using var fs = new FileStream(duongDan, FileMode.Open, FileAccess.Read);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            ms.Position = 0;
            return Image.FromStream(ms);
        }
        catch
        {
            return null;   // file hỏng thì coi như chưa có ảnh
        }
    }
}

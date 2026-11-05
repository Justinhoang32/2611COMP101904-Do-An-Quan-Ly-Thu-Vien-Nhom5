namespace QuanLyThuVien;

/// <summary>
/// Lưu tên tài khoản cho chức năng "Ghi nhớ đăng nhập" vào thư mục dữ liệu của người dùng Windows
/// (%LocalAppData%\QuanLyThuVien). Chỉ lưu tên tài khoản, KHÔNG lưu mật khẩu. Không phụ thuộc đường dẫn máy cá nhân.
/// </summary>
public static class GhiNhoDangNhap
{
    private static string DuongDanFile => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuanLyThuVien", "ghi_nho_dang_nhap.txt");

    /// <summary>Đọc tên tài khoản đã lưu; null nếu chưa có.</summary>
    public static string Doc()
    {
        try { return File.Exists(DuongDanFile) ? File.ReadAllText(DuongDanFile).Trim() : null; }
        catch { return null; }
    }

    public static void Luu(string tenDangNhap)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DuongDanFile)!);
            File.WriteAllText(DuongDanFile, tenDangNhap ?? "");
        }
        catch { /* không lưu được thì bỏ qua, không ảnh hưởng đăng nhập */ }
    }

    public static void Xoa()
    {
        try { if (File.Exists(DuongDanFile)) File.Delete(DuongDanFile); } catch { }
    }
}

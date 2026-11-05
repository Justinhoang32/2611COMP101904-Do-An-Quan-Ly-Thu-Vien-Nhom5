namespace QuanLyThuVien;

internal static class Program
{
    /// <summary>
    /// Điểm vào chương trình.
    /// Cấu hình DPI: SetHighDpiMode(PerMonitorV2) phải gọi TRƯỚC khi tạo bất kỳ cửa sổ nào và trước EnableVisualStyles.
    /// PerMonitorV2 giúp Windows báo DPI của từng màn hình cho ứng dụng; WinForms tự co giãn control khi
    /// đổi Scale hoặc kéo cửa sổ sang màn hình khác DPI (sự kiện DpiChanged).
    /// </summary>
    [STAThread]
    static void Main()
    {
        // 1. Chế độ DPI theo từng màn hình (bản V2)
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        // 2. Bật giao diện visual styles và dùng GDI+ (không dùng chế độ chữ tương thích cũ)
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        // 3. Font mặc định cho toàn ứng dụng (đơn vị Point, không dùng Pixel)
        Application.SetDefaultFont(new Font(CauHinhUngDung.TenFont, CauHinhUngDung.CoChu, FontStyle.Regular, GraphicsUnit.Point));

        // 4. Vòng lặp: Đăng nhập -> Màn hình chính -> (Đăng xuất thì quay lại Đăng nhập)
        bool tiepTuc = true;
        while (tiepTuc)
        {
            using (var dangNhap = new Forms.frmDangNhap())
            {
                if (dangNhap.ShowDialog() != DialogResult.OK) break;   // đóng form đăng nhập = thoát
            }
            using (var chinh = new Forms.frmMain())
            {
                tiepTuc = chinh.ShowDialog() == DialogResult.Retry;     // Retry = người dùng bấm Đăng xuất
            }
        }
    }
}

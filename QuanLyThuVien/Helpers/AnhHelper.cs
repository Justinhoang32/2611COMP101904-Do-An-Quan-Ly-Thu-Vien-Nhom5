using System.Drawing.Drawing2D;

namespace QuanLyThuVien;

/// <summary>Tiện ích ảnh bìa sách: đọc từ file, giải mã từ byte[], tạo ảnh thu nhỏ hoặc ảnh thay thế có chữ cái đầu.</summary>
public static class AnhHelper
{
    /// <summary>
    /// Đọc file ảnh do người dùng chọn thành byte[]. Kiểm tra file là ảnh hợp lệ (png, jpg, bmp, gif).
    /// Trả về null và gán thông báo lỗi nếu không đọc được.
    /// </summary>
    public static byte[] DocFileAnh(string duongDan, out string loi)
    {
        loi = null;
        try
        {
            var fi = new FileInfo(duongDan);
            if (!fi.Exists) { loi = "Không tìm thấy tệp ảnh đã chọn."; return null; }
            if (fi.Length > 20L * 1024 * 1024) { loi = "Tệp ảnh quá lớn. Vui lòng chọn ảnh nhỏ hơn 2 MB."; return null; }
            byte[] du = File.ReadAllBytes(duongDan);
            using var anh = DocAnh(du);   // thử giải mã để chắc chắn là ảnh
            if (anh == null) { loi = "Tệp đã chọn không phải ảnh hợp lệ."; return null; }
            return du;
        }
        catch (Exception)
        {
            loi = "Không thể đọc tệp ảnh. Vui lòng chọn tệp khác.";
            return null;
        }
    }

    /// <summary>Giải mã byte[] thành ảnh độc lập (không giữ stream). Trả về null nếu không phải ảnh.</summary>
    public static Image DocAnh(byte[] duLieu)
    {
        if (duLieu == null || duLieu.Length == 0) return null;
        try
        {
            using var ms = new MemoryStream(duLieu);
            using var goc = Image.FromStream(ms);
            return new Bitmap(goc);
        }
        catch { return null; }
    }

    /// <summary>
    /// Tạo ảnh có đúng kích thước yêu cầu (pixel thật): có dữ liệu thì Zoom giữ tỉ lệ,
    /// không có thì vẽ nền xanh với chữ cái đầu của tên sách.
    /// </summary>
    public static Image TaoAnhThu(byte[] duLieu, string tenSach, Size kichThuoc)
    {
        var bmp = new Bitmap(Math.Max(1, kichThuoc.Width), Math.Max(1, kichThuoc.Height));
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        var vung = new Rectangle(0, 0, bmp.Width, bmp.Height);

        using var goc = DocAnh(duLieu);
        if (goc != null)
        {
            g.Clear(UiTheme.NenHover);
            float tiLe = Math.Min((float)bmp.Width / goc.Width, (float)bmp.Height / goc.Height);
            int w = (int)(goc.Width * tiLe), h = (int)(goc.Height * tiLe);
            g.DrawImage(goc, (bmp.Width - w) / 2, (bmp.Height - h) / 2, w, h);
            return bmp;
        }

        // Không có ảnh: nền gradient + chữ cái đầu (tối đa 3 từ)
        using (var nen = new LinearGradientBrush(vung, UiTheme.XanhChinh, UiTheme.XanhDam, 60f))
            g.FillRectangle(nen, vung);
        string chu = string.Concat((tenSach ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(3).Select(t => char.ToUpper(t[0])));
        if (chu.Length == 0) chu = "S";
        // Đổi cỡ chữ từ pixel sang point để vẫn dùng đơn vị Point
        float coPoint = Math.Max(6f, bmp.Height / 4f * 72f / g.DpiY);
        using var font = new Font(CauHinhUngDung.TenFont, coPoint, FontStyle.Bold, GraphicsUnit.Point);
        TextRenderer.DrawText(g, chu, font, vung, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        return bmp;
    }
}

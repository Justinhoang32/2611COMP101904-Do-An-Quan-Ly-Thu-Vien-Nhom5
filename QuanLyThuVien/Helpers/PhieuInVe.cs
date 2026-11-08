using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using DTO;
using DTO.Models;

namespace QuanLyThuVien;

/// <summary>Thiết lập in phiếu mượn (lấy từ khung "Thiết lập bản in").</summary>
public class CauHinhIn
{
    public string MayIn { get; set; }
    public bool KhoA5 { get; set; }
    public bool NgangGiay { get; set; }
    public int SoBan { get; set; } = 1;
    public bool GomGhiChu { get; set; } = true;
}

/// <summary>
/// Vẽ phiếu mượn sách lên Graphics. Dùng chung cho khung xem trước (ucTrangPhieu) và máy in:
/// toàn bộ vẽ trong hệ tọa độ "trang logic" 794 px chiều rộng (A4 dọc) rồi co theo vùng đích, nên bản xem trước và bản in giống nhau
/// và không phụ thuộc DPI màn hình.
/// </summary>
public static class PhieuInVe
{
    private const float RongDoc = 794f, RongNgang = 1123f;

    /// <summary>Vẽ phiếu vào vùng trang (đơn vị bất kỳ của Graphics). logo có thể null.</summary>
    public static void Ve(Graphics g, RectangleF trang, PhieuMuonDayDu p, bool gomGhiChu, Image logo)
    {
        var state = g.Save();
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        float W = trang.Width >= trang.Height ? RongNgang : RongDoc;
        float H = W * trang.Height / trang.Width;
        g.TranslateTransform(trang.X, trang.Y);
        g.ScaleTransform(trang.Width / W, trang.Width / W);    // tỉ lệ đều theo chiều rộng

        int soDong = p.ChiTiet.Count;
        float canCao = 640f + soDong * 56f + (gomGhiChu ? 70f : 0f);
        float co = canCao > H ? H / canCao : 1f;               // trang ngang / nhiều dòng: thu nhỏ vừa trang
        if (co < 1f) g.ScaleTransform(co, co);
        float rong = W / co;

        const float le = 56f;
        float x0 = le, x1 = rong - le, giua = rong / 2f;
        var den = Brushes.Black;
        using var pen = new Pen(Color.Black, 1.2f);
        using var f = new FontHelperTimes();

        // ---- Đầu phiếu ----
        var vungLogo = new RectangleF(x0, 36, 120, 66);
        if (logo != null) g.DrawImage(logo, KhungTheoTiLe(logo.Size, vungLogo));
        else { using var pb = new Pen(Color.Gray) { DashStyle = DashStyle.Dash }; g.DrawRectangle(pb, vungLogo.X, vungLogo.Y, vungLogo.Width, vungLogo.Height); Chu(g, "LOGO", f.Thuong(10), Brushes.Gray, vungLogo, Giua); }
        Chu(g, "TRƯỜNG ĐẠI HỌC SƯ PHẠM", f.Dam(13), den, new RectangleF(x0 + 120, 36, x1 - x0 - 120, 24), Giua);
        Chu(g, "THÀNH PHỐ HỒ CHÍ MINH", f.Dam(13), den, new RectangleF(x0 + 120, 62, x1 - x0 - 120, 24), Giua);
        Chu(g, "THƯ VIỆN", f.Thuong(12), den, new RectangleF(x0 + 120, 88, x1 - x0 - 120, 22), Giua);
        g.DrawLine(pen, x0, 118, x1, 118);

        // ---- Tiêu đề ----
        var ph = p.Phieu; var dg = p.DocGia;
        Chu(g, "PHIẾU MƯỢN SÁCH", f.Dam(22), den, new RectangleF(x0, 132, x1 - x0, 34), Giua);
        Chu(g, "Số: " + ph.MaPhieu, f.Thuong(13), den, new RectangleF(x0, 170, x1 - x0, 22), Giua);
        var ngayLap = ph.NgayMuon ?? DateTime.Today;
        Chu(g, $"Ngày lập: {ngayLap:dd} tháng {ngayLap:MM} năm {ngayLap:yyyy}", f.Thuong(12), den, new RectangleF(x0, 194, x1 - x0, 22), Giua);

        // ---- Thông tin độc giả ----
        float y = 232;
        Chu(g, "THÔNG TIN ĐỘC GIẢ", f.Dam(13), den, new RectangleF(x0, y, 300, 22), Trai);
        y += 30;
        Chu(g, "Họ và tên: " + ph.HoTenDocGia, f.Thuong(12.5f), den, new RectangleF(x0, y, x1 - x0 - 160, 22), Trai);
        Chu(g, "Mã thẻ: " + ph.MaDocGia, f.Thuong(12.5f), den, new RectangleF(x1 - 200, y, 200, 22), Phai);
        y += 28;
        Chu(g, $"Nhóm độc giả: {ph.NhomDocGia}" + (string.IsNullOrWhiteSpace(dg?.Khoa) ? "" : " · Khoa: " + dg.Khoa), f.Thuong(12.5f), den, new RectangleF(x0, y, x1 - x0, 22), Trai);
        y += 28;
        Chu(g, $"Mã sinh viên: {(string.IsNullOrWhiteSpace(dg?.MaSinhVien) ? "—" : dg.MaSinhVien)} · Điện thoại: {(string.IsNullOrWhiteSpace(dg?.DienThoai) ? "—" : dg.DienThoai)}", f.Thuong(12.5f), den, new RectangleF(x0, y, x1 - x0, 22), Trai);
        y += 28;
        Chu(g, "Ngày mượn: " + ph.NgayMuon?.ToString("dd/MM/yyyy"), f.Thuong(12.5f), den, new RectangleF(x0, y, 300, 22), Trai);
        Chu(g, "Ngày hẹn trả: " + ph.HenTra?.ToString("dd/MM/yyyy"), f.Dam(12.5f), den, new RectangleF(x1 - 300, y, 300, 22), Phai);
        y += 38;

        // ---- Bảng sách ----
        float[] tl = { 0.07f, 0.12f, 0.43f, 0.16f, 0.07f, 0.15f };
        float tong = x1 - x0;
        float[] cx = new float[7]; cx[0] = x0;
        for (int i = 0; i < 6; i++) cx[i + 1] = cx[i] + tong * tl[i];
        string[] td = { "STT", "Mã sách", "Tên sách / Tác giả", "Mã bản", "SL", "Tình trạng" };
        float bangTren = y;
        using (var nen = new SolidBrush(Color.FromArgb(238, 238, 238))) g.FillRectangle(nen, x0, y, tong, 40);
        for (int i = 0; i < 6; i++) Chu(g, td[i], f.Dam(11.5f), den, new RectangleF(cx[i] + 8, y, cx[i + 1] - cx[i] - 8, 40), Trai | Doc);
        y += 40;
        g.DrawLine(pen, x0, y, x1, y);
        int stt = 1;
        foreach (var c in p.ChiTiet)
        {
            const float h = 56f;
            string tinhTrang = c.DangMuon ? "Tốt" : c.TinhTrangKhiTra;
            Chu(g, stt.ToString(), f.Thuong(11.5f), den, new RectangleF(cx[0] + 8, y, cx[1] - cx[0], h), Trai | Doc);
            Chu(g, c.MaSach, f.Thuong(11.5f), den, new RectangleF(cx[1] + 8, y, cx[2] - cx[1], h), Trai | Doc);
            Chu(g, c.TenSach, f.Dam(12), den, new RectangleF(cx[2] + 8, y + 6, cx[3] - cx[2] - 12, 24), Trai);
            Chu(g, c.TenTacGia, f.Thuong(10.5f), den, new RectangleF(cx[2] + 8, y + 30, cx[3] - cx[2] - 12, 20), Trai);
            Chu(g, c.MaBanSach, f.Thuong(11.5f), den, new RectangleF(cx[3] + 8, y, cx[4] - cx[3], h), Trai | Doc);
            Chu(g, "1", f.Thuong(11.5f), den, new RectangleF(cx[4] + 8, y, cx[5] - cx[4], h), Trai | Doc);
            Chu(g, tinhTrang, f.Thuong(11.5f), den, new RectangleF(cx[5] + 8, y, cx[6] - cx[5], h), Trai | Doc);
            y += h;
            g.DrawLine(pen, x0, y, x1, y);
            stt++;
        }
        // Dòng tổng cộng
        Chu(g, $"Tổng cộng: {p.ChiTiet.Select(c => c.MaSach).Distinct().Count()} đầu sách", f.Dam(12.5f), den, new RectangleF(x0 + 12, y, tong * 0.6f, 44), Trai | Doc);
        Chu(g, $"{p.ChiTiet.Count} cuốn", f.Dam(12.5f), den, new RectangleF(x1 - 200, y, 188, 44), Phai | Doc);
        y += 44;
        g.DrawRectangle(pen, x0, bangTren, tong, y - bangTren);

        // ---- Ghi chú, cam kết ----
        y += 14;
        if (gomGhiChu && !string.IsNullOrWhiteSpace(ph.GhiChu))
        {
            Chu(g, "Ghi chú: " + ph.GhiChu, f.Thuong(11.5f), den, new RectangleF(x0, y, tong, 40), Trai);
            y += 34;
        }
        Chu(g, "Độc giả đã nhận đủ sách, kiểm tra tình trạng và cam kết giữ gìn sách, trả đúng hạn. Khi sách hư hỏng hoặc mất, vui lòng liên hệ thư viện để được hướng dẫn xử lý.",
            f.Thuong(11.5f), den, new RectangleF(x0, y, tong, 46), Trai);
        y += 58;
        Chu(g, $"Thành phố Hồ Chí Minh, ngày {ngayLap:dd} tháng {ngayLap:MM} năm {ngayLap:yyyy}", f.Thuong(11.5f), den, new RectangleF(x0, y, tong, 22), Phai);
        y += 32;

        // ---- Chữ ký ----
        float cotKy = tong / 2f;
        Chu(g, "NGƯỜI MƯỢN", f.Dam(12.5f), den, new RectangleF(x0, y, cotKy, 22), Giua);
        Chu(g, "THỦ THƯ", f.Dam(12.5f), den, new RectangleF(x0 + cotKy, y, cotKy, 22), Giua);
        Chu(g, "(Ký và ghi rõ họ tên)", f.Thuong(11f), den, new RectangleF(x0, y + 22, cotKy, 20), Giua);
        Chu(g, "(Ký và ghi rõ họ tên)", f.Thuong(11f), den, new RectangleF(x0 + cotKy, y + 22, cotKy, 20), Giua);
        Chu(g, ph.HoTenDocGia, f.Thuong(12.5f), den, new RectangleF(x0, y + 100, cotKy, 22), Giua);
        Chu(g, ph.TenThuThu, f.Thuong(12.5f), den, new RectangleF(x0 + cotKy, y + 100, cotKy, 22), Giua);
        y += 136;

        // ---- Chân trang ----
        float yChan = Math.Max(y + 10, H / co - 54);
        using (var pm = new Pen(Color.Gray, 0.8f)) g.DrawLine(pm, x0, yChan, x1, yChan);
        Chu(g, "Thư viện HCMUE · Dữ liệu minh họa", f.Thuong(9.5f), den, new RectangleF(x0, yChan + 6, 400, 18), Trai);
        Chu(g, "Trang 1/1", f.Thuong(9.5f), den, new RectangleF(x1 - 200, yChan + 6, 200, 18), Phai);

        g.Restore(state);
    }

    // ----- Tiện ích vẽ chữ -----
    private const int Trai = 0, Giua = 1, Phai = 2, Doc = 4;   // Doc = căn giữa theo chiều dọc

    private static void Chu(Graphics g, string s, Font f, Brush b, RectangleF r, int kieu)
    {
        if (string.IsNullOrEmpty(s)) return;
        using var sf = new StringFormat(StringFormatFlags.NoWrap) { Trimming = StringTrimming.EllipsisCharacter };
        sf.Alignment = (kieu & 3) == Giua ? StringAlignment.Center : (kieu & 3) == Phai ? StringAlignment.Far : StringAlignment.Near;
        sf.LineAlignment = (kieu & Doc) != 0 ? StringAlignment.Center : StringAlignment.Near;
        if (s.Length > 60 && r.Height > 30) sf.FormatFlags &= ~StringFormatFlags.NoWrap;   // đoạn dài tự xuống dòng
        g.DrawString(s, f, b, r, sf);
    }

    private static RectangleF KhungTheoTiLe(Size anh, RectangleF vung)
    {
        float t = Math.Min(vung.Width / anh.Width, vung.Height / anh.Height);
        float w = anh.Width * t, h = anh.Height * t;
        return new RectangleF(vung.X, vung.Y + (vung.Height - h) / 2, w, h);
    }

    /// <summary>Bộ font Times New Roman (đơn vị World để co theo hệ tọa độ trang logic) được giải phóng cùng lúc.</summary>
    private sealed class FontHelperTimes : IDisposable
    {
        private readonly List<Font> _ds = new();
        public Font Thuong(float c) { var f = new Font("Times New Roman", c, FontStyle.Regular, GraphicsUnit.World); _ds.Add(f); return f; }
        public Font Dam(float c) { var f = new Font("Times New Roman", c, FontStyle.Bold, GraphicsUnit.World); _ds.Add(f); return f; }
        public void Dispose() { foreach (var f in _ds) f.Dispose(); }
    }

    // ----- In thật -----
    /// <summary>In (hoặc in ra tệp PDF khi tepPdf != null) phiếu theo thiết lập. Ném InvalidOperationException nếu máy in không hợp lệ.</summary>
    public static void In(PhieuMuonDayDu p, CauHinhIn cfg, string tepPdf = null)
    {
        Image logo = CauHinhUngDung.TaiLogo();
        using var pd = new PrintDocument { DocumentName = "Phiếu mượn " + p.Phieu.MaPhieu };
        if (tepPdf != null)
        {
            pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            pd.PrinterSettings.PrintToFile = true;
            pd.PrinterSettings.PrintFileName = tepPdf;
        }
        else if (!string.IsNullOrEmpty(cfg.MayIn)) pd.PrinterSettings.PrinterName = cfg.MayIn;
        if (!pd.PrinterSettings.IsValid) throw new InvalidOperationException("Không tìm thấy máy in. Vui lòng kiểm tra máy in đã cài đặt.");
        pd.DefaultPageSettings.Landscape = cfg.NgangGiay;
        var kho = pd.PrinterSettings.PaperSizes.Cast<PaperSize>().FirstOrDefault(k => k.Kind == (cfg.KhoA5 ? PaperKind.A5 : PaperKind.A4));
        if (kho != null) pd.DefaultPageSettings.PaperSize = kho;
        if (tepPdf == null) pd.PrinterSettings.Copies = (short)Math.Max(1, cfg.SoBan);
        pd.PrintPage += (s, e) =>
        {
            Ve(e.Graphics, new RectangleF(0, 0, e.PageBounds.Width, e.PageBounds.Height), p, cfg.GomGhiChu, logo);
            e.HasMorePages = false;
        };
        try { pd.Print(); } finally { logo?.Dispose(); }
    }
}

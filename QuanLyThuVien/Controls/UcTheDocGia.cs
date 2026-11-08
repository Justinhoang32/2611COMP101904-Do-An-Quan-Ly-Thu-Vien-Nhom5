using System.Drawing.Drawing2D;

namespace QuanLyThuVien.Controls;

/// <summary>
/// Xem trước thẻ độc giả (nền xanh, chữ trắng). Hàm Ve dùng chung cho cả màn hình lẫn khi in thẻ,
/// vẽ theo tỉ lệ của vùng nên đúng ở mọi DPI và mọi cỡ giấy.
/// </summary>
public class UcTheDocGia : Control
{
    private string _hoTen = "", _maThe = "", _nhom = "", _han = "";

    public UcTheDocGia()
    {
        AutoSize = true;   // để TableLayoutPanel dùng GetPreferredSize bên dưới (nếu không, control rộng/cao 0)
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Margin = new Padding(0, 0, 0, 12);
    }

    /// <summary>Đặt nội dung thẻ rồi vẽ lại.</summary>
    public void DatNoiDung(string hoTen, string maThe, string nhom, DateTime? hanThe)
    {
        _hoTen = hoTen ?? ""; _maThe = maThe ?? ""; _nhom = nhom ?? "";
        _han = hanThe.HasValue ? "Hạn: " + hanThe.Value.ToString("dd/MM/yyyy") : "";
        Invalidate();
    }

    public override Size GetPreferredSize(Size proposedSize) => new Size(DpiHelper.Px(this, 280), DpiHelper.Px(this, 160));

    protected override void OnPaint(PaintEventArgs e) => Ve(e.Graphics, new RectangleF(0, 0, Width - 1, Height - 1), _hoTen, _maThe, _nhom, _han);

    /// <summary>Vẽ thẻ vào vùng r. Cỡ chữ tính theo chiều cao vùng (đơn vị World) để thẻ in và thẻ trên màn hình giống nhau.</summary>
    public static void Ve(Graphics g, RectangleF r, string hoTen, string maThe, string nhom, string han)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        float h = r.Height;
        var vung = Rectangle.Round(r);
        using (var duong = UiTheme.HinhChuNhatBoGoc(vung, (int)(h * 0.07f)))
        using (var nen = new LinearGradientBrush(vung, UiTheme.XanhDam, UiTheme.XanhChinh, 30f))
            g.FillPath(nen, duong);

        float le = h * 0.09f;
        using var fNho = new Font(CauHinhUngDung.TenFont, h * 0.058f, FontStyle.Bold, GraphicsUnit.World);
        using var fTen = new Font(CauHinhUngDung.TenFont, h * 0.105f, FontStyle.Bold, GraphicsUnit.World);
        using var fVua = new Font(CauHinhUngDung.TenFont, h * 0.07f, FontStyle.Regular, GraphicsUnit.World);
        using var trang = new SolidBrush(Color.White);
        using var nhat = new SolidBrush(Color.FromArgb(200, 215, 232));
        var dinhDang = new StringFormat(StringFormatFlags.NoWrap) { Trimming = StringTrimming.EllipsisCharacter };

        float rong = r.Width - le * 2;
        g.DrawString("THƯ VIỆN HCMUE", fNho, nhat, new RectangleF(r.Left + le, r.Top + h * 0.08f, rong, h * 0.1f), dinhDang);
        var phai = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Far };
        g.DrawString("THẺ ĐỘC GIẢ", fNho, nhat, new RectangleF(r.Left + le, r.Top + h * 0.08f, rong, h * 0.1f), phai);
        g.DrawString((hoTen ?? "").ToUpperInvariant(), fTen, trang, new RectangleF(r.Left + le, r.Top + h * 0.38f, rong, h * 0.16f), dinhDang);
        g.DrawString(maThe + (string.IsNullOrEmpty(nhom) ? "" : "   " + nhom), fVua, nhat, new RectangleF(r.Left + le, r.Top + h * 0.58f, rong, h * 0.11f), dinhDang);
        g.DrawString(han, fVua, trang, new RectangleF(r.Left + le, r.Bottom - h * 0.2f, rong, h * 0.11f), dinhDang);
    }
}

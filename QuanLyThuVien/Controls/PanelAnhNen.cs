using System.Drawing.Drawing2D;

namespace QuanLyThuVien.Controls;

/// <summary>
/// Panel có ảnh nền phủ kín (kiểu "cover": giữ tỉ lệ, cắt phần thừa) và lớp phủ xanh đậm để chữ dễ đọc.
/// Chưa có ảnh thì dùng nền gradient xanh.
/// </summary>
public class PanelAnhNen : Panel
{
    public Image AnhNen { get; set; }

    public PanelAnhNen()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Width <= 0 || Height <= 0) return;
        var g = e.Graphics;
        if (AnhNen != null)
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            float tiLe = Math.Max((float)Width / AnhNen.Width, (float)Height / AnhNen.Height);
            int w = (int)(AnhNen.Width * tiLe), h = (int)(AnhNen.Height * tiLe);
            g.DrawImage(AnhNen, (Width - w) / 2, (Height - h) / 2, w, h);
        }
        else
        {
            using var nen = new LinearGradientBrush(ClientRectangle, UiTheme.XanhDam, UiTheme.XanhChinh, 45f);
            g.FillRectangle(nen, ClientRectangle);
        }
        // Lớp phủ tối dần xuống dưới
        using var phu = new LinearGradientBrush(ClientRectangle, Color.FromArgb(70, 10, 32, 58), Color.FromArgb(215, 10, 32, 58), 90f);
        g.FillRectangle(phu, ClientRectangle);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { AnhNen?.Dispose(); AnhNen = null; }
        base.Dispose(disposing);
    }
}

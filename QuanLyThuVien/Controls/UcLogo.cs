using System.Drawing.Drawing2D;

namespace QuanLyThuVien.Controls;

/// <summary>
/// Hiển thị logo HCMUE từ CauHinhUngDung.DuongDanLogo (SizeMode kiểu Zoom, giữ tỉ lệ).
/// Nếu chưa có file logo thì vẽ khung xanh có chữ "HCMUE" thay thế.
/// </summary>
public class UcLogo : Control
{
    private Image _anh;
    private int _kichThuocLogic = 40;

    public UcLogo()
    {
        AutoSize = true;   // để TableLayoutPanel dùng GetPreferredSize bên dưới (nếu không, control rộng/cao 0)
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Margin = new Padding(0);
        _anh = CauHinhUngDung.TaiLogo();
    }

    /// <summary>Kích thước vuông ở 96 DPI (mặc định 40).</summary>
    public int KichThuocLogic
    {
        get => _kichThuocLogic;
        set { _kichThuocLogic = value; PerformLayout(); Invalidate(); }
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        int px = DpiHelper.Px(this, _kichThuocLogic);   // tính lúc chạy nên nhân theo DPI thật
        return new Size(px, px);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        var vung = new Rectangle(0, 0, Width - 1, Height - 1);
        if (_anh != null)
        {
            // Zoom: giữ tỉ lệ, nằm gọn trong control
            float tiLe = Math.Min((float)Width / _anh.Width, (float)Height / _anh.Height);
            int w = (int)(_anh.Width * tiLe), h = (int)(_anh.Height * tiLe);
            g.DrawImage(_anh, (Width - w) / 2, (Height - h) / 2, w, h);
            return;
        }
        // Chưa có logo: khung xanh + chữ HCMUE
        using var duong = UiTheme.HinhChuNhatBoGoc(vung, DpiHelper.Px(this, 6));
        using var nen = new SolidBrush(UiTheme.XanhChinh);
        g.FillPath(nen, duong);
        using var font = DpiHelper.TaoFont(Math.Max(6.5f, _kichThuocLogic / 7f), FontStyle.Bold);
        TextRenderer.DrawText(g, "HCMUE", font, vung, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _anh?.Dispose(); _anh = null; }
        base.Dispose(disposing);
    }
}

using System.Drawing.Drawing2D;

namespace QuanLyThuVien.Controls;

/// <summary>Ảnh đại diện hình tròn hiển thị chữ cái viết tắt của họ tên (ví dụ "LH").</summary>
public class AvatarKyTu : Control
{
    private string _kyTu = "";

    public AvatarKyTu()
    {
        AutoSize = true;   // để TableLayoutPanel dùng GetPreferredSize bên dưới (nếu không, control rộng/cao 0)
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Margin = new Padding(0);
    }

    public string KyTu { get => _kyTu; set { _kyTu = value ?? ""; Invalidate(); } }

    public override Size GetPreferredSize(Size proposedSize)
    {
        int px = DpiHelper.Px(this, 40);
        return new Size(px, px);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var vung = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var nen = new SolidBrush(UiTheme.XanhNhat)) g.FillEllipse(nen, vung);
        using var font = DpiHelper.TaoFont(10f, FontStyle.Bold);
        TextRenderer.DrawText(g, _kyTu, font, vung, UiTheme.XanhChinh,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }
}

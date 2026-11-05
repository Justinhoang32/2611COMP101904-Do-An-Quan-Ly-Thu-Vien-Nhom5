using System.Drawing.Drawing2D;

namespace QuanLyThuVien.Controls;

/// <summary>
/// Thanh 3 bước của màn hình Lập phiếu mượn: 01 Thông tin mượn → 02 Chọn sách → 03 Kiểm tra &amp; tạo phiếu.
/// Chỉ để hiển thị tiến độ (SoBuocHoanThanh = 0..3); bước kế tiếp được tô xanh.
/// </summary>
public class ucStepper : Control
{
    private static readonly string[] Nhan = { "Thông tin mượn", "Chọn sách", "Kiểm tra & tạo phiếu" };
    private int _hoanThanh;

    public ucStepper()
    {
        AutoSize = true;   // để TableLayoutPanel dùng GetPreferredSize bên dưới (nếu không, control rộng/cao 0)
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Margin = new Padding(0, 0, 0, 16);
    }

    /// <summary>Số bước đã hoàn thành (0..3).</summary>
    public int SoBuocHoanThanh { get => _hoanThanh; set { _hoanThanh = Math.Max(0, Math.Min(3, value)); Invalidate(); } }

    public override Size GetPreferredSize(Size proposedSize) => new Size(DpiHelper.Px(this, 520), DpiHelper.Px(this, 56));

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using (var pen = new Pen(UiTheme.VienNhat)) g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

        int dk = DpiHelper.Px(this, 28);          // đường kính vòng tròn
        int vungRong = Width / 3;
        using var fSo = DpiHelper.TaoFont(8.5f, FontStyle.Bold);
        for (int i = 0; i < 3; i++)
        {
            bool xong = i < _hoanThanh, hienTai = i == _hoanThanh;
            var mau = xong ? UiTheme.XanhLa : hienTai ? UiTheme.XanhChinh : UiTheme.ChuPhu;
            int x = i * vungRong + DpiHelper.Px(this, 16);
            var vong = new Rectangle(x, (Height - dk) / 2, dk, dk);
            if (xong || hienTai) { using var nen = new SolidBrush(mau); g.FillEllipse(nen, vong); }
            else { using var pen = new Pen(UiTheme.VienNhat, 2f); g.DrawEllipse(pen, vong); }
            TextRenderer.DrawText(g, xong ? "✓" : (i + 1).ToString("00"), fSo, vong, xong || hienTai ? Color.White : UiTheme.ChuPhu,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            using var fChu = DpiHelper.TaoFont(9.5f, hienTai || xong ? FontStyle.Bold : FontStyle.Regular);
            var vungChu = new Rectangle(vong.Right + DpiHelper.Px(this, 10), 0, vungRong - dk - DpiHelper.Px(this, 36), Height);
            TextRenderer.DrawText(g, Nhan[i], fChu, vungChu, xong || hienTai ? UiTheme.ChuChinh : UiTheme.ChuPhu,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }
}

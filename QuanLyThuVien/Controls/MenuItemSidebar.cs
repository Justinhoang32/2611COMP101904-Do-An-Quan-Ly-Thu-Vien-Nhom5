namespace QuanLyThuVien.Controls;

/// <summary>
/// Một mục trong thanh menu bên trái: tự vẽ chữ, vạch xanh khi được chọn, huy hiệu số (ví dụ số phiếu quá hạn).
/// Mọi kích thước lúc vẽ đều nhân theo DPI thật của control (DpiHelper.Px).
/// </summary>
public class MenuItemSidebar : Control
{
    private bool _dangChon;
    private bool _dangRe;
    private string _huyHieu;

    public MenuItemSidebar()
    {
        AutoSize = true;   // để TableLayoutPanel dùng GetPreferredSize bên dưới (nếu không, control rộng/cao 0)
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        TabStop = true;
        Cursor = Cursors.Hand;
        Margin = new Padding(0);
    }

    /// <summary>Mục đang được chọn (tô nền xanh nhạt, chữ đậm).</summary>
    public bool DangChon { get => _dangChon; set { _dangChon = value; Invalidate(); } }

    /// <summary>Số hiển thị trong huy hiệu đỏ bên phải; null hoặc rỗng thì ẩn.</summary>
    public string HuyHieu { get => _huyHieu; set { _huyHieu = value; Invalidate(); } }

    public override Size GetPreferredSize(Size proposedSize) =>
        new Size(DpiHelper.Px(this, 200), DpiHelper.Px(this, 44));

    protected override void OnMouseEnter(EventArgs e) { _dangRe = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _dangRe = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    // Hỗ trợ bàn phím: Enter / Space tương đương bấm chuột
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) { OnClick(EventArgs.Empty); e.Handled = true; }
        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(_dangChon ? UiTheme.XanhNhat : (_dangRe ? UiTheme.NenHover : UiTheme.NenTrang));

        // Vạch xanh bên trái khi được chọn
        if (_dangChon)
        {
            using var vach = new SolidBrush(UiTheme.XanhChinh);
            g.FillRectangle(vach, 0, 0, DpiHelper.Px(this, 4), Height);
        }

        int leTrai = DpiHelper.Px(this, 22);
        int lePhai = DpiHelper.Px(this, 16);

        // Huy hiệu số (vẽ trước để biết chừa chỗ cho chữ)
        int rongHuyHieu = 0;
        if (!string.IsNullOrEmpty(_huyHieu))
        {
            using var fontNho = DpiHelper.TaoFont(8f, FontStyle.Bold);
            Size chu = TextRenderer.MeasureText(g, _huyHieu, fontNho, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            int cao = DpiHelper.Px(this, 20);
            rongHuyHieu = Math.Max(cao, chu.Width + DpiHelper.Px(this, 12));
            var vungHuyHieu = new Rectangle(Width - lePhai - rongHuyHieu, (Height - cao) / 2, rongHuyHieu, cao);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var duong = UiTheme.HinhChuNhatBoGoc(vungHuyHieu, cao / 2);
            using var nen = new SolidBrush(UiTheme.Do);
            g.FillPath(nen, duong);
            TextRenderer.DrawText(g, _huyHieu, fontNho, vungHuyHieu, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            rongHuyHieu += DpiHelper.Px(this, 8);
        }

        // Chữ của mục menu (NoPrefix để hiển thị đúng ký tự & trong "Tác giả & thể loại")
        using var font = DpiHelper.TaoFont(10f, _dangChon ? FontStyle.Bold : FontStyle.Regular);
        var vungChu = new Rectangle(leTrai, 0, Math.Max(1, Width - leTrai - lePhai - rongHuyHieu), Height);
        TextRenderer.DrawText(g, Text, font, vungChu, _dangChon ? UiTheme.XanhChinh : UiTheme.ChuChinh,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

        // Khung focus khi dùng bàn phím
        if (Focused && ShowFocusCues)
        {
            var r = new Rectangle(1, 1, Width - 3, Height - 3);
            ControlPaint.DrawFocusRectangle(g, r);
        }
    }
}

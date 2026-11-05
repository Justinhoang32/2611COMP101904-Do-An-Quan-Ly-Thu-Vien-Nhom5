namespace QuanLyThuVien.Controls;

/// <summary>
/// "Thẻ" nền trắng có viền mảnh, dùng chung cho mọi khối nội dung.
/// Kế thừa TableLayoutPanel (1 cột, tự co theo nội dung) nên chiều cao tự tính đúng ở mọi mức DPI.
/// Thêm nội dung bằng UiTheme.ThemHang(the, control).
/// </summary>
public class ThePanel : TableLayoutPanel
{
    public ThePanel()
    {
        ColumnCount = 1;
        ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Padding = new Padding(18);
        Margin = new Padding(0, 0, 0, 16);
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(UiTheme.VienNhat);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);   // viền ngoài của thẻ
    }
}

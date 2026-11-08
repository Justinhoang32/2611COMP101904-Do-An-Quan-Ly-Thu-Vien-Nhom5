using System.Drawing.Drawing2D;

namespace QuanLyThuVien.Controls;

/// <summary>Một cột của biểu đồ cột: nhãn (ví dụ "T10") và giá trị.</summary>
public class CotBieuDo
{
    public string Nhan { get; set; }
    public int GiaTri { get; set; }
    public CotBieuDo(string nhan, int giaTri) { Nhan = nhan; GiaTri = giaTri; }
}

/// <summary>
/// Biểu đồ cột tự vẽ bằng GDI+ (không dùng thư viện ngoài). Cột cuối được tô đậm.
/// Mọi kích thước tính theo DPI thật của control nên không bị mờ/lệch khi Scale lớn.
/// </summary>
public class ucBarChart : Control
{
    private List<CotBieuDo> _cot = new();
    private int _chiSoNoiBat = -1;

    public ucBarChart()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.White;
    }

    /// <summary>Nạp dữ liệu; chiSoNoiBat là cột tô đậm (mặc định cột cuối).</summary>
    public void DatDuLieu(IEnumerable<CotBieuDo> cot, int chiSoNoiBat = -1)
    {
        _cot = cot.ToList();
        _chiSoNoiBat = chiSoNoiBat >= 0 ? chiSoNoiBat : _cot.Count - 1;
        Invalidate();
    }

    public override Size GetPreferredSize(Size proposedSize) =>
        new Size(Math.Max(DpiHelper.Px(this, 320), MinimumSize.Width), Math.Max(DpiHelper.Px(this, 170), MinimumSize.Height));

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);
        if (_cot.Count == 0) return;

        using var fNho = DpiHelper.TaoFont(8.5f);
        using var fSo = DpiHelper.TaoFont(8.5f);
        int leDuoi = DpiHelper.Px(this, 24), leTren = DpiHelper.Px(this, 22);
        int vungCao = Height - leDuoi - leTren;
        if (vungCao <= 4) return;
        int max = Math.Max(1, _cot.Max(c => c.GiaTri));
        int o = Width / _cot.Count;                       // độ rộng mỗi ô cột
        int rongCot = Math.Min(DpiHelper.Px(this, 36), (int)(o * 0.6));
        using var duongKe = new Pen(UiTheme.VienNhat);
        g.DrawLine(duongKe, 0, Height - leDuoi, Width, Height - leDuoi);

        for (int i = 0; i < _cot.Count; i++)
        {
            var c = _cot[i];
            int cao = c.GiaTri <= 0 ? 0 : Math.Max(DpiHelper.Px(this, 3), (int)(vungCao * (c.GiaTri / (double)max)));
            int x = i * o + (o - rongCot) / 2;
            var r = new Rectangle(x, Height - leDuoi - cao, rongCot, cao);
            var mau = i == _chiSoNoiBat ? UiTheme.XanhChinh : UiForm.Pha(UiTheme.XanhChinh, 0.65f);
            if (cao > 0)
            {
                using var br = new SolidBrush(mau);
                g.FillRectangle(br, r);
            }
            var vungSo = new Rectangle(i * o, r.Top - leTren + DpiHelper.Px(this, 4), o, leTren - DpiHelper.Px(this, 4));
            TextRenderer.DrawText(g, c.GiaTri.ToString(), fSo, vungSo, UiTheme.ChuPhu, TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom | TextFormatFlags.NoPadding);
            var vungNhan = new Rectangle(i * o, Height - leDuoi, o, leDuoi);
            TextRenderer.DrawText(g, c.Nhan, fNho, vungNhan, UiTheme.ChuPhu, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}

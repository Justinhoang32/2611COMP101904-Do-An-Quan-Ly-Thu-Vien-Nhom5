namespace QuanLyThuVien.Controls;

/// <summary>
/// Thanh phân trang dạng "‹ 1 2 3 … 12 ›". Phát sự kiện TrangChanged khi người dùng chọn trang khác.
/// Kích thước nút tính lúc chạy nên nhân theo DPI thật; tự dựng lại khi DPI đổi.
/// </summary>
public class ucPagination : FlowLayoutPanel
{
    private int _trang = 1;
    private int _tong;

    /// <summary>Phát ra số trang mới được chọn.</summary>
    public event EventHandler<int> TrangChanged;

    public ucPagination()
    {
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        WrapContents = false;
        FlowDirection = FlowDirection.LeftToRight;
        Margin = new Padding(0);
    }

    /// <summary>Đặt trang hiện tại và tổng số trang rồi vẽ lại.</summary>
    public void DatTrang(int trangHienTai, int tongSoTrang)
    {
        _trang = Math.Max(1, trangHienTai);
        _tong = Math.Max(0, tongSoTrang);
        Dung();
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        Dung();   // nút có kích thước tính bằng code nên dựng lại theo DPI mới
    }

    // Dựng lại các nút trang
    private void Dung()
    {
        SuspendLayout();
        foreach (Control c in Controls.Cast<Control>().ToList()) { Controls.Remove(c); c.Dispose(); }
        if (_tong > 0)
        {
            ThemNut("‹", _trang - 1, _trang > 1, false);
            // Các trang cần hiện: đầu, cuối, và trang hiện tại ± 1
            var hien = new SortedSet<int> { 1, _tong, _trang - 1, _trang, _trang + 1 };
            hien.RemoveWhere(p => p < 1 || p > _tong);
            int truoc = 0;
            foreach (int p in hien)
            {
                if (truoc != 0 && p - truoc > 1) ThemDauCham();
                ThemNut(p.ToString(), p, true, p == _trang);
                truoc = p;
            }
            ThemNut("›", _trang + 1, _trang < _tong, false);
        }
        ResumeLayout(true);
    }

    private void ThemNut(string chu, int trangDich, bool choPhep, bool dangChon)
    {
        var nut = new Button
        {
            Text = chu, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlatStyle = FlatStyle.Flat,
            Font = DpiHelper.TaoFont(9.5f, dangChon ? FontStyle.Bold : FontStyle.Regular),
            MinimumSize = new Size(DpiHelper.Px(this, 34), DpiHelper.Px(this, 34)),
            Padding = DpiHelper.Px(this, 6, 2, 6, 2),
            Margin = new Padding(0, 0, DpiHelper.Px(this, 6), 0),
            BackColor = dangChon ? UiTheme.XanhChinh : Color.White,
            ForeColor = dangChon ? Color.White : UiTheme.ChuChinh,
            Enabled = choPhep, UseVisualStyleBackColor = false, Cursor = choPhep ? Cursors.Hand : Cursors.Default
        };
        nut.FlatAppearance.BorderColor = dangChon ? UiTheme.XanhChinh : UiTheme.VienNhat;
        nut.FlatAppearance.BorderSize = 1;
        if (choPhep && !dangChon) nut.Click += (s, e) => TrangChanged?.Invoke(this, trangDich);
        Controls.Add(nut);
    }

    private void ThemDauCham()
    {
        var l = UiTheme.TaoNhan("…", 10f, FontStyle.Regular, UiTheme.ChuPhu);
        l.Margin = new Padding(0, DpiHelper.Px(this, 6), DpiHelper.Px(this, 8), 0);
        Controls.Add(l);
    }
}

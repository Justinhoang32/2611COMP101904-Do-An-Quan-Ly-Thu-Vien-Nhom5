namespace QuanLyThuVien.Controls;

/// <summary>Thẻ số liệu: nhãn nhỏ / con số lớn / ghi chú phụ (ví dụ "1.280" - "đầu sách").</summary>
public class ucStatCard : ThePanel
{
    private readonly Label _lblTieuDe;
    private readonly Label _lblGiaTri;
    private readonly Label _lblGhiChu;

    public ucStatCard()
    {
        Padding = new Padding(16, 14, 16, 14);
        _lblTieuDe = UiTheme.TaoNhan("", 9f, FontStyle.Regular, UiTheme.ChuPhu);
        _lblGiaTri = UiTheme.TaoNhan("0", 22f, FontStyle.Bold, UiTheme.XanhChinh);
        _lblGiaTri.Margin = new Padding(0, 4, 0, 2);
        _lblGhiChu = UiTheme.TaoNhan("", 8.5f, FontStyle.Regular, UiTheme.ChuPhu);
        UiTheme.ThemHang(this, _lblTieuDe);
        UiTheme.ThemHang(this, _lblGiaTri);
        UiTheme.ThemHang(this, _lblGhiChu);
    }

    public string TieuDe { get => _lblTieuDe.Text; set => _lblTieuDe.Text = value; }
    public string GiaTri { get => _lblGiaTri.Text; set => _lblGiaTri.Text = value; }
    public string GhiChu { get => _lblGhiChu.Text; set => _lblGhiChu.Text = value; }
    public Color MauGiaTri { get => _lblGiaTri.ForeColor; set => _lblGiaTri.ForeColor = value; }
}

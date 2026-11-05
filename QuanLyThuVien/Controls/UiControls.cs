using System.ComponentModel;

namespace QuanLyThuVien.Controls;

// Bộ control "có sẵn kiểu dáng" dùng chung. Mỗi control tự áp font, màu, đệm trong constructor để file Designer chỉ cần
// khai báo vài thuộc tính (Text, Kieu...) và Visual Studio vẫn mở/chỉnh được bằng màn hình kéo-thả.

/// <summary>Kiểu nút: Chinh = nền xanh chữ trắng; Phu = nền trắng viền xanh.</summary>
public enum KieuNut { Chinh, Phu }

/// <summary>Nút phẳng theo giao diện thống nhất của ứng dụng.</summary>
public class NutBam : Button
{
    private KieuNut _kieu = KieuNut.Chinh;

    public NutBam()
    {
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        FlatStyle = FlatStyle.Flat;
        Font = DpiHelper.TaoFont(9.5f, FontStyle.Bold);
        Padding = new Padding(14, 7, 14, 7);
        Margin = new Padding(0);
        Cursor = Cursors.Hand;
        UseVisualStyleBackColor = false;
        FlatAppearance.BorderSize = 1;
        ApMau();
    }

    /// <summary>Kiểu hiển thị của nút.</summary>
    [DefaultValue(KieuNut.Chinh)]
    public KieuNut Kieu { get => _kieu; set { _kieu = value; ApMau(); } }

    private void ApMau()
    {
        if (_kieu == KieuNut.Chinh)
        {
            BackColor = UiTheme.XanhChinh; ForeColor = Color.White;
            FlatAppearance.BorderColor = UiTheme.XanhChinh; FlatAppearance.MouseOverBackColor = UiTheme.XanhDam;
        }
        else
        {
            BackColor = Color.White; ForeColor = UiTheme.XanhChinh;
            FlatAppearance.BorderColor = UiTheme.XanhChinh; FlatAppearance.MouseOverBackColor = UiTheme.XanhNhat;
        }
    }
}

/// <summary>Các kiểu chữ có sẵn của nhãn.</summary>
public enum KieuChu { Thuong, TieuDeTrang, TieuDeThe, TieuDeCon, MoTa, Nho, NhanTruong, Dam, SoLon, SoVua }

/// <summary>Cơ chế cho nhãn tự xuống dòng theo bề rộng của vùng chứa (thay cho chiều cao cố định).</summary>
internal static class XuongDong
{
    public static void Gan(Label nhan)
    {
        void CapNhat()
        {
            var cha = nhan.Parent;
            if (cha == null || !nhan.AutoSize) return;
            int rong = cha.ClientSize.Width - cha.Padding.Horizontal - nhan.Margin.Horizontal;
            if (rong > 0 && nhan.MaximumSize.Width != rong) nhan.MaximumSize = new Size(rong, 0);
        }
        nhan.ParentChanged += (s, e) => { if (nhan.Parent != null) { nhan.Parent.SizeChanged += (a, b) => CapNhat(); CapNhat(); } };
        nhan.HandleCreated += (s, e) => CapNhat();
    }
}

/// <summary>Nhãn chữ theo kiểu có sẵn (KieuChu); MauChu để đổi màu riêng.</summary>
public class NhanVan : Label
{
    private KieuChu _kieu = KieuChu.Thuong;
    private Color? _mau;

    public NhanVan()
    {
        AutoSize = true;
        BackColor = Color.Transparent;
        Margin = new Padding(0);
        ApKieu();
        XuongDong.Gan(this);
    }

    [DefaultValue(KieuChu.Thuong)]
    public KieuChu Kieu { get => _kieu; set { _kieu = value; ApKieu(); } }

    /// <summary>Màu chữ riêng (bỏ trống = theo kiểu).</summary>
    public Color MauChu
    {
        get => _mau ?? ForeColor;
        set { _mau = value; ForeColor = value; }
    }
    private bool ShouldSerializeMauChu() => _mau.HasValue;
    private void ResetMauChu() { _mau = null; ApKieu(); }

    private void ApKieu()
    {
        (float co, FontStyle kieu, Color mau) = _kieu switch
        {
            KieuChu.TieuDeTrang => (18f, FontStyle.Bold, UiTheme.ChuChinh),
            KieuChu.TieuDeThe => (12f, FontStyle.Bold, UiTheme.ChuChinh),
            KieuChu.TieuDeCon => (10.5f, FontStyle.Bold, UiTheme.ChuChinh),
            KieuChu.MoTa => (9.5f, FontStyle.Regular, UiTheme.ChuPhu),
            KieuChu.Nho => (9f, FontStyle.Regular, UiTheme.ChuPhu),
            KieuChu.NhanTruong => (9f, FontStyle.Bold, UiTheme.ChuPhu),
            KieuChu.Dam => (9.5f, FontStyle.Bold, UiTheme.ChuChinh),
            KieuChu.SoLon => (22f, FontStyle.Bold, UiTheme.XanhChinh),
            KieuChu.SoVua => (18f, FontStyle.Bold, UiTheme.XanhChinh),
            _ => (9.5f, FontStyle.Regular, UiTheme.ChuChinh)
        };
        Font = DpiHelper.TaoFont(co, kieu);
        ForeColor = _mau ?? mau;
    }
}

/// <summary>Liên kết chữ xanh, không gạch chân.</summary>
public class LienKetXanh : LinkLabel
{
    public LienKetXanh()
    {
        AutoSize = true;
        Font = DpiHelper.TaoFont(9.5f);
        LinkColor = UiTheme.XanhChinh; ActiveLinkColor = UiTheme.XanhDam; VisitedLinkColor = UiTheme.XanhChinh;
        LinkBehavior = LinkBehavior.HoverUnderline;
        BackColor = Color.Transparent;
        Margin = new Padding(0);
    }
}

/// <summary>Màu của chip / hộp thông báo.</summary>
public enum MauChip { Xanh, XanhLa, Do, Cam, Xam }

internal static class MauHelper
{
    public static Color Lay(MauChip m) => m switch
    {
        MauChip.XanhLa => UiTheme.XanhLa, MauChip.Do => UiTheme.Do, MauChip.Cam => UiTheme.Cam, MauChip.Xam => UiTheme.ChuPhu, _ => UiTheme.XanhChinh
    };
}

/// <summary>Nhãn dạng chip: nền nhạt cùng tông với màu chữ.</summary>
public class ChipNhan : Label
{
    private MauChip _mau = MauChip.Xanh;
    public ChipNhan()
    {
        AutoSize = true;
        Font = DpiHelper.TaoFont(8.5f, FontStyle.Bold);
        Padding = new Padding(8, 3, 8, 3);
        Margin = new Padding(0, 0, 8, 6);
        ApMau();
    }
    [DefaultValue(MauChip.Xanh)]
    public MauChip Mau { get => _mau; set { _mau = value; ApMau(); } }
    private void ApMau() { var m = MauHelper.Lay(_mau); ForeColor = m; BackColor = UiForm.Pha(m, 0.88f); }
}

/// <summary>Hộp thông báo nền nhạt (chữ tự xuống dòng). Mặc định ẩn khi Visible = false.</summary>
public class HopThongBao : Label
{
    private MauChip _mau = MauChip.Xanh;
    public HopThongBao()
    {
        AutoSize = true;
        Font = DpiHelper.TaoFont(9f);
        Padding = new Padding(10, 8, 10, 8);
        Margin = new Padding(0, 0, 0, 12);
        Dock = DockStyle.Fill;
        ApMau();
        XuongDong.Gan(this);
    }
    [DefaultValue(MauChip.Xanh)]
    public MauChip Mau { get => _mau; set { _mau = value; ApMau(); } }
    private void ApMau() { var m = MauHelper.Lay(_mau); ForeColor = m; BackColor = UiForm.Pha(m, 0.9f); }
}

/// <summary>
/// Ô nhập có nhãn nhỏ phía trên: đặt NhanText, rồi thả MỘT control (TextBox, ComboBox, DateTimePicker...) vào trong.
/// Control con tự được xếp ngay dưới nhãn, rộng bằng ô.
/// </summary>
public class TruongNhap : Panel
{
    private readonly Label _lbl = new() { AutoSize = false, BackColor = Color.Transparent };
    private string _nhan = "";
    private bool _batBuoc;

    public TruongNhap()
    {
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Margin = new Padding(0, 0, 12, 12);
        Controls.Add(_lbl);
        CapNhatNhan();
    }

    [DefaultValue("")] public string NhanText { get => _nhan; set { _nhan = value ?? ""; CapNhatNhan(); } }
    [DefaultValue(false)] public bool BatBuoc { get => _batBuoc; set { _batBuoc = value; CapNhatNhan(); } }

    private void CapNhatNhan()
    {
        _lbl.Text = _nhan + (_batBuoc ? " *" : "");
        _lbl.Font = DpiHelper.TaoFont(9f, FontStyle.Bold);
        _lbl.ForeColor = UiTheme.ChuPhu;
        PerformLayout();
    }

    private Control Chinh => Controls.Cast<Control>().FirstOrDefault(c => c != _lbl);

    private Size TinhKichThuoc(int rong, bool apDung)
    {
        int hNhan = TextRenderer.MeasureText(_lbl.Text, _lbl.Font).Height + 4;
        int khoang = DpiHelper.Px(this, 4);
        var c = Chinh;
        int hC = c?.Height ?? 0;
        if (apDung)
        {
            _lbl.SetBounds(0, 0, rong, hNhan);
            c?.SetBounds(0, hNhan + khoang, rong, hC);
        }
        return new Size(Math.Max(rong, c?.Width ?? 0), hNhan + khoang + hC);
    }

    protected override void OnLayout(LayoutEventArgs e) { base.OnLayout(e); TinhKichThuoc(Math.Max(ClientSize.Width, 10), true); }
    public override Size GetPreferredSize(Size proposedSize) => TinhKichThuoc(proposedSize.Width > 0 ? proposedSize.Width : Math.Max(Width, 120), false);
}

/// <summary>Kiểu tiêu đề: Trang = chữ lớn đầu trang; The = tiêu đề của một thẻ.</summary>
public enum KieuTieuDe { Trang, The }

/// <summary>
/// Hàng tiêu đề (đầu trang hoặc đầu thẻ): TieuDe + MoTa bên trái; các control thả vào (nút, chip, liên kết) tự xếp sang phải.
/// </summary>
public class TieuDeKhoi : Panel
{
    private readonly Label _ten = new() { AutoSize = false, BackColor = Color.Transparent };
    private readonly Label _moTa = new() { AutoSize = false, BackColor = Color.Transparent };
    private KieuTieuDe _kieu = KieuTieuDe.Trang;

    public TieuDeKhoi()
    {
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Margin = new Padding(0, 0, 0, 16);
        Controls.Add(_ten);
        Controls.Add(_moTa);
        ApKieu();
    }

    [DefaultValue(KieuTieuDe.Trang)] public KieuTieuDe Kieu { get => _kieu; set { _kieu = value; ApKieu(); } }
    [DefaultValue("")] public string TieuDe { get => _ten.Text; set { _ten.Text = value ?? ""; PerformLayout(); } }
    [DefaultValue("")] public string MoTa { get => _moTa.Text; set { _moTa.Text = value ?? ""; PerformLayout(); } }

    private void ApKieu()
    {
        bool trang = _kieu == KieuTieuDe.Trang;
        _ten.Font = DpiHelper.TaoFont(trang ? 18f : 12f, FontStyle.Bold);
        _ten.ForeColor = UiTheme.ChuChinh;
        _moTa.Font = DpiHelper.TaoFont(trang ? 9.5f : 9f);
        _moTa.ForeColor = UiTheme.ChuPhu;
        PerformLayout();
    }

    private int Tinh(int rong, bool apDung)
    {
        var phai = Controls.Cast<Control>().Where(c => c != _ten && c != _moTa && c.Visible).ToList();
        int khoang = DpiHelper.Px(this, 10);
        int tongPhai = phai.Sum(c => c.GetPreferredSize(Size.Empty).Width) + khoang * Math.Max(0, phai.Count - 1);
        int conLai = Math.Max(DpiHelper.Px(this, 80), rong - tongPhai - (phai.Count > 0 ? khoang : 0));

        int hTen = TextRenderer.MeasureText(_ten.Text.Length == 0 ? "A" : _ten.Text, _ten.Font, new Size(conLai, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + 2;
        int hMoTa = _moTa.Text.Length == 0 ? 0 : TextRenderer.MeasureText(_moTa.Text, _moTa.Font, new Size(conLai, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + 2;
        int gap = hMoTa > 0 ? DpiHelper.Px(this, 4) : 0;
        int hTrai = hTen + gap + hMoTa;
        int hPhai = phai.Count == 0 ? 0 : phai.Max(c => c.GetPreferredSize(Size.Empty).Height);
        int cao = Math.Max(hTrai, hPhai);
        if (apDung)
        {
            _ten.SetBounds(0, 0, conLai, hTen);
            _moTa.SetBounds(0, hTen + gap, conLai, hMoTa);
            _moTa.Visible = hMoTa > 0;
            int x = rong - tongPhai;
            foreach (var c in phai)
            {
                var k = c.GetPreferredSize(Size.Empty);
                c.SetBounds(x, (cao - k.Height) / 2, k.Width, k.Height);
                x += k.Width + khoang;
            }
        }
        return cao;
    }

    protected override void OnLayout(LayoutEventArgs e) { base.OnLayout(e); Tinh(Math.Max(ClientSize.Width, 10), true); }
    public override Size GetPreferredSize(Size proposedSize)
    {
        int rong = proposedSize.Width > 0 ? proposedSize.Width : Math.Max(Width, 200);
        return new Size(rong, Tinh(rong, false));
    }
}

/// <summary>DataGridView đã áp kiểu thống nhất (màu, font, viền); chiều cao dòng/tiêu đề cấu hình theo DPI bằng DpiHelper.CauHinhDataGridView khi Load.</summary>
public class LuoiDuLieu : DataGridView
{
    public LuoiDuLieu()
    {
        BorderStyle = BorderStyle.None;
        BackgroundColor = Color.White;
        CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        GridColor = UiTheme.VienNhat;
        EnableHeadersVisualStyles = false;
        ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.NenHover, ForeColor = UiTheme.ChuPhu, Font = DpiHelper.TaoFont(8.5f, FontStyle.Bold),
            SelectionBackColor = UiTheme.NenHover, SelectionForeColor = UiTheme.ChuPhu, Padding = new Padding(8, 0, 0, 0),
            Alignment = DataGridViewContentAlignment.MiddleLeft
        };
        DefaultCellStyle = new DataGridViewCellStyle
        {
            Font = DpiHelper.TaoFont(9.5f), ForeColor = UiTheme.ChuChinh, SelectionBackColor = UiTheme.XanhNhat, SelectionForeColor = UiTheme.ChuChinh,
            Padding = new Padding(8, 2, 4, 2), Alignment = DataGridViewContentAlignment.MiddleLeft
        };
        AllowUserToAddRows = false;
        AllowUserToDeleteRows = false;
        AllowUserToResizeRows = false;
        ReadOnly = true;
        MultiSelect = false;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        RowHeadersVisible = false;
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
    }
}

/// <summary>Cột liên kết (ví dụ "Sửa", "Xem") màu xanh thống nhất; giá trị chữ lấy từ ô (hoặc từ Text nếu UseColumnTextForLinkValue).</summary>
public class CotLienKet : DataGridViewLinkColumn
{
    public CotLienKet()
    {
        LinkColor = UiTheme.XanhChinh; ActiveLinkColor = UiTheme.XanhDam; VisitedLinkColor = UiTheme.XanhChinh;
        LinkBehavior = LinkBehavior.HoverUnderline;
        TrackVisitedState = false;
    }
}

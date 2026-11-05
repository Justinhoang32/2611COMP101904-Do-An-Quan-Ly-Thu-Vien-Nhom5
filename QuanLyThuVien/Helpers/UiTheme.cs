using System.Drawing.Drawing2D;

namespace QuanLyThuVien;

/// <summary>
/// Bảng màu, font và hàm tạo control dùng chung để mọi màn hình thống nhất.
/// QUY ƯỚC KÍCH THƯỚC: giá trị Padding/Margin/Size đặt trong CONSTRUCTOR là giá trị 96 DPI (WinForms tự nhân theo DPI).
/// Giá trị tính lúc chạy (Paint, Resize, GetPreferredSize...) phải nhân bằng DpiHelper.Px().
/// </summary>
public static class UiTheme
{
    // ----- Màu -----
    public static readonly Color XanhChinh = Color.FromArgb(27, 75, 122);
    public static readonly Color XanhDam = Color.FromArgb(14, 42, 74);
    public static readonly Color XanhNhat = Color.FromArgb(232, 238, 245);
    public static readonly Color NenManHinh = Color.FromArgb(243, 246, 249);
    public static readonly Color NenTrang = Color.White;
    public static readonly Color NenHover = Color.FromArgb(246, 248, 251);
    public static readonly Color VienNhat = Color.FromArgb(221, 227, 234);
    public static readonly Color ChuChinh = Color.FromArgb(31, 41, 55);
    public static readonly Color ChuPhu = Color.FromArgb(107, 114, 128);
    public static readonly Color Do = Color.FromArgb(192, 57, 43);
    public static readonly Color XanhLa = Color.FromArgb(46, 125, 50);
    public static readonly Color Cam = Color.FromArgb(217, 119, 6);

    // ----- Tạo control -----

    /// <summary>Tạo Label tự co giãn (AutoSize) với font Point thống nhất.</summary>
    public static Label TaoNhan(string noiDung, float coChu = 9.5f, FontStyle kieu = FontStyle.Regular, Color? mau = null)
    {
        return new Label
        {
            Text = noiDung,
            AutoSize = true,
            Font = DpiHelper.TaoFont(coChu, kieu),
            ForeColor = mau ?? ChuChinh,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };
    }

    /// <summary>Tạo Button phẳng. chinh = true: nền xanh chữ trắng; false: nền trắng viền xanh.</summary>
    public static Button TaoNut(string noiDung, bool chinh = true)
    {
        var nut = new Button
        {
            Text = noiDung,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlatStyle = FlatStyle.Flat,
            Font = DpiHelper.TaoFont(9.5f, FontStyle.Bold),
            Padding = new Padding(14, 7, 14, 7),
            Margin = new Padding(0),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        nut.FlatAppearance.BorderSize = 1;
        if (chinh)
        {
            nut.BackColor = XanhChinh; nut.ForeColor = Color.White; nut.FlatAppearance.BorderColor = XanhChinh;
            nut.FlatAppearance.MouseOverBackColor = XanhDam;
        }
        else
        {
            nut.BackColor = Color.White; nut.ForeColor = XanhChinh; nut.FlatAppearance.BorderColor = XanhChinh;
            nut.FlatAppearance.MouseOverBackColor = XanhNhat;
        }
        return nut;
    }

    /// <summary>Tạo LinkLabel màu xanh, không gạch chân.</summary>
    public static LinkLabel TaoLienKet(string noiDung, float coChu = 9.5f)
    {
        return new LinkLabel
        {
            Text = noiDung, AutoSize = true, Font = DpiHelper.TaoFont(coChu, FontStyle.Regular),
            LinkColor = XanhChinh, ActiveLinkColor = XanhDam, VisitedLinkColor = XanhChinh,
            LinkBehavior = LinkBehavior.HoverUnderline, BackColor = Color.Transparent, Margin = new Padding(0)
        };
    }

    /// <summary>Tạo TableLayoutPanel một cột rộng 100%, các hàng tự co theo nội dung.</summary>
    public static TableLayoutPanel TaoBangMotCot()
    {
        var t = new TableLayoutPanel { ColumnCount = 1, RowCount = 0, BackColor = Color.Transparent, Margin = new Padding(0) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        return t;
    }

    /// <summary>Thêm một hàng vào bảng một cột.</summary>
    public static void ThemHang(TableLayoutPanel bang, Control control, SizeType kieu = SizeType.AutoSize, float giaTri = 0f)
    {
        bang.RowStyles.Add(new RowStyle(kieu, giaTri));
        bang.RowCount = bang.RowStyles.Count;
        bang.Controls.Add(control, 0, bang.RowCount - 1);
    }

    /// <summary>
    /// Cho Label tự xuống dòng theo chiều rộng của vùng chứa (thay cho đặt chiều cao cố định, tránh cắt chữ khi Scale tăng).
    /// </summary>
    public static void TuXuongDong(Label nhan, Control vungChua)
    {
        nhan.AutoSize = true;
        void CapNhat()
        {
            int rong = vungChua.ClientSize.Width - vungChua.Padding.Horizontal - nhan.Margin.Horizontal;
            if (rong > 0) nhan.MaximumSize = new Size(rong, 0);
        }
        vungChua.SizeChanged += (s, e) => CapNhat();
        vungChua.HandleCreated += (s, e) => CapNhat();
        CapNhat();
    }

    /// <summary>Tạo đường viền hình chữ nhật bo góc (dùng khi tự vẽ).</summary>
    public static GraphicsPath HinhChuNhatBoGoc(Rectangle r, int banKinh)
    {
        int d = Math.Max(1, banKinh * 2);
        var p = new GraphicsPath();
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    /// <summary>Khối hai dòng "Thư viện HCMUE" / "HỆ THỐNG QUẢN LÝ THƯ VIỆN" dùng ở header và sidebar.</summary>
    public static TableLayoutPanel ThemBangTieuDeThuongHieu()
    {
        var t = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent, Anchor = AnchorStyles.Left };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.Controls.Add(TaoNhan("Thư viện HCMUE", 12f, FontStyle.Bold), 0, 0);
        t.Controls.Add(TaoNhan("HỆ THỐNG QUẢN LÝ THƯ VIỆN", 7.5f, FontStyle.Regular, ChuPhu), 0, 1);
        return t;
    }
}

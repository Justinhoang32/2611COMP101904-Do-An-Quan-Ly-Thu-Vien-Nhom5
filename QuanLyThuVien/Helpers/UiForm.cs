namespace QuanLyThuVien;

/// <summary>Hàm dựng giao diện dùng chung cho các màn hình: ô nhập có nhãn, tiêu đề trang, lưới dữ liệu, chip trạng thái.</summary>
public static class UiForm
{
    /// <summary>Tạo ô nhập có nhãn nhỏ phía trên (nhãn đậm màu xám, thêm dấu * nếu bắt buộc).</summary>
    public static TableLayoutPanel TaoTruong(string nhan, Control control, bool batBuoc = false)
    {
        var t = UiTheme.TaoBangMotCot();
        t.Dock = DockStyle.Fill;
        t.AutoSize = true;
        t.Margin = new Padding(0, 0, 12, 12);
        var l = UiTheme.TaoNhan(nhan + (batBuoc ? " *" : ""), 9f, FontStyle.Bold, UiTheme.ChuPhu);
        l.Margin = new Padding(0, 0, 0, 4);
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0);
        UiTheme.ThemHang(t, l);
        UiTheme.ThemHang(t, control);
        return t;
    }

    /// <summary>Tạo hàng tiêu đề trang: tiêu đề lớn + mô tả bên trái, các nút bên phải.</summary>
    public static TableLayoutPanel TaoTieuDe(string tieuDe, string moTa, params Control[] nutPhai)
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 0, 0, 16) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var trai = UiTheme.TaoBangMotCot();
        trai.AutoSize = true;
        trai.Dock = DockStyle.Fill;
        UiTheme.ThemHang(trai, UiTheme.TaoNhan(tieuDe, 18f, FontStyle.Bold));
        var lblMoTa = UiTheme.TaoNhan(moTa, 9.5f, FontStyle.Regular, UiTheme.ChuPhu);
        lblMoTa.Margin = new Padding(0, 4, 0, 0);
        UiTheme.ThemHang(trai, lblMoTa);
        t.Controls.Add(trai, 0, 0);
        if (nutPhai.Length > 0)
        {
            var phai = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Anchor = AnchorStyles.Right, Margin = new Padding(0) };
            foreach (var n in nutPhai) { n.Margin = new Padding(phai.Controls.Count == 0 ? 0 : 10, 0, 0, 0); phai.Controls.Add(n); }
            t.Controls.Add(phai, 1, 0);
        }
        return t;
    }

    /// <summary>Tạo DataGridView đã áp kiểu thống nhất (màu, font, viền). Cấu hình theo DPI gọi sau bằng DpiHelper.CauHinhDataGridView.</summary>
    public static DataGridView TaoLuoi()
    {
        var dgv = new DataGridView
        {
            Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackgroundColor = Color.White,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, GridColor = UiTheme.VienNhat,
            EnableHeadersVisualStyles = false
        };
        dgv.ColumnHeadersDefaultCellStyle.BackColor = UiTheme.NenHover;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = UiTheme.ChuPhu;
        dgv.ColumnHeadersDefaultCellStyle.Font = DpiHelper.TaoFont(8.5f, FontStyle.Bold);
        dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = UiTheme.NenHover;
        dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = UiTheme.ChuPhu;
        dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
        dgv.DefaultCellStyle.Font = DpiHelper.TaoFont(9.5f);
        dgv.DefaultCellStyle.ForeColor = UiTheme.ChuChinh;
        dgv.DefaultCellStyle.SelectionBackColor = UiTheme.XanhNhat;
        dgv.DefaultCellStyle.SelectionForeColor = UiTheme.ChuChinh;
        dgv.DefaultCellStyle.Padding = new Padding(8, 2, 4, 2);
        dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        return dgv;
    }

    /// <summary>Tạo cột liên kết (ví dụ "Sửa", "Xem") với màu xanh thống nhất.</summary>
    public static DataGridViewLinkColumn TaoCotLienKet(string tieuDe, string chu, float trong)
    {
        return new DataGridViewLinkColumn
        {
            HeaderText = tieuDe, FillWeight = trong, Text = chu, UseColumnTextForLinkValue = chu != null,
            LinkColor = UiTheme.XanhChinh, ActiveLinkColor = UiTheme.XanhDam, VisitedLinkColor = UiTheme.XanhChinh,
            LinkBehavior = LinkBehavior.HoverUnderline
        };
    }

    /// <summary>Đặt chiều cao hàng lưới (hàng 0 của thẻ) vừa đủ tiêu đề cột + các dòng; tính theo chiều cao thật nên đúng ở mọi DPI.</summary>
    public static void CanChieuCao(TableLayoutPanel the, int chiSoHang, DataGridView dgv, int chieuCaoThongBaoTrong = 0)
    {
        if (the == null || dgv == null || chiSoHang >= the.RowStyles.Count) return;
        int noiDung = dgv.ColumnHeadersHeight + dgv.Rows.GetRowsHeight(DataGridViewElementStates.Visible);
        if (dgv.Rows.Count == 0) noiDung += chieuCaoThongBaoTrong;
        int cao = Math.Max(noiDung + 2, DpiHelper.Px(dgv, 110));
        if (the.RowStyles[chiSoHang].Height != cao) the.RowStyles[chiSoHang].Height = cao;
    }

    /// <summary>Tạo nhãn dạng "chip" có nền nhạt cùng tông với màu chữ (dùng cho trạng thái, thẻ thông tin).</summary>
    public static Label TaoChip(string noiDung, Color mau)
    {
        var chip = UiTheme.TaoNhan(noiDung, 8.5f, FontStyle.Bold, mau);
        chip.Padding = new Padding(8, 3, 8, 3);
        chip.Margin = new Padding(0, 0, 8, 6);
        chip.BackColor = Pha(mau, 0.88f);
        return chip;
    }

    /// <summary>Pha một màu với màu trắng (tiLeTrang = 0..1) để có nền nhạt.</summary>
    public static Color Pha(Color mau, float tiLeTrang) =>
        Color.FromArgb((int)(mau.R + (255 - mau.R) * tiLeTrang), (int)(mau.G + (255 - mau.G) * tiLeTrang), (int)(mau.B + (255 - mau.B) * tiLeTrang));

    /// <summary>Hộp thông báo nền nhạt (xanh/đỏ/xanh lá...) có viền trái đậm, chữ tự xuống dòng.</summary>
    public static Label TaoHopThongBao(Color mau)
    {
        var l = UiTheme.TaoNhan("", 9f, FontStyle.Regular, mau);
        l.BackColor = Pha(mau, 0.9f);
        l.Padding = new Padding(10, 8, 10, 8);
        l.Margin = new Padding(0, 0, 0, 12);
        l.Dock = DockStyle.Fill;
        l.Visible = false;
        return l;
    }
}

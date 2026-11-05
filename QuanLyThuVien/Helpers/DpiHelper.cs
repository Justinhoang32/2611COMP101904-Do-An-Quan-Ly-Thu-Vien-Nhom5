namespace QuanLyThuVien;

/// <summary>
/// Tiện ích DPI dùng chung cho mọi Form / UserControl.
/// Quy ước: mọi giá trị pixel viết trong code là pixel ở 96 DPI (100%); hàm ở đây nhân theo DPI thực của control.
/// </summary>
public static class DpiHelper
{
    public const float DpiChuan = 96f;

    /// <summary>Hệ số co giãn hiện tại của control (100% = 1.0, 150% = 1.5...).</summary>
    public static float HeSo(Control c) => c.DeviceDpi / DpiChuan;

    /// <summary>Đổi số pixel logic (96 DPI) sang pixel thật theo DPI của control.</summary>
    public static int Px(Control c, int pixelLogic) => (int)Math.Round(pixelLogic * HeSo(c));

    public static Padding Px(Control c, Padding p) =>
        new Padding(Px(c, p.Left), Px(c, p.Top), Px(c, p.Right), Px(c, p.Bottom));

    public static Padding Px(Control c, int trai, int tren, int phai, int duoi) =>
        new Padding(Px(c, trai), Px(c, tren), Px(c, phai), Px(c, duoi));

    public static Size Px(Control c, Size s) => new Size(Px(c, s.Width), Px(c, s.Height));

    /// <summary>
    /// QUY ƯỚC: gọi hàm này trong sự kiện Load của UserControl/Form (sau khi đã co giãn theo DPI), KHÔNG gọi trong constructor.
    /// Cấu hình chuẩn cho DataGridView: cột Fill, không sửa trực tiếp, chọn nguyên dòng, tắt thêm dòng,
    /// chiều cao dòng/tiêu đề theo DPI, chữ tự xuống dòng để không bị cắt. Tự cập nhật khi DPI đổi.
    /// </summary>
    public static void CauHinhDataGridView(DataGridView dgv, int chieuCaoDong = 44, int chieuCaoTieuDe = 40)
    {
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.AllowUserToResizeRows = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.RowHeadersVisible = false;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;       // chữ dài tự xuống dòng
        dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;    // dòng tự cao theo nội dung

        CapNhatKichThuocDataGridView(dgv, chieuCaoDong, chieuCaoTieuDe);
        // Khi kéo cửa sổ sang màn hình khác DPI hoặc đổi Scale: tính lại kích thước
        dgv.DpiChangedAfterParent += (s, e) => CapNhatKichThuocDataGridView(dgv, chieuCaoDong, chieuCaoTieuDe);
    }

    private static void CapNhatKichThuocDataGridView(DataGridView dgv, int chieuCaoDong, int chieuCaoTieuDe)
    {
        dgv.RowTemplate.Height = Px(dgv, chieuCaoDong);
        dgv.RowTemplate.MinimumHeight = Px(dgv, chieuCaoDong);
        dgv.ColumnHeadersHeight = Px(dgv, chieuCaoTieuDe);
        foreach (DataGridViewRow dong in dgv.Rows) dong.MinimumHeight = Px(dgv, chieuCaoDong);
    }

    /// <summary>Đặt độ rộng tối thiểu của cột theo DPI (cột vẫn Fill nhưng không bị bóp quá nhỏ).</summary>
    public static void DatDoRongToiThieuCot(DataGridViewColumn cot, int pixelLogic)
    {
        if (cot.DataGridView != null) cot.MinimumWidth = Px(cot.DataGridView, pixelLogic);
    }

    /// <summary>
    /// Chuẩn hóa Form: tự co giãn theo DPI, giữa màn hình, có kích thước tối thiểu.
    /// LƯU Ý: gọi trong constructor nên truyền giá trị LOGIC (96 DPI). WinForms tự nhân theo DPI khi hiển thị
    /// và khi DPI đổi (kể cả MinimumSize) nên KHÔNG nhân thêm bằng Px() ở đây, tránh nhân đôi.
    /// </summary>
    public static void CauHinhForm(Form form, int rongToiThieu, int caoToiThieu)
    {
        form.AutoScaleMode = AutoScaleMode.Dpi;
        form.AutoScaleDimensions = new SizeF(DpiChuan, DpiChuan);
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimumSize = new Size(rongToiThieu, caoToiThieu);
    }

    /// <summary>Chuẩn hóa UserControl: AutoScaleMode = Dpi, AutoScaleDimensions = 96.</summary>
    public static void CauHinhUserControl(UserControl uc)
    {
        uc.AutoScaleMode = AutoScaleMode.Dpi;
        uc.AutoScaleDimensions = new SizeF(DpiChuan, DpiChuan);
    }

    /// <summary>Tạo font dùng chung (Segoe UI, đơn vị Point).</summary>
    public static Font TaoFont(float coChuPoint, FontStyle kieu = FontStyle.Regular) =>
        new Font(CauHinhUngDung.TenFont, coChuPoint, kieu, GraphicsUnit.Point);
}

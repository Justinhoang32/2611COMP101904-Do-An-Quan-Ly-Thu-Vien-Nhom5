using System.Globalization;
using DTO;
using DTO.Models;
using QuanLyThuVien.Controls;
using QuanLyThuVien.Presenters;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.UserControls;

/// <summary>
/// Màn hình 11 – Báo cáo quá hạn. 4 thẻ thống kê, danh sách cần nhắc trả (lọc ngày đối chiếu/nhóm/mức độ/từ khóa, phân trang),
/// thống kê theo tháng với biểu đồ cột tự vẽ. Chỉ hiển thị; xử lý ở BaoCaoQuaHanPresenter.
/// </summary>
public partial class ucBaoCaoQuaHan : UserControl, IBaoCaoQuaHanView
{
    private static readonly CultureInfo Vi = new("vi-VN");

    private Label lblTrong, lblPhatSinh, lblThuHoi, lblTheoDoi;
    private Font _fontDam;

    private readonly BaoCaoQuaHanPresenter _presenter;

    #region IBaoCaoQuaHanView
    public DateTime NgayDoiChieu => dtpDoiChieu.Value.Date;
    public string Nhom => cboNhom.SelectedIndex <= 0 ? null : cboNhom.Text;
    public int SoNgayToiThieu => cboMucDo.SelectedIndex switch { 2 => 4, 3 => 8, _ => 1 };
    public int? SoNgayToiDa => cboMucDo.SelectedIndex switch { 1 => 3, 2 => 7, _ => null };
    public string TuKhoa => txtTimKiem.Text.Trim();
    public DateTime ThangThongKe => dtpThang.Value;

    public event EventHandler Loaded;
    public event EventHandler LocClicked;
    public event EventHandler<int> TrangChanged;
    public event EventHandler<string> ChiTietClicked;
    public event EventHandler XuatClicked;
    public event EventHandler InClicked;
    public event EventHandler ThangChanged;
    #endregion

    public ucBaoCaoQuaHan(IDieuHuong dieuHuong)
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không tạo Presenter
        GanSuKien();
        _presenter = new BaoCaoQuaHanPresenter(this, dieuHuong);
    }

    #region Gắn sự kiện và phần không biểu diễn được trong Designer
    // Giao diện tĩnh nằm trong ucBaoCaoQuaHan.Designer.cs; ở đây chỉ gắn sự kiện, cấu hình lưới theo DPI và vẽ ô tùy biến.
    private void GanSuKien()
    {
        _fontDam = DpiHelper.TaoFont(9.5f, FontStyle.Bold);
        lblPhatSinh = lblSo1; lblThuHoi = lblSo2; lblTheoDoi = lblSo3;
        dtpDoiChieu.Value = DateTime.Today;
        dtpThang.MaxDate = DateTime.Today;
        dtpThang.Value = DateTime.Today;

        btnXuat.Click += (s, e) => XuatClicked?.Invoke(this, EventArgs.Empty);
        btnIn.Click += (s, e) => InClicked?.Invoke(this, EventArgs.Empty);
        txtTimKiem.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LocClicked?.Invoke(this, EventArgs.Empty); } };
        btnLoc.Click += (s, e) => LocClicked?.Invoke(this, EventArgs.Empty);
        dtpThang.ValueChanged += (s, e) => ThangChanged?.Invoke(this, EventArgs.Empty);
        pagination.TrangChanged += (s, t) => TrangChanged?.Invoke(this, t);

        dgv.CellPainting += DgvCellPainting;
        dgv.CellContentClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == colChiTiet.Index && dgv.Rows[e.RowIndex].Tag is QuaHanHienThi q) ChiTietClicked?.Invoke(this, q.MaPhieu);
        };
        dgv.RowHeightChanged += (s, e) => UiForm.CanChieuCao(cardLuoi, 1, dgv, 60);
        lblTrong = UiTheme.TaoNhan("Không có phiếu quá hạn phù hợp.", 10f, FontStyle.Regular, UiTheme.ChuPhu);
        lblTrong.AutoSize = false; lblTrong.Height = 60; lblTrong.Dock = DockStyle.Bottom; lblTrong.TextAlign = ContentAlignment.MiddleCenter; lblTrong.Visible = false;
        dgv.Controls.Add(lblTrong);

        Load += (s, e) =>
        {
            DpiHelper.CauHinhDataGridView(dgv, 76, 40);
            DpiHelper.DatDoRongToiThieuCot(colPhieu, 120);
            DpiHelper.DatDoRongToiThieuCot(colDocGia, 150);
            DpiHelper.DatDoRongToiThieuCot(colSach, 190);
            DpiHelper.DatDoRongToiThieuCot(colHenTra, 90);
            DpiHelper.DatDoRongToiThieuCot(colTre, 90);
            DpiHelper.DatDoRongToiThieuCot(colCuon, 60);
            DpiHelper.DatDoRongToiThieuCot(colChiTiet, 80);
            Loaded?.Invoke(this, EventArgs.Empty);
        };
        Disposed += (s, e) => _fontDam?.Dispose();
    }
    #endregion

    #region Lưới
    // Vẽ các ô nhiều dòng / chip số ngày trễ
    private void DgvCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || dgv.Rows[e.RowIndex].Tag is not QuaHanHienThi q) return;
        int c = e.ColumnIndex;
        if (c != colPhieu.Index && c != colDocGia.Index && c != colSach.Index && c != colTre.Index) return;

        e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
        var vung = Rectangle.Inflate(e.CellBounds, -DpiHelper.Px(dgv, 8), -DpiHelper.Px(dgv, 4));
        const TextFormatFlags co = TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
        using var fNho = DpiHelper.TaoFont(8.5f);
        using var fThuong = DpiHelper.TaoFont(9f);

        if (c == colTre.Index)
        {
            // Chip đỏ "• N ngày"
            string chu = $"• {q.SoNgayTre} ngày";
            var kt = TextRenderer.MeasureText(e.Graphics, chu, _fontDam, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
            var chip = new Rectangle(vung.Left, e.CellBounds.Top + (e.CellBounds.Height - kt.Height - DpiHelper.Px(dgv, 8)) / 2, kt.Width + DpiHelper.Px(dgv, 16), kt.Height + DpiHelper.Px(dgv, 8));
            using (var br = new SolidBrush(UiForm.Pha(UiTheme.Do, 0.88f))) e.Graphics.FillRectangle(br, chip);
            TextRenderer.DrawText(e.Graphics, chu, _fontDam, chip, UiTheme.Do, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
        else
        {
            string[] dong; Font[] font; Color[] mau;
            if (c == colPhieu.Index) { dong = new[] { q.MaPhieu, "Mượn: " + q.NgayMuon.ToString("dd/MM/yyyy") }; font = new[] { _fontDam, fNho }; mau = new[] { UiTheme.XanhChinh, UiTheme.ChuPhu }; }
            else if (c == colDocGia.Index) { dong = new[] { q.HoTen, $"{q.MaDocGia} · {q.DienThoai}" }; font = new[] { _fontDam, fNho }; mau = new[] { UiTheme.ChuChinh, UiTheme.ChuPhu }; }
            else
            {
                dong = (q.DanhSachSach ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
                font = Enumerable.Repeat(fThuong, dong.Length).ToArray(); mau = Enumerable.Repeat(UiTheme.ChuChinh, dong.Length).ToArray();
            }
            int tong = 0;
            var cao = new int[dong.Length];
            for (int i = 0; i < dong.Length; i++) { cao[i] = TextRenderer.MeasureText(e.Graphics, dong[i], font[i], new Size(vung.Width, int.MaxValue), co).Height + 2; tong += cao[i]; }
            int y = vung.Top + Math.Max(0, (vung.Height - tong) / 2);
            for (int i = 0; i < dong.Length; i++) { TextRenderer.DrawText(e.Graphics, dong[i], font[i], new Rectangle(vung.Left, y, vung.Width, cao[i]), mau[i], co); y += cao[i]; }
        }
        e.Handled = true;
    }
    #endregion

    #region IBaoCaoQuaHanView – hiển thị
    public void HienThiThongKe(ThongKeQuaHan tk, DateTime ngay)
    {
        cardPhieu.GiaTri = tk.SoPhieu.ToString("N0", Vi);
        cardPhieu.GhiChu = "Tính đến ngày " + ngay.ToString("dd/MM/yyyy");
        cardSach.GiaTri = tk.SoCuon.ToString("N0", Vi);
        cardSach.GhiChu = $"Thuộc {tk.SoPhieu} phiếu quá hạn";
        cardDocGia.GiaTri = tk.SoDocGia.ToString("N0", Vi);
        cardDocGia.GhiChu = $"{tk.SoSinhVien} sinh viên · {tk.SoGiangVienCanBo} giảng viên/cán bộ";
        cardTre.GiaTri = tk.SoNgayTreNhieuNhat + " ngày";
        cardTre.GhiChu = tk.SoPhieu == 0 ? "Không có phiếu quá hạn" : "Ưu tiên nhắc trả trong hôm nay";
    }

    public void HienThiDanhSach(KetQuaPhanTrang<QuaHanHienThi> kq)
    {
        dgv.SuspendLayout();
        dgv.Rows.Clear();
        using var g = CreateGraphics();
        using var fThuong = DpiHelper.TaoFont(9f);
        int hDong = TextRenderer.MeasureText(g, "Ag", fThuong).Height + 2;
        foreach (var q in kq.DuLieu)
        {
            int i = dgv.Rows.Add(q.MaPhieu, q.HoTen, q.DanhSachSach, q.HenTra.ToString("dd/MM/yyyy"), q.SoNgayTre + " ngày", q.SoCuon, "Chi tiết →");
            dgv.Rows[i].Tag = q;
            // Chiều cao dòng theo số sách để không cắt chữ
            int soDong = Math.Max(2, (q.DanhSachSach ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
            dgv.Rows[i].Height = Math.Max(DpiHelper.Px(dgv, 64), soDong * hDong + DpiHelper.Px(dgv, 20));
        }
        dgv.ResumeLayout(true);
        lblTrong.Visible = kq.DuLieu.Count == 0;
        lblChipSoPhieu.Text = $"• {kq.TongSoDong} phiếu quá hạn";
        lblHienThi.Text = kq.TongSoDong == 0 ? "Không có kết quả" : $"Hiển thị {kq.TuDong}–{kq.DenDong} / {kq.TongSoDong.ToString("N0", Vi)} kết quả";
        pagination.DatTrang(kq.TrangHienTai, kq.TongSoTrang);
        UiForm.CanChieuCao(cardLuoi, 1, dgv, 60);
    }

    public void HienThiThongKeThang(int thang, int nam, int phatSinh, int daThuHoi, List<KeyValuePair<string, int>> cacThang)
    {
        lblThongKeTieuDe.Text = $"Thống kê tháng {thang:00}/{nam}";
        lblPhatSinh.Text = phatSinh.ToString("00");
        lblThuHoi.Text = daThuHoi.ToString("00");
        lblTheoDoi.Text = Math.Max(0, phatSinh - daThuHoi).ToString("00");
        chart.DatDuLieu(cacThang.Select(k => new CotBieuDo(k.Key, k.Value)));
    }

    public void XuatExcel(string tenGoiY, string tieuDe, string[] cot, List<string[]> dong)
    {
        try
        {
            string tep = QuanLyThuVien.XuatExcel.Xuat(this, tenGoiY, tieuDe, cot, dong);
            if (tep != null) HienThiThongTin("Đã xuất báo cáo ra tệp:\n" + tep);
        }
        catch (IOException) { HienThiLoi("Không thể ghi tệp. Hãy đóng tệp Excel nếu đang mở rồi thử lại."); }
        catch (UnauthorizedAccessException) { HienThiLoi("Không có quyền ghi vào vị trí đã chọn. Vui lòng chọn thư mục khác."); }
    }

    // In danh sách (khổ A4 ngang, nhiều trang nếu dài)
    public void InDanhSach(List<QuaHanHienThi> ds, DateTime ngay)
    {
        using var fTieuDe = new Font("Segoe UI", 14f, FontStyle.Bold);
        using var fDam = new Font("Segoe UI", 9f, FontStyle.Bold);
        using var fThuong = new Font("Segoe UI", 9f);
        int daIn = 0;
        InHelper.InNhieuTrang(this, "Báo cáo quá hạn", (g, v, trang) =>
        {
            g.DrawString("DANH SÁCH SÁCH QUÁ HẠN - THƯ VIỆN HCMUE", fTieuDe, Brushes.Black, v.Left, v.Top);
            g.DrawString($"Tính đến ngày {ngay:dd/MM/yyyy} · Tổng {ds.Count} phiếu · Trang {trang + 1}", fThuong, Brushes.Gray, v.Left, v.Top + 32);
            float[] x = { 0, 0.14f, 0.34f, 0.72f, 0.82f, 0.92f };
            string[] tdct = { "Phiếu", "Độc giả", "Sách chưa trả", "Hẹn trả", "Trễ", "Số cuốn" };
            float y = v.Top + 64;
            for (int i = 0; i < 6; i++) g.DrawString(tdct[i], fDam, Brushes.Black, v.Left + v.Width * x[i], y);
            y += 22;
            g.DrawLine(Pens.Black, v.Left, y, v.Right, y);
            y += 4;
            while (daIn < ds.Count)
            {
                var q = ds[daIn];
                var sach = (q.DanhSachSach ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
                float cao = Math.Max(2, sach.Length) * (fThuong.GetHeight(g) + 1) + 6;
                if (y + cao > v.Bottom) return true;     // hết chỗ: sang trang mới
                g.DrawString(q.MaPhieu + "\n" + q.NgayMuon.ToString("dd/MM/yyyy"), fThuong, Brushes.Black, v.Left, y);
                g.DrawString(q.HoTen + "\n" + q.MaDocGia + " · " + q.DienThoai, fThuong, Brushes.Black, v.Left + v.Width * x[1], y);
                g.DrawString(string.Join("\n", sach), fThuong, Brushes.Black, v.Left + v.Width * x[2], y);
                g.DrawString(q.HenTra.ToString("dd/MM/yyyy"), fThuong, Brushes.Black, v.Left + v.Width * x[3], y);
                g.DrawString(q.SoNgayTre + " ngày", fThuong, Brushes.Black, v.Left + v.Width * x[4], y);
                g.DrawString(q.SoCuon.ToString(), fThuong, Brushes.Black, v.Left + v.Width * x[5], y);
                y += cao;
                g.DrawLine(Pens.LightGray, v.Left, y - 2, v.Right, y - 2);
                daIn++;
            }
            return false;
        }, ngang: true);
    }

    public void DatTrangThaiDangXuLy(bool dangXuLy)
    {
        if (IsDisposed) return;
        UseWaitCursor = dangXuLy;
        btnLoc.Enabled = btnXuat.Enabled = btnIn.Enabled = !dangXuLy;
    }

    public void HienThiThongTin(string thongBao) => MessageBox.Show(this, thongBao, "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public void HienThiLoi(string thongBao) => MessageBox.Show(this, thongBao, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    #endregion

}

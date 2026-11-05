using System.Globalization;
using DTO;
using DTO.Models;
using QuanLyThuVien.Controls;
using QuanLyThuVien.Presenters;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.UserControls;

/// <summary>
/// Màn hình 6 – Lập phiếu mượn. Bên trái: thông tin mượn, tìm &amp; thêm sách, sách trong phiếu.
/// Bên phải: panel "Kiểm tra phiếu" (danh sách điều kiện, lỗi, nút tạo phiếu). Chỉ hiển thị, nghiệp vụ nằm ở LapPhieuPresenter/BLL.
/// </summary>
public partial class ucLapPhieuMuon : UserControl, ILapPhieuView
{
    private static readonly CultureInfo Vi = new("vi-VN");

    private bool _dangNap;

    private readonly LapPhieuPresenter _presenter;

    #region ILapPhieuView – thuộc tính và sự kiện
    public string MaDocGiaChon => (cboDocGia.SelectedItem as LuaChonDocGia)?.Ma;
    public DateTime NgayMuon => dtpNgayMuon.Value.Date;
    public DateTime HenTra => dtpHenTra.Value.Date;
    public string GhiChu => txtGhiChu.Text.Trim();
    public string TuKhoaSach => txtTimSach.Text.Trim();

    public event EventHandler Loaded;
    public event EventHandler DocGiaChanged;
    public event EventHandler NgayMuonChanged;
    public event EventHandler HenTraChanged;
    public event EventHandler GhiChuChanged;
    public event EventHandler TimSachClicked;
    public event EventHandler<string> ThemSachClicked;
    public event EventHandler<ThayDoiSoLuongArgs> DoiSoLuongClicked;
    public event EventHandler<string> XoaSachClicked;
    public event EventHandler ThemTuDanhMucClicked;
    public event EventHandler TaoPhieuClicked;
    public event EventHandler HuyClicked;
    #endregion

    public ucLapPhieuMuon(IDieuHuong dieuHuong)
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không tạo Presenter
        GanSuKien();
        _presenter = new LapPhieuPresenter(this, dieuHuong);
    }

    #region Gắn sự kiện và phần không biểu diễn được trong Designer
    // Giao diện tĩnh nằm trong ucLapPhieuMuon.Designer.cs; ở đây gắn sự kiện và giá trị ngày mặc định.
    // Các dòng danh sách (kết quả tìm, sách trong phiếu, mục kiểm tra) được tạo động theo dữ liệu bên dưới.
    private void GanSuKien()
    {
        dtpNgayMuon.Value = DateTime.Today;
        dtpHenTra.Value = DateTime.Today.AddDays(HangSo.SoNgayMuonMacDinh);
        lblTomTat.Text = "";

        cboDocGia.SelectedIndexChanged += (s, e) => { if (!_dangNap) DocGiaChanged?.Invoke(this, EventArgs.Empty); };
        dtpNgayMuon.ValueChanged += (s, e) => { if (!_dangNap) NgayMuonChanged?.Invoke(this, EventArgs.Empty); };
        dtpHenTra.ValueChanged += (s, e) => { if (!_dangNap) HenTraChanged?.Invoke(this, EventArgs.Empty); };
        txtGhiChu.TextChanged += (s, e) => { if (!_dangNap) GhiChuChanged?.Invoke(this, EventArgs.Empty); };
        txtTimSach.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; TimSachClicked?.Invoke(this, EventArgs.Empty); } };
        btnTimSach.Click += (s, e) => TimSachClicked?.Invoke(this, EventArgs.Empty);
        btnThemDanhMuc.Click += (s, e) => ThemTuDanhMucClicked?.Invoke(this, EventArgs.Empty);
        btnTao.Click += (s, e) => TaoPhieuClicked?.Invoke(this, EventArgs.Empty);
        btnHuy.Click += (s, e) => HuyClicked?.Invoke(this, EventArgs.Empty);

        Load += (s, e) =>
        {
            stepper.Height = DpiHelper.Px(this, 56);
            Loaded?.Invoke(this, EventArgs.Empty);
        };
    }
    #endregion

    #region ILapPhieuView – hiển thị
    public void NapDocGia(List<LuaChonDocGia> ds)
    {
        _dangNap = true;
        try { cboDocGia.Items.Clear(); cboDocGia.Items.AddRange(ds.ToArray()); cboDocGia.SelectedIndex = -1; }
        finally { _dangNap = false; }
    }

    public void ChonDocGia(string ma)
    {
        _dangNap = true;
        try { for (int i = 0; i < cboDocGia.Items.Count; i++) if (((LuaChonDocGia)cboDocGia.Items[i]).Ma == ma) { cboDocGia.SelectedIndex = i; break; } }
        finally { _dangNap = false; }
    }

    public void DatGhiChu(string ghiChu) { _dangNap = true; try { txtGhiChu.Text = ghiChu ?? ""; } finally { _dangNap = false; } }
    public void DatHenTra(DateTime ngay) { _dangNap = true; try { dtpHenTra.Value = ngay; } finally { _dangNap = false; } }
    public void HienThiMaPhieu(string ma) { lblChipPhieu.Text = "Dự kiến: " + ma; }
    public void HienThiBuoc(int soBuoc) => stepper.SoBuocHoanThanh = soBuoc;

    public void HienThiThongTinDocGia(KetQuaKiemTraDocGia kq)
    {
        flpChipDocGia.SuspendLayout();
        flpChipDocGia.Controls.Clear();
        if (kq == null) { lblCanhBaoDocGia.Visible = false; flpChipDocGia.ResumeLayout(); return; }
        flpChipDocGia.Controls.Add(UiForm.TaoChip(kq.DocGia.NhomDocGia ?? "Độc giả", UiTheme.XanhChinh));
        flpChipDocGia.Controls.Add(UiForm.TaoChip(kq.TheConHan ? "Thẻ còn hạn" : "Thẻ hết hạn", kq.TheConHan ? UiTheme.XanhLa : UiTheme.Do));
        flpChipDocGia.Controls.Add(UiForm.TaoChip($"Đang mượn {kq.SoDangMuon}/{kq.HanMuc} cuốn", kq.SoDangMuon >= kq.HanMuc ? UiTheme.Do : UiTheme.ChuPhu));
        if (kq.CoQuaHan) flpChipDocGia.Controls.Add(UiForm.TaoChip("Có phiếu quá hạn", UiTheme.Do));
        flpChipDocGia.ResumeLayout();
        lblCanhBaoDocGia.Text = kq.LyDo;
        lblCanhBaoDocGia.Visible = kq.LyDo != null;
    }

    // Tạo một dòng danh sách: thông tin bên trái (tên + mô tả), điều khiển bên phải
    private TableLayoutPanel TaoDong(string tren, string duoi, Color mauDuoi, params Control[] phai)
    {
        var d = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, RowCount = 1, Margin = new Padding(0), Padding = new Padding(0, 8, 0, 8) };
        d.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        d.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        d.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var trai = UiTheme.TaoBangMotCot();
        trai.AutoSize = true;
        trai.Dock = DockStyle.Fill;
        var l1 = UiTheme.TaoNhan(tren, 9.5f, FontStyle.Bold);
        var l2 = UiTheme.TaoNhan(duoi, 8.5f, FontStyle.Regular, mauDuoi);
        l2.Margin = new Padding(0, 2, 0, 0);
        UiTheme.ThemHang(trai, l1);
        UiTheme.ThemHang(trai, l2);
        d.Controls.Add(trai, 0, 0);
        var fl = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Anchor = AnchorStyles.Right, Margin = new Padding(8, 0, 0, 0) };
        foreach (var c in phai) fl.Controls.Add(c);
        d.Controls.Add(fl, 1, 0);
        d.Paint += (s, e) => { using var pen = new Pen(UiTheme.VienNhat); e.Graphics.DrawLine(pen, 0, d.Height - 1, d.Width, d.Height - 1); };
        return d;
    }

    public void HienThiKetQuaTim(List<SachHienThi> ds)
    {
        tlpKetQuaTim.SuspendLayout();
        tlpKetQuaTim.Controls.Clear();
        tlpKetQuaTim.RowStyles.Clear();
        tlpKetQuaTim.RowCount = 0;
        if (ds == null || ds.Count == 0)
        {
            lblTrongKetQua.Text = "Không tìm thấy sách phù hợp.";
            UiTheme.ThemHang(tlpKetQuaTim, lblTrongKetQua);
        }
        else
        {
            foreach (var s in ds)
            {
                bool het = s.SoBanCoSan <= 0 || s.TinhTrangLuuHanh == HangSo.LuuHanhDocTaiCho;
                var lnk = UiTheme.TaoLienKet(het ? "Không thể mượn" : "+ Thêm", 9f);
                lnk.Enabled = !het;
                string ma = s.MaSach;
                lnk.LinkClicked += (o, e) => ThemSachClicked?.Invoke(this, ma);
                string mota = $"{s.MaSach} · {s.TenTacGia} · còn {s.SoBanCoSan}/{s.TongSoBan} bản" + (s.TinhTrangLuuHanh == HangSo.LuuHanhDocTaiCho ? " · chỉ đọc tại chỗ" : "");
                UiTheme.ThemHang(tlpKetQuaTim, TaoDong(s.TenSach, mota, UiTheme.ChuPhu, lnk));
            }
        }
        tlpKetQuaTim.ResumeLayout();
    }

    public void HienThiDongPhieu(List<DongPhieuHienThi> ds)
    {
        tlpDongPhieu.SuspendLayout();
        tlpDongPhieu.Controls.Clear();
        tlpDongPhieu.RowStyles.Clear();
        tlpDongPhieu.RowCount = 0;
        if (ds == null || ds.Count == 0) UiTheme.ThemHang(tlpDongPhieu, lblTrongPhieu);
        else
        {
            foreach (var d in ds)
            {
                string ma = d.MaSach;
                var btnGiam = TaoNutNho("−"); var btnTang = TaoNutNho("+"); var btnXoa = TaoNutNho("✕");
                var lblSl = UiTheme.TaoNhan(d.SoLuong.ToString(), 10f, FontStyle.Bold);
                lblSl.Margin = new Padding(8, 6, 8, 0);
                btnGiam.Enabled = d.SoLuong > 1;
                btnTang.Enabled = d.SoLuong < d.Con;                 // không cho vượt tồn kho
                btnGiam.Click += (o, e) => DoiSoLuongClicked?.Invoke(this, new ThayDoiSoLuongArgs { MaSach = ma, Delta = -1 });
                btnTang.Click += (o, e) => DoiSoLuongClicked?.Invoke(this, new ThayDoiSoLuongArgs { MaSach = ma, Delta = 1 });
                btnXoa.Click += (o, e) => XoaSachClicked?.Invoke(this, ma);
                btnXoa.Margin = new Padding(12, 0, 0, 0);
                string mota = $"{d.MaSach} · {d.TenTacGia} · còn {d.Con}/{d.Tong} bản" + (d.Loi != null ? " · " + d.Loi : "");
                UiTheme.ThemHang(tlpDongPhieu, TaoDong(d.TenSach, mota, d.Loi != null ? UiTheme.Do : UiTheme.ChuPhu, btnGiam, lblSl, btnTang, btnXoa));
            }
        }
        tlpDongPhieu.ResumeLayout();
    }

    private static Button TaoNutNho(string chu)
    {
        var b = UiTheme.TaoNut(chu, false);
        b.Padding = new Padding(6, 2, 6, 2);
        b.MinimumSize = new Size(DpiHelper.Px(b, 32), DpiHelper.Px(b, 30));
        b.Margin = new Padding(0);
        return b;
    }

    public void HienThiKiemTra(KiemTraPhieuHienThi kt)
    {
        lblTenDocGia.Text = kt.TenDocGia;
        lblTomTat.Text = $"{kt.SoDauSach} đầu sách · {kt.TongCuon} cuốn · hạn mức {kt.HanMuc} cuốn · mượn {kt.SoNgay} ngày";
        tlpMucKiemTra.SuspendLayout();
        tlpMucKiemTra.Controls.Clear();
        tlpMucKiemTra.RowStyles.Clear();
        tlpMucKiemTra.RowCount = 0;
        foreach (var m in kt.Muc)
        {
            var l = UiTheme.TaoNhan((m.Dat ? "✓  " : "✗  ") + m.Nhan, 9.5f, FontStyle.Regular, m.Dat ? UiTheme.XanhLa : UiTheme.Do);
            l.Margin = new Padding(0, 0, 0, 6);
            UiTheme.ThemHang(tlpMucKiemTra, l);
        }
        tlpMucKiemTra.ResumeLayout();
        hopLoi.Text = kt.LoiChinh; hopLoi.Visible = kt.LoiChinh != null;
        hopDat.Text = kt.ThongBaoSan; hopDat.Visible = kt.ThongBaoSan != null;
        btnTao.Enabled = kt.ChoPhepTao;
    }

    public bool XacNhan(string cauHoi) =>
        MessageBox.Show(this, cauHoi, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public void DatTrangThaiDangXuLy(bool dangXuLy)
    {
        if (IsDisposed) return;
        UseWaitCursor = dangXuLy;
        if (dangXuLy) btnTao.Enabled = false;
        btnTimSach.Enabled = btnHuy.Enabled = !dangXuLy;
    }

    public void HienThiThongTin(string thongBao) => MessageBox.Show(this, thongBao, "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public void HienThiLoi(string thongBao) => MessageBox.Show(this, thongBao, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    #endregion
}

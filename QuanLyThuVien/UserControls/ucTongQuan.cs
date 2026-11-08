using System.Globalization;
using DTO;
using DTO.Models;
using QuanLyThuVien.Controls;
using QuanLyThuVien.Presenters;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.UserControls;

/// <summary>
/// Màn hình 2 – Tổng quan thư viện. 4 thẻ thống kê, biểu đồ lượt mượn theo tháng (GDI+), khối "Việc cần xử lý"
/// và bảng 4 phiếu mượn gần đây. Chỉ hiển thị; dữ liệu do TongQuanPresenter nạp.
/// </summary>
public partial class ucTongQuan : UserControl, ITongQuanView
{
    private static readonly CultureInfo Vi = new("vi-VN");

    private Font _fontDam;

    public event EventHandler Loaded;
    public event EventHandler<MucMenu> DiDenClicked;
    public event EventHandler<string> ChiTietPhieuClicked;
    public event EventHandler<string> TraSachClicked;

    private readonly TongQuanPresenter _presenter;

    public ucTongQuan(IDieuHuong dieuHuong)
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không tạo Presenter
        GanSuKien();
        _presenter = new TongQuanPresenter(this, dieuHuong);
    }

    #region Gắn sự kiện và phần không biểu diễn được trong Designer
    // Giao diện tĩnh nằm trong ucTongQuan.Designer.cs; ở đây chỉ gắn sự kiện, cấu hình lưới và nội dung phụ thuộc thời điểm chạy.
    private void GanSuKien()
    {
        _fontDam = DpiHelper.TaoFont(9.5f, FontStyle.Bold);
        BackColor = UiTheme.NenManHinh;

        int nam = DateTime.Today.Year;
        tdkBieuDo.MoTa = $"Tổng lượt sách được mượn trong năm {nam}";
        chipNam.Text = "• Năm " + nam;
        lblGhiChuBieuDo.Text = "■ Lượt sách mượn · cập nhật đến " + DateTime.Today.ToString("dd/MM/yyyy");

        btnLap.Click += (s, e) => DiDenClicked?.Invoke(this, MucMenu.LapPhieuMuon);
        lnkXemThongKe.LinkClicked += (s, e) => DiDenClicked?.Invoke(this, MucMenu.BaoCaoQuaHan);
        lnkQuaHan.LinkClicked += (s, e) => DiDenClicked?.Invoke(this, MucMenu.BaoCaoQuaHan);
        lnkSapHan.LinkClicked += (s, e) => DiDenClicked?.Invoke(this, MucMenu.DocGia);
        lnkKiemTra.LinkClicked += (s, e) => DiDenClicked?.Invoke(this, MucMenu.QuanLySach);
        lnkTatCa.LinkClicked += (s, e) => DiDenClicked?.Invoke(this, MucMenu.PhieuMuon);
        // Đường kẻ dưới mỗi mục "Việc cần xử lý"
        foreach (var b in new[] { tlpQuaHan, tlpSapHan, tlpKiemTra })
        {
            var muc = b;
            muc.Paint += (s, e) => { using var pen = new Pen(UiTheme.VienNhat); e.Graphics.DrawLine(pen, 0, muc.Height - 1, muc.Width, muc.Height - 1); };
        }

        dgv.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == colTrangThai.Index)
            {
                string tt = Convert.ToString(e.Value);
                e.CellStyle.ForeColor = tt == HangSo.PhieuQuaHan ? UiTheme.Do : tt == HangSo.PhieuDaTra ? UiTheme.XanhLa : tt == HangSo.PhieuTraMotPhan ? UiTheme.Cam : UiTheme.XanhChinh;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                e.CellStyle.Font = _fontDam;
            }
            else if (e.ColumnIndex == colMa.Index) { e.CellStyle.ForeColor = UiTheme.XanhChinh; e.CellStyle.Font = _fontDam; }
        };
        dgv.CellContentClick += (s, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != colThaoTac.Index || dgv.Rows[e.RowIndex].Tag is not PhieuMuonHienThi p) return;
            if (p.SoCuonConMuon > 0 && p.TrangThaiHienThi != HangSo.PhieuDangMuon) TraSachClicked?.Invoke(this, p.MaPhieu);
            else ChiTietPhieuClicked?.Invoke(this, p.MaPhieu);
        };
        dgv.RowHeightChanged += (s, e) => UiForm.CanChieuCao(cardGanDay, 1, dgv, 60);

        Load += (s, e) =>
        {
            DpiHelper.CauHinhDataGridView(dgv, 52, 40);
            DpiHelper.DatDoRongToiThieuCot(colMa, 110);
            DpiHelper.DatDoRongToiThieuCot(colDocGia, 150);
            DpiHelper.DatDoRongToiThieuCot(colNgay, 90);
            DpiHelper.DatDoRongToiThieuCot(colHen, 90);
            DpiHelper.DatDoRongToiThieuCot(colCuon, 60);
            DpiHelper.DatDoRongToiThieuCot(colTrangThai, 100);
            DpiHelper.DatDoRongToiThieuCot(colThaoTac, 100);
            Loaded?.Invoke(this, EventArgs.Empty);
        };
        Disposed += (s, e) => _fontDam?.Dispose();
    }
    #endregion

    #region ITongQuanView – hiển thị
    public void HienThiLoiChao(string cau) => tieuDeTrang.MoTa = cau;

    public void HienThiTongQuan(ThongKeTongQuan tq)
    {
        cardSach.GiaTri = tq.Sach.SoDauSach.ToString("N0", Vi);
        cardSach.GhiChu = $"{tq.Sach.SoBanSach.ToString("N0", Vi)} bản sách · {tq.SoTheLoai} thể loại";
        cardDocGia.GiaTri = tq.DocGia.DangHoatDong.ToString("N0", Vi);
        cardDocGia.GhiChu = $"+{tq.DocGiaMoiTrongThang} độc giả trong tháng {DateTime.Today.Month}";
        cardPhieu.GiaTri = tq.Phieu.DungHan.ToString("N0", Vi);
        cardPhieu.GhiChu = $"{tq.Phieu.CuonLuuHanh.ToString("N0", Vi)} cuốn đang được lưu hành";
        cardQuaHan.GiaTri = tq.Phieu.QuaHan.ToString("N0", Vi);
        cardQuaHan.GhiChu = $"{tq.Phieu.CuonQuaHan} cuốn cần liên hệ thu hồi";

        lblTongLuot.Text = tq.TongLuotMuonNam.ToString("N0", Vi);
        chart.DatDuLieu(tq.LuotMuonTheoThang.Select(t => new CotBieuDo("T" + t.Thang, t.GiaTri)), DateTime.Today.Month - 1);

        lblChipViec.Text = $"• {tq.SoViecCanXuLy} việc";
        lblSoQuaHan.Text = tq.Phieu.QuaHan.ToString("00");
        lblSoSapHan.Text = tq.DocGia.SapHetHan.ToString("00");
        lblSoKiemTra.Text = (tq.SachHuHong + tq.SachMat).ToString("00");
        lblMoTaKiemTra.Text = $"{tq.SachHuHong} hư hỏng · {tq.SachMat} báo mất";
    }

    public void HienThiPhieuGanDay(List<PhieuMuonHienThi> ds)
    {
        dgv.SuspendLayout();
        dgv.Rows.Clear();
        foreach (var p in ds)
        {
            string thaoTac = p.SoCuonConMuon > 0 && p.TrangThaiHienThi != HangSo.PhieuDangMuon ? "Ghi nhận trả" : "Chi tiết →";
            int i = dgv.Rows.Add(p.MaPhieu, p.HoTenDocGia, p.NgayMuon?.ToString("dd/MM/yyyy"), p.HenTra?.ToString("dd/MM/yyyy"), p.TongSoCuon, p.TrangThaiHienThi, thaoTac);
            dgv.Rows[i].Tag = p;
        }
        dgv.ResumeLayout(true);
        UiForm.CanChieuCao(cardGanDay, 1, dgv, 60);
    }

    public void DatTrangThaiDangXuLy(bool dangXuLy) { if (!IsDisposed) UseWaitCursor = dangXuLy; }
    public void HienThiThongTin(string thongBao) => MessageBox.Show(this, thongBao, "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public void HienThiLoi(string thongBao) => MessageBox.Show(this, thongBao, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    #endregion

}

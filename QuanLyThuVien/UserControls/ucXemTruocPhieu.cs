using DTO.Models;
using QuanLyThuVien.Controls;
using QuanLyThuVien.Presenters;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.UserControls;

/// <summary>
/// Màn hình 10 – Xem trước phiếu mượn. Bên trái: tờ giấy A4 xem trước (vẽ bằng PhieuInVe, giống bản in).
/// Bên phải: thiết lập bản in (máy in, khổ giấy, hướng, số bản, ghi chú) + nút In / Lưu PDF.
/// </summary>
public partial class ucXemTruocPhieu : UserControl, IXemTruocPhieuView, INhanThamSo
{
    private bool _daLoad, _dangNap;
    private string _maCho;

    private readonly XemTruocPhieuPresenter _presenter;

    #region IXemTruocPhieuView
    public CauHinhIn CauHinh => new()
    {
        MayIn = cboMayIn.Text, KhoA5 = cboKho.SelectedIndex == 1, NgangGiay = cboHuong.SelectedIndex == 1,
        SoBan = (int)nudSoBan.Value, GomGhiChu = chkGhiChu.Checked
    };

    public event EventHandler Loaded;
    public event EventHandler<string> MoPhieu;
    public event EventHandler QuayLaiClicked;
    public event EventHandler InClicked;
    public event EventHandler LuuPdfClicked;
    public event EventHandler CauHinhThayDoi;
    #endregion

    public ucXemTruocPhieu(IDieuHuong dieuHuong)
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // chế độ thiết kế: không tạo Presenter
        GanSuKien();
        _presenter = new XemTruocPhieuPresenter(this, dieuHuong);
    }

    public void NhanThamSo(object thamSo) { _maCho = thamSo as string; PhatMaCho(); }

    private void PhatMaCho()
    {
        if (!_daLoad || string.IsNullOrEmpty(_maCho)) return;
        string ma = _maCho; _maCho = null;
        MoPhieu?.Invoke(this, ma);
    }

    #region Gắn sự kiện và phần không biểu diễn được trong Designer
    // Giao diện tĩnh nằm trong ucXemTruocPhieu.Designer.cs; ở đây gắn sự kiện, giữ tỉ lệ tờ giấy xem trước theo hướng giấy.
    private void GanSuKien()
    {
        btnQuayLai.Click += (s, e) => QuayLaiClicked?.Invoke(this, EventArgs.Empty);
        btnIn.Click += (s, e) => InClicked?.Invoke(this, EventArgs.Empty);
        btnInPhai.Click += (s, e) => InClicked?.Invoke(this, EventArgs.Empty);
        btnLuuPdf.Click += (s, e) => LuuPdfClicked?.Invoke(this, EventArgs.Empty);

        // Tờ giấy: chiều cao = chiều rộng x tỉ lệ giấy (A4/A5 cùng tỉ lệ; ngang thì đảo)
        void CanChieuCao()
        {
            int rong = theGiay.ClientSize.Width - theGiay.Padding.Horizontal;
            if (rong <= 0) return;
            bool ngang = cboHuong.SelectedIndex == 1;
            int cao = (int)(rong * (ngang ? 1 / 1.414 : 1.414)) + 1;
            theGiay.RowStyles[0].Height = cao;
            theGiay.Height = cao + theGiay.Padding.Vertical;
        }
        theGiay.SizeChanged += (s, e) => CanChieuCao();
        CauHinhThayDoi += (s, e) => CanChieuCao();

        void Doi(object s, EventArgs e)
        {
            if (_dangNap) return;
            nudSoBan_Doi();
            CauHinhThayDoi?.Invoke(this, EventArgs.Empty);
            trang.DatNoiDung(_phieuHienTai, chkGhiChu.Checked, cboHuong.SelectedIndex == 1);
        }
        cboKho.SelectedIndexChanged += Doi; cboHuong.SelectedIndexChanged += Doi; chkGhiChu.CheckedChanged += Doi; nudSoBan.ValueChanged += Doi;

        Load += (s, e) => { _daLoad = true; Loaded?.Invoke(this, EventArgs.Empty); PhatMaCho(); };
    }
    #endregion

    private PhieuMuonDayDu _phieuHienTai;
    private void nudSoBan_Doi() => btnInPhai.Text = $"In {(int)nudSoBan.Value} bản";

    #region IXemTruocPhieuView – hiển thị
    public void HienThiPhieu(PhieuMuonDayDu p)
    {
        _phieuHienTai = p;
        int dauSach = p.ChiTiet.Select(c => c.MaSach).Distinct().Count();
        tieuDeTrang.MoTa = $"Phiếu {p.Phieu.MaPhieu} · {dauSach} đầu sách, {p.ChiTiet.Count} cuốn · Bản in dành cho độc giả và thư viện.";
        trang.DatNoiDung(p, chkGhiChu.Checked, cboHuong.SelectedIndex == 1);
        CauHinhThayDoi?.Invoke(this, EventArgs.Empty);
        lblGhiChuDuoi.Text = "Bản xem trước thể hiện đúng nội dung sẽ in. Đặt logo tại Resources\\logo_hcmue.png để hiển thị logo trường.";
    }

    public void NapMayIn(List<string> ds, string macDinh)
    {
        _dangNap = true;
        try
        {
            cboMayIn.Items.Clear();
            cboMayIn.Items.AddRange(ds.ToArray());
            int i = cboMayIn.FindStringExact(macDinh ?? "");
            cboMayIn.SelectedIndex = i >= 0 ? i : (ds.Count > 0 ? 0 : -1);
        }
        finally { _dangNap = false; }
    }

    public string ChonNoiLuuPdf(string tenGoiY)
    {
        using var hop = new SaveFileDialog { Title = "Lưu phiếu mượn PDF", Filter = "Tệp PDF (*.pdf)|*.pdf", FileName = tenGoiY, DefaultExt = "pdf", AddExtension = true, OverwritePrompt = true };
        return hop.ShowDialog(this) == DialogResult.OK ? hop.FileName : null;
    }

    public void DatTrangThaiDangXuLy(bool dangXuLy) { if (!IsDisposed) UseWaitCursor = dangXuLy; }
    public void HienThiThongTin(string thongBao) => MessageBox.Show(this, thongBao, "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public void HienThiLoi(string thongBao) => MessageBox.Show(this, thongBao, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    #endregion
}

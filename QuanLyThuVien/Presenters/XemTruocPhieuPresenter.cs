using System.Drawing.Printing;
using BLL;
using DTO.Models;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>Presenter màn hình Xem trước phiếu mượn: tải phiếu đầy đủ, liệt kê máy in, in và lưu PDF (Microsoft Print to PDF).</summary>
public class XemTruocPhieuPresenter
{
    private readonly IXemTruocPhieuView _v;
    private readonly IDieuHuong _nav;
    private PhieuMuonDayDu _phieu;

    public XemTruocPhieuPresenter(IXemTruocPhieuView view, IDieuHuong dieuHuong)
    {
        _v = view;
        _nav = dieuHuong;
        _v.Loaded += (s, e) =>
        {
            var ds = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
            _v.NapMayIn(ds, new PrinterSettings().PrinterName);
        };
        _v.MoPhieu += async (s, ma) => await Chay(async () =>
        {
            _phieu = await Task.Run(() => PhieuMuonBLL.LayDayDu(ma));
            _v.HienThiPhieu(_phieu);
        });
        _v.QuayLaiClicked += (s, e) => _nav.DieuHuong(MucMenu.PhieuMuon);
        _v.InClicked += (s, e) => Chay(In);
        _v.LuuPdfClicked += (s, e) => Chay(LuuPdf);
    }

    private Task Chay(Func<Task> viec) => PresenterHelper.ChayAn(_v, viec);
    private Task Chay(Action viec) => PresenterHelper.ChayAn(_v, () => { viec(); return Task.CompletedTask; });

    private void In()
    {
        if (_phieu == null) throw new NghiepVuException("Chưa có phiếu để in.");
        try { PhieuInVe.In(_phieu, _v.CauHinh); }
        catch (InvalidPrinterException) { throw new NghiepVuException("Không tìm thấy máy in đã chọn. Vui lòng chọn máy in khác."); }
        catch (InvalidOperationException ex) { throw new NghiepVuException(ex.Message); }
        _v.HienThiThongTin("Đã gửi phiếu " + _phieu.Phieu.MaPhieu + " tới máy in.");
    }

    private void LuuPdf()
    {
        if (_phieu == null) throw new NghiepVuException("Chưa có phiếu để lưu.");
        bool coPdf = PrinterSettings.InstalledPrinters.Cast<string>().Any(m => m.Equals("Microsoft Print to PDF", StringComparison.OrdinalIgnoreCase));
        if (!coPdf) throw new NghiepVuException("Máy chưa có \"Microsoft Print to PDF\". Hãy bật tính năng này trong Windows hoặc chọn In rồi chọn máy in PDF khác.");
        string tep = _v.ChonNoiLuuPdf("PhieuMuon_" + _phieu.Phieu.MaPhieu);
        if (tep == null) return;
        try { PhieuInVe.In(_phieu, _v.CauHinh, tep); }
        catch (InvalidOperationException ex) { throw new NghiepVuException(ex.Message); }
        _v.HienThiThongTin("Đã lưu PDF:\n" + tep);
    }
}

using BLL;
using DTO.Models;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>Presenter màn hình Tổng quan: lời chào, thống kê, biểu đồ lượt mượn theo tháng, việc cần xử lý, 4 phiếu mượn gần đây.</summary>
public class TongQuanPresenter
{
    private readonly ITongQuanView _v;
    private readonly IDieuHuong _nav;

    public TongQuanPresenter(ITongQuanView view, IDieuHuong dieuHuong)
    {
        _v = view;
        _nav = dieuHuong;
        _v.Loaded += async (s, e) => await PresenterHelper.ChayAn(_v, TaiDuLieu);
        _v.DiDenClicked += (s, muc) => _nav.DieuHuong(muc);
        _v.ChiTietPhieuClicked += (s, ma) => _nav.DieuHuong(MucMenu.XemTruocPhieu, ma);
        _v.TraSachClicked += (s, ma) => _nav.DieuHuong(MucMenu.GhiNhanTraSach, ma);
    }

    private async Task TaiDuLieu()
    {
        int gio = DateTime.Now.Hour;
        string buoi = gio < 11 ? "buổi sáng" : gio < 13 ? "buổi trưa" : gio < 18 ? "buổi chiều" : "buổi tối";
        string ten = PhienDangNhap.NguoiDungHienTai?.HoTen ?? "bạn";
        _v.HienThiLoiChao($"Chào {buoi}, {ten}. Cùng bắt đầu một ngày làm việc hiệu quả.");

        _v.DatTrangThaiDangXuLy(true);
        try
        {
            var tq = await Task.Run(() => ThongKeBLL.LayTongQuan());
            var gan = await Task.Run(() => PhieuMuonBLL.TimKiem(new PhieuBoLoc(), 1, 4));
            _v.HienThiTongQuan(tq);
            _v.HienThiPhieuGanDay(gan.DuLieu);
        }
        finally { _v.DatTrangThaiDangXuLy(false); }
    }
}

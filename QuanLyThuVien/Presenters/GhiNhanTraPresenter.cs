using BLL;
using DTO.Models;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>Presenter màn hình Ghi nhận trả sách: tải phiếu, tính tóm tắt khi thay đổi lựa chọn, xác nhận trả (trả một phần hoặc toàn bộ).</summary>
public class GhiNhanTraPresenter
{
    private readonly IGhiNhanTraView _v;
    private readonly IDieuHuong _nav;
    private PhieuMuonDayDu _phieu;

    public GhiNhanTraPresenter(IGhiNhanTraView view, IDieuHuong dieuHuong)
    {
        _v = view;
        _nav = dieuHuong;
        _v.TaiPhieuClicked += async (s, e) => await Chay(() => TaiPhieu(_v.MaPhieuNhap));
        _v.MoPhieu += async (s, ma) => await Chay(() => TaiPhieu(ma));
        _v.VeDanhSachClicked += (s, e) => _nav.DieuHuong(MucMenu.PhieuMuon);
        _v.LuaChonThayDoi += (s, e) => CapNhatTomTat();
        _v.XacNhanClicked += async (s, e) => await Chay(XacNhanTra);
    }

    private Task Chay(Func<Task> viec) => PresenterHelper.ChayAn(_v, viec);

    // Tải phiếu theo mã; phiếu đã trả đủ sẽ bị BLL từ chối kèm thông báo
    private async Task TaiPhieu(string ma)
    {
        _v.DatTrangThaiDangXuLy(true);
        try
        {
            PhieuMuonDayDu p;
            try { p = await Task.Run(() => TraSachBLL.LayPhieuDeTra(ma)); }
            catch { _phieu = null; _v.XoaPhieu(); throw; }
            _phieu = p;
            _v.HienThiPhieu(p);
            CapNhatTomTat();
        }
        finally { _v.DatTrangThaiDangXuLy(false); }
    }

    private void CapNhatTomTat()
    {
        if (_phieu == null) return;
        var chon = _v.DanhSachChon;
        _v.HienThiTomTat(_phieu, TraSachBLL.TomTat(_phieu, chon), chon.Count);
    }

    private async Task XacNhanTra()
    {
        if (_phieu == null) throw new NghiepVuException("Vui lòng nhập mã phiếu và tải phiếu trước.");
        var chon = _v.DanhSachChon;
        var ngay = _v.NgayTra;
        string ghiChu = _v.GhiChuChung;
        string ma = _phieu.Phieu.MaPhieu;
        var tom = TraSachBLL.TomTat(_phieu, chon);
        if (chon.Count > 0 && !_v.XacNhan($"Xác nhận trả {chon.Count} cuốn của phiếu {ma}?" + (tom.SoConMuon > 0 ? $"\n{tom.SoConMuon} cuốn tiếp tục được mượn." : ""))) return;

        _v.DatTrangThaiDangXuLy(true);
        KetQuaTraSach kq;
        try { kq = await Task.Run(() => TraSachBLL.XacNhanTra(ma, chon, ngay, ghiChu)); }
        finally { _v.DatTrangThaiDangXuLy(false); }

        _nav.LamMoiHuyHieuQuaHan();
        string tb = $"Đã ghi nhận trả {kq.SoTot + kq.SoHuHong + kq.SoMat} cuốn.";
        if (kq.SoHuHong > 0) tb += $"\n{kq.SoHuHong} cuốn hư hỏng chờ xử lý.";
        if (kq.SoMat > 0) tb += $"\n{kq.SoMat} cuốn báo mất.";
        tb += "\nTrạng thái phiếu: " + kq.TrangThaiPhieu + ".";
        _v.HienThiThongTin(tb);
        if (kq.SoConMuon == 0) _nav.DieuHuong(MucMenu.PhieuMuon);
        else await TaiPhieu(ma);        // còn sách: tải lại để tiếp tục trả
    }
}

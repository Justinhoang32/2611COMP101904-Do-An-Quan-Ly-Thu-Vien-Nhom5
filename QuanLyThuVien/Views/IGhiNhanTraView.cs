using DTO.Models;

namespace QuanLyThuVien.Views;

/// <summary>Giao diện màn hình Ghi nhận trả sách.</summary>
public interface IGhiNhanTraView : IViewCoThongBao
{
    string MaPhieuNhap { get; }
    DateTime NgayTra { get; }
    string GhiChuChung { get; }
    /// <summary>Các bản sách đang được tick chọn kèm tình trạng và ghi chú từng cuốn.</summary>
    List<ChiTietTraSach> DanhSachChon { get; }

    event EventHandler Loaded;
    event EventHandler TaiPhieuClicked;
    event EventHandler VeDanhSachClicked;
    event EventHandler LuaChonThayDoi;
    event EventHandler XacNhanClicked;
    event EventHandler<string> MoPhieu;          // mở sẵn một phiếu khi được điều hướng từ màn hình khác

    void HienThiPhieu(PhieuMuonDayDu phieu);
    void XoaPhieu();
    void HienThiTomTat(PhieuMuonDayDu phieu, KetQuaTraSach tomTat, int soChon);
    bool XacNhan(string cauHoi);
}

using DTO.Models;

namespace QuanLyThuVien.Views;

/// <summary>Giao diện màn hình Tổng quan thư viện.</summary>
public interface ITongQuanView : IViewCoThongBao
{
    event EventHandler Loaded;
    event EventHandler<MucMenu> DiDenClicked;        // các liên kết/nút chuyển sang màn hình khác
    event EventHandler<string> ChiTietPhieuClicked;
    event EventHandler<string> TraSachClicked;

    void HienThiLoiChao(string cau);
    void HienThiTongQuan(ThongKeTongQuan tq);
    void HienThiPhieuGanDay(List<PhieuMuonHienThi> ds);
}

using DTO.Models;

namespace QuanLyThuVien.Views;

/// <summary>Giao diện màn hình Phiếu mượn (danh sách phiếu, thống kê, tab trạng thái).</summary>
public interface IPhieuMuonView : IViewCoThongBao
{
    string TuKhoa { get; }
    DateTime? TuNgay { get; }
    DateTime? DenNgay { get; }

    event EventHandler Loaded;
    event EventHandler ApDungClicked;
    event EventHandler<string> TabChanged;          // Tất cả / Đang mượn / Đã trả / Quá hạn
    event EventHandler<int> TrangChanged;
    event EventHandler TaoPhieuClicked;
    event EventHandler<string> TraSachClicked;      // mã phiếu
    event EventHandler<string> ChiTietClicked;
    event EventHandler<string> InClicked;
    event EventHandler XuatClicked;

    void HienThiThongKe(ThongKePhieu tk);
    void HienThiDanhSach(KetQuaPhanTrang<PhieuMuonHienThi> kq);
    void XuatExcel(string tenGoiY, string tieuDe, string[] cot, List<string[]> dong);
}

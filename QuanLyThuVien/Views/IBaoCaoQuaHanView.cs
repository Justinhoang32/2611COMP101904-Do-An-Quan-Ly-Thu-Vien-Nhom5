using DTO.Models;

namespace QuanLyThuVien.Views;

/// <summary>Giao diện màn hình Báo cáo quá hạn.</summary>
public interface IBaoCaoQuaHanView : IViewCoThongBao
{
    DateTime NgayDoiChieu { get; }
    string Nhom { get; }                 // null = tất cả
    int SoNgayToiThieu { get; }
    int? SoNgayToiDa { get; }
    string TuKhoa { get; }
    DateTime ThangThongKe { get; }       // ngày bất kỳ trong tháng cần thống kê

    event EventHandler Loaded;
    event EventHandler LocClicked;
    event EventHandler<int> TrangChanged;
    event EventHandler<string> ChiTietClicked;
    event EventHandler XuatClicked;
    event EventHandler InClicked;
    event EventHandler ThangChanged;

    void HienThiThongKe(ThongKeQuaHan tk, DateTime ngayDoiChieu);
    void HienThiDanhSach(KetQuaPhanTrang<QuaHanHienThi> kq);
    void HienThiThongKeThang(int thang, int nam, int phatSinh, int daThuHoi, List<KeyValuePair<string, int>> cacThang);
    void XuatExcel(string tenGoiY, string tieuDe, string[] cot, List<string[]> dong);
    void InDanhSach(List<QuaHanHienThi> ds, DateTime ngayDoiChieu);
}

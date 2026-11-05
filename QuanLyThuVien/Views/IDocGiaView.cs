using DTO.Entities;
using DTO.Models;

namespace QuanLyThuVien.Views;

/// <summary>Giao diện màn hình Độc giả (danh sách, hồ sơ, cấp/gia hạn thẻ, in thẻ).</summary>
public interface IDocGiaView : IViewCoThongBao
{
    // Bộ lọc danh sách
    string TuKhoaLoc { get; }
    string NhomLoc { get; }         // null = tất cả
    string TrangThaiLoc { get; }    // null = tất cả
    // Dữ liệu panel "Cấp / gia hạn thẻ"
    string NhomThe { get; }
    DateTime NgayCap { get; }
    DateTime HanThe { get; }

    event EventHandler Loaded;
    event EventHandler LocClicked;
    event EventHandler<int> TrangChanged;
    event EventHandler<string> DocGiaDuocChon;
    event EventHandler ThemClicked;
    event EventHandler SuaClicked;
    event EventHandler CapTheClicked;
    event EventHandler InTheClicked;
    event EventHandler XuatClicked;

    void HienThiThongKe(ThongKeDocGia thongKe);
    void HienThiDanhSach(KetQuaPhanTrang<DocGiaHienThi> ketQua, string maDangChon);
    void HienThiHoSo(DocGiaHienThi docGia);   // null = chưa chọn ai
    DocGia NhapHoSo(DocGia khoiTao, bool them);   // hiện hộp thoại; null nếu hủy
    void InThe();
    void XuatExcel(string tenGoiY, string tieuDe, string[] cot, List<string[]> dong);
}

using DTO.Entities;
using DTO.Models;

namespace QuanLyThuVien.Views;

/// <summary>Giao diện màn hình Tìm kiếm sách (lọc kết hợp nhiều điều kiện, thêm sách vào phiếu nháp).</summary>
public interface ITimKiemSachView : IViewCoThongBao
{
    string MaSach { get; }
    string TenSach { get; }
    string TacGia { get; }
    string MaTheLoai { get; }       // null = tất cả
    string LuuHanh { get; }         // null = tất cả
    bool ChiConSan { get; }

    event EventHandler Loaded;
    event EventHandler TimClicked;
    event EventHandler XoaLocClicked;
    event EventHandler<int> TrangChanged;
    event EventHandler<string> ThemVaoPhieuClicked;
    event EventHandler<string> ChiTietClicked;
    event EventHandler MoPhieuClicked;
    event EventHandler VeDanhMucClicked;

    void NapTheLoai(List<TheLoai> ds);
    void DatLaiBoLoc();
    void HienThiKetQua(KetQuaPhanTrang<SachHienThi> kq);
    void HienThiPhieuNhap(int soDauSach, int tongCuon);
    void HienThiTrangThaiNhanh(string noiDung, bool thanhCong);
    void MoChiTiet(SachHienThi sach);
}

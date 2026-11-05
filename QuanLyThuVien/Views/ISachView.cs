using DTO.Entities;
using DTO.Models;

namespace QuanLyThuVien.Views;

/// <summary>Giao diện màn hình Quản lý sách (danh mục sách, thêm/sửa đầu sách, nhập danh mục).</summary>
public interface ISachView
{
    // ----- Bộ lọc -----
    string TuKhoaLoc { get; }
    string MaTheLoaiLoc { get; }      // null = tất cả
    string TinhTrangLoc { get; }      // null = tất cả; "Có sẵn" / "Hết sách"

    // ----- Dữ liệu trong biểu mẫu thêm/sửa -----
    string MaSachDangSua { get; }     // null khi đang thêm mới
    string TenSach { get; }
    string MaTacGia { get; }
    string MaTheLoai { get; }
    string NamXuatBan { get; }
    int TongSoBan { get; }
    string ViTriKe { get; }
    string NhaXuatBan { get; }
    string NgonNgu { get; }
    string TinhTrangLuuHanh { get; }
    byte[] AnhBiaMoi { get; }         // null nếu người dùng không đổi ảnh

    // ----- Sự kiện -----
    event EventHandler Loaded;
    event EventHandler LocClicked;
    event EventHandler<int> TrangChanged;
    event EventHandler ThemClicked;
    event EventHandler<string> SuaClicked;
    event EventHandler LuuClicked;
    event EventHandler HuyClicked;
    event EventHandler NhapDanhMucClicked;
    event EventHandler AnhBiaDaChon;

    // ----- Hiển thị -----
    void NapTheLoai(List<TheLoai> ds);
    void NapTacGia(List<TacGia> ds);
    void HienThiDanhSach(KetQuaPhanTrang<SachHienThi> ketQua);
    void HienThiThongKe(ThongKeSach thongKe);
    void MoFormThem();
    void MoFormSua(SachHienThi sach);
    void DongForm();
    void XoaAnhBiaMoi();
    string ChonFileNhap();            // hộp thoại chọn file CSV; null nếu hủy
    void DatTrangThaiDangXuLy(bool dangXuLy);
    void HienThiThongTin(string thongBao);
    void HienThiLoi(string thongBao);
}

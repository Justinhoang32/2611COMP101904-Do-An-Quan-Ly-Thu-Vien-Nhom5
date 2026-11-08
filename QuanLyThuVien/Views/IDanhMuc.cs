using DTO.Models;

namespace QuanLyThuVien.Views;

/// <summary>Một dòng của danh mục tác giả hoặc thể loại (mã, tên, mô tả, số đầu sách).</summary>
public class DanhMucItem
{
    public string Ma { get; set; }
    public string Ten { get; set; }
    public string MoTa { get; set; }
    public int SoDauSach { get; set; }
}

/// <summary>Giao diện của một khối danh mục (dùng cho cả Tác giả và Thể loại): danh sách, tìm kiếm, thêm/sửa.</summary>
public interface IDanhMucView : IViewCoThongBao
{
    string TuKhoa { get; }
    string MaDangSua { get; }   // null khi đang thêm mới
    string Ten { get; }
    string MoTa { get; }

    event EventHandler TimKiem;
    event EventHandler<int> TrangChanged;
    event EventHandler ThemClicked;
    event EventHandler<string> SuaClicked;
    event EventHandler LuuClicked;
    event EventHandler HuyClicked;

    void HienThiDanhSach(KetQuaPhanTrang<DanhMucItem> ketQua, string tenDoiTuong);
    void HienThiTong(string noiDung);
    void MoFormThem();
    void MoFormSua(string ma, string ten, string moTa);
    void DongForm();
    void DatQuyenSua(bool choPhep);
}

/// <summary>Giao diện màn hình Tác giả &amp; thể loại: chứa hai khối danh mục.</summary>
public interface ITacGiaTheLoaiView
{
    IDanhMucView TacGia { get; }
    IDanhMucView TheLoai { get; }
    event EventHandler Loaded;
    void DatQuyen(bool laQuanTri);
}

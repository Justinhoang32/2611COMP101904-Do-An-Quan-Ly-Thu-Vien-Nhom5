namespace QuanLyThuVien.Views;

/// <summary>Giao diện của màn hình Đăng nhập: chỉ khai báo dữ liệu hiển thị và sự kiện người dùng.</summary>
public interface IDangNhapView
{
    string TenDangNhap { get; set; }
    string MatKhau { get; }
    bool GhiNho { get; set; }

    event EventHandler Loaded;
    event EventHandler DangNhapClicked;
    event EventHandler QuenMatKhauClicked;
    event EventHandler LienHeClicked;
    event EventHandler TraCuuClicked;

    void HienThiLoi(string thongBao);
    void HienThiThongTin(string thongBao);
    void DatTrangThaiDangXuLy(bool dangXuLy);
    void XoaMatKhau();
    void DangNhapThanhCong();
}

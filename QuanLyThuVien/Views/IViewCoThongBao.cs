namespace QuanLyThuVien.Views;

/// <summary>Các phương thức thông báo mà mọi View đều có; Presenter dùng chung để báo lỗi/thông tin/trạng thái chờ.</summary>
public interface IViewCoThongBao
{
    void HienThiLoi(string thongBao);
    void HienThiThongTin(string thongBao);
    void DatTrangThaiDangXuLy(bool dangXuLy);
}

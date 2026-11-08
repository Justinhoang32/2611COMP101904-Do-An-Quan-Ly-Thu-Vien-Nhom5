using BLL;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>Presenter màn hình Đăng nhập: nhận sự kiện từ View, gọi BLL, đẩy kết quả về View.</summary>
public class DangNhapPresenter
{
    private readonly IDangNhapView _view;

    public DangNhapPresenter(IDangNhapView view)
    {
        _view = view;
        _view.Loaded += OnLoaded;
        _view.DangNhapClicked += OnDangNhap;
        _view.QuenMatKhauClicked += (s, e) => _view.HienThiThongTin(
            "Vui lòng liên hệ Quản trị viên hoặc quầy thư viện để được cấp lại mật khẩu.");
        _view.LienHeClicked += (s, e) => _view.HienThiThongTin(
            "Liên hệ Thư viện Trường Đại học Sư phạm TP. Hồ Chí Minh\nQuầy thư viện, Cơ sở An Dương Vương để được hỗ trợ.");
        _view.TraCuuClicked += (s, e) => _view.HienThiThongTin(
            "Vui lòng đăng nhập để sử dụng chức năng tra cứu thư viện.");
    }

    // Khi form mở: điền lại tên tài khoản đã ghi nhớ
    private void OnLoaded(object sender, EventArgs e)
    {
        string ten = GhiNhoDangNhap.Doc();
        if (!string.IsNullOrEmpty(ten))
        {
            _view.TenDangNhap = ten;
            _view.GhiNho = true;
        }
    }

    // Bấm Đăng nhập: chạy BLL ở luồng nền để giao diện không bị đứng khi kết nối database
    private async void OnDangNhap(object sender, EventArgs e)
    {
        _view.DatTrangThaiDangXuLy(true);
        try
        {
            string taiKhoan = _view.TenDangNhap;
            string matKhau = _view.MatKhau;
            // Lỗi nghiệp vụ (sai mật khẩu...) được bắt ngay trong luồng nền và trả về dạng chữ,
            // nhờ vậy debugger của Visual Studio không dừng lại ở dòng throw.
            string loiNghiepVu = await Task.Run(() =>
            {
                try { TaiKhoanBLL.DangNhap(taiKhoan, matKhau); return (string)null; }
                catch (NghiepVuException ex) { return ex.Message; }
            });
            if (loiNghiepVu != null)
            {
                _view.HienThiLoi(loiNghiepVu);   // lỗi nhập liệu / sai mật khẩu
                _view.XoaMatKhau();
                return;
            }

            // Ghi nhớ / xóa ghi nhớ tên tài khoản
            if (_view.GhiNho) GhiNhoDangNhap.Luu(taiKhoan.Trim());
            else GhiNhoDangNhap.Xoa();

            _view.DangNhapThanhCong();
        }
        catch (NghiepVuException ex)
        {
            _view.HienThiLoi(ex.Message);   // lỗi nhập liệu / sai mật khẩu
            _view.XoaMatKhau();
        }
        catch (Exception)
        {
            _view.HienThiLoi("Không thể kết nối cơ sở dữ liệu.\nVui lòng kiểm tra SQL Server (.\\SQLEXPRESS) đã chạy và database QuanLyThuVien đã được tạo.");
        }
        finally
        {
            _view.DatTrangThaiDangXuLy(false);
        }
    }
}

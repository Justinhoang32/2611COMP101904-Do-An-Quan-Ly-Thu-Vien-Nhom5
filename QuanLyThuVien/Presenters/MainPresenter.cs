using System.Globalization;
using BLL;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>Presenter của khung chính: thông tin người dùng, điều hướng, phân quyền menu, đăng xuất, huy hiệu quá hạn.</summary>
public class MainPresenter
{
    private const string CoSo = "Cơ sở An Dương Vương";
    private readonly IMainView _view;

    // Đường dẫn breadcrumb hiển thị ở header, theo thiết kế PDF
    private static readonly Dictionary<MucMenu, string> DuongDan = new()
    {
        { MucMenu.TongQuan, "Tổng quan" },
        { MucMenu.QuanLySach, "Quản lý sách" },
        { MucMenu.TacGiaTheLoai, "Tác giả & thể loại" },
        { MucMenu.DocGia, "Độc giả" },
        { MucMenu.PhieuMuon, "Phiếu mượn" },
        { MucMenu.LapPhieuMuon, "Phiếu mượn / Lập phiếu" },
        { MucMenu.XemTruocPhieu, "Phiếu mượn / Xem trước bản in" },
        { MucMenu.GhiNhanTraSach, "Ghi nhận trả sách" },
        { MucMenu.TimKiemSach, "Tìm kiếm sách" },
        { MucMenu.BaoCaoQuaHan, "Báo cáo quá hạn" }
    };

    public MainPresenter(IMainView view)
    {
        _view = view;
        _view.Loaded += OnLoaded;
        _view.MenuDuocChon += OnMenuDuocChon;
        _view.DangXuatClicked += OnDangXuat;
        _view.YeuCauLamMoiHuyHieu += (s, e) => CapNhatHuyHieu();
    }

    private void OnLoaded(object sender, EventArgs e)
    {
        try
        {
            var nd = PhienDangNhap.NguoiDungHienTai;
            if (nd == null) { _view.KetThuc(true); return; }   // chưa đăng nhập thì quay lại màn hình đăng nhập

            DateTime hom = DateTime.Today;
            var vi = new CultureInfo("vi-VN");
            _view.HienThiNguoiDung(nd.HoTen, $"{nd.TenVaiTro} {CoSo}", VietTat(nd.HoTen));
            _view.HienThiNgayHienTai(vi.TextInfo.ToTitleCase(hom.ToString("dddd, dd/MM/yyyy", vi)));
            _view.HienThiNamHoc(TinhNamHoc(hom));
            _view.HienThiNgayDuLieu($"Dữ liệu ngày {hom:dd/MM/yyyy}");
            CapNhatHuyHieu();
            OnMenuDuocChon(this, new YeuCauDieuHuong(MucMenu.TongQuan));   // màn hình mặc định
        }
        catch (Exception)
        {
            _view.HienThiLoi("Không thể tải thông tin phiên làm việc. Vui lòng đăng nhập lại.");
            _view.KetThuc(true);
        }
    }

    // Chuyển màn hình (kiểm tra quyền trước)
    private void OnMenuDuocChon(object sender, YeuCauDieuHuong e)
    {
        try
        {
            _view.HienThiManHinh(e, DuongDan.TryGetValue(e.Muc, out var d) ? d : e.Muc.ToString());
        }
        catch (Exception ex)
        {
            _view.HienThiLoi("Không thể mở màn hình này. Vui lòng thử lại.\n(Chi tiết: " + ex.Message + ")");
        }
    }

    private void OnDangXuat(object sender, EventArgs e)
    {
        if (!_view.XacNhan("Bạn có chắc muốn đăng xuất?")) return;
        TaiKhoanBLL.DangXuat();
        _view.KetThuc(true);
    }

    // Cập nhật số phiếu quá hạn trên menu; lỗi thì bỏ qua (huy hiệu không quan trọng bằng chức năng chính)
    private void CapNhatHuyHieu()
    {
        try { _view.HienThiSoQuaHan(ThongKeBLL.DemPhieuQuaHan()); }
        catch { _view.HienThiSoQuaHan(0); }
    }

    // Chữ viết tắt: lấy chữ cái đầu của tối đa 2 từ cuối trong họ tên ("Nguyễn Lan Hương" -> "LH")
    private static string VietTat(string hoTen)
    {
        var tu = (hoTen ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(tu.Skip(Math.Max(0, tu.Length - 2)).Select(t => char.ToUpper(t[0]))) is { Length: > 0 } s ? s : "?";
    }

    // Năm học bắt đầu từ tháng 9: tháng 9-12 là năm nay - năm sau; tháng 1-8 là năm trước - năm nay
    private static string TinhNamHoc(DateTime ngay)
    {
        int batDau = ngay.Month >= 9 ? ngay.Year : ngay.Year - 1;
        return $"Năm học {batDau} – {batDau + 1}";
    }
}

using BLL;
using DTO;
using DTO.Models;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>Presenter màn hình Phiếu mượn: lọc theo tab/từ khóa/ngày, phân trang 7 dòng, thống kê, xuất Excel và điều hướng.</summary>
public class PhieuMuonPresenter
{
    private const int KichThuocTrang = 7;
    private readonly IPhieuMuonView _v;
    private readonly IDieuHuong _nav;
    private string _tab = HangSo.TatCa;
    private int _trang = 1;

    public PhieuMuonPresenter(IPhieuMuonView view, IDieuHuong dieuHuong)
    {
        _v = view;
        _nav = dieuHuong;
        _v.Loaded += async (s, e) => await Chay(TaiDuLieu);
        _v.ApDungClicked += async (s, e) => { _trang = 1; await Chay(TaiDuLieu); };
        _v.TabChanged += async (s, tab) => { _tab = tab; _trang = 1; await Chay(TaiDuLieu); };
        _v.TrangChanged += async (s, t) => { _trang = t; await Chay(TaiDuLieu); };
        _v.TaoPhieuClicked += (s, e) => _nav.DieuHuong(MucMenu.LapPhieuMuon);
        _v.TraSachClicked += (s, ma) => _nav.DieuHuong(MucMenu.GhiNhanTraSach, ma);
        _v.ChiTietClicked += (s, ma) => _nav.DieuHuong(MucMenu.XemTruocPhieu, ma);
        _v.InClicked += (s, ma) => _nav.DieuHuong(MucMenu.XemTruocPhieu, ma);
        _v.XuatClicked += async (s, e) => await Chay(XuatDanhSach);
    }

    private Task Chay(Func<Task> viec) => PresenterHelper.ChayAn(_v, viec);

    private PhieuBoLoc TaoBoLoc() => new PhieuBoLoc { TuKhoa = _v.TuKhoa, TrangThai = _tab, TuNgay = _v.TuNgay, DenNgay = _v.DenNgay };

    private async Task TaiDuLieu()
    {
        if (_v.TuNgay.HasValue && _v.DenNgay.HasValue && _v.TuNgay > _v.DenNgay)
            throw new NghiepVuException("\"Từ ngày\" phải trước hoặc bằng \"Đến ngày\".");
        var loc = TaoBoLoc();
        int trang = _trang;
        _v.DatTrangThaiDangXuLy(true);
        try
        {
            var kq = await Task.Run(() => PhieuMuonBLL.TimKiem(loc, trang, KichThuocTrang));
            if (kq.DuLieu.Count == 0 && trang > 1 && kq.TongSoDong > 0)
            {
                _trang = Math.Max(1, kq.TongSoTrang);
                kq = await Task.Run(() => PhieuMuonBLL.TimKiem(loc, _trang, KichThuocTrang));
            }
            _v.HienThiThongKe(await Task.Run(() => PhieuMuonBLL.ThongKe()));
            _v.HienThiDanhSach(kq);
        }
        finally { _v.DatTrangThaiDangXuLy(false); }
    }

    private async Task XuatDanhSach()
    {
        var loc = TaoBoLoc();
        var ds = await Task.Run(() => PhieuMuonBLL.LayTatCaDeXuat(loc));
        if (ds.Count == 0) throw new NghiepVuException("Không có phiếu mượn nào để xuất.");
        var cot = new[] { "Mã phiếu", "Độc giả", "Mã thẻ", "Nhóm", "Ngày mượn", "Hẹn trả", "Số cuốn", "Đã trả", "Trạng thái", "Số ngày trễ", "Thủ thư" };
        var dong = ds.Select(p => new[]
        {
            p.MaPhieu, p.HoTenDocGia, p.MaDocGia, p.NhomDocGia, p.NgayMuon?.ToString("dd/MM/yyyy") ?? "", p.HenTra?.ToString("dd/MM/yyyy") ?? "",
            p.TongSoCuon.ToString(), p.SoCuonDaTra.ToString(), p.TrangThaiHienThi, p.SoNgayTre > 0 ? p.SoNgayTre.ToString() : "", p.TenThuThu
        }).ToList();
        _v.XuatExcel($"PhieuMuon_{DateTime.Today:yyyyMMdd}", "Danh sách phiếu mượn - Thư viện HCMUE", cot, dong);
    }
}

using BLL;
using DTO;
using DTO.Models;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>Presenter Báo cáo quá hạn: thống kê, danh sách lọc/phân trang (6 dòng), biểu đồ theo tháng, xuất Excel và in danh sách.</summary>
public class BaoCaoQuaHanPresenter
{
    private const int KichThuocTrang = 6;
    private readonly IBaoCaoQuaHanView _v;
    private readonly IDieuHuong _nav;
    private int _trang = 1;

    public BaoCaoQuaHanPresenter(IBaoCaoQuaHanView view, IDieuHuong dieuHuong)
    {
        _v = view;
        _nav = dieuHuong;
        _v.Loaded += async (s, e) => await Chay(async () => { await TaiDanhSach(); await TaiThongKeThang(); });
        _v.LocClicked += async (s, e) => { _trang = 1; await Chay(TaiDanhSach); };
        _v.TrangChanged += async (s, t) => { _trang = t; await Chay(TaiDanhSach); };
        _v.ChiTietClicked += (s, ma) => _nav.DieuHuong(MucMenu.XemTruocPhieu, ma);
        _v.XuatClicked += async (s, e) => await Chay(Xuat);
        _v.InClicked += async (s, e) => await Chay(async () =>
        {
            var boLoc = TaoBoLoc();   // đọc giá trị từ View ở luồng giao diện, rồi mới chuyển sang luồng nền
            var ds = await Task.Run(() => ThongKeBLL.LayTatCaQuaHanDeXuat(boLoc));
            if (ds.Count == 0) throw new NghiepVuException("Không có phiếu quá hạn nào để in.");
            _v.InDanhSach(ds, _v.NgayDoiChieu);
        });
        _v.ThangChanged += async (s, e) => await Chay(TaiThongKeThang);
    }

    private Task Chay(Func<Task> viec) => PresenterHelper.ChayAn(_v, viec);

    private QuaHanBoLoc TaoBoLoc() => new QuaHanBoLoc
    {
        NgayDoiChieu = _v.NgayDoiChieu, Nhom = _v.Nhom, SoNgayTreToiThieu = _v.SoNgayToiThieu,
        SoNgayTreToiDa = _v.SoNgayToiDa, TuKhoa = _v.TuKhoa
    };

    private async Task TaiDanhSach()
    {
        var loc = TaoBoLoc();
        int trang = _trang;
        _v.DatTrangThaiDangXuLy(true);
        try
        {
            var tk = await Task.Run(() => ThongKeBLL.ThongKeQuaHan(loc.NgayDoiChieu));
            var kq = await Task.Run(() => ThongKeBLL.TimQuaHan(loc, trang, KichThuocTrang));
            if (kq.DuLieu.Count == 0 && trang > 1 && kq.TongSoDong > 0)
            {
                _trang = Math.Max(1, kq.TongSoTrang);
                kq = await Task.Run(() => ThongKeBLL.TimQuaHan(loc, _trang, KichThuocTrang));
            }
            _v.HienThiThongKe(tk, loc.NgayDoiChieu);
            _v.HienThiDanhSach(kq);
        }
        finally { _v.DatTrangThaiDangXuLy(false); }
    }

    // Thống kê một tháng + biểu đồ 6 tháng gần nhất kết thúc ở tháng đang chọn
    private async Task TaiThongKeThang()
    {
        var thang = _v.ThangThongKe;
        var namNay = await Task.Run(() => ThongKeBLL.ThongKeQuaHanTheoThang(thang.Year));
        var namTruoc = thang.Month < 6 ? await Task.Run(() => ThongKeBLL.ThongKeQuaHanTheoThang(thang.Year - 1)) : null;
        var cacThang = new List<KeyValuePair<string, int>>();
        for (int i = 5; i >= 0; i--)
        {
            var d = new DateTime(thang.Year, thang.Month, 1).AddMonths(-i);
            var nguon = d.Year == thang.Year ? namNay : namTruoc;
            int gt = nguon?.FirstOrDefault(t => t.Thang == d.Month)?.GiaTri ?? 0;
            cacThang.Add(new KeyValuePair<string, int>("T" + d.Month, gt));
        }
        var hienTai = namNay.First(t => t.Thang == thang.Month);
        _v.HienThiThongKeThang(thang.Month, thang.Year, hienTai.GiaTri, hienTai.GiaTri2, cacThang);
    }

    private async Task Xuat()
    {
        var boLoc = TaoBoLoc();   // đọc giá trị từ View ở luồng giao diện, rồi mới chuyển sang luồng nền
        var ds = await Task.Run(() => ThongKeBLL.LayTatCaQuaHanDeXuat(boLoc));
        if (ds.Count == 0) throw new NghiepVuException("Không có phiếu quá hạn nào để xuất.");
        var cot = new[] { "Mã phiếu", "Ngày mượn", "Mã thẻ", "Họ tên", "Nhóm", "Điện thoại", "Sách chưa trả", "Hẹn trả", "Số ngày trễ", "Số cuốn" };
        var dong = ds.Select(x => new[]
        {
            x.MaPhieu, x.NgayMuon.ToString("dd/MM/yyyy"), x.MaDocGia, x.HoTen, x.NhomDocGia, x.DienThoai,
            (x.DanhSachSach ?? "").Replace("\n", "; "), x.HenTra.ToString("dd/MM/yyyy"), x.SoNgayTre.ToString(), x.SoCuon.ToString()
        }).ToList();
        _v.XuatExcel($"BaoCaoQuaHan_{_v.NgayDoiChieu:yyyyMMdd}", $"Báo cáo quá hạn - tính đến {_v.NgayDoiChieu:dd/MM/yyyy}", cot, dong);
    }
}

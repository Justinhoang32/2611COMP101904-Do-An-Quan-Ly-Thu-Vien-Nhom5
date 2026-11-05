using BLL;
using DTO;
using DTO.Models;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>
/// Presenter màn hình Tìm kiếm sách. Lọc kết hợp (mã, tên, tác giả, thể loại, lưu hành, chỉ sách có sẵn), 5 dòng/trang.
/// "+ Thêm vào phiếu" kiểm tra tồn kho bằng BLL rồi ghi vào phiếu nháp dùng chung PhieuNhap.
/// </summary>
public class TimKiemSachPresenter
{
    private const int KichThuocTrang = 5;
    private readonly ITimKiemSachView _v;
    private readonly IDieuHuong _nav;
    private int _trang = 1;

    public TimKiemSachPresenter(ITimKiemSachView view, IDieuHuong dieuHuong)
    {
        _v = view;
        _nav = dieuHuong;
        PhieuNhap.ThayDoi += CapNhatPhieu;
        _v.Loaded += async (s, e) => await Chay(async () =>
        {
            _v.NapTheLoai(await Task.Run(() => TheLoaiBLL.LayTatCa()));
            CapNhatPhieu(null, EventArgs.Empty);
            await TaiDuLieu();
        });
        _v.TimClicked += async (s, e) => { _trang = 1; await Chay(TaiDuLieu); };
        _v.XoaLocClicked += async (s, e) => { _v.DatLaiBoLoc(); _trang = 1; await Chay(TaiDuLieu); };
        _v.TrangChanged += async (s, t) => { _trang = t; await Chay(TaiDuLieu); };
        _v.ThemVaoPhieuClicked += async (s, ma) => await Chay(() => ThemVaoPhieu(ma));
        _v.ChiTietClicked += async (s, ma) => await Chay(async () =>
        {
            var sach = await Task.Run(() => SachBLL.LayTheoMa(ma));
            if (sach == null) throw new NghiepVuException("Không tìm thấy sách (có thể đã bị xóa).");
            _v.MoChiTiet(sach);
        });
        _v.MoPhieuClicked += (s, e) => _nav.DieuHuong(MucMenu.LapPhieuMuon);
        _v.VeDanhMucClicked += (s, e) => _nav.DieuHuong(MucMenu.QuanLySach);
    }

    /// <summary>Gỡ đăng ký sự kiện tĩnh khi màn hình bị đóng (tránh giữ View đã hủy).</summary>
    public void HuyDangKy() => PhieuNhap.ThayDoi -= CapNhatPhieu;

    private Task Chay(Func<Task> viec) => PresenterHelper.ChayAn(_v, viec);

    private void CapNhatPhieu(object s, EventArgs e) => _v.HienThiPhieuNhap(PhieuNhap.SoDauSach, PhieuNhap.TongSoCuon);

    private async Task TaiDuLieu()
    {
        var loc = new SachBoLoc
        {
            MaSach = _v.MaSach, TenSach = _v.TenSach, TenTacGia = _v.TacGia, MaTheLoai = _v.MaTheLoai,
            TinhTrangLuuHanh = _v.LuuHanh, ChiConSach = _v.ChiConSan
        };
        int trang = _trang;
        _v.DatTrangThaiDangXuLy(true);
        try
        {
            var kq = await Task.Run(() => SachBLL.TimKiem(loc, trang, KichThuocTrang));
            if (kq.DuLieu.Count == 0 && trang > 1 && kq.TongSoDong > 0)
            {
                _trang = Math.Max(1, kq.TongSoTrang);
                kq = await Task.Run(() => SachBLL.TimKiem(loc, _trang, KichThuocTrang));
            }
            _v.HienThiKetQua(kq);
        }
        finally { _v.DatTrangThaiDangXuLy(false); }
    }

    private async Task ThemVaoPhieu(string ma)
    {
        int moi = PhieuNhap.SoLuongCua(ma) + 1;
        string loi = await Task.Run(() => PhieuMuonBLL.KiemTraSoLuong(ma, moi));
        if (loi != null) { _v.HienThiTrangThaiNhanh(loi, false); return; }
        if (PhieuNhap.TongSoCuon + 1 > HangSo.HanMucSachMoiDocGia)
        {
            _v.HienThiTrangThaiNhanh($"Mỗi phiếu tối đa {HangSo.HanMucSachMoiDocGia} cuốn.", false);
            return;
        }
        PhieuNhap.Dat(ma, moi);
        _v.HienThiTrangThaiNhanh($"Đã thêm {ma} vào phiếu nháp (hiện có {PhieuNhap.TongSoCuon} cuốn).", true);
    }
}

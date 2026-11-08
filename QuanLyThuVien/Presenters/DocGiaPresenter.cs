using BLL;
using DTO;
using DTO.Entities;
using DTO.Models;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>Presenter màn hình Độc giả: danh sách có lọc/phân trang, hồ sơ, thêm/sửa, cấp/gia hạn thẻ, in thẻ, xuất danh sách.</summary>
public class DocGiaPresenter
{
    private readonly IDocGiaView _v;
    private DocGiaBoLoc _loc = new();
    private int _trang = 1;
    private string _maChon;
    private DocGiaHienThi _hienTai;

    public DocGiaPresenter(IDocGiaView view)
    {
        _v = view;
        _v.Loaded += async (s, e) => await Chay(TaiDuLieu);
        _v.LocClicked += async (s, e) => await Chay(async () =>
        {
            _loc = new DocGiaBoLoc { TuKhoa = _v.TuKhoaLoc, Nhom = _v.NhomLoc, TrangThai = _v.TrangThaiLoc };
            _trang = 1;
            await TaiDuLieu();
        });
        _v.TrangChanged += async (s, t) => await Chay(async () => { _trang = t; await TaiDuLieu(); });
        _v.DocGiaDuocChon += async (s, ma) => await Chay(() => ChonDocGia(ma));
        _v.ThemClicked += async (s, e) => await Chay(ThemDocGia);
        _v.SuaClicked += async (s, e) => await Chay(SuaDocGia);
        _v.CapTheClicked += async (s, e) => await Chay(CapThe);
        _v.InTheClicked += (s, e) => { if (_hienTai != null) _v.InThe(); };
        _v.XuatClicked += async (s, e) => await Chay(XuatDanhSach);
    }

    private Task Chay(Func<Task> viec) => PresenterHelper.ChayAn(_v, viec);

    // Tải danh sách, thống kê và hồ sơ của độc giả đang chọn
    private async Task TaiDuLieu()
    {
        _v.DatTrangThaiDangXuLy(true);
        try
        {
            var ds = await Task.Run(() => DocGiaBLL.TimKiem(_loc, _trang));
            if (ds.DuLieu.Count == 0 && _trang > 1 && ds.TongSoDong > 0)
            {
                _trang = Math.Max(1, ds.TongSoTrang);
                ds = await Task.Run(() => DocGiaBLL.TimKiem(_loc, _trang));
            }
            _v.HienThiThongKe(await Task.Run(() => DocGiaBLL.ThongKe()));
            if (_maChon == null) _maChon = ds.DuLieu.FirstOrDefault()?.MaDocGia;   // mặc định chọn dòng đầu
            _v.HienThiDanhSach(ds, _maChon);
            await ChonDocGia(_maChon);
        }
        finally { _v.DatTrangThaiDangXuLy(false); }
    }

    private async Task ChonDocGia(string ma)
    {
        _maChon = ma;
        _hienTai = ma == null ? null : await Task.Run(() => DocGiaBLL.LayTheoMa(ma));
        _v.HienThiHoSo(_hienTai);
    }

    // Thêm độc giả: hiện hộp thoại, nếu BLL báo lỗi thì mở lại hộp thoại với dữ liệu đã nhập
    private async Task ThemDocGia()
    {
        var nhap = new DocGia { NhomDocGia = HangSo.NhomSinhVien };
        while (true)
        {
            nhap = _v.NhapHoSo(nhap, true);
            if (nhap == null) return;
            try
            {
                string ma = await Task.Run(() => DocGiaBLL.Them(nhap));
                _maChon = ma;
                _v.HienThiThongTin($"Đã thêm độc giả {ma}. Vui lòng kiểm tra và cấp thẻ ở panel bên phải.");
                await TaiDuLieu();
                return;
            }
            catch (NghiepVuException ex) { _v.HienThiLoi(ex.Message); }
        }
    }

    private async Task SuaDocGia()
    {
        if (_hienTai == null) throw new NghiepVuException("Vui lòng chọn một độc giả trong danh sách.");
        var nhap = new DocGia
        {
            MaDocGia = _hienTai.MaDocGia, HoTen = _hienTai.HoTen, NhomDocGia = _hienTai.NhomDocGia, MaSinhVien = _hienTai.MaSinhVien,
            Khoa = _hienTai.Khoa, DienThoai = _hienTai.DienThoai, Email = _hienTai.Email
        };
        while (true)
        {
            nhap = _v.NhapHoSo(nhap, false);
            if (nhap == null) return;
            try
            {
                await Task.Run(() => DocGiaBLL.Sua(nhap));
                _v.HienThiThongTin("Đã lưu hồ sơ độc giả.");
                await TaiDuLieu();
                return;
            }
            catch (NghiepVuException ex) { _v.HienThiLoi(ex.Message); }
        }
    }

    private async Task CapThe()
    {
        if (_hienTai == null) throw new NghiepVuException("Vui lòng chọn một độc giả trong danh sách.");
        string ma = _hienTai.MaDocGia;
        string nhom = _v.NhomThe;
        DateTime ngayCap = _v.NgayCap, han = _v.HanThe;
        await Task.Run(() => DocGiaBLL.CapThe(ma, nhom, ngayCap, han));
        _v.HienThiThongTin($"Đã cấp / gia hạn thẻ cho {_hienTai.HoTen}. Hạn thẻ: {han:dd/MM/yyyy}.");
        await TaiDuLieu();
    }

    // Xuất toàn bộ kết quả theo bộ lọc hiện tại
    private async Task XuatDanhSach()
    {
        var ds = await Task.Run(() => DocGiaBLL.LayTatCaDeXuat(_loc));
        if (ds.Count == 0) throw new NghiepVuException("Không có độc giả nào để xuất.");
        var cot = new[] { "Mã thẻ", "Họ tên", "Nhóm", "Mã sinh viên / cán bộ", "Khoa / đơn vị", "Điện thoại", "Email", "Ngày cấp", "Hạn thẻ", "Trạng thái", "Đang mượn" };
        var dong = ds.Select(d => new[]
        {
            d.MaDocGia, d.HoTen, d.NhomDocGia, d.MaSinhVien, d.Khoa, d.DienThoai, d.Email,
            d.NgayCap?.ToString("dd/MM/yyyy") ?? "", d.HanThe?.ToString("dd/MM/yyyy") ?? "", d.TrangThaiHienThi, d.SoSachDangMuon.ToString()
        }).ToList();
        _v.XuatExcel($"DanhSachDocGia_{DateTime.Today:yyyyMMdd}", "Danh sách độc giả - Thư viện HCMUE", cot, dong);
    }
}

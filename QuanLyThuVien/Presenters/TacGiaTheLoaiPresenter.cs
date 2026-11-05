using BLL;
using DTO;
using DTO.Models;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>Nguồn dữ liệu của một khối danh mục (che giấu việc đó là tác giả hay thể loại).</summary>
public interface INguonDanhMuc
{
    string TenDoiTuong { get; }   // "tác giả" / "thể loại"
    KetQuaPhanTrang<DanhMucItem> Tim(string tuKhoa, int trang);
    DanhMucItem Lay(string ma);
    string Them(string ten, string moTa);
    void Sua(string ma, string ten, string moTa);
}

/// <summary>Nguồn dữ liệu Tác giả (gọi TacGiaBLL).</summary>
public class NguonTacGia : INguonDanhMuc
{
    public string TenDoiTuong => "tác giả";
    public KetQuaPhanTrang<DanhMucItem> Tim(string tuKhoa, int trang)
    {
        var kq = TacGiaBLL.TimKiem(tuKhoa, trang);
        return new KetQuaPhanTrang<DanhMucItem>
        {
            TongSoDong = kq.TongSoDong, TrangHienTai = kq.TrangHienTai, KichThuocTrang = kq.KichThuocTrang,
            DuLieu = kq.DuLieu.Select(t => new DanhMucItem { Ma = t.MaTacGia, Ten = t.TenTacGia, MoTa = t.GioiThieu, SoDauSach = t.SoDauSach }).ToList()
        };
    }
    public DanhMucItem Lay(string ma)
    {
        var t = TacGiaBLL.LayTheoMa(ma);
        return t == null ? null : new DanhMucItem { Ma = t.MaTacGia, Ten = t.TenTacGia, MoTa = t.GioiThieu };
    }
    public string Them(string ten, string moTa) => TacGiaBLL.Them(ten, moTa);
    public void Sua(string ma, string ten, string moTa) => TacGiaBLL.Sua(ma, ten, moTa);
}

/// <summary>Nguồn dữ liệu Thể loại (gọi TheLoaiBLL).</summary>
public class NguonTheLoai : INguonDanhMuc
{
    public string TenDoiTuong => "thể loại";
    public KetQuaPhanTrang<DanhMucItem> Tim(string tuKhoa, int trang)
    {
        var kq = TheLoaiBLL.TimKiem(tuKhoa, trang);
        return new KetQuaPhanTrang<DanhMucItem>
        {
            TongSoDong = kq.TongSoDong, TrangHienTai = kq.TrangHienTai, KichThuocTrang = kq.KichThuocTrang,
            DuLieu = kq.DuLieu.Select(t => new DanhMucItem { Ma = t.MaTheLoai, Ten = t.TenTheLoai, MoTa = t.MoTa, SoDauSach = t.SoDauSach }).ToList()
        };
    }
    public DanhMucItem Lay(string ma)
    {
        var t = TheLoaiBLL.LayTheoMa(ma);
        return t == null ? null : new DanhMucItem { Ma = t.MaTheLoai, Ten = t.TenTheLoai, MoTa = t.MoTa };
    }
    public string Them(string ten, string moTa) => TheLoaiBLL.Them(ten, moTa);
    public void Sua(string ma, string ten, string moTa) => TheLoaiBLL.Sua(ma, ten, moTa);
}

/// <summary>Presenter của một khối danh mục: tìm kiếm, phân trang, thêm, sửa.</summary>
public class DanhMucPresenter
{
    private readonly IDanhMucView _v;
    private readonly INguonDanhMuc _nguon;
    private int _trang = 1;
    private string _tuKhoa = "";

    public DanhMucPresenter(IDanhMucView view, INguonDanhMuc nguon)
    {
        _v = view;
        _nguon = nguon;
        _v.TimKiem += async (s, e) => await Chay(async () => { _tuKhoa = _v.TuKhoa; _trang = 1; await TaiDuLieu(); });
        _v.TrangChanged += async (s, t) => await Chay(async () => { _trang = t; await TaiDuLieu(); });
        _v.ThemClicked += (s, e) => _v.MoFormThem();
        _v.SuaClicked += async (s, ma) => await Chay(async () =>
        {
            var x = await Task.Run(() => _nguon.Lay(ma));
            if (x == null) throw new NghiepVuException($"Không tìm thấy {_nguon.TenDoiTuong} cần sửa.");
            _v.MoFormSua(x.Ma, x.Ten, x.MoTa);
        });
        _v.HuyClicked += (s, e) => _v.DongForm();
        _v.LuuClicked += async (s, e) => await Chay(Luu);
    }

    private Task Chay(Func<Task> viec) => PresenterHelper.ChayAn(_v, viec);

    /// <summary>Tải trang hiện tại theo từ khóa.</summary>
    public async Task TaiDuLieu()
    {
        var kq = await Task.Run(() => _nguon.Tim(_tuKhoa, _trang));
        if (kq.DuLieu.Count == 0 && _trang > 1 && kq.TongSoDong > 0)
        {
            _trang = Math.Max(1, kq.TongSoTrang);
            kq = await Task.Run(() => _nguon.Tim(_tuKhoa, _trang));
        }
        _v.HienThiDanhSach(kq, _nguon.TenDoiTuong);
        if (string.IsNullOrWhiteSpace(_tuKhoa)) _v.HienThiTong($"{kq.TongSoDong.ToString("N0", new System.Globalization.CultureInfo("vi-VN"))} {_nguon.TenDoiTuong} trong danh mục");
    }

    private async Task Luu()
    {
        string ma = _v.MaDangSua, ten = _v.Ten, moTa = _v.MoTa;
        if (string.IsNullOrEmpty(ma))
        {
            string maMoi = await Task.Run(() => _nguon.Them(ten, moTa));
            _v.HienThiThongTin($"Đã thêm {_nguon.TenDoiTuong} {maMoi}.");
        }
        else
        {
            await Task.Run(() => _nguon.Sua(ma, ten, moTa));
            _v.HienThiThongTin($"Đã lưu {_nguon.TenDoiTuong} {ma}.");
        }
        _v.DongForm();
        await TaiDuLieu();
    }
}

/// <summary>Presenter màn hình Tác giả &amp; thể loại: phân quyền (chỉ Quản trị viên được sửa) và khởi tạo hai khối danh mục.</summary>
public class TacGiaTheLoaiPresenter
{
    private readonly ITacGiaTheLoaiView _v;
    private readonly DanhMucPresenter _tacGia, _theLoai;

    public TacGiaTheLoaiPresenter(ITacGiaTheLoaiView view)
    {
        _v = view;
        _tacGia = new DanhMucPresenter(view.TacGia, new NguonTacGia());
        _theLoai = new DanhMucPresenter(view.TheLoai, new NguonTheLoai());
        _v.Loaded += async (s, e) =>
        {
            _v.DatQuyen(PhienDangNhap.LaQuanTri);   // Thủ thư chỉ xem
            await PresenterHelper.ChayAn(_v.TacGia, _tacGia.TaiDuLieu);
            await PresenterHelper.ChayAn(_v.TheLoai, _theLoai.TaiDuLieu);
        };
    }
}

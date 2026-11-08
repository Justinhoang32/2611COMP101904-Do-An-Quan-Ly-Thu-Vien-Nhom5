using System.Text;
using BLL;
using DTO;
using DTO.Entities;
using DTO.Models;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>
/// Presenter màn hình Quản lý sách: tải danh sách theo bộ lọc + phân trang, thêm/sửa đầu sách, nhập danh mục từ CSV.
/// Các lệnh BLL (truy cập database) chạy ở luồng nền để giao diện không bị đứng.
/// </summary>
public class SachPresenter
{
    private readonly ISachView _view;
    private SachBoLoc _boLoc = new();   // bộ lọc đang áp dụng (chỉ đổi khi bấm Lọc)
    private int _trang = 1;

    public SachPresenter(ISachView view)
    {
        _view = view;
        _view.Loaded += async (s, e) => await ChayAn(async () =>
        {
            // Nạp dữ liệu cho các ô chọn rồi tải danh sách lần đầu
            _view.NapTheLoai(await Task.Run(() => TheLoaiBLL.LayTatCa()));
            _view.NapTacGia(await Task.Run(() => TacGiaBLL.LayTatCa()));
            await TaiDuLieu();
        });
        _view.LocClicked += async (s, e) => await ChayAn(async () =>
        {
            _boLoc = new SachBoLoc { TuKhoa = _view.TuKhoaLoc, MaTheLoai = _view.MaTheLoaiLoc, TinhTrang = _view.TinhTrangLoc };
            _trang = 1;
            await TaiDuLieu();
        });
        _view.TrangChanged += async (s, trang) => await ChayAn(async () => { _trang = trang; await TaiDuLieu(); });
        _view.ThemClicked += (s, e) => _view.MoFormThem();
        _view.SuaClicked += async (s, ma) => await ChayAn(async () =>
        {
            var sach = await Task.Run(() => SachBLL.LayTheoMa(ma));
            if (sach == null) throw new NghiepVuException("Không tìm thấy đầu sách. Danh sách có thể đã thay đổi, vui lòng tải lại.");
            _view.MoFormSua(sach);
        });
        _view.HuyClicked += (s, e) => _view.DongForm();
        _view.LuuClicked += async (s, e) => await ChayAn(LuuSach);
        _view.NhapDanhMucClicked += async (s, e) => await ChayAn(NhapDanhMuc);
        _view.AnhBiaDaChon += (s, e) =>
        {
            // Kiểm tra dung lượng ảnh ngay khi chọn
            try { SachBLL.KiemTraAnhBia(_view.AnhBiaMoi); }
            catch (NghiepVuException ex) { _view.XoaAnhBiaMoi(); _view.HienThiLoi(ex.Message); }
        };
    }

    // Tải danh sách trang hiện tại và thống kê
    private async Task TaiDuLieu()
    {
        _view.DatTrangThaiDangXuLy(true);
        try
        {
            var ds = await Task.Run(() => SachBLL.TimKiem(_boLoc, _trang));
            // Trang hiện tại vượt quá số trang (ví dụ sau khi lọc) thì lùi về trang cuối
            if (ds.DuLieu.Count == 0 && _trang > 1 && ds.TongSoDong > 0)
            {
                _trang = Math.Max(1, ds.TongSoTrang);
                ds = await Task.Run(() => SachBLL.TimKiem(_boLoc, _trang));
            }
            var tk = await Task.Run(() => SachBLL.ThongKe());
            _view.HienThiThongKe(tk);
            _view.HienThiDanhSach(ds);
        }
        finally { _view.DatTrangThaiDangXuLy(false); }
    }

    // Lưu thêm mới hoặc chỉnh sửa
    private async Task LuuSach()
    {
        // Đổi năm xuất bản (chuỗi) sang số; để trống = chưa nhập
        int nam = 0;
        string namText = (_view.NamXuatBan ?? "").Trim();
        if (namText.Length > 0 && !int.TryParse(namText, out nam)) throw new NghiepVuException("Năm xuất bản phải là số, ví dụ 2023.");

        var sach = new Sach
        {
            MaSach = _view.MaSachDangSua, TenSach = _view.TenSach, MaTacGia = _view.MaTacGia, MaTheLoai = _view.MaTheLoai,
            NamXuatBan = nam, TongSoBan = _view.TongSoBan, ViTriKe = _view.ViTriKe, NhaXuatBan = _view.NhaXuatBan,
            NgonNgu = _view.NgonNgu, TinhTrangLuuHanh = _view.TinhTrangLuuHanh, AnhBia = _view.AnhBiaMoi
        };
        bool them = string.IsNullOrEmpty(sach.MaSach);
        _view.DatTrangThaiDangXuLy(true);
        try
        {
            if (them)
            {
                string ma = await Task.Run(() => SachBLL.Them(sach));
                _view.HienThiThongTin($"Đã thêm đầu sách {ma}.");
            }
            else
            {
                // Chỉ ghi đè ảnh khi người dùng chọn ảnh mới
                await Task.Run(() => SachBLL.Sua(sach, sach.AnhBia != null));
                _view.HienThiThongTin($"Đã lưu thay đổi đầu sách {sach.MaSach}.");
            }
        }
        finally { _view.DatTrangThaiDangXuLy(false); }
        _view.DongForm();
        await TaiDuLieu();
    }

    // Nhập danh mục từ file CSV
    private async Task NhapDanhMuc()
    {
        string duongDan = _view.ChonFileNhap();
        if (string.IsNullOrEmpty(duongDan)) return;

        List<string[]> dong;
        int soDongDau;
        try { dong = DocCsv(duongDan, out soDongDau); }
        catch (Exception) { throw new NghiepVuException("Không thể đọc tệp đã chọn. Hãy đóng tệp nếu đang mở bằng chương trình khác rồi thử lại."); }

        _view.DatTrangThaiDangXuLy(true);
        KetQuaNhapDanhMuc kq;
        try { kq = await Task.Run(() => SachBLL.NhapDanhMuc(dong, soDongDau)); }
        finally { _view.DatTrangThaiDangXuLy(false); }

        var sb = new StringBuilder($"Đã nhập thành công {kq.SoThanhCong} đầu sách.");
        if (kq.Loi.Count > 0)
        {
            sb.Append($"\n\nCó {kq.Loi.Count} dòng bị bỏ qua:\n");
            sb.Append(string.Join("\n", kq.Loi.Take(10)));
            if (kq.Loi.Count > 10) sb.Append($"\n... và {kq.Loi.Count - 10} dòng khác.");
        }
        _view.HienThiThongTin(sb.ToString());
        _trang = 1;
        await TaiDuLieu();
    }

    // Đọc CSV (UTF-8), tự nhận dấu phân cách ; hoặc , và bỏ dòng tiêu đề nếu có
    private static List<string[]> DocCsv(string duongDan, out int soDongDau)
    {
        var dong = File.ReadAllLines(duongDan, Encoding.UTF8).ToList();
        if (dong.Count == 0) { soDongDau = 1; return new List<string[]>(); }
        char phanCach = dong[0].Count(c => c == ';') >= dong[0].Count(c => c == ',') ? ';' : ',';
        var kq = dong.Select(d => TachDong(d, phanCach)).ToList();

        // Dòng đầu là tiêu đề nếu ô đầu chứa chữ "tên sách" / "tensach"
        string o1 = kq[0].Length > 0 ? kq[0][0].Trim().Trim('﻿').ToLowerInvariant() : "";
        soDongDau = 1;
        if (o1.Contains("tên sách") || o1.Contains("tensach")) { kq.RemoveAt(0); soDongDau = 2; }
        return kq;
    }

    // Tách một dòng CSV, hỗ trợ giá trị đặt trong dấu ngoặc kép
    private static string[] TachDong(string dong, char phanCach)
    {
        var o = new List<string>();
        var cur = new StringBuilder();
        bool trongNgoac = false;
        for (int i = 0; i < dong.Length; i++)
        {
            char c = dong[i];
            if (c == '"')
            {
                if (trongNgoac && i + 1 < dong.Length && dong[i + 1] == '"') { cur.Append('"'); i++; }
                else trongNgoac = !trongNgoac;
            }
            else if (c == phanCach && !trongNgoac) { o.Add(cur.ToString()); cur.Clear(); }
            else cur.Append(c);
        }
        o.Add(cur.ToString());
        return o.ToArray();
    }

    // Bọc xử lý có bắt lỗi: lỗi nghiệp vụ hiện nguyên văn, lỗi khác hiện thông báo chung
    private async Task ChayAn(Func<Task> viec)
    {
        try { await viec(); }
        catch (NghiepVuException ex) { _view.HienThiLoi(ex.Message); }
        catch (Exception ex)
        {
            _view.HienThiLoi("Đã xảy ra lỗi khi xử lý dữ liệu. Vui lòng kiểm tra kết nối SQL Server rồi thử lại.\n(Chi tiết: " + ex.Message + ")");
        }
    }
}

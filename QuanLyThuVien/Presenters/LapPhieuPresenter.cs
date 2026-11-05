using BLL;
using DTO;
using DTO.Models;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>
/// Presenter màn hình Lập phiếu mượn. Phiếu nháp lưu ở PhieuNhap (dùng chung với màn Tìm kiếm sách);
/// mọi kiểm tra (thẻ, quá hạn, hạn mức, tồn kho) do BLL thực hiện, Presenter chỉ tổng hợp để hiển thị.
/// </summary>
public class LapPhieuPresenter
{
    private readonly ILapPhieuView _v;
    private readonly IDieuHuong _nav;
    private KetQuaKiemTraDocGia _docGia;

    public LapPhieuPresenter(ILapPhieuView view, IDieuHuong dieuHuong)
    {
        _v = view;
        _nav = dieuHuong;
        _v.Loaded += async (s, e) => await Chay(KhoiTao);
        _v.DocGiaChanged += async (s, e) => await Chay(async () =>
        {
            PhieuNhap.MaDocGia = _v.MaDocGiaChon;
            await CapNhatDocGia();
            await LamMoi();
        });
        _v.NgayMuonChanged += async (s, e) => await Chay(async () =>
        {
            _v.DatHenTra(PhieuMuonBLL.TinhHenTraMacDinh(_v.NgayMuon));   // hẹn trả mặc định = ngày mượn + 14 ngày
            await CapNhatMaPhieu();
            await LamMoi();
        });
        _v.HenTraChanged += async (s, e) => await Chay(LamMoi);
        _v.GhiChuChanged += (s, e) => PhieuNhap.GhiChu = _v.GhiChu;
        _v.TimSachClicked += async (s, e) => await Chay(TimSach);
        _v.ThemSachClicked += async (s, ma) => await Chay(async () => { await DatSoLuong(ma, PhieuNhap.SoLuongCua(ma) + 1); });
        _v.DoiSoLuongClicked += async (s, a) => await Chay(async () =>
        {
            int moi = PhieuNhap.SoLuongCua(a.MaSach) + a.Delta;
            if (moi >= 1) await DatSoLuong(a.MaSach, moi);
        });
        _v.XoaSachClicked += async (s, ma) => await Chay(async () => { PhieuNhap.Dat(ma, 0); await LamMoi(); });
        _v.ThemTuDanhMucClicked += (s, e) => _nav.DieuHuong(MucMenu.TimKiemSach);
        _v.TaoPhieuClicked += async (s, e) => await Chay(TaoPhieu);
        _v.HuyClicked += (s, e) =>
        {
            if (PhieuNhap.SoDauSach > 0 && !_v.XacNhan("Hủy phiếu đang lập? Các sách đã chọn sẽ bị bỏ.")) return;
            PhieuNhap.XoaTatCa();
            _nav.DieuHuong(MucMenu.PhieuMuon);
        };
    }

    private Task Chay(Func<Task> viec) => PresenterHelper.ChayAn(_v, viec);

    // Nạp danh sách độc giả, khôi phục phiếu nháp và tính các giá trị mặc định
    private async Task KhoiTao()
    {
        var ds = await Task.Run(() => DocGiaBLL.LayTatCaDeXuat(new DocGiaBoLoc()));
        _v.NapDocGia(ds.Select(d => new LuaChonDocGia { Ma = d.MaDocGia, Nhan = $"{d.MaDocGia}  {d.HoTen}" }).ToList());
        if (!string.IsNullOrEmpty(PhieuNhap.MaDocGia)) _v.ChonDocGia(PhieuNhap.MaDocGia);
        _v.DatGhiChu(PhieuNhap.GhiChu);
        _v.DatHenTra(PhieuMuonBLL.TinhHenTraMacDinh(_v.NgayMuon));
        await CapNhatDocGia();
        await CapNhatMaPhieu();
        await LamMoi();
    }

    private async Task CapNhatMaPhieu()
    {
        DateTime ngay = _v.NgayMuon;
        _v.HienThiMaPhieu(await Task.Run(() => PhieuMuonBLL.TaoMaPhieuDuKien(ngay)));
    }

    // Kiểm tra độc giả đang chọn (thẻ, hạn mức, quá hạn)
    private async Task CapNhatDocGia()
    {
        string ma = _v.MaDocGiaChon;
        _docGia = string.IsNullOrEmpty(ma) ? null : await Task.Run(() => PhieuMuonBLL.KiemTraDocGia(ma));
        _v.HienThiThongTinDocGia(_docGia);
    }

    private async Task TimSach()
    {
        string tuKhoa = _v.TuKhoaSach;
        var kq = await Task.Run(() => SachBLL.TimKiem(new SachBoLoc { TuKhoa = tuKhoa }, 1, 5));
        _v.HienThiKetQuaTim(kq.DuLieu);
    }

    // Đặt số lượng một đầu sách trong phiếu nháp sau khi BLL xác nhận không vượt tồn kho
    private async Task DatSoLuong(string ma, int soLuong)
    {
        string loi = await Task.Run(() => PhieuMuonBLL.KiemTraSoLuong(ma, soLuong));
        if (loi != null) throw new NghiepVuException(loi);
        PhieuNhap.Dat(ma, soLuong);
        await LamMoi();
    }

    // Dựng lại danh sách dòng sách và panel "Kiểm tra phiếu"
    private async Task LamMoi()
    {
        var dong = PhieuNhap.Dong;
        var hienThi = new List<DongPhieuHienThi>();
        foreach (var d in dong)
        {
            var s = await Task.Run(() => SachBLL.LayTheoMa(d.MaSach));
            string loi = s == null ? "Không tìm thấy sách trong danh mục." : await Task.Run(() => PhieuMuonBLL.KiemTraSoLuong(d.MaSach, d.SoLuong));
            hienThi.Add(new DongPhieuHienThi
            {
                MaSach = d.MaSach, TenSach = s?.TenSach ?? d.MaSach, TenTacGia = s?.TenTacGia, Con = s?.SoBanCoSan ?? 0,
                Tong = s?.TongSoBan ?? 0, SoLuong = d.SoLuong, Loi = loi
            });
        }
        _v.HienThiDongPhieu(hienThi);

        // Tổng hợp kiểm tra
        int tongCuon = dong.Sum(d => d.SoLuong);
        int soLoiTon = hienThi.Count(h => h.Loi != null);
        bool coDocGia = _docGia != null;
        bool henTraHopLe = _v.HenTra.Date > _v.NgayMuon.Date;
        bool trongHanMuc = coDocGia && _docGia.SoDangMuon + tongCuon <= _docGia.HanMuc;
        var kt = new KiemTraPhieuHienThi
        {
            MaPhieu = null, TenDocGia = _docGia?.DocGia.HoTen ?? "Chưa chọn", SoDauSach = dong.Count, TongCuon = tongCuon,
            HanMuc = HangSo.HanMucSachMoiDocGia, SoNgay = (_v.HenTra.Date - _v.NgayMuon.Date).Days
        };
        kt.Muc.Add(new MucKiemTra { Nhan = "Thẻ còn hiệu lực", Dat = coDocGia && _docGia.TheConHan });
        kt.Muc.Add(new MucKiemTra { Nhan = "Không có phiếu quá hạn", Dat = coDocGia && !_docGia.CoQuaHan });
        kt.Muc.Add(new MucKiemTra { Nhan = coDocGia && !trongHanMuc ? "Vượt hạn mức mượn" : "Trong hạn mức mượn", Dat = trongHanMuc });
        kt.Muc.Add(new MucKiemTra { Nhan = "Ngày hẹn trả sau ngày mượn", Dat = henTraHopLe });
        kt.Muc.Add(new MucKiemTra { Nhan = soLoiTon > 0 ? $"{soLoiTon} đầu sách không đủ tồn" : "Đủ tồn kho cho các đầu sách", Dat = soLoiTon == 0 && dong.Count > 0 });

        // Lý do chưa tạo được phiếu (ưu tiên theo thứ tự)
        var dongLoi = hienThi.FirstOrDefault(h => h.Loi != null);
        if (!coDocGia) kt.LoiChinh = "Chưa chọn độc giả.";
        else if (_docGia.LyDo != null) kt.LoiChinh = _docGia.LyDo;
        else if (dong.Count == 0) kt.LoiChinh = "Phiếu chưa có sách nào. Vui lòng tìm và thêm sách.";
        else if (dongLoi != null)
            kt.LoiChinh = $"Chưa thể tạo phiếu: số lượng vượt tồn kho. {dongLoi.MaSach} yêu cầu {dongLoi.SoLuong} bản nhưng chỉ còn {dongLoi.Con} bản. Phiếu chỉ được tạo sau khi điều chỉnh hợp lệ.";
        else if (!trongHanMuc) kt.LoiChinh = $"Vượt hạn mức: độc giả đang mượn {_docGia.SoDangMuon}, chọn thêm {tongCuon}, tối đa {_docGia.HanMuc} cuốn.";
        else if (!henTraHopLe) kt.LoiChinh = "Ngày hẹn trả phải sau ngày mượn.";
        kt.ChoPhepTao = kt.LoiChinh == null;
        if (kt.ChoPhepTao) kt.ThongBaoSan = $"Phiếu có {dong.Count} đầu sách, tổng {tongCuon} cuốn và sẵn sàng tạo.";
        _v.HienThiKiemTra(kt);

        // Tiến độ các bước
        int buoc = 0;
        if (coDocGia && _docGia.HopLe) buoc = 1;
        if (buoc >= 1 && dong.Count > 0) buoc = 2;
        if (kt.ChoPhepTao) buoc = 3;
        _v.HienThiBuoc(buoc);
    }

    // Tạo phiếu rồi chuyển sang màn Xem trước bản in
    private async Task TaoPhieu()
    {
        string maDocGia = _v.MaDocGiaChon;
        DateTime muon = _v.NgayMuon, hen = _v.HenTra;
        string ghiChu = _v.GhiChu;
        var dong = PhieuNhap.Dong;
        _v.DatTrangThaiDangXuLy(true);
        string maPhieu;
        try { maPhieu = await Task.Run(() => PhieuMuonBLL.TaoPhieu(maDocGia, muon, hen, ghiChu, dong)); }
        finally { _v.DatTrangThaiDangXuLy(false); }
        PhieuNhap.XoaTatCa();
        _nav.LamMoiHuyHieuQuaHan();
        _nav.DieuHuong(MucMenu.XemTruocPhieu, maPhieu);
    }
}

using DAL;
using DTO;
using DTO.Entities;
using DTO.Models;

namespace BLL;

/// <summary>Nghiệp vụ phiếu mượn: kiểm tra độc giả, lập phiếu, tra cứu, thống kê.</summary>
public static class PhieuMuonBLL
{
    public static KetQuaPhanTrang<PhieuMuonHienThi> TimKiem(PhieuBoLoc boLoc, int trang, int kichThuoc = 7)
    {
        PhienDangNhap.YeuCauDangNhap();
        return PhieuMuonDAL.TimKiem(boLoc, DateTime.Today, Math.Max(1, trang), kichThuoc);
    }

    public static List<PhieuMuonHienThi> LayTatCaDeXuat(PhieuBoLoc boLoc) =>
        PhieuMuonDAL.TimKiem(boLoc, DateTime.Today, 1, 1_000_000).DuLieu;

    public static ThongKePhieu ThongKe() => ThongKeDAL.LayPhieu(DateTime.Today);

    /// <summary>Lấy phiếu đầy đủ (phiếu + độc giả + các bản sách) cho màn Trả sách / Xem trước bản in.</summary>
    public static PhieuMuonDayDu LayDayDu(string maPhieu)
    {
        PhienDangNhap.YeuCauDangNhap();
        maPhieu = (maPhieu ?? "").Trim();
        if (maPhieu.Length == 0) throw new NghiepVuException("Vui lòng nhập mã phiếu.");
        var phieu = PhieuMuonDAL.LayTheoMa(maPhieu, DateTime.Today);
        if (phieu == null) throw new NghiepVuException($"Không tìm thấy phiếu {maPhieu}.");
        return new PhieuMuonDayDu
        {
            Phieu = phieu,
            DocGia = DocGiaDAL.LayTheoMa(phieu.MaDocGia, DateTime.Today),
            ChiTiet = ChiTietMuonDAL.LayTheoPhieu(maPhieu)
        };
    }

    /// <summary>Ngày hẹn trả mặc định = ngày mượn + 14 ngày.</summary>
    public static DateTime TinhHenTraMacDinh(DateTime ngayMuon) => ngayMuon.Date.AddDays(HangSo.SoNgayMuonMacDinh);

    /// <summary>Mã phiếu dự kiến dạng PMyyMMdd-NNN (NNN là số thứ tự trong ngày).</summary>
    public static string TaoMaPhieuDuKien(DateTime ngayMuon)
    {
        string tienTo = "PM" + ngayMuon.ToString("yyMMdd") + "-";
        string maCuoi = PhieuMuonDAL.LayMaPhieuCuoiTrongNgay(ngayMuon);
        return SinhMa.TiepTheo(maCuoi, tienTo, 3);
    }

    /// <summary>Kiểm tra độc giả trước khi lập phiếu: thẻ còn hạn, không có phiếu quá hạn, còn hạn mức.</summary>
    public static KetQuaKiemTraDocGia KiemTraDocGia(string maDocGia)
    {
        PhienDangNhap.YeuCauDangNhap();
        if (string.IsNullOrWhiteSpace(maDocGia)) throw new NghiepVuException("Vui lòng chọn độc giả.");
        var dg = DocGiaDAL.LayTheoMa(maDocGia.Trim(), DateTime.Today);
        if (dg == null) throw new NghiepVuException("Không tìm thấy độc giả.");

        var kq = new KetQuaKiemTraDocGia
        {
            DocGia = dg,
            SoDangMuon = dg.SoSachDangMuon,
            HanMuc = HangSo.HanMucSachMoiDocGia,
            CoQuaHan = dg.SoPhieuQuaHan > 0,
            TheConHan = dg.HanThe == null || dg.HanThe.Value.Date >= DateTime.Today
        };
        if (!kq.TheConHan) kq.LyDo = "Thẻ độc giả đã hết hạn. Vui lòng gia hạn thẻ trước khi mượn.";
        else if (kq.CoQuaHan) kq.LyDo = "Độc giả đang có phiếu quá hạn. Vui lòng trả sách trước khi mượn thêm.";
        else if (kq.SoDangMuon >= kq.HanMuc) kq.LyDo = $"Độc giả đã mượn đủ {kq.HanMuc} cuốn.";
        kq.HopLe = kq.LyDo == null;
        return kq;
    }

    /// <summary>
    /// Kiểm tra số lượng của một dòng sách (dùng khi bấm tăng/giảm): không vượt số bản còn.
    /// Trả về null nếu hợp lệ, hoặc câu thông báo lỗi.
    /// </summary>
    public static string KiemTraSoLuong(string maSach, int soLuong)
    {
        var s = SachDAL.LayTheoMa(maSach);
        if (s == null) return "Không tìm thấy sách.";
        if (s.TinhTrangLuuHanh != HangSo.LuuHanhChoMuon) return $"Sách {maSach} chỉ đọc tại chỗ, không cho mượn.";
        if (soLuong < 1) return "Số lượng phải từ 1 trở lên.";
        if (soLuong > s.SoBanCoSan)
            return s.SoBanCoSan == 0 ? $"Sách {maSach} đã hết bản." : $"Chỉ còn {s.SoBanCoSan} bản. Vui lòng giảm số lượng xuống {s.SoBanCoSan}.";
        return null;
    }

    /// <summary>
    /// Tạo phiếu mượn. Kiểm tra: danh sách sách, độc giả hợp lệ, hạn mức, tồn kho, ngày hẹn trả.
    /// Thành công trả về mã phiếu mới.
    /// </summary>
    public static string TaoPhieu(string maDocGia, DateTime ngayMuon, DateTime henTra, string ghiChu, List<DongSachMuon> dong)
    {
        PhienDangNhap.YeuCauDangNhap();
        if (dong == null || dong.Count == 0) throw new NghiepVuException("Phiếu chưa có sách nào. Vui lòng thêm ít nhất một đầu sách.");

        // Gộp dòng trùng mã sách và kiểm tra số lượng từng dòng
        var gop = dong.GroupBy(d => d.MaSach).Select(g => new DongSachMuon { MaSach = g.Key, SoLuong = g.Sum(x => x.SoLuong) }).ToList();
        foreach (var d in gop)
        {
            string loi = KiemTraSoLuong(d.MaSach, d.SoLuong);
            if (loi != null) throw new NghiepVuException(loi);
        }

        var kt = KiemTraDocGia(maDocGia);
        if (!kt.HopLe) throw new NghiepVuException(kt.LyDo);
        int tongCuon = gop.Sum(d => d.SoLuong);
        if (kt.SoDangMuon + tongCuon > kt.HanMuc)
            throw new NghiepVuException($"Vượt hạn mức: độc giả đang mượn {kt.SoDangMuon}, chọn thêm {tongCuon}, tối đa {kt.HanMuc} cuốn.");

        if (henTra.Date <= ngayMuon.Date) throw new NghiepVuException("Ngày hẹn trả phải sau ngày mượn.");
        ghiChu = (ghiChu ?? "").Trim();
        if (ghiChu.Length > 255) throw new NghiepVuException("Ghi chú phiếu tối đa 255 ký tự.");

        var pm = new PhieuMuon
        {
            MaPhieu = TaoMaPhieuDuKien(ngayMuon),
            MaDocGia = maDocGia.Trim(),
            MaTaiKhoan = PhienDangNhap.NguoiDungHienTai.MaTaiKhoan,   // thủ thư đang đăng nhập
            NgayMuon = ngayMuon.Date,
            HenTra = henTra.Date,
            TrangThai = HangSo.PhieuDangMuon,
            GhiChu = ghiChu
        };
        try
        {
            PhieuMuonDAL.TaoPhieu(pm, gop);
        }
        catch (InvalidOperationException ex)
        {
            // Lỗi tồn kho phát sinh ở transaction -> chuyển thành thông báo thân thiện
            throw new NghiepVuException(ex.Message);
        }
        return pm.MaPhieu;
    }
}

using DAL;
using DTO;
using DTO.Models;

namespace BLL;

/// <summary>Nghiệp vụ ghi nhận trả sách (trả một phần hoặc toàn bộ phiếu).</summary>
public static class TraSachBLL
{
    private static readonly string[] TinhTrangHopLe = { HangSo.TinhTrangTot, HangSo.TinhTrangHuHong, HangSo.TinhTrangMat };

    /// <summary>Lấy phiếu cần trả; báo lỗi nếu phiếu không còn bản nào đang mượn.</summary>
    public static PhieuMuonDayDu LayPhieuDeTra(string maPhieu)
    {
        var p = PhieuMuonBLL.LayDayDu(maPhieu);
        if (p.ChiTiet.All(c => !c.DangMuon))
            throw new NghiepVuException($"Phiếu {p.Phieu.MaPhieu} đã trả đủ sách.");
        return p;
    }

    /// <summary>Tính tóm tắt trước khi xác nhận (số tốt / hư hỏng / mất / còn mượn) để hiển thị.</summary>
    public static KetQuaTraSach TomTat(PhieuMuonDayDu phieu, List<ChiTietTraSach> chon)
    {
        var kq = new KetQuaTraSach
        {
            SoTot = chon.Count(c => c.TinhTrang == HangSo.TinhTrangTot),
            SoHuHong = chon.Count(c => c.TinhTrang == HangSo.TinhTrangHuHong),
            SoMat = chon.Count(c => c.TinhTrang == HangSo.TinhTrangMat)
        };
        kq.SoConMuon = phieu.ChiTiet.Count(c => c.DangMuon) - chon.Count;
        kq.TrangThaiPhieu = kq.SoConMuon == 0 ? HangSo.PhieuDaTra : HangSo.PhieuTraMotPhan;
        return kq;
    }

    /// <summary>Xác nhận trả các bản đã chọn. Kiểm tra tình trạng, lý do hư hỏng/mất, ngày trả.</summary>
    public static KetQuaTraSach XacNhanTra(string maPhieu, List<ChiTietTraSach> chon, DateTime ngayTra, string ghiChuChung)
    {
        PhienDangNhap.YeuCauDangNhap();
        var p = LayPhieuDeTra(maPhieu);
        if (chon == null || chon.Count == 0) throw new NghiepVuException("Vui lòng chọn ít nhất một cuốn để trả.");

        foreach (var c in chon)
        {
            // Bản sách phải thuộc phiếu và đang mượn
            var goc = p.ChiTiet.FirstOrDefault(x => x.MaSach == c.MaSach && x.MaBanSach == c.MaBanSach);
            if (goc == null || !goc.DangMuon)
                throw new NghiepVuException($"Bản sách {c.MaBanSach} không có trong phiếu hoặc đã được trả.");
            if (Array.IndexOf(TinhTrangHopLe, c.TinhTrang) < 0)
                throw new NghiepVuException($"Vui lòng chọn tình trạng khi trả cho bản {c.MaBanSach}.");
            c.GhiChu = (c.GhiChu ?? "").Trim();
            if (c.TinhTrang != HangSo.TinhTrangTot && c.GhiChu.Length == 0)
                throw new NghiepVuException($"Bản {c.MaBanSach}: sách {c.TinhTrang.ToLower()} cần ghi rõ lý do.");
            if (c.GhiChu.Length > 255) throw new NghiepVuException("Ghi chú từng cuốn tối đa 255 ký tự.");
        }
        if (chon.Select(c => c.MaBanSach).Distinct().Count() != chon.Count)
            throw new NghiepVuException("Danh sách trả có bản sách bị trùng.");

        if (ngayTra.Date < p.Phieu.NgayMuon.Value.Date) throw new NghiepVuException("Ngày trả không được trước ngày mượn.");
        if (ngayTra.Date > DateTime.Today) throw new NghiepVuException("Ngày trả không được ở tương lai.");

        // Ghép ghi chú chung vào ghi chú phiếu (nếu có)
        ghiChuChung = (ghiChuChung ?? "").Trim();
        string ghiChuThem = ghiChuChung.Length == 0 ? "" : $" | Trả {ngayTra:dd/MM/yyyy}: {ghiChuChung}";

        try
        {
            return PhieuMuonDAL.TraSach(p.Phieu.MaPhieu, chon, ngayTra, ghiChuThem, DateTime.Today);
        }
        catch (InvalidOperationException ex)
        {
            throw new NghiepVuException(ex.Message);
        }
    }
}

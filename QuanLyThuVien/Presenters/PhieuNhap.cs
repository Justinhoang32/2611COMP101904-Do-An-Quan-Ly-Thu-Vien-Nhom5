using DTO.Models;

namespace QuanLyThuVien.Presenters;

/// <summary>
/// Phiếu mượn nháp dùng chung giữa màn "Tìm kiếm sách" (thêm sách vào phiếu) và "Lập phiếu mượn".
/// Chỉ lưu trong bộ nhớ của phiên làm việc. Mọi kiểm tra nghiệp vụ vẫn do BLL thực hiện.
/// </summary>
public static class PhieuNhap
{
    private static readonly List<DongSachMuon> _dong = new();

    /// <summary>Độc giả đã chọn cho phiếu nháp (có thể null).</summary>
    public static string MaDocGia { get; set; }

    /// <summary>Ghi chú phiếu nháp.</summary>
    public static string GhiChu { get; set; }

    /// <summary>Phát ra mỗi khi danh sách sách trong phiếu thay đổi.</summary>
    public static event EventHandler ThayDoi;

    /// <summary>Bản sao danh sách dòng sách hiện tại.</summary>
    public static List<DongSachMuon> Dong => _dong.Select(d => new DongSachMuon { MaSach = d.MaSach, SoLuong = d.SoLuong }).ToList();

    public static int SoDauSach => _dong.Count;
    public static int TongSoCuon => _dong.Sum(d => d.SoLuong);

    /// <summary>Số lượng hiện có của một mã sách trong phiếu (0 nếu chưa có).</summary>
    public static int SoLuongCua(string maSach) => _dong.FirstOrDefault(d => d.MaSach == maSach)?.SoLuong ?? 0;

    /// <summary>Đặt số lượng cho một mã sách (thêm mới nếu chưa có; số lượng ≤ 0 thì xóa dòng).</summary>
    public static void Dat(string maSach, int soLuong)
    {
        var d = _dong.FirstOrDefault(x => x.MaSach == maSach);
        if (soLuong <= 0) { if (d != null) _dong.Remove(d); }
        else if (d == null) _dong.Add(new DongSachMuon { MaSach = maSach, SoLuong = soLuong });
        else d.SoLuong = soLuong;
        ThayDoi?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>Xóa toàn bộ phiếu nháp (sau khi tạo phiếu hoặc hủy lập phiếu).</summary>
    public static void XoaTatCa()
    {
        _dong.Clear();
        MaDocGia = null;
        GhiChu = null;
        ThayDoi?.Invoke(null, EventArgs.Empty);
    }
}

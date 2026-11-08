using DTO.Models;

namespace QuanLyThuVien.Views;

/// <summary>Giao diện màn hình Xem trước phiếu mượn (bản in).</summary>
public interface IXemTruocPhieuView : IViewCoThongBao
{
    CauHinhIn CauHinh { get; }

    event EventHandler Loaded;
    event EventHandler<string> MoPhieu;
    event EventHandler QuayLaiClicked;
    event EventHandler InClicked;
    event EventHandler LuuPdfClicked;
    event EventHandler CauHinhThayDoi;

    void HienThiPhieu(PhieuMuonDayDu p);
    void NapMayIn(List<string> ds, string macDinh);
    /// <summary>Hỏi nơi lưu tệp PDF; trả về null nếu hủy.</summary>
    string ChonNoiLuuPdf(string tenGoiY);
}

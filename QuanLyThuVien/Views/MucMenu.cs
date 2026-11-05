namespace QuanLyThuVien.Views;

/// <summary>Các màn hình trong khung chính (8 mục sidebar + 2 màn hình phụ + mục quản trị).</summary>
public enum MucMenu
{
    TongQuan,
    QuanLySach,
    TacGiaTheLoai,
    DocGia,
    PhieuMuon,
    GhiNhanTraSach,
    TimKiemSach,
    BaoCaoQuaHan,
    LapPhieuMuon,        // màn hình phụ của Phiếu mượn
    XemTruocPhieu        // màn hình phụ của Phiếu mượn
}

/// <summary>Yêu cầu chuyển màn hình, có thể kèm tham số (ví dụ mã phiếu cần trả).</summary>
public class YeuCauDieuHuong : EventArgs
{
    public MucMenu Muc { get; }
    public object ThamSo { get; }
    public YeuCauDieuHuong(MucMenu muc, object thamSo = null) { Muc = muc; ThamSo = thamSo; }
}

/// <summary>Các màn hình con dùng interface này để yêu cầu khung chính chuyển màn hình (qua Presenter).</summary>
public interface IDieuHuong
{
    void DieuHuong(MucMenu muc, object thamSo = null);
    void LamMoiHuyHieuQuaHan();
}

/// <summary>Màn hình con implement interface này để nhận tham số khi được mở từ màn hình khác.</summary>
public interface INhanThamSo
{
    void NhanThamSo(object thamSo);
}

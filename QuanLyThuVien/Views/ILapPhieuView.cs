using DTO.Models;

namespace QuanLyThuVien.Views;

/// <summary>Một lựa chọn trong ô chọn độc giả.</summary>
public class LuaChonDocGia
{
    public string Ma { get; set; }
    public string Nhan { get; set; }
    public override string ToString() => Nhan;
}

/// <summary>Một dòng sách trong phiếu đang lập, kèm tồn kho và thông báo lỗi (nếu số lượng không hợp lệ).</summary>
public class DongPhieuHienThi
{
    public string MaSach { get; set; }
    public string TenSach { get; set; }
    public string TenTacGia { get; set; }
    public int Con { get; set; }
    public int Tong { get; set; }
    public int SoLuong { get; set; }
    public string Loi { get; set; }
}

/// <summary>Một mục trong danh sách kiểm tra (đạt / không đạt).</summary>
public class MucKiemTra
{
    public string Nhan { get; set; }
    public bool Dat { get; set; }
}

/// <summary>Dữ liệu panel "Kiểm tra phiếu".</summary>
public class KiemTraPhieuHienThi
{
    public string MaPhieu { get; set; }
    public string TenDocGia { get; set; }
    public int SoDauSach { get; set; }
    public int TongCuon { get; set; }
    public int HanMuc { get; set; }
    public int SoNgay { get; set; }
    public List<MucKiemTra> Muc { get; set; } = new();
    public string LoiChinh { get; set; }       // null nếu hợp lệ
    public string ThongBaoSan { get; set; }    // hiện khi hợp lệ
    public bool ChoPhepTao { get; set; }
}

/// <summary>Tham số sự kiện tăng/giảm số lượng một dòng sách.</summary>
public class ThayDoiSoLuongArgs : EventArgs
{
    public string MaSach { get; set; }
    public int Delta { get; set; }
}

/// <summary>Giao diện màn hình Lập phiếu mượn.</summary>
public interface ILapPhieuView : IViewCoThongBao
{
    string MaDocGiaChon { get; }
    DateTime NgayMuon { get; }
    DateTime HenTra { get; }
    string GhiChu { get; }
    string TuKhoaSach { get; }

    event EventHandler Loaded;
    event EventHandler DocGiaChanged;
    event EventHandler NgayMuonChanged;
    event EventHandler HenTraChanged;
    event EventHandler GhiChuChanged;
    event EventHandler TimSachClicked;
    event EventHandler<string> ThemSachClicked;
    event EventHandler<ThayDoiSoLuongArgs> DoiSoLuongClicked;
    event EventHandler<string> XoaSachClicked;
    event EventHandler ThemTuDanhMucClicked;
    event EventHandler TaoPhieuClicked;
    event EventHandler HuyClicked;

    void NapDocGia(List<LuaChonDocGia> ds);
    void ChonDocGia(string ma);
    void DatGhiChu(string ghiChu);
    void DatHenTra(DateTime ngay);
    void HienThiMaPhieu(string ma);
    void HienThiThongTinDocGia(KetQuaKiemTraDocGia ketQua);   // null = chưa chọn
    void HienThiKetQuaTim(List<SachHienThi> ds);
    void HienThiDongPhieu(List<DongPhieuHienThi> ds);
    void HienThiKiemTra(KiemTraPhieuHienThi kt);
    void HienThiBuoc(int soBuocHoanThanh);
    bool XacNhan(string cauHoi);
}

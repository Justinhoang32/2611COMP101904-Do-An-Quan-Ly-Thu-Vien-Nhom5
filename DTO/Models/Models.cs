using DTO.Entities;

namespace DTO.Models;

// Các lớp "mô hình hiển thị": ghép nhiều bảng / tính toán thêm để View hiển thị. Không phải bảng trong DB.

/// <summary>Kết quả phân trang dùng chung cho mọi lưới dữ liệu.</summary>
public class KetQuaPhanTrang<T>
{
    public List<T> DuLieu { get; set; } = new();
    public int TongSoDong { get; set; }
    public int TrangHienTai { get; set; } = 1;
    public int KichThuocTrang { get; set; } = HangSo.KichThuocTrangMacDinh;
    public int TongSoTrang => KichThuocTrang <= 0 ? 0 : (int)Math.Ceiling(TongSoDong / (double)KichThuocTrang);
    public int TuDong => TongSoDong == 0 ? 0 : (TrangHienTai - 1) * KichThuocTrang + 1;
    public int DenDong => Math.Min(TrangHienTai * KichThuocTrang, TongSoDong);
}

/// <summary>Người dùng đăng nhập (tài khoản + tên vai trò).</summary>
public class NguoiDung : TaiKhoan
{
    public string TenVaiTro { get; set; }
}

/// <summary>Sách kèm tên tác giả, tên thể loại.</summary>
public class SachHienThi : Sach
{
    public string TenTacGia { get; set; }
    public string TenTheLoai { get; set; }
    public int SoBanDangMuon => TongSoBan - SoBanCoSan;
    public string TrangThaiHienThi => SoBanCoSan > 0 ? HangSo.SachCoSan : HangSo.SachHetSach;
}

public class TheLoaiHienThi : TheLoai { public int SoDauSach { get; set; } }
public class TacGiaHienThi : TacGia { public int SoDauSach { get; set; } }

/// <summary>Độc giả kèm số sách đang mượn, số phiếu quá hạn và trạng thái thẻ đã tính.</summary>
public class DocGiaHienThi : DocGia
{
    public int SoSachDangMuon { get; set; }
    public int SoPhieuQuaHan { get; set; }
    public string TrangThaiHienThi { get; set; }
}

/// <summary>Phiếu mượn kèm thông tin độc giả, thủ thư, số cuốn và trạng thái đã tính.</summary>
public class PhieuMuonHienThi : PhieuMuon
{
    public string HoTenDocGia { get; set; }
    public string NhomDocGia { get; set; }
    public string DienThoai { get; set; }
    public string TenThuThu { get; set; }
    public int TongSoCuon { get; set; }
    public int SoCuonDaTra { get; set; }
    public int SoCuonConMuon => TongSoCuon - SoCuonDaTra;
    public string TrangThaiHienThi { get; set; }   // Đang mượn / Trả một phần / Đã trả / Quá hạn
    public int SoNgayTre { get; set; }
}

/// <summary>Một bản sách trong phiếu, kèm tên sách và tác giả.</summary>
public class ChiTietPhieuHienThi : ChiTietMuon
{
    public string TenSach { get; set; }
    public string TenTacGia { get; set; }
    public bool DangMuon => TinhTrangKhiTra == HangSo.ChuaGhiNhan;
}

/// <summary>Phiếu đầy đủ: thông tin phiếu + độc giả + danh sách bản sách (dùng cho Trả sách và Xem trước bản in).</summary>
public class PhieuMuonDayDu
{
    public PhieuMuonHienThi Phieu { get; set; }
    public DocGiaHienThi DocGia { get; set; }
    public List<ChiTietPhieuHienThi> ChiTiet { get; set; } = new();
}

/// <summary>Dòng sách người dùng chọn khi lập phiếu (mã sách + số lượng).</summary>
public class DongSachMuon
{
    public string MaSach { get; set; }
    public int SoLuong { get; set; }
}

/// <summary>Một bản sách được chọn trả trong lần ghi nhận.</summary>
public class ChiTietTraSach
{
    public string MaSach { get; set; }
    public string MaBanSach { get; set; }
    public string TinhTrang { get; set; }   // Tốt / Hư hỏng / Mất
    public string GhiChu { get; set; }
}

public class KetQuaTraSach
{
    public int SoTot { get; set; }
    public int SoHuHong { get; set; }
    public int SoMat { get; set; }
    public int SoConMuon { get; set; }
    public string TrangThaiPhieu { get; set; }
}

/// <summary>Kết quả kiểm tra độc giả trước khi lập phiếu (thẻ, hạn mức, quá hạn).</summary>
public class KetQuaKiemTraDocGia
{
    public DocGiaHienThi DocGia { get; set; }
    public int SoDangMuon { get; set; }
    public int HanMuc { get; set; }
    public bool TheConHan { get; set; }
    public bool CoQuaHan { get; set; }
    public bool HopLe { get; set; }
    public string LyDo { get; set; }   // null nếu hợp lệ
}

// ---------- Bộ lọc ----------
public class SachBoLoc
{
    public string TuKhoa { get; set; }       // tìm chung (màn Quản lý sách)
    public string MaSach { get; set; }       // tìm riêng (màn Tìm kiếm sách)
    public string TenSach { get; set; }
    public string TenTacGia { get; set; }
    public string MaTheLoai { get; set; }
    public string TinhTrang { get; set; }    // null / "Có sẵn" / "Hết sách"
    public string TinhTrangLuuHanh { get; set; }
    public bool ChiConSach { get; set; }
}

public class DocGiaBoLoc
{
    public string TuKhoa { get; set; }
    public string Nhom { get; set; }         // null = tất cả
    public string TrangThai { get; set; }    // null = tất cả
}

public class PhieuBoLoc
{
    public string TuKhoa { get; set; }
    public string TrangThai { get; set; } = HangSo.TatCa;   // Tất cả / Đang mượn / Đã trả / Quá hạn
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
}

public class QuaHanBoLoc
{
    public DateTime NgayDoiChieu { get; set; } = DateTime.Today;
    public string Nhom { get; set; }              // null = tất cả
    public int SoNgayTreToiThieu { get; set; } = 1;
    public int? SoNgayTreToiDa { get; set; }      // null = không giới hạn
    public string TuKhoa { get; set; }
}

// ---------- Thống kê ----------
public class ThongKeSach
{
    public int SoDauSach { get; set; }
    public int SoBanSach { get; set; }
    public int SoDauSachHetBan { get; set; }
}

public class ThongKeDocGia
{
    public int DangHoatDong { get; set; }
    public int SinhVien { get; set; }
    public int GiangVienCanBo { get; set; }
    public int SapHetHan { get; set; }
}

public class ThongKePhieu
{
    public int TongPhieu => DungHan + QuaHan + DaTraTatCa;
    public int DungHan { get; set; }          // đang mượn đúng hạn (tab "Đang mượn")
    public int QuaHan { get; set; }
    public int DaTraTatCa { get; set; }
    public int DaTraTrongThang { get; set; }
    public int DenHanHomNay { get; set; }
    public int CuonLuuHanh { get; set; }
    public int CuonQuaHan { get; set; }
    public int CuonDaTraTrongThang { get; set; }
    public int CuonDenHan { get; set; }
}

public class ThongKeThang
{
    public int Thang { get; set; }
    public int GiaTri { get; set; }
    public int GiaTri2 { get; set; }
}

public class ThongKeTongQuan
{
    public ThongKeSach Sach { get; set; }
    public ThongKeDocGia DocGia { get; set; }
    public ThongKePhieu Phieu { get; set; }
    public int SoTheLoai { get; set; }
    public int TongDocGia { get; set; }
    public int DocGiaMoiTrongThang { get; set; }
    public int SachHuHong { get; set; }
    public int SachMat { get; set; }
    public List<ThongKeThang> LuotMuonTheoThang { get; set; } = new();
    public int TongLuotMuonNam => LuotMuonTheoThang.Sum(t => t.GiaTri);
    public int SoViecCanXuLy => Phieu.QuaHan + DocGia.SapHetHan + SachHuHong + SachMat;
}

/// <summary>Một dòng của Báo cáo quá hạn (mỗi phiếu quá hạn một dòng).</summary>
public class QuaHanHienThi
{
    public string MaPhieu { get; set; }
    public DateTime NgayMuon { get; set; }
    public DateTime HenTra { get; set; }
    public string MaDocGia { get; set; }
    public string HoTen { get; set; }
    public string NhomDocGia { get; set; }
    public string DienThoai { get; set; }
    public int SoNgayTre { get; set; }
    public int SoCuon { get; set; }
    public string DanhSachSach { get; set; }   // mỗi sách một dòng: "S001 Tâm lý học đại cương"
}

public class ThongKeQuaHan
{
    public int SoPhieu { get; set; }
    public int SoCuon { get; set; }
    public int SoDocGia { get; set; }
    public int SoSinhVien { get; set; }
    public int SoGiangVienCanBo { get; set; }
    public int SoNgayTreNhieuNhat { get; set; }
}

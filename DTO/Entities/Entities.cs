namespace DTO.Entities;

// Mỗi lớp dưới đây khớp CHÍNH XÁC với một bảng trong database (tên thuộc tính = tên cột).

/// <summary>Bảng VaiTro.</summary>
public class VaiTro
{
    public int MaVaiTro { get; set; }
    public string TenVaiTro { get; set; }
}

/// <summary>Bảng TaiKhoan (người dùng hệ thống).</summary>
public class TaiKhoan
{
    public int MaTaiKhoan { get; set; }
    public string TenDangNhap { get; set; }
    public string MatKhau { get; set; }
    public string HoTen { get; set; }
    public int MaVaiTro { get; set; }
}

/// <summary>Bảng TheLoai.</summary>
public class TheLoai
{
    public string MaTheLoai { get; set; }
    public string TenTheLoai { get; set; }
    public string MoTa { get; set; }
}

/// <summary>Bảng TacGia.</summary>
public class TacGia
{
    public string MaTacGia { get; set; }
    public string TenTacGia { get; set; }
    public string GioiThieu { get; set; }
}

/// <summary>Bảng Sach.</summary>
public class Sach
{
    public string MaSach { get; set; }
    public string TenSach { get; set; }
    public string MaTacGia { get; set; }
    public string MaTheLoai { get; set; }
    public int NamXuatBan { get; set; }          // 0 = chưa nhập
    public int TongSoBan { get; set; }
    public int SoBanCoSan { get; set; }
    public string ViTriKe { get; set; }
    public string NhaXuatBan { get; set; }
    public string NgonNgu { get; set; }
    public byte[] AnhBia { get; set; }           // ảnh bìa lưu trong DB (VARBINARY)
    public string TinhTrangLuuHanh { get; set; }
}

/// <summary>Bảng DocGia.</summary>
public class DocGia
{
    public string MaDocGia { get; set; }
    public string HoTen { get; set; }
    public string NhomDocGia { get; set; }
    public string MaSinhVien { get; set; }
    public string Khoa { get; set; }
    public string DienThoai { get; set; }
    public string Email { get; set; }
    public DateTime? NgayCap { get; set; }
    public DateTime? HanThe { get; set; }
    public string TrangThai { get; set; }
}

/// <summary>Bảng PhieuMuon.</summary>
public class PhieuMuon
{
    public string MaPhieu { get; set; }
    public string MaDocGia { get; set; }
    public int MaTaiKhoan { get; set; }
    public DateTime? NgayMuon { get; set; }
    public DateTime? HenTra { get; set; }
    public string TrangThai { get; set; }
    public string GhiChu { get; set; }
}

/// <summary>Bảng ChiTietMuon (mỗi dòng là MỘT bản sách cụ thể).</summary>
public class ChiTietMuon
{
    public string MaPhieu { get; set; }
    public string MaSach { get; set; }
    public string MaBanSach { get; set; }
    public string TinhTrangKhiTra { get; set; }
    public string GhiChu { get; set; }
    public DateTime? NgayTra { get; set; }
}

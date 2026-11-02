using System;

namespace QuanLyThuVien.DTO
{
    public class PhieuMuonDTO
    {
        public string MaPhieu { get; set; }
        public string MaDocGia { get; set; }
        public int MaTaiKhoan { get; set; }
        public DateTime NgayMuon { get; set; }
        public DateTime HenTra { get; set; }
        public string GhiChu { get; set; }
    }
}
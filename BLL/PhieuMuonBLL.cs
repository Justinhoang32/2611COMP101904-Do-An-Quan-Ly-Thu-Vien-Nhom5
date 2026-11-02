using System.Data;
using QuanLyThuVien.DAL;

namespace QuanLyThuVien.BLL
{
    public class PhieuMuonBLL
    {
        private PhieuMuonDAL phieuMuonDAL = new PhieuMuonDAL();

        public DataTable LayDanhSachPhieuMuon()
        {
            return phieuMuonDAL.LayDanhSachPhieuMuon();
        }

        public bool LapPhieuMuon(string maPhieu, string maDocGia, string ngayMuon, string ngayHenTra, out string err)
        {
            err = "";
            if (string.IsNullOrWhiteSpace(maPhieu) || string.IsNullOrWhiteSpace(maDocGia))
            {
                err = "Mã phiếu mượn và mã độc giả không được để trống!";
                return false;
            }

            int result = phieuMuonDAL.ThemPhieuMuon(maPhieu, maDocGia, ngayMuon, ngayHenTra);
            if (result > 0) return true;

            err = "Không thể tạo phiếu mượn!";
            return false;
        }
    }
}

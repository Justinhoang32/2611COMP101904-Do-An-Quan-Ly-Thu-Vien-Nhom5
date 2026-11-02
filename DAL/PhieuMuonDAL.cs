using System.Data;
using System.Data.SqlClient;

namespace QuanLyThuVien.DAL
{
    public class PhieuMuonDAL
    {
        // Lấy danh sách phiếu mượn kết hợp thông tin độc giả
        public DataTable LayDanhSachPhieuMuon()
        {
            string query = "SELECT pm.MaPhieuMuon, pm.MaDocGia, dg.HoTen, pm.NgayMuon, pm.NgayHenTra, pm.TinhTrang " +
                           "FROM PhieuMuon pm JOIN DocGia dg ON pm.MaDocGia = dg.MaDocGia";
            return DatabaseHelper.ExecuteQuery(query);
        }

        // Lập phiếu mượn mới
        public int ThemPhieuMuon(string maPhieu, string maDocGia, string ngayMuon, string ngayHenTra)
        {
            string query = "INSERT INTO PhieuMuon (MaPhieuMuon, MaDocGia, NgayMuon, NgayHenTra, TinhTrang) " +
                           "VALUES (@MaPhieuMuon, @MaDocGia, @NgayMuon, @NgayHenTra, N'Đang mượn')";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaPhieuMuon", maPhieu),
                new SqlParameter("@MaDocGia", maDocGia),
                new SqlParameter("@NgayMuon", ngayMuon),
                new SqlParameter("@NgayHenTra", ngayHenTra)
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters);
        }
    }
}
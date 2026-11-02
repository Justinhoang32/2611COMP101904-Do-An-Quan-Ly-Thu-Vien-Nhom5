using System;
using System.Data;
using System.Data.SqlClient;
using QuanLyThuVien.DTO;

namespace QuanLyThuVien.DAL
{
    public class PhieuMuonDAL
    {
        // Gọi Stored Procedure có chứa Transaction từ SQL Server
        public bool TaoPhieuMuon(PhieuMuonDTO phieu)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_TaoPhieuMuon", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    // Truyền tham số cho Procedure
                    cmd.Parameters.AddWithValue("@MaPhieu", phieu.MaPhieu);
                    cmd.Parameters.AddWithValue("@MaDocGia", phieu.MaDocGia);
                    cmd.Parameters.AddWithValue("@MaTaiKhoan", phieu.MaTaiKhoan);
                    cmd.Parameters.AddWithValue("@NgayMuon", phieu.NgayMuon);
                    cmd.Parameters.AddWithValue("@HenTra", phieu.HenTra);
                    cmd.Parameters.AddWithValue("@GhiChu", phieu.GhiChu);
                    
                    try 
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Quăng lỗi ra màn hình WinForms nếu Transaction bị ROLLBACK
                        throw new Exception("Lỗi CSDL khi lập phiếu mượn: " + ex.Message);
                    }
                }
            }
        }
    }
}
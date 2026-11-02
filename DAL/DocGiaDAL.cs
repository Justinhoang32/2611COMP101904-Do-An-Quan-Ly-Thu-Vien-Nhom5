using System.Data;
using System.Data.SqlClient;

namespace QuanLyThuVien.DAL
{
    public class DocGiaDAL
    {
        // Lấy danh sách độc giả
        public DataTable LayDanhSachDocGia()
        {
            string query = "SELECT * FROM DocGia";
            return DatabaseHelper.ExecuteQuery(query);
        }

        // Thêm độc giả mới
        public int ThemDocGia(string maDocGia, string hoTen, string ngaySinh, string dienThoai, string email)
        {
            string query = "INSERT INTO DocGia (MaDocGia, HoTen, NgaySinh, DienThoai, Email) VALUES (@MaDocGia, @HoTen, @NgaySinh, @DienThoai, @Email)";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaDocGia", maDocGia),
                new SqlParameter("@HoTen", hoTen),
                new SqlParameter("@NgaySinh", string.IsNullOrEmpty(ngaySinh) ? DBNull.Value : (object)ngaySinh),
                new SqlParameter("@DienThoai", dienThoai),
                new SqlParameter("@Email", email)
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters);
        }
    }
}
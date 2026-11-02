using System.Data;
using System.Data.SqlClient;

namespace QuanLyThuVien.DAL
{
    public class TaiKhoanDAL
    {
        public DataTable KiemTraDangNhap(string tenDangNhap, string matKhau)
        {
            string query = "SELECT * FROM TaiKhoan WHERE TenDangNhap = @TenDangNhap AND MatKhau = @MatKhau";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TenDangNhap", tenDangNhap),
                new SqlParameter("@MatKhau", matKhau)
            };
            return DatabaseHelper.ExecuteQuery(query, parameters);
        }
    }
}
using System.Data.SqlClient;

namespace QuanLyThuVien.DAL
{
    public class DatabaseConnection
    {
        // Nhớ đổi Data Source=.\SQLEXPRESS thành tên Server SQL của máy bạn nhé
        private static string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=QuanLyThuVien;Integrated Security=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
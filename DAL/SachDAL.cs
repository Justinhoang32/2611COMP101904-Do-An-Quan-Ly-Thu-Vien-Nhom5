using System.Data;
using System.Data.SqlClient;

namespace QuanLyThuVien.DAL
{
    public class SachDAL
    {
        // Lấy toàn bộ danh sách sách
        public DataTable LayDanhSachSach()
        {
            string query = "SELECT * FROM Sach";
            return DatabaseHelper.ExecuteQuery(query);
        }

        // Thêm sách mới
        public int ThemSach(string maSach, string tenSach, string theLoai, string tacGia, int soLuongTon)
        {
            string query = "INSERT INTO Sach (MaSach, TenSach, TheLoai, TacGia, SoLuongTon) VALUES (@MaSach, @TenSach, @TheLoai, @TacGia, @SoLuongTon)";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaSach", maSach),
                new SqlParameter("@TenSach", tenSach),
                new SqlParameter("@TheLoai", theLoai),
                new SqlParameter("@TacGia", tacGia),
                new SqlParameter("@SoLuongTon", soLuongTon)
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters);
        }
    }
}
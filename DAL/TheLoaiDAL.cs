using System.Data;

namespace QuanLyThuVien.DAL
{
    public class TacGiaTheLoaiDAL // hoặc TheLoaiDAL tùy tên file của bạn
    {
        public DataTable LayDanhSachTheLoai()
        {
            string query = "SELECT * FROM TheLoai";
            return DatabaseHelper.ExecuteQuery(query);
        }
    }
}
using System.Data;

namespace QuanLyThuVien.DAL
{
    public class TacGiaDAL
    {
        public DataTable LayDanhSachTacGia()
        {
            string query = "SELECT * FROM TacGia";
            return DatabaseHelper.ExecuteQuery(query);
        }
    }
}
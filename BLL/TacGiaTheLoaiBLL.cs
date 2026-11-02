using System.Data;
using QuanLyThuVien.DAL;

namespace QuanLyThuVien.BLL
{
    public class TacGiaTheLoaiBLL
    {
        // Nếu bạn đã có lớp DAL cho Tác Giả / Thể Loại, bạn gọi trực tiếp vào đây.
        // Hoặc sử dụng tạm DatabaseHelper để truy vấn nhanh danh mục:
        public DataTable LayDanhSachTheLoai()
        {
            string query = "SELECT * FROM TheLoai";
            return DatabaseHelper.ExecuteQuery(query);
        }

        public DataTable LayDanhSachTacGia()
        {
            string query = "SELECT * FROM TacGia";
            return DatabaseHelper.ExecuteQuery(query);
        }
    }
}
using System.Data;
using QuanLyThuVien.DAL;

namespace QuanLyThuVien.BLL
{
    public class SachBLL
    {
        private SachDAL sachDAL = new SachDAL();

        // Lấy danh sách sách để hiển thị lên giao diện
        public DataTable LayDanhSachSach()
        {
            return sachDAL.LayDanhSachSach();
        }

        // Xử lý logic kiểm tra trước khi thêm sách mới
        public bool ThemSach(string maSach, string tenSach, string theLoai, string tacGia, int soLuongTon, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(maSach) || string.IsNullOrWhiteSpace(tenSach))
            {
                errorMessage = "Mã sách và tên sách không được để trống!";
                return false;
            }

            if (soLuongTon < 0)
            {
                errorMessage = "Số lượng tồn không được nhỏ hơn 0!";
                return false;
            }

            int result = sachDAL.ThemSach(maSach, tenSach, theLoai, tacGia, soLuongTon);
            if (result > 0)
            {
                return true;
            }

            errorMessage = "Thêm sách thất bại!";
            return false;
        }
    }
}
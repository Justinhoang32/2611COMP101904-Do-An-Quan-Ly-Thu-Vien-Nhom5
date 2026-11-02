using System.Data;
using QuanLyThuVien.DAL;

namespace QuanLyThuVien.BLL
{
    public class DocGiaBLL
    {
        private DocGiaDAL docGiaDAL = new DocGiaDAL();

        // Lấy danh sách độc giả
        public DataTable LayDanhSachDocGia()
        {
            return docGiaDAL.LayDanhSachDocGia();
        }

        // Xử lý logic thêm độc giả
        public bool ThemDocGia(string maDocGia, string hoTen, string ngaySinh, string dienThoai, string email, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(maDocGia) || string.IsNullOrWhiteSpace(hoTen))
            {
                errorMessage = "Mã độc giả và họ tên không được để trống!";
                return false;
            }

            int result = docGiaDAL.ThemDocGia(maDocGia, hoTen, ngaySinh, dienThoai, email);
            if (result > 0)
            {
                return true;
            }

            errorMessage = "Thêm độc giả thất bại!";
            return false;
        }
    }
}
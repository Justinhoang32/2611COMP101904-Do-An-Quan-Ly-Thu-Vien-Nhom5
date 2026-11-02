using System.Collections.Generic;
using QuanLyThuVien.DTO;
using QuanLyThuVien.DAL;

namespace QuanLyThuVien.BLL
{
    public class SachBLL
    {
        private SachDAL sachDAL = new SachDAL();

        // Xử lý nghiệp vụ, kiểm tra điều kiện trước khi đẩy xuống DAL
        public List<SachDTO> HienThiDanhSachSach()
        {
            return sachDAL.GetAllSach();
        }
    }
}
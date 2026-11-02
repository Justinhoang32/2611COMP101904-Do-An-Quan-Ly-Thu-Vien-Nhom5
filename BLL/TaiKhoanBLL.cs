using System.Data;
using QuanLyThuVien.DAL;

namespace QuanLyThuVien.BLL
{
    public class TaiKhoanBLL
    {
        private TaiKhoanDAL taiKhoanDAL = new TaiKhoanDAL();

        public bool DangNhap(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
            {
                return false;
            }

            DataTable dt = taiKhoanDAL.KiemTraDangNhap(tenDangNhap, matKhau);
            return dt.Rows.Count > 0;
        }
    }
}
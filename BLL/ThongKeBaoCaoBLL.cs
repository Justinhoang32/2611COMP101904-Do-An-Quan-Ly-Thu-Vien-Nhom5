using System.Data;
using QuanLyThuVien.DAL;

namespace QuanLyThuVien.BLL
{
    public class ThongKeBaoCaoBLL
    {
        // Lấy danh sách phiếu mượn quá hạn chưa trả
        public DataTable LayDanhSachQuaHan()
        {
            string query = "SELECT pm.MaPhieuMuon, dg.HoTen, pm.NgayMuon, pm.NgayHenTra, pm.TinhTrang " +
                           "FROM PhieuMuon pm JOIN DocGia dg ON pm.MaDocGia = dg.MaDocGia " +
                           "WHERE pm.NgayHenTra < GETDATE() AND pm.TinhTrang = N'Đang mượn'";
            return DatabaseHelper.ExecuteQuery(query);
        }
    }
}
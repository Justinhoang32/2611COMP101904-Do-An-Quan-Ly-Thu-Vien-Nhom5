using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using QuanLyThuVien.DTO;

namespace QuanLyThuVien.DAL
{
    public class SachDAL
    {
        // Hàm truy xuất toàn bộ sách từ SQL
        public List<SachDTO> GetAllSach()
        {
            List<SachDTO> list = new List<SachDTO>();
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Sach";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SachDTO {
                            MaSach = reader["MaSach"].ToString(),
                            TenSach = reader["TenSach"].ToString(),
                            SoBanCoSan = Convert.ToInt32(reader["SoBanCoSan"])
                            // Có thể thêm các thuộc tính khác ở đây
                        });
                    }
                }
            }
            return list;
        }
    }
}
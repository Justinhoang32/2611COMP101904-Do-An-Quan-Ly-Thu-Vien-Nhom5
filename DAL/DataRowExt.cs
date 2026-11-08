using System.Data;

namespace DAL;

/// <summary>Phương thức mở rộng giúp đọc giá trị từ DataRow an toàn khi cột là NULL.</summary>
internal static class DataRowExt
{
    public static string Chuoi(this DataRow r, string cot) => r[cot] == DBNull.Value ? null : Convert.ToString(r[cot]);
    public static int So(this DataRow r, string cot) => r[cot] == DBNull.Value ? 0 : Convert.ToInt32(r[cot]);
    public static DateTime? Ngay(this DataRow r, string cot) => r[cot] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r[cot]);
    public static byte[] NhiPhan(this DataRow r, string cot) => r[cot] == DBNull.Value ? null : (byte[])r[cot];
}

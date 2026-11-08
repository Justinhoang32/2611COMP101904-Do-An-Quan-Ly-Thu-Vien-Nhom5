using System.Data;
using Microsoft.Data.SqlClient;

namespace DAL;

/// <summary>
/// Lớp duy nhất giữ chuỗi kết nối và thực thi câu lệnh SQL.
/// Mọi lớp DAL khác BẮT BUỘC đi qua lớp này để truy cập database.
/// </summary>
public static class DatabaseHelper
{
    // Chuỗi kết nối đặt ở MỘT nơi duy nhất (không chứa tên máy cá nhân, dùng SQLEXPRESS cục bộ).
    private const string ChuoiKetNoi =
        @"Data Source=.\SQLEXPRESS;Initial Catalog=QuanLyThuVien;Integrated Security=True;TrustServerCertificate=True";

    /// <summary>Tạo kết nối mới (chưa mở). Nơi gọi chịu trách nhiệm dùng "using".</summary>
    public static SqlConnection TaoKetNoi() => new SqlConnection(ChuoiKetNoi);

    /// <summary>Tạo tham số; giá trị null được đổi thành DBNull.</summary>
    public static SqlParameter Tham(string ten, object giaTri) => new SqlParameter(ten, giaTri ?? DBNull.Value);

    /// <summary>Tạo tham số VARBINARY(MAX) (dùng cho ảnh bìa), cho phép null.</summary>
    public static SqlParameter ThamNhiPhan(string ten, byte[] giaTri)
    {
        var p = new SqlParameter(ten, SqlDbType.VarBinary, -1);
        p.Value = (object)giaTri ?? DBNull.Value;
        return p;
    }

    /// <summary>Tạo chuỗi LIKE "%từ khóa%", đã thoát các ký tự đặc biệt % _ [ để tìm đúng nghĩa đen.</summary>
    public static string DangLike(string tuKhoa)
    {
        string s = (tuKhoa ?? "").Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        return "%" + s + "%";
    }

    /// <summary>Sao chép danh sách tham số (một SqlParameter không dùng lại được cho lệnh khác).</summary>
    public static SqlParameter[] SaoChep(IEnumerable<SqlParameter> ds) =>
        ds.Select(p => (SqlParameter)((ICloneable)p).Clone()).ToArray();

    /// <summary>Chuyển kết quả scalar (có thể null) sang int.</summary>
    public static int LaySo(object giaTri) => giaTri == null ? 0 : Convert.ToInt32(giaTri);

    // ---------------- Thực thi không transaction (tự mở/đóng kết nối) ----------------

    /// <summary>Chạy SELECT, trả về DataTable.</summary>
    public static DataTable ExecuteQuery(string sql, params SqlParameter[] tham)
    {
        using var cn = TaoKetNoi();
        using var cmd = new SqlCommand(sql, cn);
        if (tham != null) cmd.Parameters.AddRange(tham);
        using var da = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        da.Fill(dt);   // Fill tự mở và đóng kết nối
        return dt;
    }

    /// <summary>Chạy INSERT/UPDATE/DELETE, trả về số dòng bị ảnh hưởng.</summary>
    public static int ExecuteNonQuery(string sql, params SqlParameter[] tham)
    {
        using var cn = TaoKetNoi();
        using var cmd = new SqlCommand(sql, cn);
        if (tham != null) cmd.Parameters.AddRange(tham);
        cn.Open();
        return cmd.ExecuteNonQuery();
    }

    /// <summary>Chạy lệnh trả về một giá trị; null nếu không có dòng hoặc giá trị NULL.</summary>
    public static object ExecuteScalar(string sql, params SqlParameter[] tham)
    {
        using var cn = TaoKetNoi();
        using var cmd = new SqlCommand(sql, cn);
        if (tham != null) cmd.Parameters.AddRange(tham);
        cn.Open();
        var kq = cmd.ExecuteScalar();
        return kq == DBNull.Value ? null : kq;
    }

    // ---------------- Thực thi trong transaction (dùng chung kết nối do nơi gọi mở) ----------------

    public static DataTable ExecuteQuery(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] tham)
    {
        using var cmd = new SqlCommand(sql, cn, tx);
        if (tham != null) cmd.Parameters.AddRange(tham);
        using var da = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static int ExecuteNonQuery(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] tham)
    {
        using var cmd = new SqlCommand(sql, cn, tx);
        if (tham != null) cmd.Parameters.AddRange(tham);
        return cmd.ExecuteNonQuery();
    }

    public static object ExecuteScalar(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] tham)
    {
        using var cmd = new SqlCommand(sql, cn, tx);
        if (tham != null) cmd.Parameters.AddRange(tham);
        var kq = cmd.ExecuteScalar();
        return kq == DBNull.Value ? null : kq;
    }
}

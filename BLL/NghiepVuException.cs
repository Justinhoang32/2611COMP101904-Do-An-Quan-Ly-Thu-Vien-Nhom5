namespace BLL;

/// <summary>
/// Lỗi nghiệp vụ / dữ liệu không hợp lệ (nhập thiếu, trùng khóa, hết sách, quá hạn mức...).
/// Thông báo trong Message đã thân thiện, Presenter hiển thị thẳng cho người dùng.
/// </summary>
public class NghiepVuException : Exception
{
    public NghiepVuException(string thongBao) : base(thongBao) { }
}

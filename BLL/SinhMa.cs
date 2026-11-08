namespace BLL;

/// <summary>Tiện ích sinh mã tự tăng dạng "tiền tố + số", ví dụ TL007, S012, DG0007.</summary>
internal static class SinhMa
{
    /// <summary>Từ mã lớn nhất hiện có (có thể null) sinh ra mã kế tiếp với số chữ số cố định.</summary>
    public static string TiepTheo(string maCuoi, string tienTo, int soChuSo)
    {
        int so = 0;
        // Cắt phần số sau tiền tố rồi cộng 1
        if (!string.IsNullOrEmpty(maCuoi) && maCuoi.StartsWith(tienTo))
            int.TryParse(maCuoi.Substring(tienTo.Length), out so);
        return tienTo + (so + 1).ToString().PadLeft(soChuSo, '0');
    }
}

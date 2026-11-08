using System.Drawing.Printing;

namespace QuanLyThuVien;

/// <summary>Tiện ích in: mở hộp thoại chọn máy in rồi in một trang do hàm vẽ cung cấp.</summary>
public static class InHelper
{
    /// <summary>
    /// In một trang. Hàm ve nhận Graphics (đơn vị 1/100 inch) và vùng in (đã trừ lề).
    /// Trả về true nếu người dùng đã chọn in.
    /// </summary>
    public static bool In(IWin32Window chu, string tenTaiLieu, Action<Graphics, Rectangle> ve)
    {
        using var pd = new PrintDocument { DocumentName = tenTaiLieu };
        pd.PrintPage += (s, e) =>
        {
            ve(e.Graphics, e.MarginBounds);
            e.HasMorePages = false;
        };
        using var hop = new PrintDialog { Document = pd, UseEXDialog = true };
        if (hop.ShowDialog(chu) != DialogResult.OK) return false;
        pd.Print();
        return true;
    }

    /// <summary>
    /// In nhiều trang. Hàm vePage nhận (Graphics, vùng in, chỉ số trang bắt đầu từ 0) và trả về true nếu còn trang tiếp theo.
    /// </summary>
    public static bool InNhieuTrang(IWin32Window chu, string tenTaiLieu, Func<Graphics, Rectangle, int, bool> vePage, bool ngang = false)
    {
        using var pd = new PrintDocument { DocumentName = tenTaiLieu };
        pd.DefaultPageSettings.Landscape = ngang;
        int trang = 0;
        pd.PrintPage += (s, e) => { e.HasMorePages = vePage(e.Graphics, e.MarginBounds, trang++); };
        using var hop = new PrintDialog { Document = pd, UseEXDialog = true };
        if (hop.ShowDialog(chu) != DialogResult.OK) return false;
        pd.Print();
        return true;
    }
}

using ClosedXML.Excel;

namespace QuanLyThuVien;

/// <summary>Xuất dữ liệu bảng ra tệp Excel (.xlsx) bằng thư viện ClosedXML (NuGet, cài ở project giao diện).</summary>
public static class XuatExcel
{
    /// <summary>
    /// Hỏi nơi lưu rồi ghi tệp. Trả về đường dẫn tệp đã lưu, hoặc null nếu người dùng hủy.
    /// Ném IOException nếu tệp đang được mở bởi chương trình khác.
    /// </summary>
    public static string Xuat(IWin32Window chu, string tenGoiY, string tieuDe, string[] cot, IEnumerable<string[]> dong)
    {
        using var hop = new SaveFileDialog
        {
            Title = "Lưu tệp Excel", Filter = "Tệp Excel (*.xlsx)|*.xlsx", FileName = tenGoiY, DefaultExt = "xlsx", AddExtension = true, OverwritePrompt = true
        };
        if (hop.ShowDialog(chu) != DialogResult.OK) return null;

        using var sach = new XLWorkbook();
        var ws = sach.Worksheets.Add("Dữ liệu");
        // Dòng 1: tiêu đề báo cáo
        ws.Cell(1, 1).Value = tieuDe;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        // Dòng 3: tiêu đề cột
        for (int c = 0; c < cot.Length; c++)
        {
            var o = ws.Cell(3, c + 1);
            o.Value = cot[c];
            o.Style.Font.Bold = true;
            o.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8EEF5");
        }
        // Từ dòng 4: dữ liệu (ghi dạng chuỗi để giữ nguyên số 0 đầu và định dạng ngày)
        int r = 4;
        foreach (var d in dong)
        {
            for (int c = 0; c < cot.Length && c < d.Length; c++) ws.Cell(r, c + 1).SetValue(d[c] ?? "");
            r++;
        }
        ws.Columns().AdjustToContents();
        sach.SaveAs(hop.FileName);
        return hop.FileName;
    }
}

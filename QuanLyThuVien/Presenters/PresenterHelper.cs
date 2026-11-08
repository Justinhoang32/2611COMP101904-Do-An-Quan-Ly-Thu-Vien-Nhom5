using BLL;
using QuanLyThuVien.Views;

namespace QuanLyThuVien.Presenters;

/// <summary>Tiện ích dùng chung cho Presenter.</summary>
public static class PresenterHelper
{
    /// <summary>
    /// Chạy một tác vụ bất đồng bộ có bắt lỗi: lỗi nghiệp vụ hiện nguyên văn, lỗi khác hiện thông báo chung thân thiện.
    /// </summary>
    public static async Task ChayAn(IViewCoThongBao view, Func<Task> viec)
    {
        try { await viec(); }
        catch (NghiepVuException ex) { view.HienThiLoi(ex.Message); }
        catch (Exception ex)
        {
            view.HienThiLoi("Đã xảy ra lỗi khi xử lý dữ liệu. Vui lòng kiểm tra kết nối SQL Server rồi thử lại.\n(Chi tiết: " + ex.Message + ")");
        }
    }
}

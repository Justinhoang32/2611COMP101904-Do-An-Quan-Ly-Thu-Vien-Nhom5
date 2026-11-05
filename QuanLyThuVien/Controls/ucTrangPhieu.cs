using DTO.Models;

namespace QuanLyThuVien.Controls;

/// <summary>
/// Khung "tờ giấy" xem trước phiếu mượn: nền trắng, viền mảnh, tỉ lệ A4/A5 theo hướng giấy.
/// Nội dung vẽ bởi PhieuInVe.Ve nên khớp bản in. Chiều cao tự tính từ chiều rộng.
/// </summary>
public class ucTrangPhieu : Control
{
    private PhieuMuonDayDu _phieu;
    private bool _gomGhiChu = true, _ngang;
    private Image _logo;

    public ucTrangPhieu()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.White;
        _logo = CauHinhUngDung.TaiLogo();
    }

    public void DatNoiDung(PhieuMuonDayDu p, bool gomGhiChu, bool ngang)
    {
        _phieu = p; _gomGhiChu = gomGhiChu; _ngang = ngang;
        SetBounds(Left, Top, Width, Height);   // tính lại chiều cao theo tỉ lệ
        Invalidate();
    }

    // Luôn giữ tỉ lệ giấy: A4 dọc 1:1,414; ngang 1,414:1 (A5 cùng tỉ lệ)
    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
        double tl = _ngang ? 1 / 1.414 : 1.414;
        base.SetBoundsCore(x, y, width, Math.Max(10, (int)(width * tl)), specified);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Color.White);
        if (_phieu != null) PhieuInVe.Ve(e.Graphics, new RectangleF(0, 0, Width, Height), _phieu, _gomGhiChu, _logo);
        using var pen = new Pen(UiTheme.VienNhat);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _logo?.Dispose();
        base.Dispose(disposing);
    }
}

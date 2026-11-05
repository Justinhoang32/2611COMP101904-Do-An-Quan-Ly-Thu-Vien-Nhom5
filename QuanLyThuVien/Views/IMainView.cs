namespace QuanLyThuVien.Views;

/// <summary>Giao diện của khung chính (sidebar + header + vùng nội dung + footer).</summary>
public interface IMainView
{
    event EventHandler Loaded;
    event EventHandler<YeuCauDieuHuong> MenuDuocChon;
    event EventHandler DangXuatClicked;
    event EventHandler YeuCauLamMoiHuyHieu;

    void HienThiNguoiDung(string hoTen, string chucVu, string kyTuVietTat);
    void HienThiNgayHienTai(string ngay);
    void HienThiNamHoc(string namHoc);
    void HienThiNgayDuLieu(string noiDung);
    void HienThiSoQuaHan(int soPhieu);
    void HienThiManHinh(YeuCauDieuHuong yeuCau, string duongDan);

    void HienThiThongTin(string thongBao);
    void HienThiLoi(string thongBao);
    bool XacNhan(string cauHoi);
    void KetThuc(bool dangXuat);
}

using DTO;
using DTO.Entities;

namespace QuanLyThuVien.Forms;

/// <summary>
/// Hộp thoại nhập / sửa hồ sơ độc giả. Chỉ thu thập dữ liệu; việc kiểm tra hợp lệ do BLL thực hiện qua Presenter.
/// Thẻ (ngày cấp, hạn thẻ) được cấp riêng ở panel "Cấp / gia hạn thẻ" của màn hình Độc giả.
/// Giao diện tĩnh nằm trong frmHoSoDocGia.Designer.cs (kéo-thả được).
/// </summary>
public partial class frmHoSoDocGia : Form
{
    private readonly string _maDocGia;

    /// <summary>Dữ liệu người dùng đã nhập (hợp lệ khi DialogResult = OK).</summary>
    public DocGia KetQua { get; private set; }

    public frmHoSoDocGia(DocGia khoiTao, bool them)
    {
        InitializeComponent();
        _maDocGia = khoiTao?.MaDocGia;

        // Chữ phụ thuộc chế độ thêm / sửa
        Text = them ? "Thêm độc giả" : "Sửa hồ sơ độc giả";
        lblTieuDe.Text = Text;
        lblGhiChu.Text = them ? "Mã thẻ được tạo tự động sau khi lưu." : "Mã thẻ: " + _maDocGia;
        btnLuu.Text = them ? "Thêm độc giả" : "Lưu hồ sơ";

        btnLuu.Click += (s, e) =>
        {
            // Thu thập dữ liệu; BLL sẽ kiểm tra và báo lỗi nếu chưa hợp lệ
            KetQua = new DocGia
            {
                MaDocGia = _maDocGia, HoTen = txtHoTen.Text, NhomDocGia = cboNhom.SelectedItem as string,
                MaSinhVien = txtMaSv.Text, Khoa = txtKhoa.Text, DienThoai = txtDienThoai.Text, Email = txtEmail.Text
            };
            DialogResult = DialogResult.OK;
        };

        // Điền dữ liệu khởi tạo (khi sửa hoặc khi nhập lại sau lỗi)
        if (khoiTao != null)
        {
            txtHoTen.Text = khoiTao.HoTen; txtMaSv.Text = khoiTao.MaSinhVien; txtKhoa.Text = khoiTao.Khoa;
            txtDienThoai.Text = khoiTao.DienThoai; txtEmail.Text = khoiTao.Email;
            int i = cboNhom.FindStringExact(khoiTao.NhomDocGia ?? "");
            cboNhom.SelectedIndex = i >= 0 ? i : 0;
        }
        else cboNhom.SelectedIndex = 0;
    }
}

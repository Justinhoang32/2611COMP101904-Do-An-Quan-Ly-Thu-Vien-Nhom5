# QUẢN LÝ THƯ VIỆN – THƯ VIỆN HCMUE

Ứng dụng desktop **C# WinForms (.NET 8)** quản lý thư viện: sách, tác giả – thể loại, độc giả, phiếu mượn – trả, báo cáo quá hạn.

## 1. Thông tin đề tài

| Mục | Nội dung |
|---|---|
| **Tên đề tài** | Xây dựng phần mềm Quản lý thư viện  |
| **Công nghệ** | C# WinForms, .NET 8 (`net8.0-windows`), SQL Server Express, ADO.NET (Microsoft.Data.SqlClient) |
| **Kiến trúc** | MVP kết hợp 3 lớp (DTO – DAL – BLL – giao diện) |
| **Giảng viên hướng dẫn** | ThS. Lê Thanh Thoại |
| **Môn học / Lớp** | LẬP TRÌNH WINDOWS - Lớp 2611COMP101904 |

### Thành viên nhóm (4 người)

| STT | Họ và tên | MSSV | Nhiệm vụ |
|---|---|---|---|
| 1 | Hoàng Minh Nhật | 51.01.104.066 |Code kiến trúc MVP, đăng nhập, màn hình chính, lập phiếu mượn, trả sách, in phiếu, quản lý Git, tổng hợp báo cáo, dựng LaTeX.|
| 2 | Nguyễn Thanh Thủy | 50.01.104.160 |Thiết kế CSDL, script SQL, code quản lý sách, nhập CSV, tác giả và thể loại, viết Chương 1, Chương 5 và Chương 9.|
| 3 | Phạm Gia Minh Hiếu | 51.01.104.028 |Code quản lý độc giả, cấp thẻ, tìm kiếm sách, danh sách phiếu mượn, viết Chương , Chương 6 và Chương 3, Dựng LaTeX. |
| 4 | Phan Thành Vinh | 51.01.104.126 | Thiết kế CSDL, script SQL, tổng quan, báo cáo quá hạn, kiểm thử, viết Chương 4, Chương 7, Chương 8 và Chương 10. |

> Tên 4 thành viên lấy từ 4 tài khoản quản trị trong dữ liệu mẫu. Hãy sửa lại cho đúng và điền MSSV, phân công.

## 2. Chức năng

Ứng dụng có 11 màn hình theo bản thiết kế PDF:

1. **Đăng nhập** – xác thực tài khoản, ghi nhớ tên đăng nhập.
2. **Tổng quan** – thống kê sách, độc giả, phiếu, biểu đồ lượt mượn theo tháng, việc cần xử lý.
3. **Quản lý sách** – tìm, lọc, phân trang; thêm / sửa đầu sách; tải ảnh bìa từ máy; nhập danh mục từ file CSV.
4. **Tác giả & thể loại** – thêm / sửa / tra cứu (chỉ Quản trị viên được sửa, Thủ thư chỉ xem).
5. **Độc giả** – hồ sơ, cấp / gia hạn thẻ, xem trước và in thẻ, xuất danh sách ra Excel.
6. **Phiếu mượn** – danh sách phiếu theo tab (Tất cả / Đang mượn / Đã trả / Quá hạn), lọc theo ngày.
7. **Lập phiếu mượn** – chọn độc giả, thêm sách, kiểm tra điều kiện, tạo phiếu.
8. **Tìm kiếm sách** – tìm nhiều điều kiện kết hợp, thêm vào phiếu đang lập.
9. **Ghi nhận trả sách** – chọn từng bản sách, tình trạng Tốt / Hư hỏng / Mất.
10. **Xem trước và in phiếu mượn** – xem trước đúng nội dung in, in ra máy in hoặc lưu PDF.
11. **Báo cáo quá hạn** – danh sách phiếu quá hạn, thống kê, xuất Excel.

### Quy tắc nghiệp vụ

- Mỗi độc giả mượn tối đa **5 cuốn** cùng lúc; thời hạn mượn mặc định **14 ngày**.
- Không cho mượn nếu thẻ hết hạn hoặc độc giả đang có phiếu quá hạn.
- Sách "Chỉ đọc tại chỗ" hoặc hết bản không thể thêm vào phiếu.
- Mã phiếu dạng `PMyyMMdd-NNN` (ví dụ `PM261105-001`); mã bản sách dạng `S001-017`.
- Ảnh bìa tối đa **2 MB**, lưu thẳng trong database (cột `VARBINARY`).
- Phân quyền: Quản trị viên (vai trò 1) và Thủ thư (vai trò 2).

## 3. Kiến trúc

```
UI (WinForms)  ──►  Presenter  ──►  BLL  ──►  DAL  ──►  SQL Server
  View/IView                         │          │
                                     └── DTO ◄──┘   (các lớp dữ liệu dùng chung)
```

| Project | Vai trò |
|---|---|
| `DTO` | Lớp dữ liệu: entity khớp bảng DB, model hiển thị, hằng số (`HangSo`). |
| `DAL` | Truy cập database bằng ADO.NET, câu lệnh có tham số. Chuỗi kết nối chỉ nằm trong `DatabaseHelper`. |
| `BLL` | Nghiệp vụ và kiểm tra dữ liệu. Báo lỗi nghiệp vụ bằng `NghiepVuException`. |
| `QuanLyThuVien` | Giao diện (View), Presenter, các control tự vẽ, helper in ấn / Excel / DPI. |

Tham chiếu một chiều: `QuanLyThuVien → BLL → DAL → DTO`.

## 4. Cấu trúc thư mục

```
Buoc2/
├─ QuanLyThuVien.sln   (mở file này bằng Visual Studio)
├─ DTO/            Entities/, Models/, HangSo.cs
├─ DAL/            DatabaseHelper.cs + *DAL.cs (SachDAL, DocGiaDAL, PhieuMuonDAL...)
├─ BLL/            *BLL.cs (SachBLL, PhieuMuonBLL, TraSachBLL...), NghiepVuException, PhienDangNhap
├─ QuanLyThuVien/
│  ├─ Forms/          frmDangNhap, frmMain, frmHoSoDocGia, frmChiTietSach
│  ├─ UserControls/   các màn hình con (ucQuanLySach, ucDocGia, ucLapPhieuMuon...)
│  ├─ Controls/       control dùng chung (NutBam, NhanVan, ThePanel, ucBarChart...)
│  ├─ Views/          các interface IxxxView
│  ├─ Presenters/     các xxxPresenter
│  ├─ Resources/      logo_hcmue.png, anh_nen_dang_nhap.jpg (hình ảnh)
│  ├─ Helpers/        DpiHelper, UiTheme, InHelper, PhieuInVe, XuatExcel...
│  └─ Program.cs
├─ SQL/            QuanLyThuVien_CapNhat.sql
└─ MauNhap/        mau_nhap_danh_muc.csv (file mẫu nhập danh mục sách)
```

Mỗi màn hình có 2 file: `tên.cs` (logic, sự kiện) và `tên.Designer.cs` (giao diện, mở bằng Designer kéo-thả được).

## 5. Hướng dẫn cài đặt và chạy

### 5.1. Yêu cầu

- Windows 10/11
- Visual Studio 2022 (17.8 trở lên) với workload **.NET desktop development**
- SQL Server Express, instance `.\SQLEXPRESS`, và SQL Server Management Studio (SSMS)

### 5.2. Tạo database

1. Mở SSMS, kết nối `.\SQLEXPRESS` bằng **Windows Authentication**.
2. Mở và chạy (F5) file **`QuanLyThuVien_FixLoi.sql`** (file database gốc) để tạo database `QuanLyThuVien` và dữ liệu mẫu.
3. Chạy tiếp file **`SQL/QuanLyThuVien_CapNhat.sql`** để bổ sung các cột mà giao diện cần (nhà xuất bản, ngôn ngữ, ảnh bìa, tình trạng lưu hành, ngày trả...). File này chạy lại nhiều lần vẫn an toàn.

> Phải chạy đúng thứ tự: file gốc trước, file cập nhật sau.

### 5.3. Mở dự án

Mở file **`QuanLyThuVien.sln`** ở thư mục gốc bằng Visual Studio 2022. Solution gồm 4 project (`DTO`, `DAL`, `BLL`, `QuanLyThuVien`) với tham chiếu đã khai báo sẵn trong `.csproj`: `QuanLyThuVien → BLL → DAL → DTO`.

Chuột phải project `QuanLyThuVien` → **Set as Startup Project** (nếu chưa là project khởi động).

### 5.4. NuGet

Chuột phải Solution → **Restore NuGet Packages**. Các gói cần có:

| Project | Gói |
|---|---|
| DAL | `Microsoft.Data.SqlClient` |
| QuanLyThuVien | `ClosedXML` 0.104.1 (xuất Excel) |

### 5.5. Hình ảnh (logo, ảnh nền, ảnh bìa sách)

Thư mục để đặt hình: **`QuanLyThuVien\Resources`** (cùng cấp với `Program.cs`). Đặt đúng tên file:

| Hình | Tên file | Dùng ở đâu |
|---|---|---|
| Logo HCMUE | `Resources\logo_hcmue.png` | Đăng nhập, menu bên trái, bản in phiếu, thẻ độc giả |
| Ảnh nền đăng nhập | `Resources\anh_nen_dang_nhap.jpg` | Nửa trái màn Đăng nhập |

Sau khi chép file, **Rebuild** rồi chạy lại (thư mục `Resources` được tự chép sang thư mục chạy). Chưa có ảnh thì chương trình vẫn chạy bình thường. Xem thêm `QuanLyThuVien\Resources\README.md`.

**Ảnh bìa sách** không đặt trong thư mục này: chọn từ máy tính ngay trong ứng dụng (Quản lý sách → Thêm / Sửa đầu sách → *Đổi ảnh bìa*, tối đa 2 MB) và được lưu trong database.

### 5.6. Chạy

Nhấn **F5**. Đăng nhập bằng một trong các tài khoản mẫu:

| Vai trò | Tên đăng nhập | Mật khẩu |
|---|---|---|
| Quản trị viên | `admin_minhnhat`, `admin_tthuy`, `admin_hieu`, `admin_vinh` | `123456` |
| Thủ thư | `lib_lanhuong`, `lib_baongoc` | `123456` |

### 5.7. Đổi server SQL (nếu cần)

Nếu máy không dùng `.\SQLEXPRESS`, sửa chuỗi kết nối **một chỗ duy nhất** trong `DAL/DatabaseHelper.cs`:

```
Data Source=.\SQLEXPRESS;Initial Catalog=QuanLyThuVien;Integrated Security=True;TrustServerCertificate=True
```

### 5.8. Xử lý lỗi thường gặp

| Hiện tượng | Cách xử lý |
|---|---|
| "Không thể kết nối cơ sở dữ liệu" | Kiểm tra dịch vụ SQL Server (SQLEXPRESS) đang chạy; kiểm tra tên server trong `DatabaseHelper`. |
| Lỗi "Invalid column name ..." | Chưa chạy `QuanLyThuVien_CapNhat.sql`. |
| Lỗi thiếu gói / không build được | Restore NuGet; kiểm tra đã cài workload .NET desktop. |
| Designer báo lỗi khi mở một màn hình | Build Solution trước, rồi mở lại file ở chế độ Design. |
| Chữ / control bị cắt khi đổi Scale | Thử Scale 100%, 125%, 150% và báo lại màn hình bị lỗi. |

## 6. Ghi chú kỹ thuật

- **DPI:** `Application.SetHighDpiMode(PerMonitorV2)` trong `Program.cs`; mọi Form / UserControl dùng `AutoScaleMode.Dpi`, `AutoScaleDimensions = 96,96`; bố cục dùng `TableLayoutPanel`, `Dock`, `Anchor`; font tính theo Point.
- **Bảo mật:** truy vấn dùng tham số (chống SQL Injection). Mật khẩu trong dữ liệu mẫu đang lưu dạng văn bản thô (đủ cho đồ án; thực tế nên băm).
- **Giao dịch:** tạo phiếu và ghi nhận trả sách chạy trong `SqlTransaction`, lỗi thì hoàn tác.
- **Thông báo lỗi:** `try/catch` ở Presenter, lỗi nghiệp vụ hiện nguyên văn, lỗi hệ thống hiện thông báo thân thiện.

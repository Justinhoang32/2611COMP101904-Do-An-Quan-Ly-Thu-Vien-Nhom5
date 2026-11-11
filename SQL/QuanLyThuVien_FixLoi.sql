USE master;
GO

IF DB_ID('QuanLyThuVien') IS NOT NULL
BEGIN
    ALTER DATABASE QuanLyThuVien SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyThuVien;
END
GO

CREATE DATABASE QuanLyThuVien;
GO

USE QuanLyThuVien;
GO

-- Xóa các đối tượng cũ nếu tồn tại theo đúng thứ tự phụ thuộc khóa ngoại
IF OBJECT_ID('sp_TaoPhieuMuon', 'P') IS NOT NULL DROP PROCEDURE sp_TaoPhieuMuon;
IF OBJECT_ID('ChiTietMuon', 'U') IS NOT NULL DROP TABLE ChiTietMuon;
IF OBJECT_ID('PhieuMuon', 'U') IS NOT NULL DROP TABLE PhieuMuon;
IF OBJECT_ID('DocGia', 'U') IS NOT NULL DROP TABLE DocGia;
IF OBJECT_ID('Sach', 'U') IS NOT NULL DROP TABLE Sach;
IF OBJECT_ID('TaiKhoan', 'U') IS NOT NULL DROP TABLE TaiKhoan;
IF OBJECT_ID('VaiTro', 'U') IS NOT NULL DROP TABLE VaiTro;
IF OBJECT_ID('TheLoai', 'U') IS NOT NULL DROP TABLE TheLoai;
IF OBJECT_ID('TacGia', 'U') IS NOT NULL DROP TABLE TacGia;
GO

CREATE TABLE VaiTro (
    MaVaiTro INT PRIMARY KEY,
    TenVaiTro NVARCHAR(50)
);


CREATE TABLE TaiKhoan (
    MaTaiKhoan INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100),
    MaVaiTro INT FOREIGN KEY REFERENCES VaiTro(MaVaiTro)
);


CREATE TABLE TheLoai (
    MaTheLoai VARCHAR(10) PRIMARY KEY,
    TenTheLoai NVARCHAR(100) NOT NULL
);


CREATE TABLE TacGia (
    MaTacGia VARCHAR(10) PRIMARY KEY,
    TenTacGia NVARCHAR(100) NOT NULL
);


CREATE TABLE Sach (
    MaSach VARCHAR(10) PRIMARY KEY,
    TenSach NVARCHAR(200) NOT NULL,
    MaTacGia VARCHAR(10) FOREIGN KEY REFERENCES TacGia(MaTacGia),
    MaTheLoai VARCHAR(10) FOREIGN KEY REFERENCES TheLoai(MaTheLoai),
    NamXuatBan INT,
    TongSoBan INT,
    SoBanCoSan INT,
    ViTriKe VARCHAR(50)
);


CREATE TABLE DocGia (
    MaDocGia VARCHAR(20) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NhomDocGia NVARCHAR(50),
    MaSinhVien VARCHAR(20),
    Khoa NVARCHAR(100),
    DienThoai VARCHAR(15),
    Email VARCHAR(100),
    NgayCap DATE,
    HanThe DATE,
    TrangThai NVARCHAR(50)
);


CREATE TABLE PhieuMuon (
    MaPhieu VARCHAR(20) PRIMARY KEY,
    MaDocGia VARCHAR(20) FOREIGN KEY REFERENCES DocGia(MaDocGia),
    MaTaiKhoan INT FOREIGN KEY REFERENCES TaiKhoan(MaTaiKhoan),
    NgayMuon DATE,
    HenTra DATE,
    TrangThai NVARCHAR(50),
    GhiChu NVARCHAR(255)
);


CREATE TABLE ChiTietMuon (
    MaPhieu VARCHAR(20) FOREIGN KEY REFERENCES PhieuMuon(MaPhieu),
    MaSach VARCHAR(10) FOREIGN KEY REFERENCES Sach(MaSach),
    MaBanSach VARCHAR(20),
    TinhTrangKhiTra NVARCHAR(50),
    GhiChu NVARCHAR(255),
    PRIMARY KEY (MaPhieu, MaSach, MaBanSach)
);
GO


CREATE PROCEDURE sp_TaoPhieuMuon
    @MaPhieu VARCHAR(20), 
    @MaDocGia VARCHAR(20), 
    @MaTaiKhoan INT, 
    @NgayMuon DATE, 
    @HenTra DATE, 
    @GhiChu NVARCHAR(255)
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO PhieuMuon (MaPhieu, MaDocGia, MaTaiKhoan, NgayMuon, HenTra, TrangThai, GhiChu)
        VALUES (@MaPhieu, @MaDocGia, @MaTaiKhoan, @NgayMuon, @HenTra, N'Đang mượn', @GhiChu);
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

INSERT INTO TheLoai VALUES 
('TL001', N'Tâm lý học'), ('TL002', N'Giáo dục học'), 
('TL003', N'Văn hóa'), ('TL004', N'Toán học'), 
('TL005', N'Ngôn ngữ học'), ('TL006', N'Văn học');

INSERT INTO TacGia VALUES 
('TG001', N'Nguyễn Quang Uẩn'), ('TG002', N'Phạm Viết Vượng'), 
('TG003', N'Trần Ngọc Thêm'), ('TG004', N'Đỗ Hương Trà'), 
('TG005', N'Nguyễn Hữu Việt Hưng'), ('TG006', N'Đỗ Hữu Châu');

INSERT INTO Sach VALUES 
('S001', N'Tâm lý học đại cương', 'TG001', 'TL001', 2023, 24, 8, 'TL A01'), 
('S002', N'Giáo dục học', 'TG002', 'TL002', 2023, 18, 5, 'GD B02'), 
('S003', N'Cơ sở văn hóa Việt Nam', 'TG003', 'TL003', 2023, 15, 1, 'VH A03'), 
('S004', N'Dạy học phát triển năng lực', 'TG004', 'TL002', 2023, 12, 0, 'GD B03'), 
('S005', N'Đại số tuyến tính', 'TG005', 'TL004', 2023, 20, 12, 'TH C01'), 
('S006', N'Ngôn ngữ học đại cương', 'TG006', 'TL005', 2023, 16, 6, 'NN B04');

INSERT INTO DocGia VALUES 
('DG0001', N'Nguyễn Minh Anh', N'Sinh viên', '49.01.103.001', N'Tâm lý học', '0903 456 781', 'minhanh.4901@student.hcmue.edu.vn', '2023-09-30', '2027-09-30', N'Hoạt động'), 
('DG0002', N'Trần Hoàng Nam', N'Sinh viên', '49.01.101.028', N'Công nghệ thông tin', '0912 345 678', 'nam.4901@student.hcmue.edu.vn', '2023-09-30', '2027-09-30', N'Hoạt động'), 
('DG0004', N'Phạm Gia Hân', N'Sinh viên', '48.01.001.042', N'Giáo dục mầm non', '0908 221 436', 'giahan.4801@student.hcmue.edu.vn', '2022-09-30', '2027-09-30', N'Có quá hạn');

INSERT INTO PhieuMuon VALUES 
('PM261102-015', 'DG0001', 5, '2026-11-02', '2026-11-16', N'Đang mượn', N'Mượn phục vụ học phần Tâm lý học.'), 
('PM261015-008', 'DG0004', 5, '2026-10-15', '2026-10-29', N'Quá hạn', N'');

INSERT INTO ChiTietMuon VALUES 
('PM261102-015', 'S001', 'S001-017', N'Chưa ghi nhận', N''), 
('PM261102-015', 'S002', 'S002-009', N'Chưa ghi nhận', N''), 
('PM261102-015', 'S003', 'S003-012', N'Chưa ghi nhận', N''), 
('PM261015-008', 'S001', 'S001-005', N'Chưa ghi nhận', N''), 
('PM261015-008', 'S003', 'S003-008', N'Chưa ghi nhận', N'');

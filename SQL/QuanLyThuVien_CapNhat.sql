
USE QuanLyThuVien;
GO


IF COL_LENGTH('Sach', 'NhaXuatBan') IS NULL ALTER TABLE Sach ADD NhaXuatBan NVARCHAR(100) NULL;
IF COL_LENGTH('Sach', 'NgonNgu') IS NULL ALTER TABLE Sach ADD NgonNgu NVARCHAR(50) NULL;
IF COL_LENGTH('Sach', 'AnhBia') IS NULL ALTER TABLE Sach ADD AnhBia VARBINARY(MAX) NULL;
IF COL_LENGTH('Sach', 'TinhTrangLuuHanh') IS NULL
    ALTER TABLE Sach ADD TinhTrangLuuHanh NVARCHAR(50) NOT NULL
        CONSTRAINT DF_Sach_TinhTrangLuuHanh DEFAULT N'Được phép cho mượn';

IF COL_LENGTH('TacGia', 'GioiThieu') IS NULL ALTER TABLE TacGia ADD GioiThieu NVARCHAR(500) NULL;
IF COL_LENGTH('TheLoai', 'MoTa') IS NULL ALTER TABLE TheLoai ADD MoTa NVARCHAR(500) NULL;
-- ChiTietMuon: ngày trả thực tế của từng bản sách
IF COL_LENGTH('ChiTietMuon', 'NgayTra') IS NULL ALTER TABLE ChiTietMuon ADD NgayTra DATE NULL;
GO

UPDATE Sach SET NhaXuatBan = N'NXB Đại học Sư phạm' WHERE NhaXuatBan IS NULL;
UPDATE Sach SET NgonNgu = N'Tiếng Việt' WHERE NgonNgu IS NULL;
UPDATE TacGia SET GioiThieu = N'Nhà nghiên cứu văn hóa, tác giả các công trình về bản sắc văn hóa Việt Nam.' WHERE MaTacGia = 'TG003' AND GioiThieu IS NULL;
UPDATE TheLoai SET MoTa = N'Cơ sở văn hóa, văn hóa Việt Nam và nghiên cứu văn hóa học.' WHERE MaTheLoai = 'TL003' AND MoTa IS NULL;


IF NOT EXISTS (SELECT 1 FROM DocGia WHERE MaDocGia = 'DG0003')
    INSERT INTO DocGia VALUES ('DG0003', N'Lê Thảo Nguyên', N'Giảng viên', 'GV0108', N'Ngữ văn', '0987 654 321', 'nguyen.le@hcmue.edu.vn', '2023-01-05', '2027-12-31', N'Hoạt động');
IF NOT EXISTS (SELECT 1 FROM DocGia WHERE MaDocGia = 'DG0005')
    INSERT INTO DocGia VALUES ('DG0005', N'Võ Ngọc Bảo', N'Sinh viên', '48.01.103.019', N'Tâm lý học', '0935 112 233', 'ngocbao.4801@student.hcmue.edu.vn', '2022-09-30', '2026-11-08', N'Hoạt động');
IF NOT EXISTS (SELECT 1 FROM DocGia WHERE MaDocGia = 'DG0006')
    INSERT INTO DocGia VALUES ('DG0006', N'Đặng Quốc Huy', N'Cán bộ', 'CB0062', N'Phòng Hành chính', '0977 445 566', 'huy.dq@hcmue.edu.vn', '2021-10-31', '2026-10-31', N'Hoạt động');
GO

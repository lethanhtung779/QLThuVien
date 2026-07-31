/* ============================================================
   QLTV - QUẢN LÝ THƯ VIỆN
   Script tạo CSDL SQL Server (dùng cho EF6 Database First)
   ------------------------------------------------------------
   Cách chạy: mở SQL Server Management Studio -> New Query -> chạy toàn bộ
   ============================================================ */

CREATE DATABASE QLTV
GO

USE QLTV
GO

/* ============ 1. BẢNG NHÂN VIÊN (đăng nhập) ============ */
CREATE TABLE NhanVien
(
    MaNhanVien  INT IDENTITY(1,1) PRIMARY KEY,
    TenNhanVien NVARCHAR(100) NOT NULL,
    TaiKhoan    VARCHAR(50)   NOT NULL UNIQUE,
    MatKhau     VARCHAR(255)  NOT NULL,
    ChucVu      NVARCHAR(50),
    GioiTinh    BIT           DEFAULT 1,      -- 1: Nam, 0: Nữ
    NgaySinh    DATE,
    DiaChi      NVARCHAR(200),
    Sdt         VARCHAR(15),
    Email       VARCHAR(100),
    NgayTao     DATETIME      DEFAULT GETDATE()
)
GO

/* ============ 2. BẢNG THỂ LOẠI ============ */
CREATE TABLE TheLoai
(
    MaTheLoai   INT IDENTITY(1,1) PRIMARY KEY,
    TenTheLoai  NVARCHAR(100) NOT NULL UNIQUE,
    MoTa        NVARCHAR(255)
)
GO

/* ============ 3. BẢNG NHÀ XUẤT BẢN ============ */
CREATE TABLE NhaXuatBan
(
    MaNXB   INT IDENTITY(1,1) PRIMARY KEY,
    TenNXB  NVARCHAR(100) NOT NULL UNIQUE,
    DiaChi  NVARCHAR(200),
    Sdt     VARCHAR(15),
    Email   VARCHAR(100)
)
GO

/* ============ 4. BẢNG TÁC GIẢ ============ */
CREATE TABLE TacGia
(
    MaTacGia    INT IDENTITY(1,1) PRIMARY KEY,
    TenTacGia   NVARCHAR(100) NOT NULL,
    NgaySinh    DATE,
    GhiChu      NVARCHAR(255)
)
GO

/* ============ 5. BẢNG SÁCH ============ */
CREATE TABLE Sach
(
    MaSach      INT IDENTITY(1,1) PRIMARY KEY,
    TenSach     NVARCHAR(200) NOT NULL,
    MaTheLoai   INT NOT NULL REFERENCES TheLoai(MaTheLoai),
    MaNXB       INT NOT NULL REFERENCES NhaXuatBan(MaNXB),
    NamXuatBan  INT,
    SoLuong     INT DEFAULT 0,            -- tổng số bản
    SoLuongCon  INT DEFAULT 0,            -- số bản còn trong kho
    TriGia      DECIMAL(18,2) DEFAULT 0,
    GhiChu      NVARCHAR(255)
)
GO

/* ============ 6. BẢNG TRUNG GIAN SÁCH - TÁC GIẢ ============ */
CREATE TABLE Sach_TacGia
(
    MaSach      INT NOT NULL REFERENCES Sach(MaSach),
    MaTacGia    INT NOT NULL REFERENCES TacGia(MaTacGia),
    PRIMARY KEY (MaSach, MaTacGia)
)
GO

/* ============ 7. BẢNG ĐỘC GIẢ ============ */
CREATE TABLE DocGia
(
    MaDocGia    INT IDENTITY(1,1) PRIMARY KEY,
    TenDocGia   NVARCHAR(100) NOT NULL,
    NgaySinh    DATE,
    GioiTinh    BIT DEFAULT 1,             -- 1: Nam, 0: Nữ
    DiaChi      NVARCHAR(200),
    Sdt         VARCHAR(15),
    Email       VARCHAR(100),
    NgayLapThe  DATE DEFAULT GETDATE(),    -- ngày lập thẻ
    NgayHetHan  DATE,                      -- ngày hết hạn thẻ
    TrangThai   BIT DEFAULT 1              -- 1: còn hiệu lực, 0: khóa
)
GO

/* ============ 8. BẢNG PHIẾU MƯỢN ============ */
CREATE TABLE PhieuMuon
(
    MaPhieuMuon INT IDENTITY(1,1) PRIMARY KEY,
    MaDocGia    INT NOT NULL REFERENCES DocGia(MaDocGia),
    MaNhanVien  INT NOT NULL REFERENCES NhanVien(MaNhanVien),
    NgayMuon    DATE DEFAULT GETDATE(),
    NgayHenTra  DATE,                      -- hạn trả dự kiến
    GhiChu      NVARCHAR(255)
)
GO

/* ============ 9. BẢNG CHI TIẾT PHIẾU MƯỢN ============ */
CREATE TABLE ChiTietPhieuMuon
(
    MaPhieuMuon INT NOT NULL REFERENCES PhieuMuon(MaPhieuMuon),
    MaSach      INT NOT NULL REFERENCES Sach(MaSach),
    NgayTra     DATE,                      -- NULL: chưa trả
    TrangThai   BIT DEFAULT 0,             -- 0: đang mượn, 1: đã trả
    PRIMARY KEY (MaPhieuMuon, MaSach)
)
GO

/* ============ 10. BẢNG QUY ĐỊNH THƯ VIỆN ============ */
CREATE TABLE QuyDinh
(
    MaQuyDinh   INT IDENTITY(1,1) PRIMARY KEY,
    TenQuyDinh  NVARCHAR(100) NOT NULL,
    GiaTri      NVARCHAR(255),
    MoTa        NVARCHAR(255)
)
GO

/* ============ 11. BẢNG PHIẾU PHẠT ============ */
CREATE TABLE PhieuPhat
(
    MaPhieuPhat INT IDENTITY(1,1) PRIMARY KEY,
    MaPhieuMuon INT NOT NULL REFERENCES PhieuMuon(MaPhieuMuon),
    MaDocGia    INT NOT NULL REFERENCES DocGia(MaDocGia),
    NgayPhat    DATE DEFAULT GETDATE(),
    LyDo        NVARCHAR(255),
    SoTien      DECIMAL(18,2) DEFAULT 0,
    TrangThai   BIT DEFAULT 0              -- 0: chưa thu, 1: đã thu
)
GO

/* ============================================================
   DỮ LIỆU MẪU
   ============================================================ */

-- Nhân viên (tài khoản: admin / mật khẩu: 123456 — lưu dạng SHA256 hash)
-- Chức vụ "Quản lý" = quyền Admin (thấy mọi menu), "Thủ thư" = quyền thường
INSERT INTO NhanVien (TenNhanVien, TaiKhoan, MatKhau, ChucVu, GioiTinh, NgaySinh, DiaChi, Sdt, Email)
VALUES (N'Nguyễn Văn A', 'admin', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', N'Quản lý', 1, '1995-03-15', N'Hà Nội', '0912345678', 'admin@thuvien.vn')
GO

-- Thể loại
INSERT INTO TheLoai (TenTheLoai, MoTa) VALUES
(N'Văn học',        N'Sách văn học trong nước và nước ngoài'),
(N'Khoa học',       N'Sách khoa học tự nhiên'),
(N'Lịch sử',        N'Sách lịch sử Việt Nam và thế giới'),
(N'Công nghệ TT',   N'Sách tin học, lập trình, CNTT'),
(N'Kinh tế',        N'Sách kinh tế, quản trị kinh doanh')
GO

-- Nhà xuất bản
INSERT INTO NhaXuatBan (TenNXB, DiaChi, Sdt, Email) VALUES
(N'NXB Giáo dục Việt Nam', N'Hà Nội',     '02438220801', 'info@gdvn.vn'),
(N'NXB Kim Đồng',         N'Hà Nội',     '02439433680', 'info@kimdong.vn'),
(N'NXB Trẻ',              N'TP.HCM',     '02839319966', 'info@nxbtre.vn')
GO

-- Tác giả
INSERT INTO TacGia (TenTacGia, NgaySinh, GhiChu) VALUES
(N'Ngô Tất Tố',       '1893-01-01', N'Nhà văn hiện thực phê phán'),
(N'Nam Cao',          '1917-10-29', N'Nhà văn hiện thực'),
(N'Nguyễn Nhật Ánh',  '1955-05-07', N'Nhà văn thiếu nhi'),
(N'Trần Trọng Kim',   '1883-01-01', N'Nhà sử học'),
(N'Đỗ Văn Nam',       '1980-06-20', N'Giảng viên lập trình')
GO

-- Sách
INSERT INTO Sach (TenSach, MaTheLoai, MaNXB, NamXuatBan, SoLuong, SoLuongCon, TriGia, GhiChu) VALUES
(N'Tắt đèn',                1, 3, 1939, 10, 10, 65000,  NULL),
(N'Chí Phèo',               1, 3, 1941, 10, 10, 55000,  NULL),
(N'Kính vạn hoa - Tập 1',   1, 2, 1995, 15, 15, 90000,  NULL),
(N'Lập trình C# cơ bản',    4, 1, 2020,  8,  8, 120000, N'Sách dùng cho sinh viên'),
(N'Việt Nam sử lược',       3, 1, 2015,  5,  5, 150000, NULL)
GO

-- Sách - Tác giả
INSERT INTO Sach_TacGia (MaSach, MaTacGia) VALUES
(1, 1),
(2, 2),
(3, 3),
(4, 5),
(5, 4)
GO

-- Độc giả (hạn thẻ tự tính 1 năm kể từ ngày chạy script để không bao giờ hết hạn ngay)
INSERT INTO DocGia (TenDocGia, NgaySinh, GioiTinh, DiaChi, Sdt, Email, NgayLapThe, NgayHetHan, TrangThai) VALUES
(N'Trần Thị B', '2000-05-10', 0, N'Cầu Giấy, Hà Nội',   '0987654321', 'btt@mail.com', CAST(GETDATE() AS DATE), DATEADD(year, 1, GETDATE()), 1),
(N'Lê Văn C',   '1999-08-20', 1, N'Thanh Xuân, Hà Nội', '0978123456', 'clv@mail.com', CAST(GETDATE() AS DATE), DATEADD(year, 1, GETDATE()), 1),
(N'Phạm Thị D', '2001-12-01', 0, N'Đống Đa, Hà Nội',    '0965111222', 'dpt@mail.com', CAST(GETDATE() AS DATE), DATEADD(year, 1, GETDATE()), 1)
GO

-- Phiếu mượn + chi tiết (1 phiếu ĐANG MƯỢN và QUÁ HẠN để test nhắc trả/phạt, 1 phiếu đã trả)
INSERT INTO PhieuMuon (MaDocGia, MaNhanVien, NgayMuon, NgayHenTra, GhiChu) VALUES
(1, 1, DATEADD(day, -5, GETDATE()),  DATEADD(day, -3, GETDATE()), NULL),
(2, 1, DATEADD(day, -20, GETDATE()), DATEADD(day, -6, GETDATE()), NULL)
GO

INSERT INTO ChiTietPhieuMuon (MaPhieuMuon, MaSach, NgayTra, TrangThai) VALUES
(1, 1, NULL,                                         0),   -- độc giả 1 đang mượn "Tắt đèn"
(1, 2, NULL,                                         0),   -- độc giả 1 đang mượn "Chí Phèo"
(2, 4, DATEADD(day, -1, GETDATE()),                  1)    -- độc giả 2 đã trả "Lập trình C#"
GO

-- Quy định thư viện
INSERT INTO QuyDinh (TenQuyDinh, GiaTri, MoTa) VALUES
(N'Số sách mượn tối đa',   '3',     N'Số đầu sách tối đa mỗi độc giả được mượn'),
(N'Thời hạn mượn (ngày)',  '15',    N'Số ngày mượn tối đa'),
(N'Tiền phạt quá hạn',     '2000',  N'Đơn vị: đồng/ngày')
GO

-- Kiểm tra dữ liệu
SELECT * FROM Sach
SELECT * FROM DocGia
SELECT * FROM PhieuMuon

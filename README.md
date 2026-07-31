# Quản Lý Thư Viện (QLTV)

Ứng dụng desktop quản lý thư viện xây dựng bằng **WPF (.NET Framework 4.8)** theo mô hình **MVVM**, sử dụng **Entity Framework 6 – Database First** và **SQL Server**.

## Tính năng

- **Đăng nhập**: kiểm tra tài khoản, mật khẩu lưu dạng băm SHA256, chọn chức vụ hiển thị theo quyền
- **Phân quyền Admin / Thủ thư**: chỉ quản lý được xem *Quản lý nhân viên* và *Quy định & phạt*
- **Quản lý sách**: CRUD, tìm kiếm theo tên/thể loại/tác giả, kiểm tra số lượng tồn
- **Quản lý độc giả**: CRUD, tìm kiếm, **gia hạn thẻ** thêm 1 năm
- **Mượn / trả sách**:
  - Lập phiếu mượn: kiểm tra hạn thẻ, giới hạn sách theo quy định, tự trừ số lượng tồn
  - Trả sách: tự cộng lại tồn, **tự sinh phiếu phạt** khi trả trễ (mức phạt lấy từ Quy định)
  - Lịch sử phiếu mượn: lọc theo từ khóa, khoảng ngày, **in phiếu mượn** (PrintDialog)
- **Thống kê**: số sách, độc giả, phiếu mượn, sách quá hạn... kèm **xuất Excel** (EPPlus)
- **Quy định & phạt**: chỉnh thời hạn mượn, giới hạn sách, mức phạt; thu tiền phạt
- **Validation**: `IDataErrorInfo`, viền đỏ khi nhập sai, danh sách lỗi khi lưu

## Công nghệ

| Thành phần | Công nghệ |
|---|---|
| Giao diện | WPF (.NET Framework 4.8) |
| Kiến trúc | MVVM (`INotifyPropertyChanged`, `RelayCommand`, `DataTemplate`) |
| Data access | Entity Framework 6.5.1 (Database First – `QLTVModel.edmx`) |
| Cơ sở dữ liệu | SQL Server (script `Database/QLTV.sql`) |
| Xuất Excel | EPPlus 4.5.3.3 |
| Bảo mật | Mật khẩu băm SHA256 |

## Cài đặt và chạy

1. **Tạo database**: chạy `Database/QLTV.sql` trên SQL Server (SSMS)
2. **Mở dự án**: mở `QLTV.sln` bằng Visual Studio 2019/2022/2026 (bản VS2026 có thể mở `QLTV.slnx`)
3. **Sửa connection string** trong `QLTV/App.config` (thẻ `add name="QLTVEntities"`) cho đúng tên SQL Server, ví dụ `Data Source=.\SQLEXPRESS`
4. **Restore NuGet** (hoặc bỏ qua nếu đã có thư mục `packages/` trong repo)
5. Nhấn **F5** để chạy

> Không cần cài thêm gì khác — thư mục `packages/` đã được commit sẵn để clone về là build được ngay.

## Tài khoản mặc định

| Tài khoản | Mật khẩu | Quyền |
|---|---|---|
| `admin` | `123456` | Quản lý (đầy đủ menu) |

Tài khoản thủ thư: tạo trong màn *Quản lý nhân viên* (Admin).

## Cấu trúc dự án

```
QLTV/
├── Database/QLTV.sql          Script tạo DB + dữ liệu mẫu
├── Models/                    Sinh tự động bởi EF6 (KHÔNG sửa)
├── ViewModels/                BaseViewModel, RelayCommand + 8 ViewModel
├── Views/                     LoginWindow, MainWindow + 7 UserControl
├── Helpers/                   PasswordHelper, converters
└── TaiLieu/BaoCaoDoAn.md      Tài liệu đồ án chi tiết (ERD, use case...)
```

## Tài liệu

- Tài liệu đồ án đầy đủ: `TaiLieu/BaoCaoDoAn.md`
- Hướng dẫn triển khai lên máy khác: `Publish/HUONG_DAN.md`

## Đồ án môn học

Đồ án .NET do sinh viên thực hiện nhằm mô phỏng hệ thống quản lý thư viện hoàn chỉnh (đăng nhập phân quyền, mượn/trả, phạt trễ hạn, thống kê, xuất báo cáo).

using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QLTV.Helpers;
using QLTV.Models;

namespace QLTV.ViewModels
{
    public class NhanVienListItem
    {
        public NhanVien NhanVien { get; set; }
        public string GioiTinhText => (NhanVien.GioiTinh ?? true) ? "Nam" : "Nữ";
    }

    public class NhanVienViewModel : BaseViewModel
    {
        private string _searchText;
        private NhanVienListItem _selectedNhanVien;
        private bool _isFormVisible;
        private int? _editingMaNhanVien;
        private string _tenNhanVien;
        private string _taiKhoan;
        private string _matKhau;
        private string _chucVu;
        private bool _gioiTinh;
        private DateTime? _ngaySinh;
        private string _diaChi;
        private string _sdt;
        private string _email;

        public ObservableCollection<NhanVienListItem> DanhSachNhanVien { get; } = new ObservableCollection<NhanVienListItem>();
        public ObservableCollection<string> DanhSachChucVu { get; } = new ObservableCollection<string> { "Quản lý", "Thủ thư" };

        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public override string this[string columnName]
        {
            get
            {
                if (!IsValidationActive) return string.Empty;

                switch (columnName)
                {
                    case nameof(TenNhanVien):
                        return string.IsNullOrWhiteSpace(TenNhanVien) ? "Vui lòng nhập tên nhân viên." : string.Empty;
                    case nameof(TaiKhoan):
                        return string.IsNullOrWhiteSpace(TaiKhoan) ? "Vui lòng nhập tài khoản." : string.Empty;
                    case nameof(Sdt):
                        if (string.IsNullOrWhiteSpace(Sdt)) return string.Empty;
                        return System.Text.RegularExpressions.Regex.IsMatch(Sdt.Trim(), @"^0\d{9,10}$")
                            ? string.Empty
                            : "Số điện thoại không hợp lệ (VD: 0912345678).";
                    case nameof(Email):
                        if (string.IsNullOrWhiteSpace(Email)) return string.Empty;
                        return System.Text.RegularExpressions.Regex.IsMatch(Email.Trim(),
                            @"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                            ? string.Empty
                            : "Email không hợp lệ.";
                    default:
                        return string.Empty;
                }
            }
        }

        public string Title => "Quản lý nhân viên";

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public NhanVienListItem SelectedNhanVien
        {
            get => _selectedNhanVien;
            set => SetProperty(ref _selectedNhanVien, value);
        }

        public bool IsFormVisible
        {
            get => _isFormVisible;
            set => SetProperty(ref _isFormVisible, value);
        }

        public string TenNhanVien
        {
            get => _tenNhanVien;
            set => SetProperty(ref _tenNhanVien, value);
        }

        public string TaiKhoan
        {
            get => _taiKhoan;
            set => SetProperty(ref _taiKhoan, value);
        }

        public string MatKhau
        {
            get => _matKhau;
            set => SetProperty(ref _matKhau, value);
        }

        public string ChucVu
        {
            get => _chucVu;
            set => SetProperty(ref _chucVu, value);
        }

        public bool GioiTinh
        {
            get => _gioiTinh;
            set => SetProperty(ref _gioiTinh, value);
        }

        public DateTime? NgaySinh
        {
            get => _ngaySinh;
            set => SetProperty(ref _ngaySinh, value);
        }

        public string DiaChi
        {
            get => _diaChi;
            set => SetProperty(ref _diaChi, value);
        }

        public string Sdt
        {
            get => _sdt;
            set => SetProperty(ref _sdt, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public NhanVienViewModel()
        {
            SearchCommand = new RelayCommand(_ => LoadNhanVienList());
            AddCommand = new RelayCommand(_ => ShowForm(null));
            EditCommand = new RelayCommand(_ => ShowForm(SelectedNhanVien), _ => SelectedNhanVien != null);
            DeleteCommand = new RelayCommand(_ => DeleteNhanVien(), _ => SelectedNhanVien != null);
            SaveCommand = new RelayCommand(_ => SaveNhanVien());
            CancelCommand = new RelayCommand(_ => CloseForm());

            LoadNhanVienList();
        }

        private void LoadNhanVienList()
        {
            DanhSachNhanVien.Clear();

            using (var db = new QLTVEntities())
            {
                var query = db.NhanViens.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                    query = query.Where(n => n.TenNhanVien.Contains(SearchText) || n.TaiKhoan.Contains(SearchText));

                foreach (var n in query.OrderBy(n => n.TenNhanVien).ToList())
                    DanhSachNhanVien.Add(new NhanVienListItem { NhanVien = n });
            }
        }

        private void ShowForm(NhanVienListItem item)
        {
            ResetValidation();
            _editingMaNhanVien = item?.NhanVien.MaNhanVien;

            if (item == null)
            {
                TenNhanVien = string.Empty;
                TaiKhoan = string.Empty;
                MatKhau = string.Empty;
                ChucVu = string.Empty;
                GioiTinh = true;
                NgaySinh = null;
                DiaChi = string.Empty;
                Sdt = string.Empty;
                Email = string.Empty;
            }
            else
            {
                TenNhanVien = item.NhanVien.TenNhanVien;
                TaiKhoan = item.NhanVien.TaiKhoan;
                MatKhau = item.NhanVien.MatKhau;
                ChucVu = item.NhanVien.ChucVu;
                GioiTinh = item.NhanVien.GioiTinh ?? true;
                NgaySinh = item.NhanVien.NgaySinh;
                DiaChi = item.NhanVien.DiaChi;
                Sdt = item.NhanVien.Sdt;
                Email = item.NhanVien.Email;
            }

            IsFormVisible = true;
        }

        private void SaveNhanVien()
        {
            ErrorSummary = GetValidationSummary(
                nameof(TenNhanVien), nameof(TaiKhoan), nameof(Sdt), nameof(Email));

            if (!string.IsNullOrEmpty(ErrorSummary))
                return;

            using (var db = new QLTVEntities())
            {
                if (_editingMaNhanVien == null &&
                    db.NhanViens.Any(n => n.TaiKhoan == TaiKhoan.Trim()))
                {
                    MessageBox.Show("Tài khoản này đã tồn tại.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                NhanVien nhanVien;

                if (_editingMaNhanVien == null)
                {
                    nhanVien = new NhanVien();
                    db.NhanViens.Add(nhanVien);
                }
                else
                {
                    nhanVien = db.NhanViens.Find(_editingMaNhanVien);
                }

                nhanVien.TenNhanVien = TenNhanVien.Trim();
                nhanVien.TaiKhoan = TaiKhoan.Trim();
                nhanVien.MatKhau = PasswordHelper.Hash(string.IsNullOrWhiteSpace(MatKhau) ? "123456" : MatKhau);
                nhanVien.ChucVu = ChucVu;
                nhanVien.GioiTinh = GioiTinh;
                nhanVien.NgaySinh = NgaySinh;
                nhanVien.DiaChi = DiaChi;
                nhanVien.Sdt = Sdt;
                nhanVien.Email = Email;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    MessageBox.Show("Không thể lưu dữ liệu. Kiểm tra tài khoản không bị trùng.",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            CloseForm();
            LoadNhanVienList();
        }

        private void DeleteNhanVien()
        {
            if (SelectedNhanVien == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa nhân viên \"{SelectedNhanVien.NhanVien.TenNhanVien}\"?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            using (var db = new QLTVEntities())
            {
                var nhanVien = db.NhanViens.Find(SelectedNhanVien.NhanVien.MaNhanVien);
                if (nhanVien == null) return;

                db.NhanViens.Remove(nhanVien);

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    MessageBox.Show("Không thể xóa nhân viên vì đang có phiếu mượn liên quan.",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            LoadNhanVienList();
        }

        private void CloseForm()
        {
            IsFormVisible = false;
            SelectedNhanVien = null;
        }
    }
}

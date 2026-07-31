using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using QLTV.Models;

namespace QLTV.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private MenuItem _selectedMenuItem;
        private BaseViewModel _currentViewModel;

        public ObservableCollection<MenuItem> MenuItems { get; }
        public string UserInfo { get; }
        public bool IsAdmin { get; }
        public ICommand LogoutCommand { get; }

        public event Action LogoutRequested;

        public MenuItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                if (SetProperty(ref _selectedMenuItem, value) && value != null)
                    CurrentViewModel = value.ViewModel;
            }
        }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public MainViewModel(NhanVien nhanVien)
        {
            UserInfo = $"{nhanVien.TenNhanVien} - {nhanVien.ChucVu}";
            IsAdmin = nhanVien.ChucVu != null &&
                (nhanVien.ChucVu.Trim().Equals("Quản lý", StringComparison.OrdinalIgnoreCase) ||
                 nhanVien.ChucVu.IndexOf("admin", StringComparison.OrdinalIgnoreCase) >= 0);

            LogoutCommand = new RelayCommand(_ => LogoutRequested?.Invoke());

            MenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem("Tổng quan", new DashboardViewModel(nhanVien)),
                new MenuItem("Quản lý sách", new SachViewModel()),
                new MenuItem("Quản lý độc giả", new DocGiaViewModel()),
                new MenuItem("Mượn / Trả sách", new PhieuMuonViewModel(nhanVien)),
                new MenuItem("Thống kê", new ThongKeViewModel())
            };

            if (IsAdmin)
            {
                MenuItems.Add(new MenuItem("Quản lý nhân viên", new NhanVienViewModel()));
                MenuItems.Add(new MenuItem("Quy định & phạt", new QuyDinhViewModel()));
            }

            SelectedMenuItem = MenuItems[0];
        }
    }
}

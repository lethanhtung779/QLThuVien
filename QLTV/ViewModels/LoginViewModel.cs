using System;
using System.Linq;
using System.Windows.Input;
using QLTV.Helpers;
using QLTV.Models;

namespace QLTV.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _taiKhoan;
        private string _matKhau;
        private string _errorMessage;

        public override string this[string columnName]
        {
            get
            {
                if (!IsValidationActive) return string.Empty;

                switch (columnName)
                {
                    case nameof(TaiKhoan):
                        return string.IsNullOrWhiteSpace(TaiKhoan) ? "Vui lòng nhập tài khoản." : string.Empty;
                    case nameof(MatKhau):
                        return string.IsNullOrWhiteSpace(MatKhau) ? "Vui lòng nhập mật khẩu." : string.Empty;
                    default:
                        return string.Empty;
                }
            }
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                    OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(_errorMessage);

        public ICommand LoginCommand { get; }

        public event Action<NhanVien> LoginSuccess;

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);
        }

        private void Login(object parameter)
        {
            ErrorMessage = string.Empty;

            ErrorSummary = GetValidationSummary(nameof(TaiKhoan), nameof(MatKhau));
            if (!string.IsNullOrEmpty(ErrorSummary))
                return;

            using (var db = new QLTVEntities())
            {
                var hashedPassword = PasswordHelper.Hash(MatKhau);
                var nhanVien = db.NhanViens.FirstOrDefault(
                    nv => nv.TaiKhoan == TaiKhoan && nv.MatKhau == hashedPassword);

                if (nhanVien == null)
                {
                    ErrorMessage = "Tài khoản hoặc mật khẩu không đúng.";
                    return;
                }

                LoginSuccess?.Invoke(nhanVien);
            }
        }
    }
}

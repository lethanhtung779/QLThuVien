using System.Windows;
using QLTV.Models;
using QLTV.ViewModels;

namespace QLTV.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            _viewModel.LoginSuccess += OnLoginSuccess;
            DataContext = _viewModel;
            txtTaiKhoan.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.MatKhau = txtMatKhau.Password;
            if (_viewModel.LoginCommand.CanExecute(null))
                _viewModel.LoginCommand.Execute(null);
        }

        private void OnLoginSuccess(NhanVien nhanVien)
        {
            var mainWindow = new MainWindow(nhanVien);
            mainWindow.Show();
            Close();
        }
    }
}

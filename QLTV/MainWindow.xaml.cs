using System.Windows;
using QLTV.Models;
using QLTV.ViewModels;
using QLTV.Views;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(NhanVien nhanVien)
        {
            InitializeComponent();
            var viewModel = new MainViewModel(nhanVien);
            viewModel.LogoutRequested += OnLogoutRequested;
            DataContext = viewModel;
        }

        private void OnLogoutRequested()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}

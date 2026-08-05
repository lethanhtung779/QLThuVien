using System.Windows;
using System.Windows.Controls;
using QLTV.ViewModels;

namespace QLTV.Views
{
    public partial class PhieuMuonView : UserControl
    {
        public PhieuMuonView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PhieuMuonViewModel viewModel)
                viewModel.RefreshData();
        }
    }
}

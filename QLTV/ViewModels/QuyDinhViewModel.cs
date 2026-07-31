using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QLTV.Models;

namespace QLTV.ViewModels
{
    public class PhieuPhatListItem
    {
        public PhieuPhat PhieuPhat { get; set; }
        public string TenDocGia { get; set; }
        public string TrangThaiText => (PhieuPhat.TrangThai ?? false) ? "Đã thu" : "Chưa thu";
    }

    public class QuyDinhViewModel : BaseViewModel
    {
        private PhieuPhatListItem _selectedPhieuPhat;

        public ObservableCollection<QuyDinh> DanhSachQuyDinh { get; } = new ObservableCollection<QuyDinh>();
        public ObservableCollection<PhieuPhatListItem> DanhSachPhieuPhat { get; } = new ObservableCollection<PhieuPhatListItem>();

        public ICommand SaveCommand { get; }
        public ICommand ThuTienCommand { get; }

        public string Title => "Quy định & phạt";

        public PhieuPhatListItem SelectedPhieuPhat
        {
            get => _selectedPhieuPhat;
            set => SetProperty(ref _selectedPhieuPhat, value);
        }

        public QuyDinhViewModel()
        {
            SaveCommand = new RelayCommand(_ => SaveQuyDinh());
            ThuTienCommand = new RelayCommand(_ => ThuTien(), _ => SelectedPhieuPhat != null && (SelectedPhieuPhat.PhieuPhat.TrangThai ?? false) == false);

            LoadData();
        }

        private void LoadData()
        {
            DanhSachQuyDinh.Clear();
            DanhSachPhieuPhat.Clear();

            using (var db = new QLTVEntities())
            {
                foreach (var q in db.QuyDinhs.OrderBy(q => q.MaQuyDinh).ToList())
                    DanhSachQuyDinh.Add(q);

                foreach (var p in db.PhieuPhats.OrderByDescending(p => p.NgayPhat).ToList())
                {
                    DanhSachPhieuPhat.Add(new PhieuPhatListItem
                    {
                        PhieuPhat = p,
                        TenDocGia = p.DocGia?.TenDocGia
                    });
                }
            }
        }

        private void SaveQuyDinh()
        {
            using (var db = new QLTVEntities())
            {
                foreach (var quyDinh in DanhSachQuyDinh)
                {
                    if (string.IsNullOrWhiteSpace(quyDinh.GiaTri))
                    {
                        MessageBox.Show("Giá trị quy định không được để trống.",
                            "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    db.QuyDinhs.Attach(quyDinh);
                    db.Entry(quyDinh).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();
            }

            MessageBox.Show("Đã lưu quy định thư viện.", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ThuTien()
        {
            if (SelectedPhieuPhat == null) return;

            using (var db = new QLTVEntities())
            {
                var phieuPhat = db.PhieuPhats.Find(SelectedPhieuPhat.PhieuPhat.MaPhieuPhat);
                if (phieuPhat != null)
                {
                    phieuPhat.TrangThai = true;
                    db.SaveChanges();
                }
            }

            LoadData();
        }
    }
}

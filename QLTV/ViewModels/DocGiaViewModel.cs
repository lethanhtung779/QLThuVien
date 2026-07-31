using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QLTV.Models;

namespace QLTV.ViewModels
{
    public class DocGiaListItem
    {
        public DocGia DocGia { get; set; }
        public string GioiTinhText => (DocGia.GioiTinh ?? true) ? "Nam" : "Nữ";
        public string TrangThaiText => (DocGia.TrangThai ?? true) ? "Còn hiệu lực" : "Đã khóa";
    }

    public class DocGiaViewModel : BaseViewModel
    {
        private string _searchText;
        private DocGiaListItem _selectedDocGia;
        private bool _isFormVisible;
        private int? _editingMaDocGia;
        private string _tenDocGia;
        private bool _gioiTinh;
        private DateTime? _ngaySinh;
        private string _diaChi;
        private string _sdt;
        private string _email;
        private DateTime? _ngayLapThe;
        private DateTime? _ngayHetHan;
        private bool _trangThai;

        public ObservableCollection<DocGiaListItem> DanhSachDocGia { get; } = new ObservableCollection<DocGiaListItem>();

        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand GiaHanCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public override string this[string columnName]
        {
            get
            {
                if (!IsValidationActive) return string.Empty;

                switch (columnName)
                {
                    case nameof(TenDocGia):
                        return string.IsNullOrWhiteSpace(TenDocGia) ? "Vui lòng nhập tên độc giả." : string.Empty;
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
                    case nameof(NgayHetHan):
                        return NgayHetHan.HasValue && NgayLapThe.HasValue && NgayHetHan < NgayLapThe
                            ? "Ngày hết hạn thẻ không được trước ngày lập thẻ."
                            : string.Empty;
                    default:
                        return string.Empty;
                }
            }
        }

        public string Title => "Quản lý độc giả";

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public DocGiaListItem SelectedDocGia
        {
            get => _selectedDocGia;
            set => SetProperty(ref _selectedDocGia, value);
        }

        public bool IsFormVisible
        {
            get => _isFormVisible;
            set => SetProperty(ref _isFormVisible, value);
        }

        public string TenDocGia
        {
            get => _tenDocGia;
            set => SetProperty(ref _tenDocGia, value);
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

        public DateTime? NgayLapThe
        {
            get => _ngayLapThe;
            set => SetProperty(ref _ngayLapThe, value);
        }

        public DateTime? NgayHetHan
        {
            get => _ngayHetHan;
            set => SetProperty(ref _ngayHetHan, value);
        }

        public bool TrangThai
        {
            get => _trangThai;
            set => SetProperty(ref _trangThai, value);
        }

        public DocGiaViewModel()
        {
            SearchCommand = new RelayCommand(_ => LoadDocGiaList());
            AddCommand = new RelayCommand(_ => ShowForm(null));
            EditCommand = new RelayCommand(_ => ShowForm(SelectedDocGia), _ => SelectedDocGia != null);
            DeleteCommand = new RelayCommand(_ => DeleteDocGia(), _ => SelectedDocGia != null);
            GiaHanCommand = new RelayCommand(_ => GiaHanThe(), _ => SelectedDocGia != null);
            SaveCommand = new RelayCommand(_ => SaveDocGia());
            CancelCommand = new RelayCommand(_ => CloseForm());

            LoadDocGiaList();
        }

        private void GiaHanThe()
        {
            if (SelectedDocGia == null) return;

            var hanMoi = (SelectedDocGia.DocGia.NgayHetHan ?? DateTime.Today) < DateTime.Today
                ? DateTime.Today.AddYears(1)
                : SelectedDocGia.DocGia.NgayHetHan.Value.AddYears(1);

            var result = MessageBox.Show(
                $"Gia hạn thẻ cho \"{SelectedDocGia.DocGia.TenDocGia}\" thêm 1 năm?\nHạn mới: {hanMoi:dd/MM/yyyy}",
                "Gia hạn thẻ", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            using (var db = new QLTVEntities())
            {
                var docGia = db.DocGias.Find(SelectedDocGia.DocGia.MaDocGia);
                if (docGia == null) return;

                docGia.NgayHetHan = hanMoi;
                docGia.TrangThai = true;
                db.SaveChanges();
            }

            LoadDocGiaList();
        }

        private void LoadDocGiaList()
        {
            DanhSachDocGia.Clear();

            using (var db = new QLTVEntities())
            {
                var query = db.DocGias.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                    query = query.Where(d => d.TenDocGia.Contains(SearchText) || d.Sdt.Contains(SearchText));

                foreach (var d in query.OrderBy(d => d.TenDocGia).ToList())
                    DanhSachDocGia.Add(new DocGiaListItem { DocGia = d });
            }
        }

        private void ShowForm(DocGiaListItem item)
        {
            ResetValidation();
            _editingMaDocGia = item?.DocGia.MaDocGia;

            if (item == null)
            {
                TenDocGia = string.Empty;
                GioiTinh = true;
                NgaySinh = null;
                DiaChi = string.Empty;
                Sdt = string.Empty;
                Email = string.Empty;
                NgayLapThe = DateTime.Today;
                NgayHetHan = DateTime.Today.AddYears(1);
                TrangThai = true;
            }
            else
            {
                TenDocGia = item.DocGia.TenDocGia;
                GioiTinh = item.DocGia.GioiTinh ?? true;
                NgaySinh = item.DocGia.NgaySinh;
                DiaChi = item.DocGia.DiaChi;
                Sdt = item.DocGia.Sdt;
                Email = item.DocGia.Email;
                NgayLapThe = item.DocGia.NgayLapThe;
                NgayHetHan = item.DocGia.NgayHetHan;
                TrangThai = item.DocGia.TrangThai ?? true;
            }

            IsFormVisible = true;
        }

        private void SaveDocGia()
        {
            ErrorSummary = GetValidationSummary(
                nameof(TenDocGia), nameof(Sdt), nameof(Email), nameof(NgayHetHan));

            if (!string.IsNullOrEmpty(ErrorSummary))
                return;

            using (var db = new QLTVEntities())
            {
                DocGia docGia;

                if (_editingMaDocGia == null)
                {
                    docGia = new DocGia();
                    db.DocGias.Add(docGia);
                }
                else
                {
                    docGia = db.DocGias.Find(_editingMaDocGia);
                }

                docGia.TenDocGia = TenDocGia.Trim();
                docGia.GioiTinh = GioiTinh;
                docGia.NgaySinh = NgaySinh;
                docGia.DiaChi = DiaChi;
                docGia.Sdt = Sdt;
                docGia.Email = Email;
                docGia.NgayLapThe = NgayLapThe ?? DateTime.Today;
                docGia.NgayHetHan = NgayHetHan;
                docGia.TrangThai = TrangThai;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    MessageBox.Show("Không thể lưu dữ liệu. Vui lòng kiểm tra lại thông tin.",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            CloseForm();
            LoadDocGiaList();
        }

        private void DeleteDocGia()
        {
            if (SelectedDocGia == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa độc giả \"{SelectedDocGia.DocGia.TenDocGia}\"?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            using (var db = new QLTVEntities())
            {
                var docGia = db.DocGias.Find(SelectedDocGia.DocGia.MaDocGia);
                if (docGia == null) return;

                db.DocGias.Remove(docGia);

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    MessageBox.Show("Không thể xóa độc giả vì đang có phiếu mượn liên quan.",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            LoadDocGiaList();
        }

        private void CloseForm()
        {
            IsFormVisible = false;
            SelectedDocGia = null;
        }
    }
}

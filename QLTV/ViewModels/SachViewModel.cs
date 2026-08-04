using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QLTV.Models;

namespace QLTV.ViewModels
{
    public class SachListItem
    {
        public Sach Sach { get; set; }
        public string TenTheLoai { get; set; }
        public string TenNXB { get; set; }
        public string TacGias { get; set; }
    }

    public class SachViewModel : BaseViewModel
    {
        private string _searchText;
        private SachListItem _selectedSach;
        private bool _isFormVisible;
        private int? _editingMaSach;
        private string _tenSach;
        private TheLoai _selectedTheLoai;
        private NhaXuatBan _selectedNxb;
        private TacGia _selectedTacGia;
        private int? _namXuatBan;
        private int? _soLuong;
        private int? _soLuongCon;
        private decimal? _triGia;
        private string _ghiChu;

        public ObservableCollection<SachListItem> DanhSachSach { get; } = new ObservableCollection<SachListItem>();
        public ObservableCollection<TheLoai> TheLoais { get; } = new ObservableCollection<TheLoai>();
        public ObservableCollection<NhaXuatBan> NhaXuatBans { get; } = new ObservableCollection<NhaXuatBan>();
        public ObservableCollection<TacGia> TacGias { get; } = new ObservableCollection<TacGia>();

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
                    case nameof(TenSach):
                        return string.IsNullOrWhiteSpace(TenSach) ? "Vui lòng nhập tên sách." : string.Empty;
                    case nameof(SelectedTheLoai):
                        return SelectedTheLoai == null ? "Vui lòng chọn thể loại." : string.Empty;
                    case nameof(SelectedNxb):
                        return SelectedNxb == null ? "Vui lòng chọn nhà xuất bản." : string.Empty;
                    case nameof(NamXuatBan):
                        return NamXuatBan.HasValue && (NamXuatBan < 1900 || NamXuatBan > DateTime.Now.Year + 1)
                            ? "Năm xuất bản không hợp lệ."
                            : string.Empty;
                    case nameof(SoLuong):
                        return SoLuong.HasValue && SoLuong < 0 ? "Tổng số lượng không được âm." : string.Empty;
                    case nameof(SoLuongCon):
                        return SoLuongCon.HasValue && SoLuongCon > (SoLuong ?? 0)
                            ? "Số lượng còn lại không được lớn hơn tổng số lượng."
                            : string.Empty;
                    case nameof(TriGia):
                        return TriGia.HasValue && TriGia < 0 ? "Đơn giá không được âm." : string.Empty;
                    default:
                        return string.Empty;
                }
            }
        }

        public string Title => "Quản lý sách";

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public SachListItem SelectedSach
        {
            get => _selectedSach;
            set => SetProperty(ref _selectedSach, value);
        }

        public bool IsFormVisible
        {
            get => _isFormVisible;
            set => SetProperty(ref _isFormVisible, value);
        }

        public string TenSach
        {
            get => _tenSach;
            set => SetProperty(ref _tenSach, value);
        }

        public TheLoai SelectedTheLoai
        {
            get => _selectedTheLoai;
            set => SetProperty(ref _selectedTheLoai, value);
        }

        public NhaXuatBan SelectedNxb
        {
            get => _selectedNxb;
            set => SetProperty(ref _selectedNxb, value);
        }

        public TacGia SelectedTacGia
        {
            get => _selectedTacGia;
            set => SetProperty(ref _selectedTacGia, value);
        }

        public int? NamXuatBan
        {
            get => _namXuatBan;
            set => SetProperty(ref _namXuatBan, value);
        }

        public int? SoLuong
        {
            get => _soLuong;
            set => SetProperty(ref _soLuong, value);
        }

        public int? SoLuongCon
        {
            get => _soLuongCon;
            set => SetProperty(ref _soLuongCon, value);
        }

        public decimal? TriGia
        {
            get => _triGia;
            set => SetProperty(ref _triGia, value);
        }

        public string GhiChu
        {
            get => _ghiChu;
            set => SetProperty(ref _ghiChu, value);
        }

        public SachViewModel()
        {
            SearchCommand = new RelayCommand(_ => LoadSachList());
            AddCommand = new RelayCommand(_ => ShowForm(null));
            EditCommand = new RelayCommand(_ => ShowForm(SelectedSach), _ => SelectedSach != null);
            DeleteCommand = new RelayCommand(_ => DeleteSach(), _ => SelectedSach != null);
            SaveCommand = new RelayCommand(_ => SaveSach());
            CancelCommand = new RelayCommand(_ => CloseForm());

            LoadDanhMuc();
            LoadSachList();
        }

        private void LoadDanhMuc()
        {
            using (var db = new QLTVEntities())
            {
                foreach (var theLoai in db.TheLoais.OrderBy(t => t.TenTheLoai).ToList())
                    TheLoais.Add(theLoai);
                foreach (var nxb in db.NhaXuatBans.OrderBy(n => n.TenNXB).ToList())
                    NhaXuatBans.Add(nxb);
                foreach (var tacGia in db.TacGias.OrderBy(t => t.TenTacGia).ToList())
                    TacGias.Add(tacGia);
            }
        }

        private void LoadSachList()
        {
            DanhSachSach.Clear();

            using (var db = new QLTVEntities())
            {
                var query = db.Saches
                    .Include(s => s.TheLoai)
                    .Include(s => s.NhaXuatBan)
                    .Include(s => s.TacGias)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                    query = query.Where(s => s.TenSach.Contains(SearchText));

                foreach (var s in query.OrderBy(s => s.TenSach).ToList())
                {
                    DanhSachSach.Add(new SachListItem
                    {
                        Sach = s,
                        TenTheLoai = s.TheLoai?.TenTheLoai,
                        TenNXB = s.NhaXuatBan?.TenNXB,
                        TacGias = string.Join(", ", s.TacGias.Select(t => t.TenTacGia))
                    });
                }
            }
        }

        private void ShowForm(SachListItem item)
        {
            ResetValidation();
            _editingMaSach = item?.Sach.MaSach;

            if (item == null)
            {
                TenSach = string.Empty;
                SelectedTheLoai = null;
                SelectedNxb = null;
                SelectedTacGia = null;
                NamXuatBan = DateTime.Now.Year;
                SoLuong = 1;
                SoLuongCon = 1;
                TriGia = 0;
                GhiChu = string.Empty;
            }
            else
            {
                TenSach = item.Sach.TenSach;
                SelectedTheLoai = TheLoais.FirstOrDefault(t => t.MaTheLoai == item.Sach.MaTheLoai);
                SelectedNxb = NhaXuatBans.FirstOrDefault(n => n.MaNXB == item.Sach.MaNXB);
                NamXuatBan = item.Sach.NamXuatBan;
                SoLuong = item.Sach.SoLuong;
                SoLuongCon = item.Sach.SoLuongCon;
                TriGia = item.Sach.TriGia;
                GhiChu = item.Sach.GhiChu;

                using (var db = new QLTVEntities())
                {
                    var tacGia = db.Saches.Find(item.Sach.MaSach)?.TacGias.FirstOrDefault();
                    SelectedTacGia = TacGias.FirstOrDefault(t => t.MaTacGia == tacGia?.MaTacGia);
                }
            }

            IsFormVisible = true;
        }

        private void SaveSach()
        {
            ErrorSummary = GetValidationSummary(
                nameof(TenSach), nameof(SelectedTheLoai), nameof(SelectedNxb),
                nameof(NamXuatBan), nameof(SoLuong), nameof(SoLuongCon), nameof(TriGia));

            if (!string.IsNullOrEmpty(ErrorSummary))
                return;

            using (var db = new QLTVEntities())
            {
                Sach sach;

                if (_editingMaSach == null)
                {
                    sach = new Sach();
                    db.Saches.Add(sach);
                }
                else
                {
                    sach = db.Saches.Find(_editingMaSach);
                    sach.TacGias.Clear();
                }

                sach.TenSach = TenSach.Trim();
                sach.MaTheLoai = SelectedTheLoai.MaTheLoai;
                sach.MaNXB = SelectedNxb.MaNXB;
                sach.NamXuatBan = NamXuatBan;
                sach.SoLuong = SoLuong;
                sach.SoLuongCon = _editingMaSach == null ? SoLuong : SoLuongCon;
                sach.TriGia = TriGia;
                sach.GhiChu = GhiChu;

                if (SelectedTacGia != null)
                    sach.TacGias.Add(db.TacGias.Find(SelectedTacGia.MaTacGia));

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
            LoadSachList();
        }

        private void DeleteSach()
        {
            if (SelectedSach == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa sách \"{SelectedSach.Sach.TenSach}\"?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            using (var db = new QLTVEntities())
            {
                var sach = db.Saches.Find(SelectedSach.Sach.MaSach);
                if (sach == null) return;

                foreach (var tacGia in sach.TacGias.ToList())
                    sach.TacGias.Remove(tacGia);

                var dangMuon = db.ChiTietPhieuMuons.Any(c => c.MaSach == sach.MaSach &&
                    (c.TrangThai ?? false) == false);

                if (dangMuon)
                {
                    MessageBox.Show(
                        $"Sách \"{sach.TenSach}\" đang được mượn chưa trả, không thể xóa. Hãy trả sách trước khi xóa.",
                        "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var lichSu = db.ChiTietPhieuMuons.Where(c => c.MaSach == sach.MaSach).ToList();
                if (lichSu.Count > 0)
                {
                    var xacNhan = MessageBox.Show(
                        $"Sách \"{sach.TenSach}\" đã được trả trong {lichSu.Count} phiếu mượn.\n" +
                        "Xóa sách sẽ xóa luôn các dòng chi tiết trong lịch sử phiếu mượn này. Tiếp tục?",
                        "Xóa sách có lịch sử", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (xacNhan != MessageBoxResult.Yes)
                        return;

                    db.ChiTietPhieuMuons.RemoveRange(lichSu);
                }

                db.Saches.Remove(sach);

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    MessageBox.Show("Không thể xóa sách vì sách đang được tham chiếu trong phiếu mượn.",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            LoadSachList();
        }

        private void CloseForm()
        {
            IsFormVisible = false;
            SelectedSach = null;
        }
    }
}

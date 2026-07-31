using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using QLTV.Models;

namespace QLTV.ViewModels
{
    public class SachChonItem : BaseViewModel
    {
        public Sach Sach { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged();
            }
        }

        public string HienThi => $"{Sach.TenSach}  (còn {Sach.SoLuongCon})";
    }

    public class PhieuMuonListItem
    {
        public PhieuMuon PhieuMuon { get; set; }
        public string TenDocGia { get; set; }
        public int SoLuongSach { get; set; }
    }

    public class ChiTietMuonItem
    {
        public ChiTietPhieuMuon ChiTiet { get; set; }
        public string TenSach { get; set; }
        public string TrangThaiText => (ChiTiet.TrangThai ?? false) ? "Đã trả" : "Đang mượn";
    }

    public class LichSuPhieuItem
    {
        public PhieuMuon PhieuMuon { get; set; }
        public string TenDocGia { get; set; }
        public string TenNhanVien { get; set; }
        public int TongSach { get; set; }
        public int SoDaTra { get; set; }
        public string TrangThaiText => SoDaTra >= TongSach ? "Đã trả hết" : "Đang mượn";
    }

    public class DocGiaQuaHanItem
    {
        public DocGia DocGia { get; set; }
        public int SoLuongSachQuaHan { get; set; }
        public int SoNgayQuaHanToiDa { get; set; }
    }

    public class SachQuaHanItem
    {
        public ChiTietPhieuMuon ChiTiet { get; set; }
        public string TenSach { get; set; }
        public int SoNgayQuaHan { get; set; }
    }

    public class PhieuMuonViewModel : BaseViewModel
    {
        private readonly NhanVien _nhanVien;
        private DocGia _selectedDocGia;
        private DateTime? _ngayMuon;
        private DateTime? _ngayHenTra;
        private string _thongBao;
        private PhieuMuonListItem _selectedPhieu;
        private string _thongBaoTra;
        private string _lichSuKeyword;
        private DateTime? _lichSuTuNgay;
        private DateTime? _lichSuDenNgay;
        private LichSuPhieuItem _selectedLichSuPhieu;
        private DocGiaQuaHanItem _selectedDocGiaQuaHan;

        public ObservableCollection<DocGia> DanhSachDocGia { get; } = new ObservableCollection<DocGia>();
        public ObservableCollection<SachChonItem> DanhSachSachKhaDung { get; } = new ObservableCollection<SachChonItem>();
        public ObservableCollection<SachChonItem> SachDaChon { get; } = new ObservableCollection<SachChonItem>();
        public ObservableCollection<PhieuMuonListItem> DanhSachPhieuDangMuon { get; } = new ObservableCollection<PhieuMuonListItem>();
        public ObservableCollection<ChiTietMuonItem> ChiTietPhieuDangChon { get; } = new ObservableCollection<ChiTietMuonItem>();
        public ObservableCollection<LichSuPhieuItem> DanhSachLichSu { get; } = new ObservableCollection<LichSuPhieuItem>();
        public ObservableCollection<ChiTietMuonItem> ChiTietLichSu { get; } = new ObservableCollection<ChiTietMuonItem>();
        public ObservableCollection<DocGiaQuaHanItem> DanhSachDocGiaQuaHan { get; } = new ObservableCollection<DocGiaQuaHanItem>();
        public ObservableCollection<SachQuaHanItem> DanhSachSachQuaHan { get; } = new ObservableCollection<SachQuaHanItem>();

        public ICommand ChonSachCommand { get; }
        public ICommand BoSachCommand { get; }
        public ICommand LamPhieuCommand { get; }
        public ICommand TraSachCommand { get; }
        public ICommand TraTatCaCommand { get; }
        public ICommand LocLichSuCommand { get; }
        public ICommand InPhieuCommand { get; }
        public ICommand InPhieuTraCommand { get; }
        public ICommand LamMoiNhacTraCommand { get; }
        public ICommand InGiayNhacTraCommand { get; }

        public string Title => "Mượn / Trả sách";

        public DocGia SelectedDocGia
        {
            get => _selectedDocGia;
            set => SetProperty(ref _selectedDocGia, value);
        }

        public DateTime? NgayMuon
        {
            get => _ngayMuon;
            set => SetProperty(ref _ngayMuon, value);
        }

        public DateTime? NgayHenTra
        {
            get => _ngayHenTra;
            set => SetProperty(ref _ngayHenTra, value);
        }

        public string ThongBao
        {
            get => _thongBao;
            set => SetProperty(ref _thongBao, value);
        }

        public PhieuMuonListItem SelectedPhieu
        {
            get => _selectedPhieu;
            set
            {
                if (SetProperty(ref _selectedPhieu, value))
                    LoadChiTietPhieu();
            }
        }

        public string ThongBaoTra
        {
            get => _thongBaoTra;
            set => SetProperty(ref _thongBaoTra, value);
        }

        public string LichSuKeyword
        {
            get => _lichSuKeyword;
            set => SetProperty(ref _lichSuKeyword, value);
        }

        public DateTime? LichSuTuNgay
        {
            get => _lichSuTuNgay;
            set => SetProperty(ref _lichSuTuNgay, value);
        }

        public DateTime? LichSuDenNgay
        {
            get => _lichSuDenNgay;
            set => SetProperty(ref _lichSuDenNgay, value);
        }

        public LichSuPhieuItem SelectedLichSuPhieu
        {
            get => _selectedLichSuPhieu;
            set
            {
                if (SetProperty(ref _selectedLichSuPhieu, value))
                    LoadChiTietLichSu();
            }
        }

        public DocGiaQuaHanItem SelectedDocGiaQuaHan
        {
            get => _selectedDocGiaQuaHan;
            set
            {
                if (SetProperty(ref _selectedDocGiaQuaHan, value))
                    LoadSachQuaHan();
            }
        }

        public PhieuMuonViewModel(NhanVien nhanVien)
        {
            _nhanVien = nhanVien;

            ChonSachCommand = new RelayCommand(_ => ChonSach());
            BoSachCommand = new RelayCommand(_ => BoSach());
            LamPhieuCommand = new RelayCommand(_ => LamPhieuMuon());
            TraSachCommand = new RelayCommand(TraSach, _ => true);
            TraTatCaCommand = new RelayCommand(_ => TraTatCa());
            LocLichSuCommand = new RelayCommand(_ => LoadLichSu());
            InPhieuCommand = new RelayCommand(_ => InPhieuMuon(), _ => SelectedLichSuPhieu != null);
            InPhieuTraCommand = new RelayCommand(_ => InPhieuTra(), _ => SelectedLichSuPhieu != null);
            LamMoiNhacTraCommand = new RelayCommand(_ => LoadDocGiaQuaHan());
            InGiayNhacTraCommand = new RelayCommand(_ => InGiayNhacTra(), _ => SelectedDocGiaQuaHan != null);

            NgayMuon = DateTime.Today;
            NgayHenTra = DateTime.Today.AddDays(GetQuyDinhSoNgayMuon());

            LoadDocGias();
            LoadSachKhaDung();
            LoadPhieuDangMuon();
            LoadLichSu();
            LoadDocGiaQuaHan();
        }

        private int GetQuyDinhSoNgayMuon()
        {
            using (var db = new QLTVEntities())
            {
                var qd = db.QuyDinhs.FirstOrDefault(q => q.TenQuyDinh.Contains("hạn mượn"));
                return qd != null && int.TryParse(qd.GiaTri, out var soNgay) ? soNgay : 15;
            }
        }

        private void LoadDocGias()
        {
            DanhSachDocGia.Clear();
            using (var db = new QLTVEntities())
            {
                foreach (var d in db.DocGias.OrderBy(d => d.TenDocGia).ToList())
                    DanhSachDocGia.Add(d);
            }
        }

        private void LoadSachKhaDung()
        {
            DanhSachSachKhaDung.Clear();
            using (var db = new QLTVEntities())
            {
                foreach (var s in db.Saches.Where(s => s.SoLuongCon > 0).OrderBy(s => s.TenSach).ToList())
                {
                    DanhSachSachKhaDung.Add(new SachChonItem { Sach = s, IsChecked = false });
                }
            }
        }

        private void LoadPhieuDangMuon()
        {
            DanhSachPhieuDangMuon.Clear();
            ChiTietPhieuDangChon.Clear();
            SelectedPhieu = null;

            using (var db = new QLTVEntities())
            {
                var listPhieu = db.PhieuMuons
                    .Where(p => p.ChiTietPhieuMuons.Any(c => (c.TrangThai ?? false) == false))
                    .OrderByDescending(p => p.NgayMuon)
                    .ToList();

                foreach (var p in listPhieu)
                {
                    DanhSachPhieuDangMuon.Add(new PhieuMuonListItem
                    {
                        PhieuMuon = p,
                        TenDocGia = p.DocGia?.TenDocGia,
                        SoLuongSach = p.ChiTietPhieuMuons.Count(c => (c.TrangThai ?? false) == false)
                    });
                }
            }
        }

        private void LoadChiTietPhieu()
        {
            ChiTietPhieuDangChon.Clear();

            if (SelectedPhieu == null) return;

            using (var db = new QLTVEntities())
            {
                var chiTiets = db.ChiTietPhieuMuons
                    .Include(c => c.Sach)
                    .Include(c => c.PhieuMuon)
                    .Where(c => c.MaPhieuMuon == SelectedPhieu.PhieuMuon.MaPhieuMuon && (c.TrangThai ?? false) == false)
                    .ToList();

                foreach (var c in chiTiets)
                {
                    ChiTietPhieuDangChon.Add(new ChiTietMuonItem
                    {
                        ChiTiet = c,
                        TenSach = c.Sach?.TenSach
                    });
                }
            }
        }

        private void LoadLichSu()
        {
            DanhSachLichSu.Clear();
            ChiTietLichSu.Clear();
            SelectedLichSuPhieu = null;

            using (var db = new QLTVEntities())
            {
                var query = db.PhieuMuons
                    .Include(p => p.DocGia)
                    .Include(p => p.NhanVien)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(LichSuKeyword))
                {
                    query = query.Where(p =>
                        p.MaPhieuMuon.ToString().Contains(LichSuKeyword) ||
                        p.DocGia.TenDocGia.Contains(LichSuKeyword));
                }

                if (LichSuTuNgay.HasValue)
                    query = query.Where(p => p.NgayMuon >= LichSuTuNgay);

                if (LichSuDenNgay.HasValue)
                    query = query.Where(p => p.NgayMuon <= LichSuDenNgay);

                foreach (var p in query.OrderByDescending(p => p.NgayMuon).ToList())
                {
                    var tongSach = p.ChiTietPhieuMuons.Count;
                    var soDaTra = p.ChiTietPhieuMuons.Count(c => (c.TrangThai ?? false));

                    DanhSachLichSu.Add(new LichSuPhieuItem
                    {
                        PhieuMuon = p,
                        TenDocGia = p.DocGia?.TenDocGia,
                        TenNhanVien = p.NhanVien?.TenNhanVien,
                        TongSach = tongSach,
                        SoDaTra = soDaTra
                    });
                }
            }
        }

        private void LoadChiTietLichSu()
        {
            ChiTietLichSu.Clear();

            if (SelectedLichSuPhieu == null) return;

            using (var db = new QLTVEntities())
            {
                var chiTiets = db.ChiTietPhieuMuons
                    .Include(c => c.Sach)
                    .Include(c => c.PhieuMuon)
                    .Where(c => c.MaPhieuMuon == SelectedLichSuPhieu.PhieuMuon.MaPhieuMuon)
                    .ToList();

                foreach (var c in chiTiets)
                {
                    ChiTietLichSu.Add(new ChiTietMuonItem
                    {
                        ChiTiet = c,
                        TenSach = c.Sach?.TenSach
                    });
                }
            }
        }

        private void ChonSach()
        {
            var danhSachChon = DanhSachSachKhaDung.Where(s => s.IsChecked).ToList();
            foreach (var item in danhSachChon)
            {
                SachDaChon.Add(item);
                DanhSachSachKhaDung.Remove(item);
            }
        }

        private void BoSach()
        {
            var danhSachBo = SachDaChon.ToList();
            foreach (var item in danhSachBo)
            {
                item.IsChecked = false;
                DanhSachSachKhaDung.Add(item);
                SachDaChon.Remove(item);
            }
        }

        private void LamPhieuMuon()
        {
            ThongBao = string.Empty;

            if (SelectedDocGia == null)
            {
                ThongBao = "Vui lòng chọn độc giả.";
                return;
            }

            if (SachDaChon.Count == 0)
            {
                ThongBao = "Vui lòng chọn ít nhất 1 cuốn sách.";
                return;
            }

            using (var db = new QLTVEntities())
            {
                var docGia = db.DocGias.Find(SelectedDocGia.MaDocGia);

                if (!(docGia.TrangThai ?? true))
                {
                    ThongBao = "Thẻ độc giả đang bị khóa.";
                    return;
                }

                if (docGia.NgayHetHan < DateTime.Today)
                {
                    ThongBao = "Thẻ độc giả đã hết hạn. Vui lòng gia hạn thẻ.";
                    return;
                }

                var maxBooks = 3;
                var quyDinh = db.QuyDinhs.FirstOrDefault(q => q.TenQuyDinh.Contains("mượn tối đa"));
                if (quyDinh != null)
                    int.TryParse(quyDinh.GiaTri, out maxBooks);

                var soSachDangMuon = db.ChiTietPhieuMuons.Count(
                    c => (c.TrangThai ?? false) == false && c.PhieuMuon.MaDocGia == docGia.MaDocGia);

                if (soSachDangMuon + SachDaChon.Count > maxBooks)
                {
                    ThongBao = $"Độc giả này đang mượn {soSachDangMuon}/{maxBooks} cuốn. Vượt giới hạn!";
                    return;
                }

                foreach (var item in SachDaChon)
                {
                    var sach = db.Saches.Find(item.Sach.MaSach);
                    if (sach == null || sach.SoLuongCon <= 0)
                    {
                        ThongBao = $"Sách \"{item.Sach.TenSach}\" đã hết, không thể cho mượn.";
                        return;
                    }
                }

                var phieu = new PhieuMuon
                {
                    MaDocGia = docGia.MaDocGia,
                    MaNhanVien = _nhanVien.MaNhanVien,
                    NgayMuon = NgayMuon ?? DateTime.Today,
                    NgayHenTra = NgayHenTra,
                    GhiChu = null
                };
                db.PhieuMuons.Add(phieu);

                foreach (var item in SachDaChon)
                {
                    var sach = db.Saches.Find(item.Sach.MaSach);
                    sach.SoLuongCon = (sach.SoLuongCon ?? 0) - 1;

                    db.ChiTietPhieuMuons.Add(new ChiTietPhieuMuon
                    {
                        MaSach = sach.MaSach,
                        NgayTra = null,
                        TrangThai = false
                    });
                }

                db.SaveChanges();
            }

            SachDaChon.Clear();
            ThongBao = "Đã lập phiếu mượn thành công.";
            LoadSachKhaDung();
            LoadPhieuDangMuon();
            LoadLichSu();
        }

        private void TraSach(object parameter)
        {
            var item = parameter as ChiTietMuonItem;
            if (item == null) return;

            if ((item.ChiTiet.TrangThai ?? false))
            {
                ThongBaoTra = "Sách này đã được trả.";
                return;
            }

            var soNgayQuaHan = 0;
            decimal tienPhatMotNgay = 0;

            using (var db = new QLTVEntities())
            {
                var ct = db.ChiTietPhieuMuons.Find(item.ChiTiet.MaPhieuMuon, item.ChiTiet.MaSach);
                var phieu = db.PhieuMuons.Find(ct.MaPhieuMuon);

                if (phieu.NgayHenTra.HasValue)
                    soNgayQuaHan = (DateTime.Today - phieu.NgayHenTra.Value).Days;

                if (soNgayQuaHan > 0)
                {
                    var quyDinh = db.QuyDinhs.FirstOrDefault(q => q.TenQuyDinh.Contains("phạt quá hạn"));
                    if (quyDinh != null)
                        decimal.TryParse(quyDinh.GiaTri, out tienPhatMotNgay);
                }
            }

            if (soNgayQuaHan > 0)
            {
                var soTien = soNgayQuaHan * tienPhatMotNgay;
                var result = MessageBox.Show(
                    $"Sách được trả trễ {soNgayQuaHan} ngày.\nTiền phạt: {soTien:N0} VNĐ.\n\nTạo phiếu phạt?",
                    "Trả sách quá hạn", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            using (var db = new QLTVEntities())
            {
                var ct = db.ChiTietPhieuMuons.Find(item.ChiTiet.MaPhieuMuon, item.ChiTiet.MaSach);
                var phieu = db.PhieuMuons.Find(ct.MaPhieuMuon);
                var sach = db.Saches.Find(ct.MaSach);

                ct.NgayTra = DateTime.Today;
                ct.TrangThai = true;
                sach.SoLuongCon = (sach.SoLuongCon ?? 0) + 1;

                if (soNgayQuaHan > 0)
                {
                    db.PhieuPhats.Add(new PhieuPhat
                    {
                        MaPhieuMuon = phieu.MaPhieuMuon,
                        MaDocGia = phieu.MaDocGia,
                        NgayPhat = DateTime.Today,
                        LyDo = $"Trả trễ {soNgayQuaHan} ngày",
                        SoTien = soNgayQuaHan * tienPhatMotNgay,
                        TrangThai = false
                    });
                }

                db.SaveChanges();
            }

            ThongBaoTra = "Đã ghi nhận trả sách.";
            LoadSachKhaDung();
            LoadPhieuDangMuon();
            LoadLichSu();
            LoadDocGiaQuaHan();
        }

        private void TraTatCa()
        {
            foreach (var item in ChiTietPhieuDangChon.ToList())
                TraSach(item);
        }

        private void LoadDocGiaQuaHan()
        {
            DanhSachDocGiaQuaHan.Clear();
            DanhSachSachQuaHan.Clear();
            SelectedDocGiaQuaHan = null;

            using (var db = new QLTVEntities())
            {
                var groups = db.ChiTietPhieuMuons
                    .Include(c => c.PhieuMuon.DocGia)
                    .Where(c => (c.TrangThai ?? false) == false && c.PhieuMuon.NgayHenTra < DateTime.Today)
                    .ToList()
                    .GroupBy(c => c.PhieuMuon.DocGia);

                foreach (var g in groups.Where(g => g.Key != null).OrderBy(g => g.Key.TenDocGia))
                {
                    DanhSachDocGiaQuaHan.Add(new DocGiaQuaHanItem
                    {
                        DocGia = g.Key,
                        SoLuongSachQuaHan = g.Count(),
                        SoNgayQuaHanToiDa = g.Max(c => (DateTime.Today - c.PhieuMuon.NgayHenTra.Value).Days)
                    });
                }
            }
        }

        private void LoadSachQuaHan()
        {
            DanhSachSachQuaHan.Clear();

            if (SelectedDocGiaQuaHan == null) return;

            using (var db = new QLTVEntities())
            {
                var items = db.ChiTietPhieuMuons
                    .Include(c => c.Sach)
                    .Include(c => c.PhieuMuon)
                    .Where(c => (c.TrangThai ?? false) == false
                        && c.PhieuMuon.MaDocGia == SelectedDocGiaQuaHan.DocGia.MaDocGia
                        && c.PhieuMuon.NgayHenTra < DateTime.Today)
                    .ToList()
                    .Select(c => new SachQuaHanItem
                    {
                        ChiTiet = c,
                        TenSach = c.Sach?.TenSach,
                        SoNgayQuaHan = (DateTime.Today - c.PhieuMuon.NgayHenTra.Value).Days
                    })
                    .OrderByDescending(c => c.SoNgayQuaHan)
                    .ToList();

                foreach (var item in items)
                    DanhSachSachQuaHan.Add(item);
            }
        }

        private void InGiayNhacTra()
        {
            if (SelectedDocGiaQuaHan == null) return;

            var docGia = SelectedDocGiaQuaHan.DocGia;
            var items = DanhSachSachQuaHan.ToList();
            decimal phatMotNgay = 0;

            using (var db = new QLTVEntities())
            {
                var quyDinh = db.QuyDinhs.FirstOrDefault(q => q.TenQuyDinh.Contains("phạt quá hạn"));
                if (quyDinh != null)
                    decimal.TryParse(quyDinh.GiaTri, out phatMotNgay);
            }

            var flowDoc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                PagePadding = new Thickness(40),
                ColumnWidth = double.MaxValue
            };

            flowDoc.Blocks.Add(new Paragraph(new Run("GIẤY NHẮC TRẢ SÁCH"))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            });

            var info = new Paragraph { Margin = new Thickness(0, 0, 0, 12) };
            info.Inlines.Add(new Run("Kính gửi: ") { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new Run(docGia.TenDocGia));
            info.Inlines.Add(new LineBreak());
            info.Inlines.Add(new Run("Số điện thoại: ") { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new Run(docGia.Sdt));
            flowDoc.Blocks.Add(info);

            var noiDung = new Paragraph
            {
                Margin = new Thickness(0, 0, 0, 12),
                TextAlignment = TextAlignment.Justify
            };
            noiDung.Inlines.Add(new Run(
                $"Thư viện xin thông báo: hiện bạn đang giữ {items.Count} cuốn sách quá hạn. " +
                $"Đề nghị bạn mang sách đến trả trước ngày {DateTime.Today.AddDays(7):dd/MM/yyyy} để tránh phát sinh phí phạt. " +
                (phatMotNgay > 0 ? $"Mức phạt hiện hành: {phatMotNgay:N0} VNĐ/ngày/sách." : "")));
            flowDoc.Blocks.Add(noiDung);

            var table = new Table
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                CellSpacing = 0,
                Margin = new Thickness(0, 0, 0, 20)
            };
            table.Columns.Add(new TableColumn { Width = new GridLength(0.6, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(3, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            var rows = new TableRowGroup();

            var headerRow = new TableRow();
            AddCell(headerRow, "STT", bold: true);
            AddCell(headerRow, "Tên sách", bold: true);
            AddCell(headerRow, "Ngày mượn", bold: true);
            AddCell(headerRow, "Hạn trả", bold: true);
            AddCell(headerRow, "Quá hạn (ngày)", bold: true);
            rows.Rows.Add(headerRow);

            var stt = 1;
            foreach (var c in items)
            {
                var row = new TableRow();
                AddCell(row, stt.ToString());
                AddCell(row, c.TenSach);
                AddCell(row, c.ChiTiet.PhieuMuon.NgayMuon?.ToString("dd/MM/yyyy"));
                AddCell(row, c.ChiTiet.PhieuMuon.NgayHenTra?.ToString("dd/MM/yyyy"));
                AddCell(row, c.SoNgayQuaHan.ToString());
                rows.Rows.Add(row);
                stt++;
            }

            table.RowGroups.Add(rows);
            flowDoc.Blocks.Add(table);

            flowDoc.Blocks.Add(new Paragraph(new Run("Trân trọng.")) { TextAlignment = TextAlignment.Right });
            flowDoc.Blocks.Add(new Paragraph(new Run("Cán bộ thư viện"))
            {
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 30, 0, 0)
            });

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var paginator = ((IDocumentPaginatorSource)flowDoc).DocumentPaginator;
                printDialog.PrintDocument(paginator, $"GiayNhacTra_{docGia.MaDocGia}");
            }
        }

        private void InPhieuTra()
        {
            if (SelectedLichSuPhieu == null) return;

            var phieu = SelectedLichSuPhieu.PhieuMuon;
            var chiTiets = ChiTietLichSu.ToList();

            if (SelectedLichSuPhieu.SoDaTra < SelectedLichSuPhieu.TongSach)
            {
                MessageBox.Show("Phiếu này chưa trả hết sách, không thể in phiếu trả.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            decimal tongPhat = 0;
            using (var db = new QLTVEntities())
            {
                tongPhat = db.PhieuPhats
                    .Where(p => p.MaPhieuMuon == phieu.MaPhieuMuon)
                    .Sum(p => p.SoTien ?? 0);
            }

            var ngayTra = chiTiets.Max(c => c.ChiTiet.NgayTra);

            var flowDoc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                PagePadding = new Thickness(40),
                ColumnWidth = double.MaxValue
            };

            flowDoc.Blocks.Add(new Paragraph(new Run("PHIẾU TRẢ SÁCH"))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            });

            var info = new Paragraph { Margin = new Thickness(0, 0, 0, 12) };
            info.Inlines.Add(new Run("Mã phiếu: ") { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new Run(phieu.MaPhieuMuon.ToString()));
            info.Inlines.Add(new LineBreak());
            info.Inlines.Add(new Run("Độc giả: ") { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new Run(SelectedLichSuPhieu.TenDocGia));
            info.Inlines.Add(new LineBreak());
            info.Inlines.Add(new Run("Ngày mượn: ") { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new Run(phieu.NgayMuon?.ToString("dd/MM/yyyy")));
            info.Inlines.Add(new Run("      Hạn trả: ") { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new Run(phieu.NgayHenTra?.ToString("dd/MM/yyyy")));
            info.Inlines.Add(new LineBreak());
            info.Inlines.Add(new Run("Ngày trả: ") { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new Run(ngayTra?.ToString("dd/MM/yyyy")));
            if (tongPhat > 0)
            {
                info.Inlines.Add(new LineBreak());
                info.Inlines.Add(new Run("Tổng tiền phạt: ") { FontWeight = FontWeights.Bold });
                info.Inlines.Add(new Run($"{tongPhat:N0} VNĐ") { Foreground = Brushes.Red });
            }
            flowDoc.Blocks.Add(info);

            var table = new Table
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                CellSpacing = 0,
                Margin = new Thickness(0, 0, 0, 20)
            };
            table.Columns.Add(new TableColumn { Width = new GridLength(0.6, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(3, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            var rows = new TableRowGroup();

            var headerRow = new TableRow();
            AddCell(headerRow, "STT", bold: true);
            AddCell(headerRow, "Tên sách", bold: true);
            AddCell(headerRow, "Hạn trả", bold: true);
            AddCell(headerRow, "Ngày trả", bold: true);
            AddCell(headerRow, "Quá hạn (ngày)", bold: true);
            rows.Rows.Add(headerRow);

            var stt = 1;
            foreach (var c in chiTiets)
            {
                var soNgayQuaHan = c.ChiTiet.NgayTra.HasValue && phieu.NgayHenTra.HasValue
                    ? Math.Max(0, (c.ChiTiet.NgayTra.Value - phieu.NgayHenTra.Value).Days)
                    : 0;

                var row = new TableRow();
                AddCell(row, stt.ToString());
                AddCell(row, c.TenSach);
                AddCell(row, phieu.NgayHenTra?.ToString("dd/MM/yyyy"));
                AddCell(row, c.ChiTiet.NgayTra?.ToString("dd/MM/yyyy"));
                AddCell(row, soNgayQuaHan > 0 ? soNgayQuaHan.ToString() : "-");
                rows.Rows.Add(row);
                stt++;
            }

            table.RowGroups.Add(rows);
            flowDoc.Blocks.Add(table);

            flowDoc.Blocks.Add(new Paragraph(new Run("Nhân viên nhận sách"))
            {
                TextAlignment = TextAlignment.Left,
                Margin = new Thickness(0, 30, 0, 0)
            });

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var paginator = ((IDocumentPaginatorSource)flowDoc).DocumentPaginator;
                printDialog.PrintDocument(paginator, $"PhieuTra_{phieu.MaPhieuMuon}");
            }
        }

        private void InPhieuMuon()
        {
            if (SelectedLichSuPhieu == null) return;

            var phieu = SelectedLichSuPhieu.PhieuMuon;
            var chiTiets = ChiTietLichSu.ToList();

            var flowDoc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                PagePadding = new Thickness(40),
                ColumnWidth = double.MaxValue
            };

            flowDoc.Blocks.Add(new Paragraph(new Run("PHIẾU MƯỢN SÁCH"))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            });

            var info = new Paragraph
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            info.Inlines.Add(new Run("Mã phiếu: ") { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new Run(phieu.MaPhieuMuon.ToString()));
            info.Inlines.Add(new LineBreak());
            info.Inlines.Add(new Run("Độc giả: ") { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new Run(SelectedLichSuPhieu.TenDocGia));
            info.Inlines.Add(new LineBreak());
            info.Inlines.Add(new Run("Nhân viên lập: ") { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new Run(SelectedLichSuPhieu.TenNhanVien));
            info.Inlines.Add(new LineBreak());
            info.Inlines.Add(new Run("Ngày mượn: ") { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new Run(phieu.NgayMuon?.ToString("dd/MM/yyyy")));
            info.Inlines.Add(new Run("      Hạn trả: ") { FontWeight = FontWeights.Bold });
            info.Inlines.Add(new Run(phieu.NgayHenTra?.ToString("dd/MM/yyyy")));
            flowDoc.Blocks.Add(info);

            var table = new Table
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                CellSpacing = 0,
                Margin = new Thickness(0, 0, 0, 20)
            };
            table.Columns.Add(new TableColumn { Width = new GridLength(0.6, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(3.4, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            var headerRow = new TableRow();
            AddCell(headerRow, "STT", bold: true);
            AddCell(headerRow, "Tên sách", bold: true);
            AddCell(headerRow, "Hạn trả", bold: true);
            AddCell(headerRow, "Trạng thái", bold: true);

            var rows = new TableRowGroup();
            rows.Rows.Add(headerRow);

            var stt = 1;
            foreach (var c in chiTiets)
            {
                var row = new TableRow();
                AddCell(row, stt.ToString());
                AddCell(row, c.TenSach);
                AddCell(row, c.ChiTiet.PhieuMuon.NgayHenTra?.ToString("dd/MM/yyyy"));
                AddCell(row, c.TrangThaiText);
                rows.Rows.Add(row);
                stt++;
            }

            table.RowGroups.Add(rows);
            flowDoc.Blocks.Add(table);

            flowDoc.Blocks.Add(new Paragraph(new Run("Xác nhận của nhân viên thư viện"))
            {
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 30, 0, 0)
            });

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var paginator = ((IDocumentPaginatorSource)flowDoc).DocumentPaginator;
                printDialog.PrintDocument(paginator, $"PhieuMuon_{phieu.MaPhieuMuon}");
            }
        }

        private static void AddCell(TableRow row, string text, bool bold = false)
        {
            var cell = new TableCell
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(6, 4, 6, 4)
            };
            cell.Blocks.Add(new Paragraph(new Run(text))
            {
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal
            });
            row.Cells.Add(cell);
        }
    }
}

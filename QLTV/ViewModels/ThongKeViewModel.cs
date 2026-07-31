using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using OfficeOpenXml;
using QLTV.Models;

namespace QLTV.ViewModels
{
    public class SachThongKeItem
    {
        public string TenSach { get; set; }
        public int SoLuotMuon { get; set; }
    }

    public class DocGiaThongKeItem
    {
        public string TenDocGia { get; set; }
        public int SoLuotMuon { get; set; }
    }

    public class SachSapHetItem
    {
        public string TenSach { get; set; }
        public int SoLuongCon { get; set; }
    }

    public class QuaHanItem
    {
        public string TenSach { get; set; }
        public string TenDocGia { get; set; }
        public DateTime NgayHenTra { get; set; }
        public int SoNgayQuaHan { get; set; }
    }

    public class ThongKeViewModel : BaseViewModel
    {
        public ObservableCollection<SachThongKeItem> SachMuonNhieu { get; } = new ObservableCollection<SachThongKeItem>();
        public ObservableCollection<DocGiaThongKeItem> DocGiaMuonNhieu { get; } = new ObservableCollection<DocGiaThongKeItem>();
        public ObservableCollection<SachSapHetItem> SachSapHet { get; } = new ObservableCollection<SachSapHetItem>();
        public ObservableCollection<QuaHanItem> SachQuaHan { get; } = new ObservableCollection<QuaHanItem>();

        public ICommand ExportExcelCommand { get; }

        public string Title => "Thống kê";

        public ThongKeViewModel()
        {
            ExportExcelCommand = new RelayCommand(_ => ExportExcel());
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new QLTVEntities())
            {
                foreach (var g in db.ChiTietPhieuMuons
                    .GroupBy(c => c.MaSach)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .ToList())
                {
                    SachMuonNhieu.Add(new SachThongKeItem
                    {
                        TenSach = g.First().Sach?.TenSach,
                        SoLuotMuon = g.Count()
                    });
                }

                foreach (var g in db.ChiTietPhieuMuons
                    .GroupBy(c => c.PhieuMuon.MaDocGia)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .ToList())
                {
                    DocGiaMuonNhieu.Add(new DocGiaThongKeItem
                    {
                        TenDocGia = g.First().PhieuMuon.DocGia?.TenDocGia,
                        SoLuotMuon = g.Count()
                    });
                }

                foreach (var s in db.Saches
                    .Where(s => s.SoLuongCon <= 2)
                    .OrderBy(s => s.SoLuongCon)
                    .ToList())
                {
                    SachSapHet.Add(new SachSapHetItem
                    {
                        TenSach = s.TenSach,
                        SoLuongCon = s.SoLuongCon ?? 0
                    });
                }

                foreach (var c in db.ChiTietPhieuMuons
                    .Where(c => (c.TrangThai ?? false) == false && c.PhieuMuon.NgayHenTra < DateTime.Today)
                    .ToList())
                {
                    SachQuaHan.Add(new QuaHanItem
                    {
                        TenSach = c.Sach?.TenSach,
                        TenDocGia = c.PhieuMuon.DocGia?.TenDocGia,
                        NgayHenTra = c.PhieuMuon.NgayHenTra ?? DateTime.Today,
                        SoNgayQuaHan = (DateTime.Today - (c.PhieuMuon.NgayHenTra ?? DateTime.Today)).Days
                    });
                }
            }
        }

        private void ExportExcel()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"ThongKe_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                using (var package = new ExcelPackage())
                {
                    WriteSheet(package, "SachMuonNhieu", new[] { "Tên sách", "Lượt mượn" },
                        SachMuonNhieu.Select(s => new object[] { s.TenSach, s.SoLuotMuon }));
                    WriteSheet(package, "DocGiaMuonNhieu", new[] { "Độc giả", "Lượt mượn" },
                        DocGiaMuonNhieu.Select(d => new object[] { d.TenDocGia, d.SoLuotMuon }));
                    WriteSheet(package, "SachSapHet", new[] { "Tên sách", "Còn lại" },
                        SachSapHet.Select(s => new object[] { s.TenSach, s.SoLuongCon }));
                    WriteSheet(package, "SachQuaHan", new[] { "Sách", "Độc giả", "Hạn trả", "Quá hạn (ngày)" },
                        SachQuaHan.Select(q => new object[] { q.TenSach, q.TenDocGia, q.NgayHenTra.ToString("dd/MM/yyyy"), q.SoNgayQuaHan }));

                    package.SaveAs(new FileInfo(dialog.FileName));
                }

                MessageBox.Show($"Đã xuất file Excel:\n{dialog.FileName}",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể xuất Excel: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void WriteSheet(ExcelPackage package, string sheetName, string[] headers, IEnumerable<object[]> rows)
        {
            var sheet = package.Workbook.Worksheets.Add(sheetName);

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                sheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            var row = 2;
            foreach (var values in rows)
            {
                for (int i = 0; i < values.Length; i++)
                    sheet.Cells[row, i + 1].Value = values[i];
                row++;
            }

            sheet.Cells[1, 1, row - 1, headers.Length].AutoFitColumns();
        }
    }
}

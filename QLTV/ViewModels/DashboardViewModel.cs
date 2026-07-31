using System;
using System.Linq;
using QLTV.Models;

namespace QLTV.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public string NhanVienName { get; }
        public int TongSach { get; private set; }
        public int TongDocGia { get; private set; }
        public int SachDangMuon { get; private set; }
        public int SachQuaHan { get; private set; }

        public DashboardViewModel(NhanVien nhanVien)
        {
            NhanVienName = nhanVien.TenNhanVien;
            LoadStats();
        }

        private void LoadStats()
        {
            using (var db = new QLTVEntities())
            {
                TongSach = db.Saches.Sum(s => s.SoLuong ?? 0);
                TongDocGia = db.DocGias.Count();
                SachDangMuon = db.ChiTietPhieuMuons.Count(c => c.TrangThai == false);
                SachQuaHan = db.ChiTietPhieuMuons.Count(
                    c => c.TrangThai == false && c.PhieuMuon.NgayHenTra < DateTime.Today);
            }
        }
    }
}

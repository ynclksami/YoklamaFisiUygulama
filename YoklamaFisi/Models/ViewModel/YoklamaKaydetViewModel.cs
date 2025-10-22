namespace YoklamaFisi.Models.ViewModel
{
    public class YoklamaKaydetViewModel
    {
        public int DersSaati { get; set; }
        public int SinifId { get; set; } // Sınıf bilgisini de taşıyacağız
        public DateTime? Tarih { get; set; }

        // Index View'dan post edilen öğrenci/durum listesi
        public List<OgrenciYoklamaDurumu> Ogrenciler { get; set; } = new List<OgrenciYoklamaDurumu>();
    }
}

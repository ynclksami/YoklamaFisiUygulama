namespace YoklamaFisi.Models.Entities
{
    public class YoklamaKaydi
    {
        public int Id { get; set; }

        public int OgrenciId { get; set; }
        public Ogrenci Ogrenci { get; set; } = null!;

        public int DersSaati { get; set; } // 1..10

        public bool GeldiMi { get; set; } // true = geldi, false = gelmedi

        public DateTime Tarih { get; set; } = DateTime.Today;
    }
}

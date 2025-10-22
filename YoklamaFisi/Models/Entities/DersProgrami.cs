using System.Drawing;

namespace YoklamaFisi.Models.Entities
{
    public class DersProgrami
    {
        public int Id { get; set; }

        public int SinifId { get; set; }
        public Sinif Sinif { get; set; } = null!;

        public int Gun { get; set; }
        public int DersSaati { get; set; }

        // Yeni ilişki:
        public int DersId { get; set; }
        public Ders Ders { get; set; } = null!;

        public int? Ogretmen1Id { get; set; }
        public int? Ogretmen2Id { get; set; }
        public int? Ogretmen3Id { get; set; }

        public Ogretmen? Ogretmen1 { get; set; }
        public Ogretmen? Ogretmen2 { get; set; }
        public Ogretmen? Ogretmen3 { get; set; }
    }
}

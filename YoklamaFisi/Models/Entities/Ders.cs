using System.ComponentModel.DataAnnotations.Schema;

namespace YoklamaFisi.Models.Entities
{
    public class Ders
    {
        public int DersId { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string? KisaAd { get; set; }
        public ICollection<DersProgrami>? DersProgramlari { get; set; }
        public int SinifId { get; set; }
        public Sinif Sinif { get; set; } = null!;
    }
}

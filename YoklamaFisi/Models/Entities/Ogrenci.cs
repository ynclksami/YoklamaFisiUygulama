using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace YoklamaFisi.Models.Entities
{
    public class Ogrenci
    {
        public int Id { get; set; }

        [Required]
        public int Numara { get; set; }

        [Required]
        public string AdSoyad { get; set; } = string.Empty;

        // Yeni: hangi sınıfa ait
        public int SinifId { get; set; }
        public Sinif Sinif { get; set; } = null!;
    }
}

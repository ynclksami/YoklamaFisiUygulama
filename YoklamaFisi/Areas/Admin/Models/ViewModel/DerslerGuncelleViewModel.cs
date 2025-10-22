using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace YoklamaFisi.Areas.Admin.Models.ViewModel
{
    public class DerslerGuncelleViewModel
    {
        public int DersId { get; set; }

        [Required(ErrorMessage = "Lütfen Ders Adını Girin.")]
        [Display(Name = "Ders Adı")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen Kısa Ders Adını Girin.")]
        [Display(Name = "Kısa Ders Adı")]
        public string? KisaAd { get; set; }

        [Required(ErrorMessage = "Lütfen bir sınıf seçin.")]
        [Display(Name = "Sınıfı")]
        public int SinifId { get; set; }
        public IEnumerable<SelectListItem> SinifListesi { get; set; } = new List<SelectListItem>();
    }
}

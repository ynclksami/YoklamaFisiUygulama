using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace YoklamaFisi.Areas.Admin.Models.ViewModel
{
    public class OgrenciEkleViewModel
    {
        // 1. Formdan alınacak öğrenci bilgileri
        [Required(ErrorMessage = "Lütfen Numarayı Girin.")]
        [Display(Name = "Öğrenci Numarası")]
        public int Numara { get; set; }

        [Required(ErrorMessage = "Lütfen Adı Soyadı Girin.")]
        [Display(Name = "Öğrenci Adı Soyadı")]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen bir sınıf seçin.")]
        [Display(Name = "Sınıfı")]
        public int SinifId { get; set; } // Sadece ID'yi alacağız

        // 2. Dropdown'ı doldurmak için Sınıf Listesi
        // IEnumerable<SelectListItem> tipi, HTML <select> elemanı için idealdir.
        public IEnumerable<SelectListItem> SinifListesi { get; set; } = new List<SelectListItem>();
    }
}
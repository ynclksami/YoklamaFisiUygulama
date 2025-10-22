using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace YoklamaFisi.Areas.Admin.Models.ViewModel
{
    public class DersProgramiEkleViewModel
    {
        public int Id { get; set; }
        public int SinifId { get; set; }
        public int Gun { get; set; }
        public int DersSaati { get; set; }
        public int DersId { get; set; }

        public int? Ogretmen1Id { get; set; }
        public int? Ogretmen2Id { get; set; }
        public int? Ogretmen3Id { get; set; }

        // DropDownList’ler için koleksiyonlar
        public IEnumerable<SelectListItem>? SinifListesi { get; set; }
        public IEnumerable<SelectListItem>? GunListesi { get; set; }
        public IEnumerable<SelectListItem>? OgretmenListesi { get; set; }
        public IEnumerable<SelectListItem>? DersListesi { get; set; }
    }
}

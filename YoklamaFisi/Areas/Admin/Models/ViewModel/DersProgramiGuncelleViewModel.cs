using Microsoft.AspNetCore.Mvc.Rendering;

namespace YoklamaFisi.Areas.Admin.Models.ViewModel
{
    public class DersProgramiGuncelleViewModel
    {
        public int Id { get; set; }

        // Ders Programı alanları
        public int SinifId { get; set; }
        public int Gun { get; set; }
        public int DersSaati { get; set; }
        public int DersId { get; set; }

        public int? Ogretmen1Id { get; set; }
        public int? Ogretmen2Id { get; set; }
        public int? Ogretmen3Id { get; set; }

        // Dropdown verileri
        public List<SelectListItem>? Siniflar { get; set; }
        public List<SelectListItem>? Ogretmenler { get; set; }
        public List<SelectListItem>? Gunler { get; set; }
        public List<SelectListItem>? Dersler { get; set; }
    }
}

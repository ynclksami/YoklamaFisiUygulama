using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using YoklamaFisi.Areas.Admin.Models.ViewModel;
using YoklamaFisi.Data;
using YoklamaFisi.Models.Entities;

namespace YoklamaFisi.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class DersProgramlariController : Controller
    {
        private readonly AppDbContext _context;
        public DersProgramlariController(AppDbContext context)
        {
            _context = context;
        }
        [Route("Index")]
        public IActionResult Index(int sayfa = 1)
        {
            var model = _context.DersProgramlari
         .Include(dp => dp.Sinif)
         .Include(dp => dp.Ogretmen1)
         .Include(dp => dp.Ogretmen2)
         .Include(dp => dp.Ogretmen3)
         .Include(dp => dp.Ders)
         .OrderBy(dp => dp.Sinif.SinifAd)
         .ThenBy(dp => dp.Gun)
         .ThenBy(dp => dp.DersSaati)
         .ThenBy(dp => dp.Ders.Ad)
         .ToList().ToPagedList(sayfa, 10); 

            return View(model);
        }
        [HttpGet]
        [Route("DersProgramlariEkle")]
        public IActionResult DersProgramlariEkle()
        {
            var viewModel = new DersProgramiEkleViewModel
            {
                SinifListesi = _context.Siniflar
            .OrderBy(s => s.SinifAd)
            .Select(s => new SelectListItem
            {
                Value = s.SinifId.ToString(),
                Text = s.SinifAd
            }),

                GunListesi = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Pazartesi" },
            new SelectListItem { Value = "2", Text = "Salı" },
            new SelectListItem { Value = "3", Text = "Çarşamba" },
            new SelectListItem { Value = "4", Text = "Perşembe" },
            new SelectListItem { Value = "5", Text = "Cuma" }
        },

                OgretmenListesi = _context.Ogretmenler
            .OrderBy(o => o.AdSoyad)
            .Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = o.AdSoyad
            }),

                // Ders listesini boş başlatıyoruz. AJAX ile doldurulacak.
                DersListesi = new List<SelectListItem>()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("DersProgramlariEkle")]
        public IActionResult DersProgramlariEkle(DersProgramiEkleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Dropdownları tekrar doldur
                viewModel.SinifListesi = _context.Siniflar
                    // ... (kodunuzun geri kalanı) ...
                    .Select(s => new SelectListItem { Value = s.SinifId.ToString(), Text = s.SinifAd });

                viewModel.GunListesi = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Pazartesi" },
            new SelectListItem { Value = "2", Text = "Salı" },
            new SelectListItem { Value = "3", Text = "Çarşamba" },
            new SelectListItem { Value = "4", Text = "Perşembe" },
            new SelectListItem { Value = "5", Text = "Cuma" }
        };

                viewModel.OgretmenListesi = _context.Ogretmenler
                    .OrderBy(o => o.AdSoyad)
                   .Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.AdSoyad });

                // Eğer kullanıcı bir sınıf seçmişse, o sınıfa ait dersleri yükle.
                if (viewModel.SinifId > 0)
                {
                    viewModel.DersListesi = _context.Dersler
                        .Where(d => d.SinifId == viewModel.SinifId) // Sadece seçili sınıfın dersleri
                        .OrderBy(d => d.Ad)
                        .Select(d => new SelectListItem { Value = d.DersId.ToString(), Text = d.Ad });
                }
                else
                {
                    // Sınıf seçmemişse boş liste gönder
                    viewModel.DersListesi = new List<SelectListItem>();
                }

                return View(viewModel);
            }

            var dersProgrami = new DersProgrami
            {
                SinifId = viewModel.SinifId,
                Gun = viewModel.Gun,
                DersSaati = viewModel.DersSaati,
                DersId = viewModel.DersId,
                Ogretmen1Id = viewModel.Ogretmen1Id,
                Ogretmen2Id = viewModel.Ogretmen2Id,
                Ogretmen3Id = viewModel.Ogretmen3Id
            };

            _context.DersProgramlari.Add(dersProgrami);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var kayit = await _context.DersProgramlari.FindAsync(id);
            if (kayit == null)
            {
                TempData["HataMesaji"] = "Kayıt bulunamadı.";
                return RedirectToAction("Index");
            }

            _context.DersProgramlari.Remove(kayit);
            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] = "Kayıt başarıyla silindi.";
            return RedirectToAction("Index");
        }
        [Route("DersProgramlariGuncelle/{id:int}")]
        [HttpGet]
        public IActionResult DersProgramlariGuncelle(int id)
        {
            var dersProg = _context.DersProgramlari
                .Include(x => x.Sinif)
                .Include(x => x.Ders)
                .FirstOrDefault(x => x.Id == id);

            if (dersProg == null)
                return NotFound();

            var model = new DersProgramiGuncelleViewModel
            {
                Id = dersProg.Id,
                SinifId = dersProg.SinifId,
                Gun = dersProg.Gun,
                DersSaati = dersProg.DersSaati,
                DersId = dersProg.DersId,
                Ogretmen1Id = dersProg.Ogretmen1Id,
                Ogretmen2Id = dersProg.Ogretmen2Id,
                Ogretmen3Id = dersProg.Ogretmen3Id,

                Siniflar = _context.Siniflar
                    .Select(s => new SelectListItem
                    {
                        Value = s.SinifId.ToString(),
                        Text = s.SinifAd
                    }).ToList(),

                Ogretmenler = _context.Ogretmenler
                    .Select(o => new SelectListItem
                    {
                        Value = o.Id.ToString(),
                        Text = o.AdSoyad
                    }).ToList(),

                Dersler = _context.Dersler
    .Where(d => d.SinifId == dersProg.SinifId)
    .OrderBy(d => d.Ad)
    .Select(d => new SelectListItem
    {
        Value = d.DersId.ToString(),
        Text = d.Ad,
        Selected = d.DersId == dersProg.DersId
    }).ToList(),



                Gunler = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Pazartesi" },
            new SelectListItem { Value = "2", Text = "Salı" },
            new SelectListItem { Value = "3", Text = "Çarşamba" },
            new SelectListItem { Value = "4", Text = "Perşembe" },
            new SelectListItem { Value = "5", Text = "Cuma" }
        }
            };

            return View(model);
        }
        [Route("DersProgramlariGuncelle/{id:int}")]
        [HttpPost]
        public IActionResult DersProgramlariGuncelle(DersProgramiGuncelleViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entity = _context.DersProgramlari.Find(model.Id);
            if (entity == null)
                return NotFound();

            entity.SinifId = model.SinifId;
            entity.Gun = model.Gun;
            entity.DersSaati = model.DersSaati;
            entity.DersId = model.DersId;
            entity.Ogretmen1Id = model.Ogretmen1Id;
            entity.Ogretmen2Id = model.Ogretmen2Id;
            entity.Ogretmen3Id = model.Ogretmen3Id;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult GetDerslerBySinif(int sinifId)
        {
            var dersler = _context.Dersler
                                  .Where(d => d.SinifId == sinifId)
                                  .OrderBy(d => d.Ad)
                                  .Select(d => new
                                  {
                                      value = d.DersId,
                                      text = d.Ad
                                  })
                                  .ToList();

            return Json(dersler);
        }

    }
}
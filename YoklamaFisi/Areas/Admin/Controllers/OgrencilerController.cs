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
    public class OgrencilerController : Controller
    {
        private readonly AppDbContext _context;
        public OgrencilerController(AppDbContext context)
        {
            _context = context;
        }
        [Route("Index")]
        public IActionResult Index(int sayfa = 1)
        {
          var ogrenciler = _context.Ogrenciler
                                     .Include(o => o.Sinif) // <-- ÖNEMLİ OLAN BU SATIR
                                     .ToList().ToPagedList(sayfa, 10);

            return View(ogrenciler);
        }
     
        [Route("OgrencilerEkle")]
        [HttpGet]
        public ActionResult OgrencilerEkle()
        {
            // 1. Veritabanından tüm sınıfları çek (SinifId ve sinifsube)
            var siniflar = _context.Siniflar
                                   .OrderBy(s => s.SinifAd) // Sınıfları isme göre sırala
                                   .ToList();

            // 2. ViewModel'i oluştur
            var viewModel = new OgrenciEkleViewModel
            {
                // 3. Sınıf listesini SelectListItem'e dönüştür (Dropdown için)
                SinifListesi = siniflar.Select(s => new SelectListItem
                {
                    Text = s.SinifAd,          // Kullanıcının göreceği metin (Örn: "9-A")
                    Value = s.SinifId.ToString() // Arka plana gönderilecek değer (Örn: 12)
                })
            };

            // 4. ViewModel'i View'e gönder
            return View(viewModel);
        }
        [Route("OgrencilerEkle")]
        [HttpPost]
        public ActionResult OgrencilerEkle(OgrenciEkleViewModel viewModel) // Parametreyi ViewModel olarak değiştir
        {
            // 1. Model geçerli mi? (Tüm [Required] alanlar dolu mu?)
            if (ModelState.IsValid)
            {
                // 2. ViewModel'den gelen verilerle yeni bir Ogrenci nesnesi oluştur
                var yeniOgrenci = new Ogrenci
                {
                    Numara = viewModel.Numara,
                    AdSoyad = viewModel.AdSoyad,
                    SinifId = viewModel.SinifId  // Dropdown'dan seçilen sınıfın ID'si
                };

                // 3. Veritabanına ekle
                _context.Ogrenciler.Add(yeniOgrenci);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            var siniflar = _context.Siniflar
                                   .OrderBy(s => s.SinifAd)
                                   .ToList();

            viewModel.SinifListesi = siniflar.Select(s => new SelectListItem
            {
                Text = s.SinifAd,
                Value = s.SinifId.ToString()
            });

            // 4. Kullanıcının girdiği veriler ve dolu sınıf listesiyle View'i geri döndür
            return View(viewModel);
        }

        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var kayit = await _context.Ogrenciler.FindAsync(id);
            if (kayit == null)
            {
                TempData["HataMesaji"] = "Kayıt bulunamadı.";
                return RedirectToAction("Index");
            }

            _context.Ogrenciler.Remove(kayit);
            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] = "Kayıt başarıyla silindi.";
            return RedirectToAction("Index");
        }
        [Route("OgrencilerGuncelle/{id:int}")]
        [HttpGet]
        public ActionResult OgrencilerGuncelle(int id)
        {
            // 1. Güncellenecek öğrenciyi bul
            var ogrenci = _context.Ogrenciler.Find(id);
            if (ogrenci == null)
                return NotFound();

            // 2. Sınıf listesini dropdown için hazırla
            var siniflar = _context.Siniflar
                                   .OrderBy(s => s.SinifAd)
                                   .ToList();

            // 3. ViewModel'e doldur
            var viewModel = new OgrenciGuncelleViewModel
            {
                Id = ogrenci.Id,
                Numara = ogrenci.Numara,
                AdSoyad = ogrenci.AdSoyad,
                SinifId = ogrenci.SinifId,
                SinifListesi = siniflar.Select(s => new SelectListItem
                {
                    Text = s.SinifAd,
                    Value = s.SinifId.ToString(),
                    Selected = (s.SinifId == ogrenci.SinifId) // Mevcut sınıf seçili gelsin
                })
            };

            return View(viewModel);
        }
        [Route("OgrencilerGuncelle/{id:int}")]
        [HttpPost]
        public ActionResult OgrencilerGuncelle(OgrenciGuncelleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var ogrenci = _context.Ogrenciler.Find(viewModel.Id);
                if (ogrenci == null)
                    return NotFound();

                // ViewModel'deki değerleri entity'e aktar
                ogrenci.Numara = viewModel.Numara;
                ogrenci.AdSoyad = viewModel.AdSoyad;
                ogrenci.SinifId = viewModel.SinifId;

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // ModelState geçersizse dropdown boş kalmaması için sınıf listesini yeniden yükle
            var siniflar = _context.Siniflar
                                   .OrderBy(s => s.SinifAd)
                                   .ToList();

            viewModel.SinifListesi = siniflar.Select(s => new SelectListItem
            {
               Text = s.SinifAd,
                Value = s.SinifId.ToString()
            });

            return View(viewModel);
        }
    }
}

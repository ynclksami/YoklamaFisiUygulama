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
    public class DerslerController : Controller
    {
        private readonly AppDbContext _context;
        public DerslerController(AppDbContext context)
        {
            _context = context;
        }
        [Route("Index")]
        public IActionResult Index(int sayfa = 1)
        {
            var dersler = _context.Dersler
                                   .Include(o => o.Sinif) // <-- ÖNEMLİ OLAN BU SATIR
                                   .ToList().ToPagedList(sayfa, 10); 

            return View(dersler);
        }
        [Route("DerslerEkle")]
        [HttpGet]
        public ActionResult DerslerEkle()
        {
            var siniflar = _context.Siniflar
                                   .OrderBy(s => s.SinifAd) // Sınıfları isme göre sırala
                                   .ToList();

            // 2. ViewModel'i oluştur
            var viewModel = new DerslerEkleViewModel
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
        [Route("DerslerEkle")]
        [HttpPost]
        public ActionResult DerslerEkle(DerslerEkleViewModel viewModel) // Parametreyi ViewModel olarak değiştir
        {
            // 1. Model geçerli mi? (Tüm [Required] alanlar dolu mu?)
            if (ModelState.IsValid)
            {
                // 2. ViewModel'den gelen verilerle yeni bir Ogrenci nesnesi oluştur
                var yeniDers = new Ders
                {
                    Ad = viewModel.Ad,
                    KisaAd = viewModel.KisaAd,
                    SinifId = viewModel.SinifId  // Dropdown'dan seçilen sınıfın ID'si
                };

                // 3. Veritabanına ekle
                _context.Dersler.Add(yeniDers);
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
            var kayit = await _context.Dersler.FindAsync(id);
            if (kayit == null)
            {
                TempData["HataMesaji"] = "Kayıt bulunamadı.";
                return RedirectToAction("Index");
            }

            _context.Dersler.Remove(kayit);
            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] = "Kayıt başarıyla silindi.";
            return RedirectToAction("Index");
        }
        [Route("DerslerGuncelle/{id:int}")]
        [HttpGet]
        public ActionResult DerslerGuncelle(int id)
        {
            // 1. Güncellenecek öğrenciyi bul
            var dersler = _context.Dersler.Find(id);
            if (dersler == null)
                return NotFound();

            // 2. Sınıf listesini dropdown için hazırla
            var siniflar = _context.Siniflar
                                   .OrderBy(s => s.SinifAd)
                                   .ToList();

            // 3. ViewModel'e doldur
            var viewModel = new DerslerGuncelleViewModel
            {
                DersId =dersler.DersId,
                Ad = dersler.Ad,
                KisaAd = dersler.KisaAd,
                SinifId = dersler.SinifId,
                SinifListesi = siniflar.Select(s => new SelectListItem
                {
                    Text = s.SinifAd,
                    Value = s.SinifId.ToString(),
                    Selected = (s.SinifId == dersler.SinifId) // Mevcut sınıf seçili gelsin
                })
            };

            return View(viewModel);
        }
        [Route("DerslerGuncelle/{id:int}")]
        [HttpPost]
        public ActionResult DerslerGuncelle(DerslerGuncelleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var dersler = _context.Dersler.Find(viewModel.DersId);
                if (dersler == null)
                    return NotFound();

                // ViewModel'deki değerleri entity'e aktar
                dersler.Ad = viewModel.Ad;
                dersler.KisaAd = viewModel.KisaAd;
                dersler.SinifId = viewModel.SinifId;

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

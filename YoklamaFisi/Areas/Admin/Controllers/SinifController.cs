using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YoklamaFisi.Data;
using YoklamaFisi.Models.Entities;
using X.PagedList.Extensions;

namespace YoklamaFisi.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class SinifController : Controller
    {
        private readonly AppDbContext _context;
        public SinifController(AppDbContext context)
        {
            _context = context;
        }

        [Route("Index")]
        public IActionResult Index(int sayfa = 1)
        {
            var siniflar = _context.Siniflar.ToList().ToPagedList(sayfa, 10);
            return View(siniflar);
        }
        [Route("SinifEkle")]
        [HttpGet]
        public ActionResult SinifEkle()
        {
            return View();
        }
        [Route("SinifEkle")]
        [HttpPost]
        public ActionResult SinifEkle(Sinif sinif)
        {
            if (ModelState.IsValid)
            {
                _context.Siniflar.Add(sinif);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sinif);
        }

        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var kayit = await _context.Siniflar.FindAsync(id);
            if (kayit == null)
            {
                TempData["HataMesaji"] = "Kayıt bulunamadı.";
                return RedirectToAction("Index");
            }

            _context.Siniflar.Remove(kayit);
            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] = "Kayıt başarıyla silindi.";
            return RedirectToAction("Index");
        }

        [Route("SinifGuncelle/{id:int}")]
        [HttpGet]
        public ActionResult SinifGuncelle(int id)
        {
            var siniflar = _context.Siniflar.Find(id);
            return View("SinifGuncelle", siniflar);
        }
        [Route("SinifGuncelle/{id:int}")]
        [HttpPost]
        public ActionResult SinifGuncelle(Sinif sinif)
        {
            if (ModelState.IsValid)
            {
                var values = _context.Siniflar.Find(sinif.SinifId);
                if (values != null)
                {
                    values.SinifAd = sinif.SinifAd;
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(sinif);
        }
    }
}

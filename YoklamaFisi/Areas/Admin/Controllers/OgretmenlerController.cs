using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;
using YoklamaFisi.Data;
using YoklamaFisi.Models.Entities;

namespace YoklamaFisi.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class OgretmenlerController : Controller
    {
        private readonly AppDbContext _context;
        public OgretmenlerController(AppDbContext context)
        {
            _context = context;
        }
        [Route("Index")]
        public IActionResult Index(int sayfa = 1)
        {
            var ogrt = _context.Ogretmenler.ToList().ToPagedList(sayfa, 10); 
            return View(ogrt);
        }
        [Route("OgretmenlerEkle")]
        [HttpGet]
        public ActionResult OgretmenlerEkle()
        {
            return View();
        }
        [Route("OgretmenlerEkle")]
        [HttpPost]
        public ActionResult OgretmenlerEkle(Ogretmen ogretmen)
        {
            if (ModelState.IsValid)
            {
                _context.Ogretmenler.Add(ogretmen);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ogretmen);
        }

        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var kayit = await _context.Ogretmenler.FindAsync(id);
            if (kayit == null)
            {
                TempData["HataMesaji"] = "Kayıt bulunamadı.";
                return RedirectToAction("Index");
            }

            _context.Ogretmenler.Remove(kayit);
            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] = "Kayıt başarıyla silindi.";
            return RedirectToAction("Index");
        }
        [Route("OgretmenlerGuncelle/{id:int}")]
        [HttpGet]
        public ActionResult OgretmenlerGuncelle(int id)
        {
            var ogretmen = _context.Ogretmenler.Find(id);
            return View("OgretmenlerGuncelle", ogretmen);
        }
        [Route("OgretmenlerGuncelle/{id:int}")]
        [HttpPost]
        public ActionResult OgretmenlerGuncelle(Ogretmen ogretmen)
        {
            if (ModelState.IsValid)
            {
                var values = _context.Ogretmenler.Find(ogretmen.Id);
                if (values != null)
                {
                    values.AdSoyad = ogretmen.AdSoyad;
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(ogretmen);
        }
    }
}

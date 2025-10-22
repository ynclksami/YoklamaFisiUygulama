using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YoklamaFisi.Data;

namespace YoklamaFisi.Areas.Admin.Controllers
{
    [Authorize(Roles ="Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context)
        {
            _context = context;
        }
        [Route("Index")]
        public IActionResult Index()
        {
            var sayi1 = _context.Ogrenciler.Count(o => o.Sinif.SinifAd == "9B");
            var sayi2 = _context.Ogrenciler.Count(o => o.Sinif.SinifAd == "10B");
            var sayi3 = _context.Ogrenciler.Count(o => o.Sinif.SinifAd == "11B");
            var sayi4 = _context.Ogrenciler.Count(o => o.Sinif.SinifAd == "12B");
            var sayi5 = _context.Ogretmenler.Count();
            var sayi6 = _context.Ogrenciler.Count();
            var sayi7 = _context.Siniflar.Count();
            var sayi8 = _context.Dersler.Count();
            var sayi9 = _context.Dersler.Count(o => o.Sinif.SinifAd == "9B");
            var sayi10 = _context.Dersler.Count(o => o.Sinif.SinifAd == "10B");
            var sayi11 = _context.Dersler.Count(o => o.Sinif.SinifAd == "11B");
            var sayi12 = _context.Dersler.Count(o => o.Sinif.SinifAd == "12B");
            ViewBag.Sayi1 = sayi1;
            ViewBag.Sayi2 = sayi2;
            ViewBag.Sayi3 = sayi3;
            ViewBag.Sayi4=sayi4;
            ViewBag.Sayi5 = sayi5;
            ViewBag.Sayi6 = sayi6;
            ViewBag.Sayi7 = sayi7;
            ViewBag.Sayi8 = sayi8;
            ViewBag.Sayi9 = sayi9;
            ViewBag.Sayi10 = sayi10;
            ViewBag.Sayi11 = sayi11;
            ViewBag.Sayi12 = sayi12;
            return View();
        }
    }
}

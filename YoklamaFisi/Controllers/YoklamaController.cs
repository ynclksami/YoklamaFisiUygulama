using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using YoklamaFisi.Data;
using YoklamaFisi.Models.Entities;
using YoklamaFisi.Models.FisPdf;
using YoklamaFisi.Models.ViewModel;

namespace YoklamaFisi.Controllers
{
    [AllowAnonymous]
    public class YoklamaController : Controller
    {
        private readonly AppDbContext _context;

        public YoklamaController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int dersSaati = 1, int sinifId = 1, DateTime? tarih = null)
        {
            DateTime seciliTarih = tarih?.Date ?? DateTime.Today.Date;
            bool isGecmisTarih = seciliTarih.Date < DateTime.Today.Date;

            // YENİ ÖZELLİK: Gün hesaplaması (Pazar=0 -> 7'ye çevrilir)
            int gun = ((int)seciliTarih.DayOfWeek == 0) ? 7 : (int)seciliTarih.DayOfWeek;

            ViewBag.DersSaati = dersSaati;
            ViewBag.SinifId = sinifId;
            ViewBag.SeciliTarih = seciliTarih;
            ViewBag.IsGecmisTarih = isGecmisTarih;

            var sinif = await _context.Siniflar.FindAsync(sinifId);
            ViewBag.SinifAdi = sinif?.SinifAd ?? "Sınıf Seçiniz";

            var siniflar = await _context.Siniflar.ToListAsync();

            // HATA DÜZELTME: "Id" -> "SinifId" olarak düzeltildi.
            ViewBag.SiniflarList = new SelectList(siniflar, "SinifId", "SinifAd", sinifId);

            // --- YENİ ÖZELLİK: Ders Programı Bilgisini Çek ---
            // O gün, o saat, o sınıfa ait ders ve öğretmen bilgilerini çekiyoruz
            var dersProgrami = await _context.DersProgramlari
                .Include(dp => dp.Ders)       // Ders bilgisini yükle
                .Include(dp => dp.Ogretmen1)  // Öğretmen 1 bilgisini yükle
                .Include(dp => dp.Ogretmen2)
                .Include(dp => dp.Ogretmen3)
                .FirstOrDefaultAsync(dp =>
                    dp.SinifId == sinifId &&
                    dp.DersSaati == dersSaati &&
                    dp.Gun == gun);

            // Bu bilgiyi View'a gönder
            ViewBag.DersProgrami = dersProgrami;

            // Öğrencileri çek
            var ogrenciler = await _context.Ogrenciler
                .Where(o => o.SinifId == sinifId)
                .OrderBy(o => o.Numara)
                .ToListAsync();

            // Sadece bu sınıftaki öğrencilerin mevcut kayıtlarını çek
            var ogrenciIdler = ogrenciler.Select(o => o.Id).ToList();

            var mevcut = await _context.YoklamaKayitlari
                .Where(k => k.Tarih.Date == seciliTarih.Date &&
                            k.DersSaati == dersSaati &&
                            ogrenciIdler.Contains(k.OgrenciId))
                .ToListAsync();

            ViewBag.MevcutKayitlar = mevcut;

            if (isGecmisTarih)
            {
                TempData["HataMesaji"] = "UYARI: Geçmiş tarihli kayıtlar düzenlenemez.";
            }

            return View(ogrenciler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kaydet(YoklamaKaydetViewModel model)
        {
            if (model.Tarih == null)
            {
                TempData["HataMesaji"] = "Kayıt tarihi eksik veya hatalı.";
                return RedirectToAction("Index", new { sinifId = model.SinifId, dersSaati = model.DersSaati });
            }

            if (model.Tarih.Value.Date < DateTime.Today.Date)
            {
                TempData["HataMesaji"] = "YAPILAMADI: Geçmiş tarihli (" + model.Tarih.Value.ToShortDateString() + ") yoklama kaydı düzenlenemez.";
                return RedirectToAction("Index", new { sinifId = model.SinifId, dersSaati = model.DersSaati, tarih = model.Tarih });
            }

            if (!ModelState.IsValid)
            {
                TempData["HataMesaji"] = "Kayıt sırasında bir hata oluştu.";
                return RedirectToAction("Index", new { sinifId = model.SinifId, dersSaati = model.DersSaati, tarih = model.Tarih });
            }

            int dersSaati = model.DersSaati;
            int sinifId = model.SinifId;
            var tarih = model.Tarih.Value;

            var ogrenciIdler = model.Ogrenciler.Select(o => o.Id).ToList();

            // Verimli sorgu: Mevcut kayıtları bir sözlüğe çek
            var mevcutKayitlar = await _context.YoklamaKayitlari
                .Where(k => ogrenciIdler.Contains(k.OgrenciId) &&
                             k.DersSaati == dersSaati &&
                             k.Tarih.Date == tarih.Date)
                .ToDictionaryAsync(k => k.OgrenciId);

            var eklenecekler = new List<YoklamaKaydi>();
            var guncellenecekler = new List<YoklamaKaydi>();

            foreach (var ogrDurumu in model.Ogrenciler)
            {
                bool gelmedi = ogrDurumu.Gelmedi;
                bool geldiMi = !gelmedi; // true = geldi, false = gelmedi

                if (mevcutKayitlar.TryGetValue(ogrDurumu.Id, out var existing))
                {
                    // Kayıt var, durumu değişmiş olabilir
                    existing.GeldiMi = geldiMi;
                    guncellenecekler.Add(existing);
                }
                else if (gelmedi) // Kayıt yok VE gelmedi olarak işaretlendi
                {
                    // Sadece gelmeyenleri (GeldiMi = false) ekliyoruz
                    eklenecekler.Add(new YoklamaKaydi
                    {
                        OgrenciId = ogrDurumu.Id,
                        DersSaati = dersSaati,
                        GeldiMi = false, // Zaten 'gelmedi' durumunda ekliyoruz
                        Tarih = tarih
                    });
                }
            }

            _context.YoklamaKayitlari.AddRange(eklenecekler);
            _context.YoklamaKayitlari.UpdateRange(guncellenecekler);

            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] = "Yoklama kaydı başarıyla tamamlandı!";

            return RedirectToAction("Index", new { dersSaati = dersSaati, sinifId = sinifId, tarih = tarih });
        }

        [HttpGet]
        public async Task<IActionResult> PdfOlustur(DateTime? tarih = null, int sinifId = 1)
        {
            var seciliTarih = tarih?.Date ?? DateTime.Today;
            var sinif = await _context.Siniflar.FindAsync(sinifId);
            var sinifAdi = sinif?.SinifAd ?? "Bilinmiyor";

            int bugununGunu = ((int)seciliTarih.DayOfWeek == 0) ? 7 : (int)seciliTarih.DayOfWeek;

            var dersProgramiList = await _context.DersProgramlari
                .Include(d => d.Ders)
                .Include(d => d.Ogretmen1)
                .Include(d => d.Ogretmen2)
                .Include(d => d.Ogretmen3)
                .Where(d => d.SinifId == sinifId && d.Gun == bugununGunu)
                .OrderBy(d => d.DersSaati)
                .ToListAsync();

            var dersProgramiDict = new Dictionary<int, string>();
            var ogretmenlerDict = new Dictionary<int, List<string>>();

            foreach (var ders in dersProgramiList)
            {
                int dersSaatiKey = ders.DersSaati;
                dersProgramiDict[dersSaatiKey] = ders.Ders?.Ad ?? "BOŞ";

                var ogretmenListesi = new List<string>();
                if (ders.Ogretmen1 != null) ogretmenListesi.Add(ders.Ogretmen1.AdSoyad);
                if (ders.Ogretmen2 != null) ogretmenListesi.Add(ders.Ogretmen2.AdSoyad);
                if (ders.Ogretmen3 != null) ogretmenListesi.Add(ders.Ogretmen3.AdSoyad);
                ogretmenlerDict[dersSaatiKey] = ogretmenListesi;
            }

            var kayitlar = await _context.YoklamaKayitlari
                .Include(k => k.Ogrenci)
                .Where(k =>
                    k.Tarih.Date == seciliTarih &&
                    !k.GeldiMi &&
                    k.Ogrenci.SinifId == sinifId)
                .ToListAsync();

            var gelmeyenBySaat = Enumerable.Range(1, 10)
                .ToDictionary(
                    i => i,
                    i => kayitlar.Where(k => k.DersSaati == i)
                                 .Select(k => k.Ogrenci.Numara)
                                 .OrderBy(n => n)
                                 .ToList()
                );

            // PDF oluşturma
            var pdf = new YoklamaFisiPdf(gelmeyenBySaat, sinifAdi, dersProgramiDict, ogretmenlerDict, seciliTarih);
            byte[] document = pdf.GeneratePdf();

            return File(document, "application/pdf", $"YoklamaFisi_{sinifAdi}_{seciliTarih:yyyyMMdd}.pdf");
        }

    }
}
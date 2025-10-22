using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using X.PagedList.Extensions;
using YoklamaFisi.Data;

namespace YoklamaFisi.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class YoklamaKayitController : Controller
    {
        private readonly AppDbContext _context;
        public YoklamaKayitController(AppDbContext context)
        {
            _context = context;
        }
        [Route("Index")]
        public async Task<IActionResult> Index(DateTime? tarih, int sayfa = 1)
        {
            var query = _context.YoklamaKayitlari
                .Include(x => x.Ogrenci)
                .ThenInclude(o => o.Sinif) // Sınıf bilgisi
                .AsQueryable();

            if (tarih.HasValue)
            {
                query = query.Where(x => x.Tarih.Date == tarih.Value.Date);
                ViewBag.SeciliTarih = tarih.Value.ToString("yyyy-MM-dd");
            }

            var list = await query.OrderByDescending(x => x.Tarih).ToListAsync();
            int sayfaBoyutu = tarih.HasValue ? (list.Count == 0 ? 1 : list.Count) : 10;
            var yoklamaKayitlari = list.ToPagedList(sayfa, sayfaBoyutu);

            // 🔹 Ders saati bazlı istatistik
            // 🔹 Ders saati bazlı istatistik (sadece Gelmeyen)
            var dersSaatiGruplari = list
                .GroupBy(x => x.DersSaati)
                .Select(g => new
                {
                    DersSaati = g.Key,
                    Gelmeyen = g.Count(x => !x.GeldiMi)
                })
                .OrderBy(g => g.DersSaati)
                .ToList();

            ViewBag.DersSaatiIstatistik = dersSaatiGruplari;

            return View(yoklamaKayitlari);
        }


        [HttpPost]
        [Route("TopluSil")]
        public async Task<IActionResult> TopluSil(List<int> secilenKayitlar)
        {
            if (secilenKayitlar != null && secilenKayitlar.Any())
            {
                var silinecekler = _context.YoklamaKayitlari
                    .Where(x => secilenKayitlar.Contains(x.Id));

                _context.YoklamaKayitlari.RemoveRange(silinecekler);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("ExportPdf")]
        public IActionResult ExportPdf(DateTime? tarih)
        {
            var query = _context.YoklamaKayitlari.Include(x => x.Ogrenci).AsQueryable();
            if (tarih.HasValue)
                query = query.Where(x => x.Tarih.Date == tarih.Value.Date);

            var data = query.OrderBy(x => x.Tarih).ToList();

            // 🔹 Ders saati bazlı istatistik (sadece Gelmeyen)
            var dersSaatiGruplari = data
                .GroupBy(x => x.DersSaati)
                .Select(g => new
                {
                    DersSaati = g.Key,
                    Gelmeyen = g.Count(x => !x.GeldiMi)
                })
                .OrderBy(g => g.DersSaati)
                .ToList();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.Content().Column(col =>
                    {
                        col.Item().Text("Yoklama Kayıtları").FontSize(18).Bold();
                        col.Item().LineHorizontal(1);

                        // 🔹 Kayıt tablosu
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("No");
                                header.Cell().Text("Öğrenci");
                                header.Cell().Text("Tarih");
                                header.Cell().Text("Geldi Mi");
                            });

                            int sayac = 1;
                            foreach (var item in data)
                            {
                                table.Cell().Text(sayac++);
                                table.Cell().Text(item.Ogrenci.AdSoyad);
                                table.Cell().Text(item.Tarih.ToString("dd.MM.yyyy"));
                                table.Cell().Text(item.GeldiMi ? "Evet" : "Hayır");
                            }
                        });

                        col.Item().LineHorizontal(1);

                        // 🔹 Ders saati bazlı istatistik (sadece Gelmeyen)
                        col.Item().PaddingTop(10).Text("Ders Saati Bazlı İstatistik").FontSize(14).Bold();

                        col.Item().Table(stat =>
                        {
                            stat.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            stat.Header(header =>
                            {
                                header.Cell().Text("Ders Saati");
                                header.Cell().Text("Gelmeyen");
                            });

                            foreach (var item in dersSaatiGruplari)
                            {
                                stat.Cell().Text(item.DersSaati);
                                stat.Cell().Text(item.Gelmeyen.ToString());
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", "YoklamaKayitlari.pdf");
        }

        [HttpGet]
        [Route("ExportExcel")]
        public IActionResult ExportExcel(DateTime? tarih)
        {
            var query = _context.YoklamaKayitlari.Include(x => x.Ogrenci).AsQueryable();
            if (tarih.HasValue)
                query = query.Where(x => x.Tarih.Date == tarih.Value.Date);

            var data = query.OrderBy(x => x.Tarih).ToList();

            var dersSaatiGruplari = data
                .GroupBy(x => x.DersSaati)
                .Select(g => new
                {
                    DersSaati = g.Key,
                    Gelmeyen = g.Count(x => !x.GeldiMi)
                })
                .OrderBy(g => g.DersSaati)
                .ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Yoklama");

            // 🔹 Kayıtlar
            ws.Cell(1, 1).Value = "No";
            ws.Cell(1, 2).Value = "Öğrenci";
            ws.Cell(1, 3).Value = "Tarih";
            ws.Cell(1, 4).Value = "Geldi Mi";

            int row = 2;
            int count = 1;
            foreach (var item in data)
            {
                ws.Cell(row, 1).Value = count++;
                ws.Cell(row, 2).Value = item.Ogrenci.AdSoyad;
                ws.Cell(row, 3).Value = item.Tarih.ToString("dd.MM.yyyy");
                ws.Cell(row, 4).Value = item.GeldiMi ? "Evet" : "Hayır";
                row++;
            }

            row += 1; // boş satır bırak

            // 🔹 Ders saati istatistik tablosu (sadece Gelmeyen)
            ws.Cell(row, 1).Value = "Ders Saati Bazlı İstatistik";
            ws.Cell(row, 1).Style.Font.Bold = true;
            row++;

            ws.Cell(row, 1).Value = "Ders Saati";
            ws.Cell(row, 2).Value = "Gelmeyen";
            ws.Range(row, 1, row, 2).Style.Font.Bold = true;
            row++;

            foreach (var item in dersSaatiGruplari)
            {
                ws.Cell(row, 1).Value = item.DersSaati;
                ws.Cell(row, 2).Value = item.Gelmeyen;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "YoklamaKayitlari.xlsx");
        }

        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var kayit = await _context.YoklamaKayitlari.FindAsync(id);
            if (kayit == null)
            {
                TempData["HataMesaji"] = "Kayıt bulunamadı.";
                return RedirectToAction("Index");
            }

            _context.YoklamaKayitlari.Remove(kayit);
            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] = "Kayıt başarıyla silindi.";
            return RedirectToAction("Index");
        }
    }
}

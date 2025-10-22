using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace YoklamaFisi.Models.FisPdf
{
    // IDocument arayüzünü uygulayan PDF sınıfı
    public class YoklamaFisiPdf : IDocument
    {
        // Private Alanlar
        private readonly Dictionary<int, List<int>> _gelmeyenBySaat;
        private readonly string _sinifAdi;
        private readonly Dictionary<int, string> _dersProgrami;
        private readonly DateTime _tarih;
        private readonly Dictionary<int, List<string>> _ogretmenProgrami;

        // Constructor
        public YoklamaFisiPdf(Dictionary<int, List<int>> gelmeyenBySaat,
                              string sinifAdi,
                              Dictionary<int, string> dersProgrami,
                              Dictionary<int, List<string>> ogretmenProgrami,
                              DateTime tarih)
        {
            _gelmeyenBySaat = gelmeyenBySaat ?? new Dictionary<int, List<int>>();
            _sinifAdi = sinifAdi;
            _dersProgrami = dersProgrami ?? new Dictionary<int, string>();
            _tarih = tarih;
            _ogretmenProgrami = ogretmenProgrami ?? new Dictionary<int, List<string>>();
        }

        // IDocument Metotları
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
                {
                    // Sayfa ayarları (Sarkmayı önlemek için optimize edildi)
                    page.Size(14f, 21f, Unit.Centimetre);
                    page.Margin(0.4f, Unit.Centimetre); // Marjin küçültüldü
                    page.DefaultTextStyle(x => x.FontSize(10)); // Font küçültüldü

                    // 🌟 Hata veren satırlar lambda ile düzeltildi 🌟
                    page.Header().Element(x => ComposeHeader(x));
                    page.Content().Element(x => x.ScaleToFit().Element(ComposeContent));
                    page.Footer().Element(x => ComposeFooter(x));
                });
        }

        // --- YARDIMCI METOTLAR ---

        private void ComposeHeader(IContainer container)
        {
            // Header İçeriği
            container.Stack(header =>
            {
                header.Item().AlignCenter().Text("TURHAL ŞEHİT AHMET ATİLLA GÜNEŞ MTAL MÜDÜRLÜĞÜ").Bold().FontSize(12);
                header.Item().AlignCenter().Text("ÖĞRENCİ GÜNLÜK YOKLAMA FİŞİ").Bold().FontSize(12);
                header.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text($"Sınıf/Şube: {_sinifAdi}");
                    row.RelativeItem().AlignRight().Text($"{_tarih:dd.MM.yyyy}");
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Stack(content =>
            {
                content.Spacing(0);
                content.Item().Height(3);

                // 1. Ders Adı Tablosu (Birleştirilmiş)
                content.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        for (int i = 0; i < 11; i++) columns.RelativeColumn();
                    });

                    table.Cell().Border(1).Padding(2).MinHeight(50).AlignMiddle().AlignCenter()
                        .Element(e => e.RotateLeft().Text("Dersin Adı").Bold());

                    int baslangicSaati = 1;
                    while (baslangicSaati <= 10)
                    {
                        string mevcutDersAdi = _dersProgrami.GetValueOrDefault(baslangicSaati, "BOŞ");
                        if (mevcutDersAdi == "BOŞ" || string.IsNullOrWhiteSpace(mevcutDersAdi))
                        {
                            table.Cell().Border(1).Padding(2).AlignMiddle().AlignCenter().Text(" ").Bold();
                            baslangicSaati++;
                            continue;
                        }

                        int span = 0;
                        for (int i = baslangicSaati; i <= 10; i++)
                        {
                            if (_dersProgrami.GetValueOrDefault(i) == mevcutDersAdi) span++;
                            else break;
                        }

                        table.Cell().ColumnSpan((uint)span).Element(element =>
                        {
                            element.Border(1).Padding(2).AlignMiddle().AlignCenter().Text(mevcutDersAdi).Bold();
                        });

                        baslangicSaati += span;
                    }
                });

                // 2. Ders Saati Başlık Satırı
                content.Item().Grid(grid =>
                {
                    grid.Columns(11);
                    grid.Item().Border(1).Padding(2).Text("Ders Saati").Bold().AlignCenter();
                    for (int i = 1; i <= 10; i++)
                        grid.Item().Border(1).Padding(2).AlignMiddle().Text(i.ToString()).Bold().AlignCenter();
                });

                // 3. Gelmeyen Öğrenci Numaraları Satırı
                content.Item().Grid(grid =>
                {
                    grid.Columns(11);
                    grid.Item().Border(1).Padding(3).AlignMiddle().AlignCenter().Element(e => e.RotateLeft()).Text("Derse Gelmeyen Öğrenci Numaraları").Bold().AlignCenter().FontSize(12);

                    for (int saat = 1; saat <= 10; saat++)
                    {
                        _gelmeyenBySaat.TryGetValue(saat, out var list);
                        list ??= new List<int>();
                        string dersAdiGelen = _dersProgrami.GetValueOrDefault(saat, "BOŞ");

                        bool dersVar = !string.IsNullOrWhiteSpace(dersAdiGelen) &&
                                       !dersAdiGelen.Equals("BOŞ", StringComparison.OrdinalIgnoreCase) &&
                                       !dersAdiGelen.Equals("ders boş", StringComparison.OrdinalIgnoreCase);

                        grid.Item().Border(1).Padding(2)
                            .MinHeight(10f, Unit.Centimetre)
                            .AlignCenter()
                            .Text(text =>
                            {
                                if (dersVar)
                                {
                                    if (list.Any())
                                    {
                                        foreach (var num in list) text.Line(num.ToString());
                                    }
                                    else
                                    {
                                        text.Line("TAM");
                                    }
                                }
                                else
                                {
                                    text.Line(" ");
                                }
                            });
                    }
                });

                // 🌟 4. Öğretmenin İmzası Satırı (DİKEY YAZI & HİZALAMA DÜZELTİLDİ) 🌟
                content.Item().Grid(grid =>
                {
                    grid.Columns(11);
                    // Soldaki Dikey Etiket
                    grid.Item().Border(1).Padding(3).MinHeight(50).AlignMiddle().AlignCenter().Element(e => e.RotateLeft()).Text("Öğretmenin İmzası").Bold().AlignCenter();

                    for (int saat = 1; saat <= 10; saat++)
                    {
                        grid.Item().Border(1).Padding(3).MinHeight(50)
                            // Hizalama ortalamadan çıkarıldı.
                            .Element(e => e.RotateLeft()
                                .AlignLeft().AlignTop() // Dikey okumaya göre, metin hücrenin sol üst köşesinden başlar.
                                .Text(text =>
                                {
                                    if (_ogretmenProgrami.TryGetValue(saat, out var ogretmenList) && ogretmenList.Any())
                                    {
                                        foreach (var ogretmen in ogretmenList)
                                        {
                                            text.Line(ogretmen).FontSize(6);
                                        }
                                    }
                                    else
                                    {
                                        text.Line(" ");
                                    }
                                })
                            );
                    }
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            // Footer İçeriği
            container.Stack(footer =>
            {
                footer.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text("1-Derste bulunmayan öğrenci numarası ile ilgili sütuna ayrı yazılır.").FontSize(6);
                    row.RelativeItem().AlignRight().Text("incelendi").FontSize(6);
                });
                footer.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text("2-Yoklama fişi ve ders defteri son dersin öğretmeni tarafından ilgili müdür yardımcısına elden teslim edilir.").FontSize(6);
                    row.RelativeItem().AlignRight().Text("Müdür Yardımcısı").FontSize(6);
                });
                footer.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text("");
                    row.RelativeItem().AlignRight().Text("İmza").FontSize(6);
                });
            });
        }
    }
}

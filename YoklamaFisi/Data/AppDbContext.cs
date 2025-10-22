using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YoklamaFisi.Models.Entities;

namespace YoklamaFisi.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Ogrenci> Ogrenciler { get; set; }
        public DbSet<YoklamaKaydi> YoklamaKayitlari { get; set; }
        public DbSet<Sinif> Siniflar { get; set; }
        public DbSet<DersProgrami> DersProgramlari { get; set; }
        public DbSet<Ogretmen> Ogretmenler { get; set; }
        public DbSet<Ders> Dersler { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Ogrenci>()
                .HasOne(o => o.Sinif)
                .WithMany() // eğer Sinif içinde List<Ogrenci> yoksa
                .HasForeignKey(o => o.SinifId)
                .OnDelete(DeleteBehavior.Restrict);
            // DersOgretmen ara tablosu
            // Öğretmen 1 İlişkisi
            modelBuilder.Entity<DersProgrami>()
                .HasOne(dp => dp.Ogretmen1)
                .WithMany() // Öğretmen modelinde DersProgrami listesi tutmuyorsak WithMany() kullanabiliriz
                .HasForeignKey(dp => dp.Ogretmen1Id)
                .IsRequired(false) // Ogretmen1Id NULL olabilir
                .OnDelete(DeleteBehavior.Restrict); // Silme kısıtlaması

            // Öğretmen 2 İlişkisi
            modelBuilder.Entity<DersProgrami>()
                .HasOne(dp => dp.Ogretmen2)
                .WithMany()
                .HasForeignKey(dp => dp.Ogretmen2Id)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Öğretmen 3 İlişkisi
            modelBuilder.Entity<DersProgrami>()
                .HasOne(dp => dp.Ogretmen3)
                .WithMany()
                .HasForeignKey(dp => dp.Ogretmen3Id)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            // SINIF - DERS
            modelBuilder.Entity<Ders>()
                .HasOne(d => d.Sinif)
                .WithMany(s => s.Dersler)
                .HasForeignKey(d => d.SinifId)
                .OnDelete(DeleteBehavior.Restrict);

            // SINIF - DERS PROGRAMI
            modelBuilder.Entity<DersProgrami>()
                .HasOne(dp => dp.Sinif)
                .WithMany()
                .HasForeignKey(dp => dp.SinifId)
                .OnDelete(DeleteBehavior.Restrict); 

            // DERS - DERS PROGRAMI
            modelBuilder.Entity<DersProgrami>()
                .HasOne(dp => dp.Ders)
                .WithMany(d => d.DersProgramlari)
                .HasForeignKey(dp => dp.DersId)
                .OnDelete(DeleteBehavior.Restrict); 

        }
    }
}
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace YoklamaFisi.Models.Entities
{
    public class AppUser : IdentityUser
    {
        [Required]
        [Display(Name = "Ad")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Soyad")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Profil Resmi")]
        [StringLength(255)]
        public string? ProfileImage { get; set; }

        // FullName property (sadece okuma için)
        [Display(Name = "Tam Ad")]
        public string FullName => $"{FirstName} {LastName}";
       // string tamAd = user.FirstName + " " + user.LastName; // Her seferinde bunu yazmak zorunda kalırdık
    }
}
using System.ComponentModel.DataAnnotations;

namespace YoklamaFisi.Areas.Admin.Models.ViewModel
{
    public class EditProfileViewModel
    {
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad zorunludur.")]
        [Display(Name = "Ad")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [Display(Name = "Soyad")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Profil Resmi")]
        public IFormFile? ProfileImage { get; set; }

        [Display(Name = "Mevcut Resim")]
        public string? ExistingImage { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (isteğe bağlı)")]
        public string? NewPassword { get; set; }
    }
}

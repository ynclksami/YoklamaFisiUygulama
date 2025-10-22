using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YoklamaFisi.Areas.Admin.Models.ViewModel;
using YoklamaFisi.Models.Entities;

namespace YoklamaFisi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProfileController(UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("SignIn", "Login");

            var model = new EditProfileViewModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ExistingImage = user.ProfileImage
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model, bool RemoveImage = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                // ✅ Profil resmi silme isteği geldiyse
                if (RemoveImage && !string.IsNullOrEmpty(user.ProfileImage))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImage.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                    user.ProfileImage = null;
                }

                // ✅ Yeni resim yüklendiyse
                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    // Eski resmi sil (varsa)
                    if (!string.IsNullOrEmpty(user.ProfileImage))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }

                    // Yeni resmi kaydet
                    var fileName = Guid.NewGuid() + Path.GetExtension(model.ProfileImage.FileName);
                    var uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/profile");
                    Directory.CreateDirectory(uploadDir);

                    var newFilePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                        await model.ProfileImage.CopyToAsync(stream);

                    user.ProfileImage = "/uploads/profile/" + fileName;
                }

                // ✅ Yeni şifre varsa güncelle
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                }

                await _userManager.UpdateAsync(user);
                TempData["Success"] = "Profil başarıyla güncellendi.";
                return RedirectToAction(nameof(Edit));
            }

            return View(model);
        }

    }
}


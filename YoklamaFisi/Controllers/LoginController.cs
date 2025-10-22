using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YoklamaFisi.Models.Entities;
using YoklamaFisi.Models.ViewModel;

namespace YoklamaFisi.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
       // private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<LoginController> _logger;

        public LoginController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, ILogger<LoginController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
           // _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }
        // POST: Giriş İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // "Remember Me" seçili değilse giriş engellenir
                if (!model.RememberMe)
                {
                    ModelState.AddModelError(string.Empty, "Giriş yapabilmek için 'Remember Me' kutusunu işaretlemelisiniz.");
                    _logger.LogWarning("Remember Me işaretlenmeden giriş denemesi yapıldı: {Email}", model.Email);
                    return View(model);
                }

                // Email ile giriş yap (UserName = Email)
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Kullanıcı giriş yaptı: {Email}", model.Email);
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Hesabınız geçici olarak kilitlendi.");
                    _logger.LogWarning("Kullanıcı hesabı kilitlendi: {Email}", model.Email);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                    _logger.LogWarning("Geçersiz giriş denemesi: {Email}", model.Email);
                }
            }

            return View(model);
        }
        // GET: Kayıt Sayfası
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Kayıt İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Yeni kullanıcı oluştur
                var user = new AppUser
                {
                    UserName = model.Email, // Email = Kullanıcı adı
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                // Kullanıcıyı oluştur (şifre otomatik hash'lenir)
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Yeni kullanıcı oluşturuldu: {Email}", model.Email);
                    // 1. "Admin" rolü var mı kontrol et, yoksa oluştur.
                  //  if (!await _roleManager.RoleExistsAsync("Admin"))
                 //   {
                //        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                 //   }
                    // Kullanıcıyı otomatik giriş yaptır (isteğe bağlı)
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("SignIn", "Login");
                }

                // Hataları göster
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // Çıkış İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Kullanıcı çıkış yaptı.");
            return RedirectToAction("SignIn", "Login");
        }
        [HttpGet]
        public IActionResult AccessDenied(int code)
        {
            return View();
        }
    }
}

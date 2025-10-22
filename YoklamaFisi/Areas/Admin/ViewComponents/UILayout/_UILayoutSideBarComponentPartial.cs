using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YoklamaFisi.Models.Entities;

namespace YoklamaFisi.Areas.Admin.ViewComponents.UILayout
{
    public class _UILayoutSideBarComponentPartial : ViewComponent
    {
        private readonly UserManager<AppUser> _userManager;

        public _UILayoutSideBarComponentPartial(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            AppUser? currentUser = null;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                currentUser = await _userManager.GetUserAsync(HttpContext.User);
            }

            return View(currentUser);
        }
    }
}

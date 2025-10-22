using Microsoft.AspNetCore.Mvc;

namespace YoklamaFisi.Areas.Admin.ViewComponents.UILayout
{
    public class _UILayoutFooterComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}

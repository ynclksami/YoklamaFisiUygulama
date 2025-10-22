using Microsoft.AspNetCore.Mvc;

namespace YoklamaFisi.Areas.Admin.ViewComponents.UILayout
{
    public class _UILayoutHeadComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}

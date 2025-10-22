using Microsoft.AspNetCore.Mvc;

namespace YoklamaFisi.Areas.Admin.ViewComponents.UILayout
{
    public class _UILayoutScriptComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyMvcApp.Pages
{
    public class LocalPageModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}

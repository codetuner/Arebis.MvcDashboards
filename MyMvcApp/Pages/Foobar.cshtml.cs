using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace MyMvcApp.Pages
{
    public class FoobarModel : PageModel
    {
        [BindProperty, Required]
        public string? Email { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                return RedirectToPage("./LocalPage");
            }
            else
            {
                return Page();
            }
        }
    }
}

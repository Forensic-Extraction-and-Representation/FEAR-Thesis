using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FEAR.Hosted.Pages.Authorization
{
    public class AuthorizeModel : PageModel
    {
        [Display(Name = "Application")]
        public string? ApplicationName { get; set; }

        [Display(Name = "Scope")]
        public string? Scope { get; set; }
    }
}

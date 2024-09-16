using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

#nullable enable

namespace MyMvcApp.Models.Local
{
    public class IndexModel
    {
        [Display(Name="Name")]
        [Required(ErrorMessage = "Sorry! The {0} is required...")]
        //[Required(ErrorMessageResourceName = "SorryTheFieldIsRequired", ErrorMessageResourceType = typeof(MyResources))]
        public string? Name { get; set; }

        [Display(Name = "ZipCode")]
        public int ZipCode { get; set; }

        //[Display(Name = "Age")]
        [Required]
        [Range(0, 150)]
        public int? Age { get; set; }

        public LocalizedString? Message { get; internal set; }
        
        public LocalizedHtmlString? HtmlMessage { get; internal set; }
        
        public DateTime Date { get; internal set; }
    }
}

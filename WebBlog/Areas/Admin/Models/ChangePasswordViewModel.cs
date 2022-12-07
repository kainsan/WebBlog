using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebBlog.Areas.Admin.Models
{
    public class ChangePasswordViewModel
    {
        [Key]
        public int AccountId { get; set; }
        [Display(Name ="Password Now")]
        public string PasswordNow { get; set; }
        [Display(Name = "New Password")]
        [Required(ErrorMessage ="Pls import Password")]
        [MinLength(5,ErrorMessage ="at least 5 char")]
        public string Password { get; set; }
        [MinLength(5, ErrorMessage = "at least 5 char")]
        [Display(Name ="Import New Password")]
        [Compare("Password",ErrorMessage ="Not same Password")]
        public string ConfirmPassword { get; set; }
    }
}

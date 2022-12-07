using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace WebBlog.Areas.Admin.Models
{
    public class LoginViewModel
    {
        [Key]
        [MaxLength(50)]
        [Required(ErrorMessage ="Vui long nhap Email")]
        [Display(Name ="Email Address")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Vui long nhap Email")]
        public string Email { get; set; }

        [Display(Name ="Password")]
        [Required(ErrorMessage ="Vui long nhap mat khau")]
        [MaxLength(30,ErrorMessage ="Mat khau toi da 30 ki tu")]
        public string Password { get; set; }
    }
}

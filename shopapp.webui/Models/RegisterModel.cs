using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace shopapp.webui.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "İsim alanı boş bırakılamaz.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Soyisim alanı boş bırakılamaz.")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Kullanıcı adı alanı boş bırakılamaz.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Şifre alanı boş bırakılamaz.")]
        [DataType(DataType.Password)]
        [MinLength(6,ErrorMessage = "Şifre minimum 6 karakter olmalıdır.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Şifre Tekrar alanı boş bırakılamaz.")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage = "Şifreler birbirleriyle eşleşmiyor.")]
        public string RePassword { get; set; }
        [Required(ErrorMessage = "E-Posta alanı boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }
    }
}
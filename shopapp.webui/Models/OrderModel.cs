using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace shopapp.webui.Models
{
    public class OrderModel
    {
        [Required(ErrorMessage = "Ad alanı boş bırakılamaz.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Soyad alanı boş bırakılamaz.")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Adres alanı boş bırakılamaz.")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Şehir alanı boş bırakılamaz.")]
        public string City { get; set; }
        [Required(ErrorMessage = "Telefon alanı boş bırakılamaz.")]
        public string Phone { get; set; }
        public string Note { get; set; }
        [Required(ErrorMessage = "E-posta alanı boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Kart Sahibi alanı boş bırakılamaz.")]
        public string CardName { get; set; }
        [Required(ErrorMessage = "Kart Numarası alanı boş bırakılamaz.")]
        public string CardNumber { get; set; }
        [Required(ErrorMessage = "Ay alanı boş bırakılamaz.")]
        public string ExpirationMonth { get; set; }
        [Required(ErrorMessage = "Yıl alanı boş bırakılamaz.")]
        public string ExpirationYear { get; set; }
        [Required(ErrorMessage = "Ad alanı boş bırakılamaz.")]
        [MinLength(3,ErrorMessage = "CVC alanı 3 karakterden oluşmalıdır.")]
        [MaxLength(3,ErrorMessage = "CVC alanı 3 karakterden oluşmalıdır.")]
        public string Cvc { get; set; }
        public CartModel CartModel { get; set; }
    }
}
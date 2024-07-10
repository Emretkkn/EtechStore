using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using shopapp.entity;

namespace shopapp.webui.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Ürün adı zorunlu bir alan.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Ürün adı 5-100 karakter arasında olmalıdır.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Url adı zorunlu bir alan.")]
        public string Url { get; set; }
        [Required(ErrorMessage = "Ürün fiyatı zorunlu bir alan.")]
        [Range(1, 1000000, ErrorMessage = "Lütfen pozitif değer girin.")]
        public double? Price { get; set; }
        [Required(ErrorMessage = "Açıklama zorunlu bir alan.")]
        [MinLength(5,ErrorMessage = "Açıklama en az 5 karakter uzunluğunda olmalıdır.")]
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsApproved { get; set; }
        public bool IsHome { get; set; }
        public List<Category> SelectedCategories { get; set; }
    }
}
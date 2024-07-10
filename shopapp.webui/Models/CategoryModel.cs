using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using shopapp.entity;

namespace shopapp.webui.Models
{
    public class CategoryModel
    {
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Kategori adı 5-50 karakter aralığında olmalıdır.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Url zorunludur.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Url adı 5-100 karakter aralığında olmalıdır.")]
        public string Url { get; set; }
        public List<Product> Products { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace shopapp.entity
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
        public string PaymentId { get; set; }
        public string ConversationId { get; set; }
        public EnumPaymentType PaymentType { get; set; }
        public EnumOrderState OrderState { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }

    public enum EnumOrderState
    {
        [Display(Name = "Ödeme Bekleniyor")]
        waiting = 0,
        [Display(Name = "Ödeme Yapılmadı")]
        unpaid = 1,
        [Display(Name = "Ödeme Tamamlandı")]
        completed = 2
    }

    public enum EnumPaymentType
    {
        [Display(Name = "Kredi Kartı")]
        CreditCard = 0,
        [Display(Name = "Eft")]
        Eft = 1,
        [Display(Name = "Nakit")]
        Cash = 2

    }
}
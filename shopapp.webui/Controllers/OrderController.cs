using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using shopapp.business.Abstract;
using shopapp.webui.Identity;
using shopapp.webui.Models;

namespace shopapp.webui.Controllers
{
    public class OrderController : Controller
    {
        private UserManager<User> _userManager;
        private IOrderService _orderService;
        public OrderController(UserManager<User> userManager, IOrderService orderService)
        {
            _userManager = userManager;
            _orderService = orderService;
        }

        public IActionResult GetOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = _orderService.GetOrders(userId);

            var orderListModel = new List<OrderListModel>();
            OrderListModel orderModel;
            foreach (var ord in orders)
            {
                orderModel = new OrderListModel();
                
                orderModel.OrderId = ord.Id;
                orderModel.OrderNumber = ord.OrderNumber;
                orderModel.OrderNumber = ord.OrderNumber;
                orderModel.OrderDate = ord.OrderDate;
                orderModel.Phone = ord.Phone;
                orderModel.FirstName = ord.FirstName;
                orderModel.LastName = ord.LastName;
                orderModel.Email = ord.Email;
                orderModel.Address = ord.Address;
                orderModel.City = ord.City;
                orderModel.OrderState = ord.OrderState;
                orderModel.PaymentType = ord.PaymentType;

                orderModel.OrderItems = ord.OrderItems.Select(i => new OrderItemModel(){
                    OrderItemId = i.Id,
                    Name = i.Product.Name,
                    Price = (double)i.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.Product.ImageUrl
                }).ToList();

                orderListModel.Add(orderModel);
            }


            return View("Orders", orderListModel);
        }
    }
}
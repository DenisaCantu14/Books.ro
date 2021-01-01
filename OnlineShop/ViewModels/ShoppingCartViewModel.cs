using System.Collections.Generic;
using OnlineShop.Models;

namespace OnlineShop.ViewModels
{
    public class ShoppingCartViewModel
    {
        public List<Cart> CartItems { get; set; }
        public decimal CartTotal { get; set; }
    }
}
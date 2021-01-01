using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using OnlineShop.Models;

namespace MvcMusicStore.Controllers
{
    [Authorize(Roles = "Admin, Editor, User")]
    public class CheckoutController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        
        //
        // GET: /Checkout/AddressAndPayment
        public ActionResult AddressAndPayment()
        {
            return View();
        }
        //
        // POST: /Checkout/AddressAndPayment
        [HttpPost]
        public ActionResult AddressAndPayment(Order order)
        {
        
            try
            {
                if (ModelState.IsValid)
                {
                    order.Username = User.Identity.Name;
                    order.OrderDate = DateTime.Now;

                    //Save Order
                    db.Orders.Add(order);
                    db.SaveChanges();
                    //Process the order
                    var CartId = User.Identity.GetUserName();
                    var cart = ShoppingCart.GetCart(CartId);
                   
                    cart.CreateOrder(order);

                    return RedirectToAction("Complete",
                        new { id = order.OrderId });

                }
                else
                {
                    return View(order);
                }


            }
            catch
            {
                //Invalid - redisplay with errors
                return View(order);
            }
        }
        //
        // GET: /Checkout/Complete
        public ActionResult Complete(int id)
        {
            // Validate customer owns this order
            bool isValid = db.Orders.Any(
                o => o.OrderId == id &&
                o.Username == User.Identity.Name);

            if (isValid)
            {
                return View(id);
            }
            else
            {
                return View("Error");
            }
        }
    }
}
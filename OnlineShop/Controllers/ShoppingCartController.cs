using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using OnlineShop.Models;
using OnlineShop.ViewModels;

namespace OnlineShop.Controllers

{
    [Authorize(Roles = "Admin, Editor, User")]
    public class ShoppingCartController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        //
        // GET: /ShoppingCart/
        public ActionResult Index()
        {
            var CartId = User.Identity.GetUserName();
            var cart = ShoppingCart.GetCart(CartId);
         
            // Set up our ViewModel
            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal()
                
            };
            if (viewModel.CartTotal == 0) 
                ViewBag.EmptyCart = true;
            else
                ViewBag.EmptyCart = false;
            // Return the view
            return View(viewModel);
        }
        //
        // GET: /Store/AddToCart/5
        public ActionResult AddToCart(int id)
        {
            // Retrieve the album from the database
            var addedBook = db.Books
                .Single(book => book.BookId == id);

            // Add it to the shopping cart
            var CartId = User.Identity.GetUserName();
            var cart = ShoppingCart.GetCart(CartId);

            cart.AddToCart(addedBook);


            // Go back to the main store page for more shopping
            //return RedirectToAction("Index");
            
            return RedirectToAction("Index", "ShoppingCart");
        }
        //
        // AJAX: /ShoppingCart/RemoveFromCart/5
        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            // Remove the item from the cart
            var CartId = User.Identity.GetUserName();
            var cart = ShoppingCart.GetCart(CartId);

            // Get the name of the book to display confirmation
            string bookTitle = db.Carts
                .Single(item => item.RecordId == id).Book.Title;

            // Remove from cart
            int itemCount = cart.RemoveFromCart(id);

            // Display the confirmation message
            var results = new ShoppingCartRemoveViewModel
            {
                Message = Server.HtmlEncode(bookTitle) +
                    " has been removed from your shopping cart.",
                CartTotal = cart.GetTotal(),
                CartCount = cart.GetCount(),
                ItemCount = itemCount,
                DeleteId = id
            };

            return RedirectToAction("Index");
        }
       
        
    }
}
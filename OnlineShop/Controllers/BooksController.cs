using Microsoft.AspNet.Identity;
using OnlineShop.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineShop.Controllers
{
    public class BooksController : Controller
    {
        // GET: Books

        private Models.ApplicationDbContext db = new Models.ApplicationDbContext();
        // GET: Books
       

        private int _perPage = 8;
        public ActionResult Index()
        {
            var books = db.Books.Where(a => a.Accepted == true).Include("Category").Include("User").OrderBy(a => a.Price);
            switch (Request.Params.Get("sort"))
            {
                case "Price: Low to High":
                    books = db.Books.Where(a => a.Accepted == true).Include("Category").Include("User").OrderBy(a => a.Price);
                    break;
                case "Price: High to Low":
                    books = db.Books.Where(a => a.Accepted == true).Include("Category").Include("User").OrderByDescending(a => a.Price);
                    break;
                case "Rating: Low to High":
                    books = db.Books.Where(a => a.Accepted == true).Include("Category").Include("User").OrderBy(a => a.avg);
                    break;
                case "Rating: High to Low":
                    books = db.Books.Where(a => a.Accepted == true).Include("Category").Include("User").OrderByDescending(a => a.avg);
                    break;
            }
            var search = "";
            if(Request.Params.Get("search") != null)
            {
                search = Request.Params.Get("search").Trim();
                List<int> bookIds = db.Books.Where(
                    at => at.Title.Contains(search) 
                        || at.Description.Contains(search) 
                        || at.Author.Contains(search) 
                        || at.Publisher.Contains(search)
                        ).Select(a => a.BookId).ToList();

                List<int> reviewIds = db.Reviews.Where(
                   at => at.Content.Contains(search)
                       ).Select(a => a.BookId).ToList();

                List<int> mergeIds = bookIds.Union(reviewIds).ToList();

                books = db.Books.Where(book => mergeIds.Contains(book.BookId)).Include("Category").Include("User").OrderBy(a => a.Price);
                
            }
            var totalItems = books.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this._perPage;
            }

            var paginatedBooks = books.Skip(offset).Take(this._perPage);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            //ViewBag.perPage = this._perPage;
            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this._perPage);
            ViewBag.Books = paginatedBooks;

            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult Accept()
        {
            var books = db.Books.Where(a => a.Accepted==false).Include("Category").Include("User");
            ViewBag.Books = books;
            return View();
        }
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public ActionResult Accept(int id)
        {
           
            try
            {
                if (ModelState.IsValid)
                {
                    Book book = db.Books.Find(id);
                    if (TryUpdateModel(book))
                        {
                            book.Accepted = true;
                            TempData["message"] = "The book was accepted!";
                            db.SaveChanges();
                        }
                     return RedirectToAction("Index");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception e)
            {
                return View();
            }
        }
        public ActionResult Show(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            Book book = db.Books.Find(id);
            ViewBag.showAddReview = false;
            ViewBag.showButtons = false;
            if (User.IsInRole("Admin") || User.IsInRole("Editor") || User.IsInRole("User"))
            {
                ViewBag.showAddReview = true;
            }
            if (User.IsInRole("Editor") || User.IsInRole("Admin") || User.IsInRole("User"))
            {
                ViewBag.showButtons = true;
            }
            ViewBag.isAdmin = User.IsInRole("Admin");
            ViewBag.currentUser = User.Identity.GetUserId();
            var nrRev = false;
            if (book.Reviews.Count() != 0)
                nrRev = true;
            ViewBag.nrRev = nrRev;
            return View(book);

        }
        [Authorize(Roles = "Admin, Editor")]
        public ActionResult New()
        {
            Book book = new Book();
            book.Categ = GetAllCategories();
            book.UserId = User.Identity.GetUserId();
            var role = false;
            if (User.IsInRole("Admin"))
                role = true;
            
            ViewBag.role = role;
           
            return View(book);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Editor")]
        public ActionResult New(Book book)
        {
            book.Categ = GetAllCategories();
            book.UserId = User.Identity.GetUserId();
            book.nrStars = 0;
            book.nrRev = 0;
            if (User.IsInRole("Admin"))
                book.Accepted = true;
            else
                book.Accepted = false;
            try
            {
                if (ModelState.IsValid)
                {
                    db.Books.Add(book);
                    db.SaveChanges();
                    if(User.IsInRole("Admin"))
                        TempData["message"] = "Book added!";
                    else
                        TempData["message"] = "Request sent!";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(book);
                }
            }
            catch (Exception)
            {
                return View(book);
            }
        }

        [Authorize(Roles = "Admin, Editor")]
        public ActionResult Edit(int id)
        {
            Book book = db.Books.Find(id);
            book.Categ = GetAllCategories();
            if (book.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                return View(book);
            }
            else
            {
                TempData["message"] = "You don't have rights to make changes to this book!";
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin, Editor")]
        public ActionResult Edit(int id, Book requestBook)
        {
            requestBook.Categ = GetAllCategories();
            try
            {
                if (ModelState.IsValid)
                {
                    Book book = db.Books.Find(id);
                    if (book.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                    {
                        if (TryUpdateModel(book))
                        {
                            book.Title = requestBook.Title;
                            book.ImageName = requestBook.ImageName;
                            book.Description = requestBook.Description;
                            book.Author = requestBook.Author;
                            book.Price = requestBook.Price;
                            book.Publisher = requestBook.Publisher;
                            book.CategoryId = requestBook.CategoryId;
                            TempData["message"] = "The book was modified!";
                            db.SaveChanges();
                        }
                        return RedirectToAction("Index");

                    }
                    else
                    {
                        TempData["message"] = "You don't have rights to make changes to this book!";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    requestBook.Categ = GetAllCategories();
                    return View(requestBook);
                }

            

        }
            catch (Exception e)
            {
                return View(requestBook);
            }
                
        
        }

        [HttpDelete]
        [Authorize(Roles = "Admin, Editor")]
        public ActionResult Delete(int id)
        {
            Book book = db.Books.Find(id);
            if (book.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Books.Remove(book);
                db.SaveChanges();
                TempData["message"] = "Book deleted!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] ="You don't have rights to delete this book!";
                return RedirectToAction("Index");
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>(); 
            var categories = from cat in db.Categories
                             select cat;

            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            return selectList;
        }


        // GET: Upload
        [Authorize(Roles = "Admin, Editor")]
        public ActionResult SaveImages()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Editor")]
        public ActionResult SaveImages(HttpPostedFileBase file)
        {

            var path = "";
            if (file != null)
            {
                if (file.ContentLength > 0)
                {
                    if (Path.GetExtension(file.FileName).ToLower() == ".jpg"
                        || Path.GetExtension(file.FileName).ToLower() == ".jpeg"
                        || Path.GetExtension(file.FileName).ToLower() == ".png"
                        || Path.GetExtension(file.FileName).ToLower() == ".gif")
                    {
                        path = Path.Combine(Server.MapPath("~/Content/UploadedImages"), file.FileName);
                        file.SaveAs(path);
                        return RedirectToAction("New");


                    }
                    else
                    {
                        return View();
                    }
                }
                else
                { return View(); }
                

            }
            else
            {
                return View();
            }

            

        }

    }
}
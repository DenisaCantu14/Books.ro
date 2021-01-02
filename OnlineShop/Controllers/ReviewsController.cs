using Microsoft.AspNet.Identity;
using OnlineShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineShop.Controllers
{
    public class ReviewsController : Controller
    {
        private Models.ApplicationDbContext db = new Models.ApplicationDbContext();
        // GET: Reviews
        public ActionResult Index()
        {


            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Editor, User")]
        public ActionResult New(Review review)
        {
            review.Date = DateTime.Now;
            review.UserId = User.Identity.GetUserId();
            Book book = db.Books.Find(review.BookId);
            try
            {
                if (ModelState.IsValid)
                {
                    book.nrStars = book.nrStars + review.NrStars;
                    book.nrRev++;
                    book.avg = book.nrStars / book.nrRev;
                    book.avg = Math.Round(book.avg, 2);
                    db.Reviews.Add(review);
                    db.SaveChanges();
                    return Redirect("/Books/Show/" + review.BookId);
                }
                else
                {
                    return Redirect("/Books/Show/" + review.BookId);
                }
            }
            catch (Exception)
            {
                return Redirect("/Books/Show/" + review.BookId);
            }
        }
        [Authorize(Roles = "Admin, Editor, User")]
        public ActionResult Edit(int id)
        {
           
            Review review = db.Reviews.Find(id);
            
            if (review.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                return View(review);
            }
            else
            {
                TempData["message"] = "You don't have rights to make changes to this review!";
                return Redirect("/Books/Show/" + review.BookId);
            }

        }

        [HttpPut]
        [Authorize(Roles = "Admin, Editor, User")]
        public ActionResult Edit(int id, Review requestReview)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Review review = db.Reviews.Find(id);
                    if (review.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                    {
                        if (TryUpdateModel(review))
                        {
                            review.Content = requestReview.Content;
                            review.NrStars = requestReview.NrStars;
                            db.SaveChanges();
                        }
                        return Redirect("/Books/Show/" + review.BookId);
                    }
                    else
                    {
                        TempData["message"] = "You don't have rights to make changes to this review!";
                        return RedirectToAction("Index");
                    }
                }
                else
                {

                    return View(requestReview);
                }
            }
            catch (Exception e)
            {
                return View();
            }
        } 

        [HttpDelete]
        [Authorize(Roles = "Admin, Editor, User")]
        public ActionResult Delete(int id)
        {
            Review review = db.Reviews.Find(id);
            var isAdmin = false;
            if(User.IsInRole("Admin"))
            {
                isAdmin = true;
            }
            if (review.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Reviews.Remove(review);
                db.SaveChanges();
                TempData["message"] = "Review deleted!";
                return Redirect("/Books/Show/" + review.BookId);
            }
            else
            {
                TempData["message"] = "You don't have rights to delete this review!";
                return Redirect("/Books/Show/" + review.BookId);
            }
        }
    }
}
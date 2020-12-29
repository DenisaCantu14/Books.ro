using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineShop.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }
        

        [Required(ErrorMessage = "Image name is required")]
        public string ImageName { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Author is required")]
        public string Author { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Required(ErrorMessage = "Publisher is required")]
        public string Publisher { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(1,100000)]
        public float Price { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        public string UserId { get; set; }
        public bool Accepted { get; set; }
        public virtual Category Category { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public IEnumerable<SelectListItem> Categ { get; set; }

    }
}
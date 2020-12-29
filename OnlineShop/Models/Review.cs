using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineShop.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; }
        public int NrStars { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Book Book { get; set; }
    }
}
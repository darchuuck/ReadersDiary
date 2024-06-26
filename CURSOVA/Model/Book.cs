﻿using System.ComponentModel.DataAnnotations;

namespace CURSOVA.Model
{

    public class Book
    {
       
        [Required]
        public string Name { get; set; }
        [Required]
        public List<string> Authors { get; set; }

        public string Url { get; set; }
        public string BookReview { get; set; }

        public long UserId { get; set; }
    }


}

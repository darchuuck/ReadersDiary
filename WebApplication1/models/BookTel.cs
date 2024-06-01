using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBook.models
{
    public class BookTel
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
